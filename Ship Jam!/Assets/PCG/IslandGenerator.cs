using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandGenerator : MonoBehaviour
{
    public float width = 50;
    public float height = 50;

    public int maxIslandCount = 20;
    public float minIslandRadius = 3;
    public float maxIslandRadius = 8;
    public float extraDistanceBetweenIslands = 0;

    public AnimationCurve scaleDistribution;

    public IslandMeshGenerator islandPrefab;

    public bool regenerate = false;

    private void OnValidate()
    {
        if (regenerate)
        {
            regenerate = false;

            Generate();
        }
    }

    internal class Circle
    {
        public Vector2 position;
        public float radius;
    }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Generate(System.DateTime.UtcNow.Millisecond);
    }
    public void Generate(int seed)
    {
        Debug.LogError("Generate");
        Random.InitState(seed);
        foreach (Transform t in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(t.gameObject);
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(t.gameObject);
                };
#endif
            }
        }

        List<Circle> circles = new List<Circle>();
        int attempts = 500;

        while (attempts > 0 && circles.Count < maxIslandCount)
        {
            attempts--;
            Vector2 pos = new Vector2(Random.Range(0, width), Random.Range(0, height));
            float radius = 0;
            if (circles.Count > 0)
            {
                circles.SortBy((Circle c) =>
                {
                    return Mathf.Abs((c.position - pos).ToVec3().Length(DistanceMethod.EUCLIDEAN) - c.radius);
                });
                float distToClosestEdge = -extraDistanceBetweenIslands + (circles[0].position - pos).ToVec3().Length(DistanceMethod.EUCLIDEAN) - circles[0].radius;
                if (distToClosestEdge < minIslandRadius)
                {
                    //invalid island
                    radius = -1;
                }
                else if (distToClosestEdge > maxIslandRadius)
                {
                    //place full size island
                    radius = minIslandRadius + scaleDistribution.Evaluate(Random.value) * (maxIslandRadius - minIslandRadius);
                }
                else
                {
                    //place island touching island or smaller
                    radius = minIslandRadius + scaleDistribution.Evaluate(Random.value) * (maxIslandRadius - minIslandRadius);
                    radius = Mathf.Clamp(radius, minIslandRadius, distToClosestEdge);
                }
            }
            else
            {
                radius = minIslandRadius + scaleDistribution.Evaluate(Random.value) * (maxIslandRadius - minIslandRadius);
            }
            if (radius >= minIslandRadius)
            {
                Circle newCircle = new Circle();
                newCircle.position = pos;
                newCircle.radius = radius;
                circles.Add(newCircle);
            }

        }
        foreach (Circle c in circles)
        {
            IslandMeshGenerator i = Instantiate(islandPrefab, transform);
            i.transform.localPosition = new Vector3(c.position.x, 0, c.position.y);
            i.transform.localScale = Vector3.one * 2 * c.radius;
            i.RepositionGridSensorCollider();
        }

        if (!Application.isPlaying)
        {
            foreach (IslandMeshGenerator i in GetComponentsInChildren<IslandMeshGenerator>())
            {
                i.OnEnable();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position + new Vector3(width/2, 0 , height/2);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center,  new Vector3(width, 1, height));
    }


}
