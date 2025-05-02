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

    private static void SetValueText(TextMeshProUGUI text, int value, string prefix, in Color goodColor, in Color badColor, bool isMoney = false)
    {
        if (value > 0)
        {
            string valueStr = isMoney ? $"{value:n0}k" : $"{value}";

            text.color = goodColor;
            text.text = $"+ {prefix}{valueStr}";
        }
        else if (value < 0)
        {
            value *= -1;

            string valueStr = isMoney ? $"{value:n0}k" : $"{value}";

            text.color = badColor;
            text.text = $"- {prefix}{valueStr}";
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

        SetValueText(m_MoneyImpactText, -data.Cost, "$", Color.green, Color.red, true);

        // Calculate pollution/happiness impact.
        float estPollution = data.EstimatedPollution;
        float estHappiness = data.EstimatedHappiness;
        if (data.Action == CardAction.PlaceBuilding)
        {
            GameManager.Instance.EstimateBuildingImpact(data.BuildingToPlace, out estHappiness, out estPollution);
        }

        SetValueText(m_PollutionImpactText, (int)estPollution, "", Color.gray, Color.green);
        SetValueText(m_HappinessImpactText, (int)estHappiness, "", Color.yellow, Color.red);

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