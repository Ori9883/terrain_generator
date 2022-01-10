using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public Vector2 mapSize;

    public Transform mapHolder;
    [Range(0, 1)] public float outlinePercent;

    public GameObject obsPrefab;
    public float obsCount;

    public List<Coordinate> allTilesCoord = new List<Coordinate>();
    private Queue<Coordinate> shuffledQueue;

    public Color foregroundColor, backgroundColor;
    public float minObsHeight, maxObsHeight;

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

                allTilesCoord.Add(new Coordinate(i, j));
            }   
        }

        shuffledQueue = new Queue<Coordinate>(Utilities.ShuffleCoords(allTilesCoord.ToArray())); //瓦片打乱顺序放入队列
        for (int i = 0; i < obsCount; i++)
        {
            Coordinate randomCoord = GetRandomCoord();

            //float obsHeight = UnityEngine.Random.Range(minObsHeight, maxObsHeight); //随机障碍物高度
            float obsHeight = Mathf.Lerp(minObsHeight, maxObsHeight, UnityEngine.Random.Range(0,1f));

            Vector3 newPos = new Vector3(-mapSize.x / 2 + 0.5f + randomCoord.x, obsHeight/2, -mapSize.y / 2 + 0.5f + randomCoord.y);
            GameObject spawnObs = Instantiate(obsPrefab, newPos, Quaternion.identity);
            spawnObs.transform.SetParent(mapHolder);
            //spawnObs.transform.localScale *= (1 - outlinePercent);
            spawnObs.transform.localScale = new Vector3(1 - outlinePercent, obsHeight, 1 - outlinePercent);

            #region lerp color
            MeshRenderer meshRenderer = spawnObs.GetComponent<MeshRenderer>();
            Material material = meshRenderer.material;

            float colorPercent = randomCoord.y / mapSize.y;
            material.color = Color.Lerp(foregroundColor, backgroundColor, colorPercent);
            meshRenderer.material = material;
            #endregion
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

    public Coordinate(int _x, int _y){
        this.x = _x;
        this.y = _y;
    }
}
