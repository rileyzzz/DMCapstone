using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum CellType
{
    Land,
    Road,
    Building,
    Water,
    WasteDump,
    WasteDump2,
    RecyclingPlant,
    RecyclingPlant2,
}

public enum ImpulseType
{
    Happiness,
    Pollution
}

class TownCell
{
    public CellType Type;
    public Vector2Int GridPos;
    public float PollutionLevel;
    public float HappinessLevel;
    private GameObject Object;

    public void Instantiate(TownGrid grid)
    {
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
        if (Type == CellType.WasteDump)
        {
            grid.Impulse(ImpulseType.Pollution, GridPos.x, GridPos.y, 5.0f, 1.0f);
        }
        else if (Type == CellType.WasteDump2)
        {
            grid.Impulse(ImpulseType.Pollution, GridPos.x, GridPos.y, 10.0f, 2.0f);
        }
        else if (Type == CellType.RecyclingPlant)
        {
            grid.Impulse(ImpulseType.Happiness, GridPos.x, GridPos.y, 10.0f, 2.0f);
        }
        else if (Type == CellType.RecyclingPlant2)
        {
            grid.Impulse(ImpulseType.Happiness, GridPos.x, GridPos.y, 20.0f, 3.0f);
        }
    }

    public void PostTick(TownGrid grid)
    {
        // Update the gameobject to reflect the current pollution/happiness amount.
        // Show a popup if it's beyond a certain level.
        if (Object)
        {
            ApplyPollution(Object, PollutionLevel);
        }
    }

    private float m_curPollution = 0.0f;
    public void ApplyPollution(GameObject obj, float amt)
    {
        if (amt == m_curPollution)
            return;
        m_curPollution = amt;

        foreach (var mesh in obj.GetComponentsInChildren<MeshRenderer>())
        {
            ApplyPollutionToMesh(mesh, amt);
        }
    }

    private class MaterialMeta
    {
        public Material baseMat;
        public Material instMat;
    }

    private static Dictionary<Material, MaterialMeta> clonedMats = new();
    private void ApplyPollutionToMesh(MeshRenderer mesh, float amt)
    {
        float normalizedAmt = 1.0f - (amt / 100.0f);
        // Ramp it.
        normalizedAmt = Mathf.Lerp(0.5f, 1.0f, normalizedAmt);

        Material[] mats = mesh.materials;
        bool updatedMaterials = false;
        for (int iMat = 0; iMat < mats.Length; ++iMat)
        {
            if (!clonedMats.TryGetValue(mats[iMat], out MaterialMeta meta))
            {
                var oldMat = mats[iMat];
                var newMat = new Material(oldMat);

                meta = new MaterialMeta() {
                    baseMat = oldMat,
                    instMat = newMat
                };
                clonedMats.Add(newMat, meta);

                updatedMaterials = true;
                mats[iMat] = newMat;
            }

            Material mat = mats[iMat];

            Color baseColor = meta.baseMat.color;
            mat.color = new Color(
                baseColor.r * normalizedAmt,
                baseColor.g * normalizedAmt,
                baseColor.b * normalizedAmt,
                baseColor.a);
        }

        if (updatedMaterials) mesh.SetMaterials(mats.ToList());
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

    TownCell[,] Cells;

    private Dictionary<(bool, bool, bool, bool), (GameObject road, int rotation)> _RoadCache;
    public Dictionary<(bool, bool, bool, bool), (GameObject road, int rotation)> RoadCache => _RoadCache;

    void Start()
    {
        BuildRoadCache();
        GenerateLand();
        GenerateCity();
        GenerateRoads();

        Cells[TownSize / 2, TownSize / 2].Type = CellType.WasteDump;

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

    bool CanPlaceCellAt(CellType type)
    {
        if (type == CellType.Land) return true;

        return false;
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
        for (int y = 0; y < TownSize; y++)
        {
            for (int x = 0; x < TownSize; x++)
            {
                Cells[x, y].PostTick(this);
            }
        }
    }
}
