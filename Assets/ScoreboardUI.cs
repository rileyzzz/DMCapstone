using TMPro;
using UnityEngine;

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

    void Start()
    {
        UpdateUI();
    }

    public void ApplyCardEffects(CardData card)
    {
        Debug.Log($"Applying Card Effects: {card.Name}");

        turns += 1;
        money -= card.Cost;
        happiness = Mathf.Clamp(happiness + card.EstimatedHappiness, 0, 100); // Keep within 0-100
        pollutedTiles += card.EstimatedPollution;

        UpdateUI();
    }

    private void UpdateUI()
    {
        Debug.Log($"Updated Scoreboard - Turns: {turns}, Money: {money}, Happiness: {happiness}, Pollution: {pollutedTiles}");

        turnsText.text = $"{turns}/10";
        moneyText.text = $"${money}";
        happinessText.text = $"{happiness}%";
        pollutionText.text = $"{pollutedTiles}";

        // Force UI refresh
        turnsText.ForceMeshUpdate();
        moneyText.ForceMeshUpdate();
        happinessText.ForceMeshUpdate();
        pollutionText.ForceMeshUpdate();
    }
}
