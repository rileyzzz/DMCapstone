using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    public TextMeshProUGUI m_NameText;
    public TextMeshProUGUI m_DescriptionText;

    public TextMeshProUGUI m_MoneyImpactText;
    public TextMeshProUGUI m_PollutionImpactText;
    public TextMeshProUGUI m_HappinessImpactText;

    public GameObject m_HintTextBox;
    public TextMeshProUGUI m_HintText;
    private CardData m_cardData;

    public CardData CardData => m_cardData;

    private void Start()
    {
        m_HintTextBox.SetActive(false);
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
        m_HintTextBox.SetActive(false);

        m_cardData = data;

        m_NameText.text = data.Name;
        m_DescriptionText.text = data.Description;
        m_HintText.text = data.HintText;

        SetValueText(m_MoneyImpactText, -data.Cost, "$", Color.green, Color.red);
        SetValueText(m_PollutionImpactText, data.EstimatedPollution, "", Color.gray, Color.green);
        SetValueText(m_HappinessImpactText, data.EstimatedHappiness, "", Color.yellow, Color.red);

        //m_PollutionImpactText.text = data.EstimatedPollution.ToString();
        //m_HappinessImpactText.text = data.EstimatedHappiness.ToString();
        Debug.Log($"set card to {data.Name}");

        // setup the text fields from card data
    }

    public void ToggleHint()
    {
        m_HintTextBox.SetActive(!m_HintTextBox.activeSelf);
    }
}