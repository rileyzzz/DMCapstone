using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    public TextMeshProUGUI m_NameText;
    public TextMeshProUGUI m_DescriptionText;

    public TextMeshProUGUI m_MoneyImpactText;
    public TextMeshProUGUI m_PollutionImpactText;
    public TextMeshProUGUI m_HappinessImpactText;
    private CardData m_cardData;

    public CardData CardData => m_cardData;

    private void Start()
    {
    }

    private static void SetValueText(TextMeshProUGUI text, int value, string prefix, in Color goodColor, in Color badColor)
    {
        if (value > 0)
        {
            text.color = goodColor;
            text.text = $"+ {prefix}{value}";
        }
        else if (value < 0)
        {
            text.color = badColor;
            text.text = $"- {prefix}{(-value)}";
        }
        else
        {
            text.color = Color.gray;
            text.text = "0";
        }
    }
    public void SetData(CardData data)
    {
        m_cardData = data;

        m_NameText.text = data.Name;
        m_DescriptionText.text = data.Description;

        SetValueText(m_MoneyImpactText, -data.Cost, "$", Color.green, Color.red);
        SetValueText(m_PollutionImpactText, data.EstimatedPollution, "", Color.gray, Color.green);
        SetValueText(m_HappinessImpactText, data.EstimatedHappiness, "", Color.yellow, Color.red);

        //m_PollutionImpactText.text = data.EstimatedPollution.ToString();
        //m_HappinessImpactText.text = data.EstimatedHappiness.ToString();
        Debug.Log($"set card to {data.Name}");

        // setup the text fields from card data
    }
}