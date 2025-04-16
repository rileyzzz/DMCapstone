using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI pollutionText;
    public TextMeshProUGUI categoryText;

    void Start()
    {
        moneyText.text = $"${GameResults.Money}";
        happinessText.text = $"{GameResults.Happiness}";
        pollutionText.text = $"{GameResults.Pollution}";
        string resultCategory = DetermineResultCategory(GameResults.Happiness, GameResults.Money, GameResults.Pollution);
        categoryText.text = $"{resultCategory}";
    }

    private string DetermineResultCategory(float happiness, int money, int pollution)
    {
        if (happiness > 85 && pollution < 20)
            return "Green Giant";

        if (money <= 500 && pollution < 50)
            return "EcoColapser";

        if (money >= 8500 && pollution > 60 && happiness < 60)
            return "Money Monger";

        if (happiness <= 50 && money > 5000)
            return "Cheapskate";


        return "Sustainable Futurist";
    }

}
