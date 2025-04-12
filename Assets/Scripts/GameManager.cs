using UnityEngine;

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


    private CardOverlay m_cardOverlay;

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

        // Game is paused while cards are on screen.
        if (m_cardOverlay.CardsVisible)
            return;

        if ((Time.time - m_lastRoundTime) > m_timeBetweenRounds)
        {
            m_cardOverlay.ShowCards();
        }
    }

    // Move the game to the next round, displaying the new set of cards.
    public void IncrementRound()
    {
        if (m_currentRound >= m_numRounds)
        {
            // No rounds left, end the game.
            return;
        }

        m_lastRoundTime = Time.time;
        m_currentRound++;
    }
}
