using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseTerrain : MonoBehaviour
{
    public int mDivisions; //length of square
    public float mSize;
    public float mHeight;

    // Start is called before the first frame update
    void Start()
    {
        CreateTerrain();
    }

    void CreateTerrain()
    {
        int divPlusOne = mDivisions + 1;
        int vertCount = divPlusOne*divPlusOne;

        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] tris = new int[mDivisions * mDivisions * 2 * 3];

        float halfSize = mSize * 0.5f;
        float divisionSize = mSize / mDivisions;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        //Iterate over all vertices
        for (int i = 0; i <= mDivisions; i++)
        {
            for (int j = 0; j <= mDivisions; j++)
            {
                float xCoord = (float)j / mDivisions;
                float yCoord = (float)i / mDivisions;
                float height = Mathf.PerlinNoise(xCoord * 4, yCoord * 4) + 0.5f * Mathf.PerlinNoise(xCoord * 8,yCoord * 8) + 0.25f * Mathf.PerlinNoise(xCoord * 16, yCoord * 16); //overlay
                height = Mathf.Pow(height, 2);
                height *= mHeight;

                verts[i * divPlusOne + j] = new Vector3(-halfSize + j * divisionSize, height, halfSize - i * divisionSize);
                uvs[i * divPlusOne + j] = new Vector2((float)j / mDivisions, (float)i / mDivisions);

                if(i<mDivisions && j < mDivisions)
                {
                    int topLeft = i * divPlusOne + j;
                    int botLeft = (i + 1) * divPlusOne + j;

                    tris[triOffset] = topLeft;
                    tris[triOffset + 1] = topLeft + 1;
                    tris[triOffset + 2] = botLeft + 1;

                    tris[triOffset + 3] = topLeft;
                    tris[triOffset + 4] = botLeft + 1;
                    tris[triOffset + 5] = botLeft;

                    triOffset += 6;
                }
            }
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;

        //render mesh
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
