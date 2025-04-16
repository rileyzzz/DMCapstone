using TMPro;
using UnityEngine;

public class EndSceneUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI pollutionText;

    void Start()
    {
        moneyText.text = $"Final Money: ${GameResults.Money}";
        happinessText.text = $"Final Happiness: {GameResults.Happiness}%";
        pollutionText.text = $"Polluted Tiles: {GameResults.Pollution}";
    }
}
