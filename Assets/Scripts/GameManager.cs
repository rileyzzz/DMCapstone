using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum GameState
{
    Waiting,
    SelectingCard,
    PlacingBuilding,
    GameOver
}

/// <summary>
/// Game manager singleton class.
/// </summary>
public class GameManager : MonoBehaviour
{
    static GameManager _Instance = null;
    public static GameManager Instance => _Instance;

    protected float m_lastRoundTime;

    public int m_numRounds = 10;

    public float m_timeBetweenRounds = 20.0f;

    [HideInInspector]
    protected int m_currentRound = 0;
    public int CurrentRound => m_currentRound;

    protected GameState m_state = GameState.Waiting;
    public GameState State => m_state;

    public Material m_hintMat;

    private CardOverlay m_cardOverlay;
    private TownGrid m_grid;
    private ScoreboardUI m_scoreboard;

    private BuildingType m_buildingToPlace;
    private GameObject m_placeHint;
    private Vector2Int m_placePos;
    private int m_placeRot;
    private float m_flPlaceRot;


    private int m_playerMoney;
    //private float m_currentHappiness;
    //private float m_currentPollution;

    public int PlayerMoney => m_playerMoney;
    public float Happiness => m_grid?.TotalHappiness ?? 0.0f;
    public float Pollution => m_grid?.TotalPollution ?? 0.0f;

    private void Awake()
    {
        if (_Instance != null)
        {
            Debug.LogError("Duplicate game manager!!");
            Destroy(gameObject);
            return;
        }

        _Instance = this;

        m_playerMoney = 10000;
    }

    void Start()
    {
        // m_currentHappiness = 35f;
        // m_currentPollution = 0f;

        m_lastRoundTime = Time.time;
        m_cardOverlay = FindFirstObjectByType<CardOverlay>();
        m_grid = FindFirstObjectByType<TownGrid>();
        m_scoreboard = FindFirstObjectByType<ScoreboardUI>();
    }

    private void OnDestroy()
    {
        if (_Instance == this)
            _Instance = null;
    }

    private void Update()
    {
        if (m_state == GameState.Waiting && ((Time.time - m_lastRoundTime) > m_timeBetweenRounds))
        {
            m_state = GameState.SelectingCard;
            m_cardOverlay.ShowCards();
        }

        if (m_state == GameState.PlacingBuilding)
        {
            UpdatePlaceHint();

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
            }
        }
    }

    // Choose cards at random from the deck.

    private HashSet<CardData> m_CardsPreviouslyShown = new();
    private Dictionary<CardData, int> m_TimesCardPlayed = new();
    public bool HasCardBeenPlayed(CardData card) => m_TimesCardPlayed.ContainsKey(card);

    public int GetTimesCardPlayed(CardData card)
    {
        if (m_TimesCardPlayed.TryGetValue(card, out int count))
            return count;

        return 0;
    }

    public void ChooseCardsFromDeck(out CardData card0, out CardData card1)
    {
        card0 = card1 = null;

        var playableCards = CardData.GetAll().Where(card => card.IsPlayable()).ToList();
        if (playableCards.Count < 2)
        {
            Debug.LogError("Not enough cards playable!");
            return;
        }

        // Ideally, don't show the same card twice.
        var idealCards = playableCards.Where(card => !m_CardsPreviouslyShown.Contains(card)).ToList();
        if (idealCards.Count >= 2)
        {
            ChooseTwoCards(idealCards, out card0, out card1);
        }
        else
        {
            // Just choose any two playable cards at random once all cards have been played.
            ChooseTwoCards(playableCards, out card0, out card1);
        }
    }

    private void ChooseTwoCards(List<CardData> cards, out CardData card0, out CardData card1)
    {
        int cardIndex = Random.Range(0, cards.Count);
        card0 = cards[cardIndex];
        m_CardsPreviouslyShown.Add(cards[cardIndex]);
        cards.RemoveAt(cardIndex);

        cardIndex = Random.Range(0, cards.Count);
        card1 = cards[cardIndex];
        m_CardsPreviouslyShown.Add(cards[cardIndex]);
    }


    public void SelectCard(CardData card)
    {
        m_playerMoney -= card.Cost;

        if (!m_TimesCardPlayed.ContainsKey(card))
            m_TimesCardPlayed.Add(card, 1);
        else
            m_TimesCardPlayed[card] = m_TimesCardPlayed[card] + 1;

        if (m_scoreboard != null)
        {
            m_scoreboard.ApplyCardEffects(card);
        }

        if (card.BuildingToPlace != BuildingType.None)
        {
            BeginPlacingBuilding(card.BuildingToPlace);
        }
        else
        {
            IncrementRound();
        }
    }

    void BeginPlacingBuilding(BuildingType building)
    {
        m_state = GameState.PlacingBuilding;
        m_buildingToPlace = building;

        var prefab = m_grid.GetBuildingPrefab(building);
        m_placeHint = Instantiate(prefab, transform);

        foreach (var mesh in m_placeHint.GetComponentsInChildren<MeshRenderer>())
            mesh.materials = new Material[] { m_hintMat };

        UpdatePlaceHint();
    }

    private Vector3 m_placeVel;
    private float m_placeRotVel;
    void UpdatePlaceHint()
    {
        if (Input.mouseScrollDelta.y > 0) m_placeRot++;
        else if (Input.mouseScrollDelta.y < 0) m_placeRot--;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, 0.0f);

        bool posChanged = false;
        if (groundPlane.Raycast(ray, out float t))
        {
            Vector3 hitPos = ray.origin + ray.direction * t;
            hitPos = (hitPos / TownGrid.CellSize);

            var oldPos = m_placePos;
            m_placePos = new Vector2Int((int)(hitPos.x + 0.5f), (int)(hitPos.z + 0.5f));
            if (oldPos != m_placePos)
                posChanged = true;
        }

        Vector3 targetPos = new Vector3(m_placePos.x, 0, m_placePos.y) * TownGrid.CellSize;
        float targetRot = m_placeRot * 90.0f;

        if (posChanged)
        {
            // Snap to the target cell.
            m_placeHint.transform.position = targetPos + Vector3.up * 5.0f;
        }

        const float smoothTime = 0.1f;
        m_placeHint.transform.position = Vector3.SmoothDamp(m_placeHint.transform.position, targetPos, ref m_placeVel, smoothTime, 10.0f, Time.deltaTime);

        // Debug.Log($"{m_placeRot} ({curRot} -> {targetRot})");

        m_flPlaceRot = Mathf.SmoothDamp(m_flPlaceRot, targetRot, ref m_placeRotVel, smoothTime, 200.0f, Time.deltaTime);
        m_placeHint.transform.rotation = Quaternion.Euler(0, m_flPlaceRot, 0);

        m_hintMat.color = m_grid.CanPlaceCellAt(m_buildingToPlace, m_placePos.x, m_placePos.y) ?
            new Color(0.5f, 0.5f, 1.0f, 0.25f) :
            new Color(1.0f, 0.5f, 0.5f, 0.25f);
    }

    private static void GetBuildingProperties(BuildingType type, out SpecialCellProperties props)
    {
        props = new();
        if (type == BuildingType.RecyclingPlant)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Happiness, 5.0f, 1.0f) };
        }
        else if (type == BuildingType.RecyclingPlant2)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Happiness, 10.0f, 2.0f) };
        }
        else if (type == BuildingType.WasteDump)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Pollution, 10.0f, 0.75f), new Impulse(ImpulseType.Happiness, 30.0f, 0.2f) };
        }
        else if (type == BuildingType.WasteDump2)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Pollution, 15.0f, 1.25f), new Impulse(ImpulseType.Happiness, 40.0f, 0.3f) };
        }
        else if (type == BuildingType.Incinerator)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Pollution, 20.0f, 0.2f) };
        }
        else if (type == BuildingType.CompostPlant)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Pollution, 20.0f, -0.2f) };
        }
        else if (type == BuildingType.WasteSorter)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Pollution, 5.0f, -0.5f) };
        }
        else if (type == BuildingType.LakeFilter)
        {
            props.Impulses = new[] { new Impulse(ImpulseType.Pollution, 3.0f, -0.75f) };
        }
    }

    public void EstimateBuildingImpact(BuildingType building, out float happiness, out float pollution)
    {
        happiness = pollution = 0;

        float gameTimeRemaining = m_timeBetweenRounds * (m_numRounds - m_currentRound) + 3.0f;
        int totalGameFixedUpdates = (int)(gameTimeRemaining / Time.fixedDeltaTime);
        int totalUpdateTicks = totalGameFixedUpdates / TownGrid.TicksPerUpdate;

        GetBuildingProperties(building, out var props);
        foreach (var impulse in props.Impulses)
        {
            // Use cone volume formula to estimate the pollution impact over time.
            // This estimates total pollution impact for one tick.
            float volume = Mathf.PI * (impulse.Radius * impulse.Radius) * (impulse.Amount / 3.0f);

            // And for the remainder of the game (total number of town ticks left), averaged over cell count...
            float impact = (totalUpdateTicks * volume) / (TownGrid.TownSize * TownGrid.TownSize);

            if (impulse.Type == ImpulseType.Happiness) happiness += impact;
            else if (impulse.Type == ImpulseType.Pollution) pollution += impact;
        }
    }


    void PlaceBuilding()
    {
        GetBuildingProperties(m_buildingToPlace, out SpecialCellProperties props);

        if (m_grid.PlaceCell(m_buildingToPlace, m_placePos.x, m_placePos.y, m_placeRot, props))
        {
            // TODO: sfx
            Debug.Log("Building placed!");
            Destroy(m_placeHint);
            m_placeHint = null;

            IncrementRound();
        }
        else
        {
            // TODO: sfx
        }
    }

    public int GetBuildingCountInTown( BuildingType type )
    {
        return m_grid.GetBuildingCount(type);
    }

    // Move the game to the next round, displaying the new set of cards.
    public void IncrementRound()
    {
        m_currentRound++;
        Debug.Log($"Increment round {m_currentRound}/{m_numRounds}");

        if (m_currentRound >= m_numRounds)
        {
            // No rounds left, end the game.
            Debug.Log("Game over!");
            m_state = GameState.GameOver;
            m_scoreboard.EndGame();
            return;
        }

        m_state = GameState.Waiting;
        m_lastRoundTime = Time.time;
    }

    public void UpdateUI()
    {
        if (m_state == GameState.GameOver)
            return;

        m_scoreboard?.UpdateUI();
    }
}
