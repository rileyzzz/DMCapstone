using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum CellType
{
    Land,
    Road,
    Building,
    Special,
    Water
}

public enum ImpulseType
{
    Happiness,
    Pollution
}

public struct SpecialCellProperties
{
    public ImpulseType Impulse;
    public float Radius;
    public float Amount;
}

class TownCell
{
    public CellType Type;
    public Vector2Int GridPos;
    public float PollutionLevel;
    public float HappinessLevel;
    private GameObject Object;

    public int SpecialRot;
    public GameObject SpecialPrefab;
    public SpecialCellProperties SpecialProps;

    private GameObject HeavyPollutionObject;
    private GameObject HeavyHappyObject;

    public void Instantiate(TownGrid grid)
    {
        if (HeavyPollutionObject) GameObject.Destroy(HeavyPollutionObject);
        if (HeavyHappyObject) GameObject.Destroy(HeavyHappyObject);
        if (Object) GameObject.Destroy(Object);

        Vector3 cellPos = new Vector3(GridPos.x * TownGrid.CellSize, 0, GridPos.y * TownGrid.CellSize);
        if (Type == CellType.Land)
        {
            var prefab = grid.LandPrefabs[Random.Range(0, grid.LandPrefabs.Count)];
            Object = GameObject.Instantiate(prefab, cellPos, Quaternion.identity, grid.transform);
        }
        else if (Type == CellType.Road)
        {
            var prefab = SelectRoadPrefab(grid, out int rotation);
            var rot = Quaternion.Euler(0, 90.0f * rotation, 0.0f);
            Object = GameObject.Instantiate(prefab, cellPos, rot, grid.transform);
        }
        else if (Type == CellType.Building)
        {
            var prefab = grid.BuildingPrefabs[Random.Range(0, grid.BuildingPrefabs.Count)];
            var rot = Quaternion.Euler(0, 90.0f * Random.Range(0, 3), 0.0f);
            Object = GameObject.Instantiate(prefab, cellPos, rot, grid.transform);
        }
        else if (Type == CellType.Special)
        {
            var rot = Quaternion.Euler(0, 90.0f * SpecialRot, 0.0f);
            Object = GameObject.Instantiate(SpecialPrefab, cellPos, rot, grid.transform);
        }

        if (Object)
        {
            // scale up a tiny bit to prevent seams.
            Object.transform.localScale = Vector3.one * 1.001f;

            ReplaceMaterials();

            // Do this after material replacement.
            HeavyPollutionObject = GameObject.Instantiate(grid.HeavyPollutionPrefab, Object.transform);
            HeavyPollutionObject.SetActive(false);

            HeavyHappyObject = GameObject.Instantiate(grid.HeavyHappyPrefab, Object.transform);
            HeavyHappyObject.SetActive(false);
        }
    }

    private GameObject SelectRoadPrefab(TownGrid grid, out int rotation)
    {
        CellType[,] neighbors = grid.GetCellNeighbors(GridPos.x, GridPos.y);
        bool north = neighbors[1, 0] == CellType.Road;
        bool east = neighbors[2, 1] == CellType.Road;
        bool south = neighbors[1, 2] == CellType.Road;
        bool west = neighbors[0, 1] == CellType.Road;

        var (obj, rot) = grid.RoadCache[(north, south, east, west)];
        rotation = rot;
        return obj;
    }

    public void Impulse(ImpulseType type, float amount)
    {
        if (type == ImpulseType.Happiness)
        {
            HappinessLevel += amount;
            HappinessLevel = Mathf.Clamp(HappinessLevel, 0.0f, 100.0f);
        }
        else if (type == ImpulseType.Pollution)
        {
            PollutionLevel += amount;
            PollutionLevel = Mathf.Clamp(PollutionLevel, 0.0f, 100.0f);
        }
    }

    public void Tick(TownGrid grid)
    {
        if (Type == CellType.Special)
        {
            grid.Impulse(SpecialProps.Impulse, GridPos.x, GridPos.y, SpecialProps.Radius, SpecialProps.Amount);
        }
    }

    public void PostTick(TownGrid grid)
    {
        // Update the gameobject to reflect the current pollution/happiness amount.
        // Show a popup if it's beyond a certain level.
        if (Object)
        {
            ApplyPollution(PollutionLevel);
        }

        if (HeavyHappyObject)
        {
            HeavyHappyObject.SetActive(HappinessLevel > 50.0f);
        }

        if (HeavyPollutionObject)
        {
            HeavyPollutionObject.SetActive(PollutionLevel > 50.0f);
        }
    }

    private float m_curPollution = 0.0f;
    public void ApplyPollution(float amt)
    {
        if (amt == m_curPollution)
            return;
        m_curPollution = amt;

        float normalizedAmt = 1.0f - (amt / 100.0f);
        // Ramp it.
        normalizedAmt = Mathf.Lerp(0.5f, 1.0f, normalizedAmt);

        foreach (var meta in m_instMaterials)
        {
            meta.instMat.color = new Color(
                meta.primary.r * normalizedAmt,
                meta.primary.g * normalizedAmt,
                meta.primary.b * normalizedAmt,
                meta.primary.a);
        }
    }

    private struct MaterialMeta
    {
        public Color primary;
        public Material instMat;
    }

    private List<MaterialMeta> m_instMaterials = new();

    private void ReplaceMaterials()
    {
        m_instMaterials.Clear();

        foreach (var mesh in Object.GetComponentsInChildren<MeshRenderer>())
        {
            Material[] mats = mesh.materials;
            for (int iMat = 0; iMat < mats.Length; ++iMat)
            {
                var oldMat = mats[iMat];
                mats[iMat] = new Material(mats[iMat]);

                Color baseColor = oldMat.color;
                m_instMaterials.Add(new MaterialMeta() {
                    primary = oldMat.color,
                    instMat = mats[iMat]
                });
            }
            mesh.materials = mats;
        }
    }
}

[System.Serializable]
public struct RoadPrefabs
{
    public GameObject Straight;
    public GameObject IntersectionT;
    public GameObject IntersectionFourWay;
    public GameObject CurveL;
}


public class TownGrid : MonoBehaviour
{
    public const float CellSize = 20.0f;
    public const int TownSize = 20;

    private const int TicksPerUpdate = 30;
    private int m_tick = 0;

    public List<GameObject> LandPrefabs;
    public RoadPrefabs RoadPrefabs;
    public List<GameObject> BuildingPrefabs;

    public GameObject WasteDumpPrefab;
    public GameObject RecyclingPlantPrefab;

    public GameObject HeavyPollutionPrefab;
    public GameObject HeavyHappyPrefab;

    TownCell[,] Cells;

    private float m_totalPollution;
    private float m_totalHappiness;

    public float TotalPollution => m_totalPollution;
    public float TotalHappiness => m_totalHappiness;

    private Dictionary<(bool, bool, bool, bool), (GameObject road, int rotation)> _RoadCache;
    public Dictionary<(bool, bool, bool, bool), (GameObject road, int rotation)> RoadCache => _RoadCache;

    void Start()
    {
        BuildRoadCache();
        GenerateLand();
        GenerateCity();
        GenerateRoads();

        // Cells[TownSize / 2, TownSize / 2].Type = CellType.WasteDump;

        InstantiateTown();
    }

    void Update()
    {
    }

    private void FixedUpdate()
    {
        // Only tick in the waiting state.
        if (GameManager.Instance.State != GameState.Waiting)
            return;

        m_tick++;
        if (m_tick >= TicksPerUpdate)
        {
            m_tick = 0;
            UpdateTown();
        }
    }

    public GameObject GetBuildingPrefab(BuildingType type)
    {
        if (type == BuildingType.WasteDump) return WasteDumpPrefab;
        if (type == BuildingType.RecyclingPlant) return RecyclingPlantPrefab;
        return null;
    }

    void BuildRoadCache()
    {
        _RoadCache = new() {
            // N, S, E, W
            { (false, false, false, false), (RoadPrefabs.Straight, 1) },
            { (false, false, false, true), (RoadPrefabs.Straight, 1) },
            { (false, false, true, false), (RoadPrefabs.Straight, 1) },
            { (false, false, true, true), (RoadPrefabs.Straight, 1) },

            { (false, true, false, false), (RoadPrefabs.Straight, 0) },
            { (false, true, false, true), (RoadPrefabs.CurveL, 0) },
            { (false, true, true, false), (RoadPrefabs.CurveL, 3) },
            { (false, true, true, true), (RoadPrefabs.IntersectionT, 1) },

            { (true, false, false, false), (RoadPrefabs.Straight, 0) },
            { (true, false, false, true), (RoadPrefabs.CurveL, 1) },
            { (true, false, true, false), (RoadPrefabs.CurveL, 2) },
            { (true, false, true, true), (RoadPrefabs.IntersectionT, 3) },

            { (true, true, false, false), (RoadPrefabs.Straight, 0) },
            { (true, true, false, true), (RoadPrefabs.IntersectionT, 0) },
            { (true, true, true, false), (RoadPrefabs.IntersectionT, 2) },
            { (true, true, true, true), (RoadPrefabs.IntersectionFourWay, 0) },
        };
    }

    void GenerateLand()
    {
        // Build the base grid.
        Cells = new TownCell[TownSize, TownSize];
        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                var cell = new TownCell();
                cell.Type = CellType.Land;
                cell.GridPos = new Vector2Int(x, y);
                Cells[x, y] = cell;
            }
        }
    }

    void GenerateCity()
    {
        const float initTownRadius = 5.0f;
        Vector2 center = new Vector2(TownSize / 2.0f, TownSize / 2.0f);
        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                Vector2 pos = new Vector2(x, y);

                if (Vector2.Distance(pos, center) < initTownRadius)
                {
                    Cells[x, y].Type = CellType.Building;
                }
            }
        }
    }

    void GenerateRoads()
    {
        // Build roads over the existing town.
        const int blockSize = 3;
        for (int y = 0; y < TownSize; y += blockSize)
        {
            for (int x = 0; x < TownSize; x++)
            {
                if (Cells[x,y].Type == CellType.Building)
                {
                    // Change building to a road cell.
                    Cells[x, y].Type = CellType.Road;
                }
            }
        }

        for (int x = 0; x < TownSize; x += blockSize)
        {
            for (int y = 0; y < TownSize; y++)
            {
                if (Cells[x, y].Type == CellType.Building)
                {
                    // Change building to a road cell.
                    Cells[x, y].Type = CellType.Road;
                }
            }
        }
    }

    CellType GetCellTypeSafe(int x, int y)
    {
        if (x < 0 || x >= TownSize || y < 0 || y >= TownSize)
            return CellType.Land;

        return Cells[x, y].Type;
    }

    public CellType[,] GetCellNeighbors(int x, int y)
    {
        CellType[,] neighbors = new CellType[3, 3];
        for (int i = 0; i <= 2; i++)
            for (int j = 0; j <= 2; j++)
                neighbors[i, j] = GetCellTypeSafe(x + (i - 1), y + (j - 1));

        return neighbors;
    }

    void InstantiateTown()
    {
        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                Cells[x, y].Instantiate(this);
            }
        }
    }

    public bool CanPlaceCellAt(BuildingType building, int x, int y)
    {
        if (Cells[x, y].Type == CellType.Land) return true;

        return false;
    }

    public bool PlaceCell(BuildingType building, int x, int y, int rot, in SpecialCellProperties props)
    {
        if (x < 0 || x >= TownSize || y < 0 || y >= TownSize || !CanPlaceCellAt(building, x, y))
            return false;

        Cells[x, y].Type = CellType.Special;
        Cells[x, y].SpecialPrefab = GetBuildingPrefab(building);
        Cells[x, y].SpecialRot = rot;
        Cells[x, y].SpecialProps = props;

        // Update the map.
        Cells[x, y].Instantiate(this);

        return true;
    }

    public void Impulse(ImpulseType type, int _x, int _y, float radius, float amt)
    {
        int r = (int)(radius + 1.0f);
        Vector2 center = new Vector2(_x, _y);
        for (int y = _y - r; y <= _y + r; y++)
        {
            for (int x = _x - r; x <= _x + r; x++)
            {
                if (x < 0 || x >= TownSize || y < 0 || y >= TownSize)
                    continue;

                Vector2 pos = new Vector2(x, y);
                float dist = Vector2.Distance(center, pos);
                float factor = 1.0f - (dist / radius);
                if (factor <= 0.0f)
                    continue;

                // This cell has been affected.
                Cells[x, y].Impulse(type, factor * amt);
            }
        }
    }


    void UpdateTown()
    {
        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                Cells[x, y].Tick(this);
            }
        }

        // Second tick to update the visual results after pollution impulse etc.
        float totalPollution = 0.0f;
        float totalHappiness = 0.0f;

        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                Cells[x, y].PostTick(this);

                totalPollution += Cells[x, y].PollutionLevel;
                totalHappiness += Cells[x, y].HappinessLevel;
            }
        }

        m_totalPollution = totalPollution / (TownSize * TownSize);
        m_totalHappiness = totalHappiness / (TownSize * TownSize);

        GameManager.Instance.UpdateUI();
    }
}
