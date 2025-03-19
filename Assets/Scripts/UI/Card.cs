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

    public void SetData(CardData data)
    {
        m_cardData = data;

        m_NameText.text = data.Name;
        m_DescriptionText.text = data.Description;

        m_MoneyImpactText.text = data.Cost.ToString();
        m_PollutionImpactText.text = data.EstimatedPollution.ToString();
        m_HappinessImpactText.text = data.EstimatedHappiness.ToString();
        Debug.Log($"set card to {data.Name}");

        // setup the text fields from card data
    }
}