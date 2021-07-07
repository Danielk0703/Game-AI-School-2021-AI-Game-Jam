using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandMeshGenerator : MonoBehaviour
{
    public bool randomiseOnEnable = true;

    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshCollider meshCollider;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    [Range(1, 50)]
    private int width = 20;
    [SerializeField]
    [Range(1, 50)]
    private int depth = 20;

    public Vector2 noiseOffset;
    public Vector2 noiseScale = Vector2.one * 0.15f;
    public float amplitude = 4;
    public float amplitudeMultiplierAboveSeaLevel = 2;

    public float rounding = 0f;
    public float scale = 0.3f;
    public Material defaultMaterial;
    Texture2D texture;
    Material materialInstance;
    float[,] heightMap;


    public GameObject tree;

    public void OnEnable()
    {
        meshFilter = meshFilter ?? GetComponent<MeshFilter>();
        meshCollider = meshCollider ?? GetComponent<MeshCollider>();
        meshRenderer = meshRenderer ?? GetComponent<MeshRenderer>();
        if (randomiseOnEnable) { noiseOffset.x = Random.Range(0, 100f);noiseOffset.y = Random.Range(0, 100f); }
        GeneratePlane();
    }

    public Texture2D gaussianDistribution;
    private float EdgeDistance(float w, float d) {
        float x = (float)w / (width + 1);
        float y = (float)d / (depth + 1);
        return gaussianDistribution.GetPixel((int)(x*gaussianDistribution.width), (int)(y * gaussianDistribution.height)).r;
    }

    private void GeneratePlane()
    {if (noiseOffset.x == -1 && noiseOffset.y == -1) {
            noiseOffset.x = Random.Range(0, 100f);
        }

        Mesh colMesh = GenerateMesh(4);
        Mesh gfxMesh = GenerateMesh(1);

        meshFilter.mesh = gfxMesh;
        meshCollider.sharedMesh = colMesh;

        ResetTexture();
        if (Application.isPlaying)
        {
            texture = new Texture2D(width, depth);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < depth; y++)
                {
                    Color colour = new Color(42 / 255f, 181 / 255f, 65 / 255f);
                    if (heightMap[x, y] < ((amplitude * amplitudeMultiplierAboveSeaLevel) - transform.position.y) / 2.5f)
                    {
                        colour = new Color(1, 246 / 255f, 150 / 255f);
                        if (heightMap[x, y] > 0)
                        {//above water level
                         //place stuff on sand here
                        }

                    }
                    else if (heightMap[x, y] > ((amplitude * amplitudeMultiplierAboveSeaLevel) - transform.position.y) / 1.8f)
                    {
                        colour = new Color(0.3f, 0.3f, 0.3f);
                        //place stuff on rock here
                    }
                    else
                    {
                        //place stuff on grass here
                        //you might want to do something different here but I placed a load of spheres as an example :)
                        Instantiate(tree, WorldPoint(x, y), Quaternion.identity, transform);
                    }
                    texture.SetPixel(x, y, colour);
                }
            }
            texture.Apply();
            materialInstance = Instantiate<Material>(defaultMaterial);
            meshRenderer.sharedMaterial = materialInstance;
            materialInstance.mainTexture = texture;
        }

    }

    public Vector3 WorldPoint(int x, int y) {
        return transform.position + scale * new Vector3(
                        ((float)x - (width / 2.0f)) * transform.lossyScale.x,
                        heightMap[x, y] * transform.lossyScale.y,
                        ((float)y - (depth / 2.0f)) * transform.lossyScale.z
                        );
    }

    private void OnValidate()
    {
        if (meshFilter != null && meshCollider!=null && gaussianDistribution != null) {
            GeneratePlane();
        }
    }


    Mesh GenerateMesh(int crunchLevel) {
        int depth = (int)((float)this.depth/(crunchLevel));
        int width = (int)((float)this.width/(crunchLevel));
        float[,] heightMap = new float[width + 1, depth + 1];
        for (int d = 0; d <= depth; d++)
        {
            for (int w = 0; w <= width; w++)
            {
                heightMap[w, d] = amplitude * Mathf.Lerp(Mathf.PerlinNoise(((w*crunchLevel) + noiseOffset.x) * noiseScale.x, ((d*crunchLevel) + noiseOffset.y) * noiseScale.y)
                    * EdgeDistance(w*crunchLevel, d*crunchLevel), EdgeDistance(w*crunchLevel, d*crunchLevel), rounding);
                if (transform.position.y + heightMap[w, d] >= 0) { heightMap[w, d] *= amplitudeMultiplierAboveSeaLevel; }
            }
        }
        if (crunchLevel == 1) {
            this.heightMap = heightMap;
        }

        // Creating a mesh object.
        Mesh mesh = new Mesh();
        // Defining vertices.
        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)];
        int i = 0;
        for (int d = 0; d <= depth; d++)
        {
            for (int w = 0; w <= width; w++)
            {
                vertices[i] = new Vector3(w *crunchLevel, heightMap[w, d], d * crunchLevel) - new Vector3((width / 2f) * (crunchLevel), 0, (depth / 2f) * (crunchLevel));
                vertices[i] *= scale;
                i++;
            }
        }
        // Defining triangles.
        int[] triangles = new int[width * depth * 2 * 3]; // 2 - polygon per quad, 3 - corners per polygon
        for (int d = 0; d < depth; d++)
        {
            for (int w = 0; w < width; w++)
            {
                // quad triangles index.
                int ti = (d * (width) + w) * 6; // 6 - polygons per quad * corners per polygon
                                                // First triangle
                triangles[ti] = (d * (width + 1)) + w;
                triangles[ti + 1] = ((d + 1) * (width + 1)) + w;
                triangles[ti + 2] = ((d + 1) * (width + 1)) + w + 1;
                // Second triangle
                triangles[ti + 3] = (d * (width + 1)) + w;
                triangles[ti + 4] = ((d + 1) * (width + 1)) + w + 1;
                triangles[ti + 5] = (d * (width + 1)) + w + 1;
            }
        }
        // Defining UV.
        Vector2[] uv = new Vector2[(width + 1) * (depth + 1)];
        i = 0;
        for (int d = 0; d <= depth; d++)
        {
            for (int w = 0; w <= width; w++)
            {
                uv[i] = new Vector2(w / (float)width, d / (float)depth);
                i++;
            }
        }
        // Assigning vertices, triangles and UV to the mesh.
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void OnDestroy()
    {
        ResetTexture();
    }

    void ResetTexture()
    {
        if (Application.isPlaying)
        {
            Destroy(texture);
            Destroy(materialInstance);
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                DestroyImmediate(materialInstance);
                DestroyImmediate(texture);
            };
#endif
        }
    }
}
