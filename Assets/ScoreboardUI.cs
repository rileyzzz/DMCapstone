using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreboardUI : MonoBehaviour
{
    public TextMeshProUGUI turnsText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI pollutionText;

    // private int money = 10000;

    private bool isGameEnding = false;

    void Start()
    {
        UpdateUI();
    }

    public void ApplyCardEffects(CardData card)
    {
        if (isGameEnding) return;

        Debug.Log($"Applying Card Effects: {card.Name}");

        
        // happiness = Mathf.Clamp(happiness + card.EstimatedHappiness, 0, 100);
        // pollutedTiles += card.EstimatedPollution;

        UpdateUI();
    }

    public void UpdateUI()
    {
        var gameManager = GameManager.Instance;

        // Debug.Log($"Updated Scoreboard - Turns: {turns}, Money: {money}, Happiness: {happiness}, Pollution: {pollutedTiles}");

        turnsText.text = $"{Mathf.Min(gameManager.CurrentRound + 1, gameManager.m_numRounds)}/10";
        moneyText.text = $"${gameManager.PlayerMoney}";
        if (gameManager.PlayerMoney < 0) moneyText.color = Color.red;

        happinessText.text = $"{gameManager.Happiness:f2}%";
        pollutionText.text = $"{gameManager.Pollution:f2}";

        turnsText.ForceMeshUpdate();
        moneyText.ForceMeshUpdate();
        happinessText.ForceMeshUpdate();
        pollutionText.ForceMeshUpdate();
    }

    public void EndGame()
    {
        if (isGameEnding)
            return;

        isGameEnding = true;
        StartCoroutine(EndGameAfterDelay(3f)); // Wait 3 seconds
    }


    private System.Collections.IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameResults.Money = GameManager.Instance.PlayerMoney;
        GameResults.Happiness = GameManager.Instance.Happiness;
        GameResults.Pollution = (int)GameManager.Instance.Pollution;
        // TODO: Store or pass final stats if needed before scene switch
        SceneManager.LoadScene("Scoring"); // Replace with your actual end scene name
    }
}
