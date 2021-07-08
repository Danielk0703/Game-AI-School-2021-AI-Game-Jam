using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class IslandDecorator : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(IslandDecorator))]
    public class customButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            IslandDecorator decorator = (IslandDecorator)target;

            if (GUILayout.Button("Decorate"))
            {
                decorator.Decorate();
            }
        }
    }
#endif
    private Decorations_ScriptableObject decorationPools;
    private IslandMeshGenerator islandInfo;
    private Dictionary<LandType, List<int[]>> landCoords;
    private Dictionary<LandType, float> landPercentages;
    private Dictionary<string, int> decorationsCounts;
    private List<GameObject> placedDecorations;
    [SerializeField] private int maxDecorations = 15;

    int rayHitLayerMask;
    private void Initialize()
    {
        decorationPools = (Decorations_ScriptableObject)Resources.Load("Decorations", typeof(Decorations_ScriptableObject));
        rayHitLayerMask = LayerMask.GetMask("Islands", "Decorations", "Sea");
        islandInfo = GetComponent<IslandMeshGenerator>();
        landCoords = new Dictionary<LandType, List<int[]>>();
        landPercentages = new Dictionary<LandType, float>();
        decorationsCounts = new Dictionary<string, int>();
        placedDecorations = new List<GameObject>();
    }
    private void GetCoordsForLands()
    {
        for (int x = 0; x < islandInfo.Width; x++)
        {
            for (int y = 0; y < islandInfo.Depth; y++)
            {
                LandType lt = islandInfo.GetLandTypeAt(x, y);
                if (!landCoords.ContainsKey(lt))
                {
                    landCoords.Add(lt, new List<int[]>());
                    landPercentages.Add(lt, 0f);
                }
                landCoords[lt].Add(new int[] { x, y });
            }
        }
        foreach (LandType lt in landCoords.Keys)
        {
            if (lt == LandType.DeepSea) continue;
            landPercentages[lt] = landCoords[lt].Count / ((islandInfo.Width * islandInfo.Depth) - landCoords[LandType.DeepSea].Count);
        }
    }
    public void Decorate()
    {
        Initialize();
        GetCoordsForLands();
        if (landCoords.Keys.ToList().Count == 0)
        {
            print("Can't place here!");
            return;
        }
        Vector3 placementPosition;
        List<LandType> landTypes = landCoords.Keys.ToList();
        for (int j = 0; j < maxDecorations; j++)
        {
            // Select a random land to put decorations onto
            LandType currentLandType;
            if (landTypes.Count == 1)
                currentLandType = landTypes[0];
            else
            {
                int randomLandIndex = UnityEngine.Random.Range(0, landTypes.Count);
                currentLandType = landTypes[randomLandIndex];
            }
            // get the decorations pool that can be placed on the land
            List<DecorationPool> landPools = decorationPools.decorationPools.FindAll(p => p.landTypePlacement == currentLandType);
            if (landPools.Count == 0)
            {
                j--;
                continue;
            }

            float roll = UnityEngine.Random.Range(0f, 1f);
            landPools = landPools.FindAll(p => RolledDecorationPool(p, roll, currentLandType));
            if (landPools.Count == 0)
            {
                j--;
                continue;
            }

            foreach (DecorationPool pool in landPools)
            {
                int placementCountRoll = UnityEngine.Random.Range(1, (int)pool.maxPlacements);
                int decorationIndex = (int)UnityEngine.Random.Range(0, pool.decorations.Count);
                string decorationName = pool.decorations[decorationIndex].name;
                int maxPlacements = (int)(pool.maxPlacementsInfluencedByLandPercentage ? pool.maxPlacements * (1f + landPercentages[currentLandType]) : pool.maxPlacements);
                if (!decorationsCounts.ContainsKey(decorationName))
                    decorationsCounts[decorationName] = 0;
                else if (decorationsCounts[decorationName] >= maxPlacements)
                {
                    continue;
                }
                GameObject newDecoration = GameObject.Instantiate(pool.decorations[decorationIndex]);
                newDecoration.name = string.Format("{0} {1}", decorationName, decorationsCounts[decorationName]);

                if (GetPlacementPosition(0.5f, out placementPosition, currentLandType))
                    newDecoration.transform.position = placementPosition;
                else
                {
                    Destroy(newDecoration);
                    continue;
                }
                if ((placementPosition + Vector3.up * newDecoration.GetComponent<Collider>().bounds.extents.y).y < 0)
                {
                    Destroy(newDecoration);
                    continue;
                }
                newDecoration.transform.SetParent(transform, true);

                Vector3 angles = new Vector3(
                    pool.maxRotationAngle.x * UnityEngine.Random.Range(0f, 1f) * pool.rotationAxis.x,
                    pool.maxRotationAngle.y * UnityEngine.Random.Range(0f, 1f) * pool.rotationAxis.y,
                    pool.maxRotationAngle.z * UnityEngine.Random.Range(0f, 1f) * pool.rotationAxis.z);
                newDecoration.transform.Rotate(angles);
                placedDecorations.Add(newDecoration);
                decorationsCounts[decorationName] += 1;
            }
            j += landPools.Count - 1;
        }
        foreach (GameObject decoration in placedDecorations)
        {
            decoration.GetComponent<Collider>().enabled = false;
        }
    }
    private bool RolledDecorationPool(DecorationPool pool, float roll, LandType landType)
    {
        if (pool.chanceSameAsLandPercentage)
        {
            if (pool.minLandPercentageRequired <= landPercentages[landType])
                return 1f - landPercentages[landType] <= roll;
            else
                return false;
        }
        else
        {
            return 1f - pool.placementChance <= roll;
        }
    }
    private bool GetPlacementPosition(float radius, out Vector3 placementPosition, LandType landType)
    {
        RaycastHit hitInfo;
        placementPosition = Vector3.zero;
        List<int[]> currentLandCoords = landCoords[landType];
        for (int j = 0; j < 1000; j++)
        {
            int coordsIndex = UnityEngine.Random.Range(0, currentLandCoords.Count);
            int[] selectedCoords = currentLandCoords[coordsIndex];
            int x = selectedCoords[0], z = selectedCoords[1];

            Vector3 meshPoint = islandInfo.WorldPoint(x, z);
            Vector3 rayOrigin = meshPoint + Vector3.up * 10f;
            Vector3 rayDirection = (meshPoint - rayOrigin).normalized;
            Ray ray = new Ray(rayOrigin, rayDirection);
            if (Physics.SphereCast(ray, radius, out hitInfo, Mathf.Infinity, rayHitLayerMask, QueryTriggerInteraction.Collide))
            {
                if ((1 << hitInfo.transform.gameObject.layer) == LayerMask.GetMask("Islands"))
                {
                    placementPosition = meshPoint;
                    landType = islandInfo.GetLandTypeAt(x, z);
                    return true;
                }
            }
        }
        return false;
    }
}
