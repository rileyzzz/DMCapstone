using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreboardUI : MonoBehaviour
{
    public TextMeshProUGUI turnsText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI pollutionText;

    private int turns = 1;
    private int money = 10000;
    private float happiness = 35f;
    private int pollutedTiles = 100;

    private bool isGameEnding = false;

    void Start()
    {
        UpdateUI();
    }

    public void ApplyCardEffects(CardData card)
    {
        if (isGameEnding) return;

        Debug.Log($"Applying Card Effects: {card.Name}");

        turns += 1;
        money -= card.Cost;
        happiness = Mathf.Clamp(happiness + card.EstimatedHappiness, 0, 100);
        pollutedTiles += card.EstimatedPollution;

        UpdateUI();

        if (turns > 10 && !isGameEnding)
        {
            isGameEnding = true;
            StartCoroutine(EndGameAfterDelay(3f)); // Wait 3 seconds
        }
    }

    private void UpdateUI()
    {
        Debug.Log($"Updated Scoreboard - Turns: {turns}, Money: {money}, Happiness: {happiness}, Pollution: {pollutedTiles}");

        turnsText.text = $"{Mathf.Min(turns, 10)}/10";
        moneyText.text = $"${money}";
        happinessText.text = $"{happiness}%";
        pollutionText.text = $"{pollutedTiles}";

        turnsText.ForceMeshUpdate();
        moneyText.ForceMeshUpdate();
        happinessText.ForceMeshUpdate();
        pollutionText.ForceMeshUpdate();
    }

    private System.Collections.IEnumerator EndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameResults.Money = money;
        GameResults.Happiness = happiness;
        GameResults.Pollution = pollutedTiles;
        // TODO: Store or pass final stats if needed before scene switch
        SceneManager.LoadScene("Scoring"); // Replace with your actual end scene name
    }
}
