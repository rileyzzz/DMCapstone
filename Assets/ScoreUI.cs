using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI happinessText;
    public TextMeshProUGUI pollutionText;
    public TextMeshProUGUI categoryText;
    public TextMeshProUGUI funFactText; // <-- Add this in the Inspector

    void Start()
    {
        moneyText.text = $"${GameResults.Money}";
        happinessText.text = $"{GameResults.Happiness}";
        pollutionText.text = $"{GameResults.Pollution}";

        string resultCategory = DetermineResultCategory(GameResults.Happiness, GameResults.Money, GameResults.Pollution);
        categoryText.text = resultCategory;
        funFactText.text = GetFunFact(resultCategory); // <-- Set fun fact text
    }

    private string DetermineResultCategory(float happiness, int money, int pollution)
    {
        if (happiness > 40 && pollution < 20)
            return "Green Giant";

        if (money <= -1000 && pollution < 50)
            return "EcoColapser";

        if (money >= 15000 && pollution > 50 && happiness < 40)
            return "Money Monger";

        if (happiness <= 50 && money > 10000)
            return "Cheapskate";

        return "Sustainable Futurist";
    }

    private string GetFunFact(string category)
    {
        switch (category)
        {
            case "Money Monger":
                return "Cared more about monetary gain rather than environmental protection and happiness. Focus more on the economy next time to keep the balance between economy and money. High profit, Low Environment.";

            case "Cheapskate":
                return "Cared way too much about money and saving as much as you could, ended up ruining the company. Extreme cut-costing. Next time, find the best way to make money and sustain the environment and keep the happiness of the people high to find the perfect balance.";

            case "Green Giant":
                return "Little to no monetary gain, but great environmental protection and consumption. Focused mainly on the environment and did not care about money.";

            case "EcoColapser":
                return "Focused way too much on the ecology and protection of the environment. You neglected money in the account and now the town is bankrupt—but at least the environment is good.";

            case "Sustainable Futurist":
                return "Had a good balance between gaining money and business production, while keeping the environment protected and healthy. This is the best scenario as you have successfully transformed the town and the people are happy.";

            default:
                return "";
        }
    }
}