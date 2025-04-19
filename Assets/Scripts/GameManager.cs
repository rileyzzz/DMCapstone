using UnityEngine;

public enum GameState
{
    Waiting,
    SelectingCard,
    PlacingBuilding
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

    // Start in a "pre-game" round.
    [HideInInspector]
    protected int m_currentRound = -1;
    public int CurrentRound => m_currentRound;

    protected GameState m_state = GameState.Waiting;
    public GameState State => m_state;

    private CardOverlay m_cardOverlay;
    private TownGrid m_grid;

    private GameObject m_buildingToPlace;

    void Start()
    {
        if (_Instance != null)
        {
            Debug.LogError("Duplicate game manager!!");
            Destroy(gameObject);
            return;
        }

        _Instance = this;

        m_lastRoundTime = Time.time;
        m_cardOverlay = FindFirstObjectByType<CardOverlay>();
        m_grid = FindFirstObjectByType<TownGrid>();
    }

    private void OnDestroy()
    {
        if (_Instance == this)
            _Instance = null;
    }

    private void Update()
    {
        // Do nothing if game has ended.
        if (m_currentRound >= m_numRounds)
            return;

        if (m_state == GameState.Waiting && ((Time.time - m_lastRoundTime) > m_timeBetweenRounds))
        {
            m_state = GameState.SelectingCard;
            m_cardOverlay.ShowCards();
        }

        if (m_state == GameState.PlacingBuilding)
        {

        }
    }

    public void SelectCard(CardData card)
    {
        if (card.BuildingToPlace)
        {
            BeginPlacingBuilding(card.BuildingToPlace);
        }
        else
        {
            IncrementRound();
        }
    }

    void BeginPlacingBuilding(GameObject building)
    {
        m_state = GameState.PlacingBuilding;
        m_buildingToPlace = building;
    }

    void PlaceBuilding()
    {

        IncrementRound();
    }

    // Move the game to the next round, displaying the new set of cards.
    public void IncrementRound()
    {
        m_state = GameState.Waiting;

        if (m_currentRound >= m_numRounds)
        {
            // No rounds left, end the game.
            return;
        }

        m_lastRoundTime = Time.time;
        m_currentRound++;
    }
}
