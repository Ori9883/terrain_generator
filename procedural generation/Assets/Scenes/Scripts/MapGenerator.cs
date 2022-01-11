using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour
{
    [Header("Generate Tile Map")]
    public GameObject tilePrefab;
    public Vector2 mapSize;
    public Transform mapHolder;
    [Range(0, 1)] public float outlinePercent;

    [Header("Generate Obstacle")]
    public GameObject obsPrefab;

    [Header("All Data List")]
    public List<Coordinate> allTilesCoord = new List<Coordinate>();
    public Dictionary<Coordinate,GameObject> allTile= new Dictionary<Coordinate, GameObject>();
    private Queue<Coordinate> shuffledQueue;

    [Header("Obstacle appearance & color")]
    public Color foregroundColor, backgroundColor;
    public float maxObsHeight;

    [Header("Map fully Accessible")]
    [Range(0, 1)] public float obsPercent;
    private Coordinate mapCenter;
    bool[,] mapObstacles; //判断位置是否有障碍物

    [Header("Road Generate")]
    public bool renderRoad;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        for (int i = 0; i < mapSize.x; i++)
        {
            for (int j = 0; j < mapSize.y; j++)
            {
                Vector3 newPos = new Vector3(-mapSize.x/2 + 0.5f + i, 0, -mapSize.y/2 + 0.5f + j);
                GameObject spawnTile = Instantiate(tilePrefab, newPos, Quaternion.Euler(90, 0, 0));
                spawnTile.transform.SetParent(mapHolder);
                spawnTile.transform.localScale *= (1 - outlinePercent);
                Coordinate newTile = new Coordinate(i, j);
                allTile.Add(newTile, spawnTile);
                allTilesCoord.Add(newTile);
            }   
        }

        shuffledQueue = new Queue<Coordinate>(Utilities.ShuffleCoords(allTilesCoord.ToArray())); //瓦片打乱顺序放入队列

        int obsCount = (int)(mapSize.x * mapSize.y * obsPercent);  //目标障碍物数量
        mapCenter = new Coordinate((int)(mapSize.x/2), (int)(mapSize.y/2));
        mapObstacles = new bool[(int)mapSize.x, (int)mapSize.y];

        int currentObsCount = 0; //目前障碍物数量

        for (int i = 0; i < obsCount; i++)
        {
            Coordinate randomCoord = GetRandomCoord();

            mapObstacles[randomCoord.x, randomCoord.y] = true; //假设坐标位置有障碍物
            currentObsCount++;
            if (randomCoord != mapCenter && MapIsFullyAccessible(mapObstacles, currentObsCount))
            {
                #region 随机障碍物高度
                float xCoord = (float)(randomCoord.x - mapSize.x / 2 + 0.5f) / mapSize.x + UnityEngine.Random.Range(0,1f);
                float yCoord = (float)(randomCoord.x - mapSize.x / 2 + 0.5f) / mapSize.y + UnityEngine.Random.Range(0, 1f);
                float obsHeight = Mathf.PerlinNoise(xCoord * 4, yCoord * 4) + 0.5f * Mathf.PerlinNoise(xCoord * 8, yCoord * 8) + 0.25f * Mathf.PerlinNoise(xCoord * 16, yCoord * 16);
                obsHeight = Mathf.Pow(obsHeight, 2);
                obsHeight *= maxObsHeight;
                #endregion
                float yScale;
                GameObject Obs = Utilities.instance.FindBuildingByHeight(obsHeight, maxObsHeight,out yScale);
                Debug.Log(obsHeight+" : "+yScale);
                Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f + randomCoord.x, 0, -mapSize.y / 2 + 0.5f + randomCoord.y);
                GameObject spawnObs = Instantiate(Obs, newPos, Quaternion.identity);
                spawnObs.transform.SetParent(mapHolder);
                spawnObs.transform.localScale = new Vector3(spawnObs.transform.localScale.x - outlinePercent, yScale, spawnObs.transform.localScale.z - outlinePercent);

                #region lerp color
                MeshRenderer meshRenderer = spawnObs.GetComponent<MeshRenderer>();
                Material material = meshRenderer.material;

                float colorPercent = randomCoord.y / mapSize.y;
                material.color = Color.Lerp(foregroundColor, backgroundColor, colorPercent);
                meshRenderer.material = material;
                #endregion
            }
            else
            {
                mapObstacles[randomCoord.x, randomCoord.y] = false;
                currentObsCount--;
            }
        } //生成目标障碍物数量次数的障碍物

        if (renderRoad)
        {
            GenerateRoad(mapObstacles); //生成道路
        }

    }

    private bool MapIsFullyAccessible(bool[,] mapObstacles, int currentObsCount)
    {
        bool[,] mapFlags = new bool[mapObstacles.GetLength(0), mapObstacles.GetLength(1)];

        Queue<Coordinate> queue = new Queue<Coordinate>();
        queue.Enqueue(mapCenter);
        mapFlags[mapCenter.x, mapCenter.y] = true;

        int accessibleCount = 1;

        while (queue.Count > 0)
        {
            Coordinate currentTile = queue.Dequeue();
            for (int i = -1; i <=1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int neighborX = currentTile.x + i;
                    int neighborY = currentTile.y + j;

                    if(i==0 || j == 0)
                    {
                        if(neighborX>=0 && neighborX < mapObstacles.GetLength(0)
                            && neighborY >=0 && neighborY < mapObstacles.GetLength(1))
                        {
                            if(!mapFlags[neighborX,neighborY] && !mapObstacles[neighborX, neighborY])
                            {
                                mapFlags[neighborX, neighborY] = true;
                                accessibleCount++;
                                queue.Enqueue(new Coordinate(neighborX, neighborY));
                            }
                        }
                    }
                }
            }
        }

        int obsTargetCount = (int)(mapSize.x * mapSize.y) - currentObsCount; //剩下可行走的数量 = 总数-假设的障碍物数量
        return accessibleCount == obsTargetCount; //如果假设可行走的 = 实际可行走的 证明在假设位置放置障碍不影响连续性
    }

    private void GenerateRoad(bool[,] mapObstacles)
    {

        for (int i = 0; i < mapObstacles.GetLength(0); i++)
        {
            for (int j = 0; j < mapObstacles.GetLength(1); j++)
            {
                if(mapObstacles[i,j] == false)
                {
                    for (int n = 0; n < allTilesCoord.Count; n++)
                    {
                        Coordinate[] array = allTilesCoord.ToArray();
                        if (array[n].x == i && array[n].y == j)
                        {
                            Coordinate target = array[n];
                            GameObject road = allTile[target];
                            MeshRenderer meshRenderer = road.GetComponent<MeshRenderer>();
                            Material material = meshRenderer.material;
                            material.color = Color.grey;
                        }
                    }
                }
            }
        }
    }

    private Coordinate GetRandomCoord()
    {
        Coordinate randomCoord = shuffledQueue.Dequeue();
        shuffledQueue.Enqueue(randomCoord);
        return randomCoord;
    }
}

[System.Serializable]
public struct Coordinate
{
    public int x;
    public int y;

    public Coordinate(int _x, int _y) {
        this.x = _x;
        this.y = _y;
    }

    public static bool operator ==(Coordinate c1, Coordinate c2)
    {
        return (c1.x == c2.x)&&(c1.y == c2.y);
    }

    public static bool operator !=(Coordinate c1, Coordinate c2){
        return !(c1 == c2);
    }
}
