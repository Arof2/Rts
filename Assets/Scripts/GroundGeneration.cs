using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class GroundGeneration : NetworkBehaviour
{
    public GameObject test;
    Mesh groundMesh;
    private GameObject groundObject;
    public Gradient grad;
    public static GroundGeneration instance;
    public GameObject[] meadowObjects;
    public Vector3[] meadowPos;
    public Vector3[,] pixelpos; // 1:x | 2:y

    public Texture2D T1 = null;
    public Texture2D T2 = null;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    public int size = 5;
    private int xSize = 5;
    private int ySize = 5;

    public List<GameObject> grassPrefabs = new List<GameObject>();
    public int grassAmount = 500;

    private float minTerrainheight;
    private float maxTerrainheight;

    public void Start()
    {
        pixelpos = new Vector3[size,size];
        meadowObjects = new GameObject[grassAmount];
        meadowPos = new Vector3[grassAmount];

        instance = this;
        xSize = size;
        ySize = size;

        groundMesh = new Mesh();
        groundMesh.name = "Ground Mesh";
        GetComponent<MeshFilter>().mesh = groundMesh;
        groundObject = gameObject;

        CreateGround();
        ApplyGroundMesh();

        



        for (int i = 0; i < 2; i++)
        {
            float[] redAry = new float[size * size];
            float[] greenAry = new float[size * size];
            float[] blueAry = new float[size * size];
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Color c = grad.Evaluate((float)Random.Range(0f, 1f));

                    redAry[x + y* size] = c.r;
                    greenAry[x + y * size] = c.g;
                    blueAry[x + y * size] = c.b;

                    if(i == 0)
                    {
                        Vector3 target = new Vector3(-x - 0.5f, 0, -y - 0.5f);
                        pixelpos[x, y] = gameObject.transform.position + target;
                        //Instantiate(test, gameObject.transform.position + target, Quaternion.identity, transform);
                    }
                        
                        
                }
            }


            if(i == 0)
            {
                T1 = GenerateTexture(redAry, greenAry, blueAry, size, size);
            }
            else
            {
                T2 = GenerateTexture(redAry, greenAry, blueAry, size, size);
            }
        }

        T1.filterMode = FilterMode.Point;
        T2.filterMode = FilterMode.Point;

        UpdateTexture();

        GenerateGrass();
    }

    public void GenerateGrass()
    {
        if(Application.isPlaying)
        {
            for (int i = 0; i < grassAmount; i++)
            {
                GameObject G = Instantiate(grassPrefabs[Random.Range(0, grassPrefabs.Count)], transform);
                G.transform.position = new Vector3(Random.Range((float)-(xSize / 2), (float)xSize / 2) - xSize / 2, maxTerrainheight, Random.Range((float)-(ySize / 2), (float)ySize / 2) - ySize / 2) + transform.GetComponentInParent<Transform>().position;
                meadowObjects[i] = G;
                meadowPos[i] = G.transform.position;
            }
        }
    }



    void CreateGround()
    {
        vertices = new Vector3[(xSize + 1) * (ySize + 1) + 1];

        for (int c = 0, i = 0; i <= xSize; i++)
        {
            for (int m = 0; m <= ySize; m++)
            {
                float y = Mathf.PerlinNoise(m * .3f, i*.3f) * 0.3f;
                vertices[c] = new Vector3(m - xSize,y,i - ySize);

                if (y > maxTerrainheight)
                    maxTerrainheight = y;
                if (y < minTerrainheight)
                    minTerrainheight = y;
                c++;
            }
        }
        
        triangles = new int[xSize * ySize * 6];

        int vert = 0;
        int tris = 0;
        for (int m = 0; m < ySize; m++)
        {
            for (int i = 0; i < xSize; i++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        uvs = new Vector2[vertices.Length];

        for (int c = 0, i = 0; i <= xSize; i++)
        {
            for (int m = 0; m <= ySize; m++)
            {
                uvs[c] = new Vector2((float)m / xSize, (float)i /ySize);
                c++;
            }
        }
    }

    void ApplyGroundMesh()
    {
        groundMesh.Clear();

        groundMesh.vertices = vertices;
        groundMesh.triangles = triangles;
        groundMesh.uv = uvs;

        groundMesh.RecalculateNormals();
    }

    int textureCount = 0;
    public Texture2D GenerateTexture(float[] redArray, float[] greenArray, float[] blueArray, int imgHeight, int imgwidth)
    {
        Texture2D texture = new Texture2D(imgwidth, imgHeight, TextureFormat.ARGB32,false);

        string filename = "Generated Texture " + textureCount;

        for (int y = 0; y < imgHeight; y++)
        {
            for (int x = 0; x < imgwidth; x++)
            {
                Color pixelColor = new Color(redArray[y * imgHeight + x], greenArray[y * imgHeight + x], blueArray[y * imgHeight + x]);
                texture.SetPixel(x, y, pixelColor);
            }
        }
        texture.name = filename;
        texture.Apply();

        //Sprite output = Sprite.Create(texture, new Rect(0, 0, imgwidth, imgHeight), new Vector2(0.5f, 0.5f));

        //byte[] bytes = texture.EncodeToPNG();
        //File.WriteAllBytes(Application.dataPath + "/Ressources/" + filename + ".png", bytes);

        textureCount++;

        return texture;
    }

    public void ChangePixel(Vector2 pos, Color col, string layer)
    {
        int xpos = (((int)pos.x) + (size)) - (int)pos.x * 2;
        int ypos = (((int)pos.y) + (size)) - (int)pos.y * 2;

        xpos -= 1;
        ypos -= 1;

        if (layer == "fore")
            T1.SetPixel(xpos, ypos, col);
        else if (layer == "back")
            T2.SetPixel(xpos, ypos, col);

        

        UpdateTexture();
    }

    public Color GetPixelColor(Vector2 pos, string layer)
    {
        Texture2D T = null;
        if (layer == "fore")
            T = T1;
        else if (layer == "back")
            T = T2;
        int xpos = ((((int)pos.x) + size) - (int)pos.x * 2) - 1;
        int ypos = ((((int)pos.y) + size) - (int)pos.y * 2) -1;
        return T.GetPixel(xpos, ypos);
    }

    public void UpdateTexture()
    {
        T1.Apply();
        T2.Apply();

        Material M = groundObject.GetComponent<MeshRenderer>().sharedMaterial;
        M.SetTexture("_Top", T1);
        M.SetTexture("_Bottom", T2);
    }
}
