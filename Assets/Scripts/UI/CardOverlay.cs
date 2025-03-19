using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CardOverlay : MonoBehaviour
{
    public RectTransform m_Overlay;
    public Animator m_CardAnim;
    public Card[] m_Cards;

    private HashSet<CardData> m_CardsPreviouslyShown = new();

    private ScoreboardUI scoreboard;

    private void Start()
    {
        scoreboard = FindObjectOfType<ScoreboardUI>();

        if (scoreboard == null)
        {
            Debug.LogError("Could not find ScoreboardUI in the scene!");
        }



        // cardData = CardData.GetAll().FirstOrDefault();
        SetRandomCards();
    }

    private void SetRandomCards()
    {
        var allCards = CardData.GetAll().Where(card => !m_CardsPreviouslyShown.Contains(card)).ToList();
        if (allCards.Count < 2)
        {
            Debug.LogError("Not enough cards!");
            return;
        }

        int cardIndex = Random.Range(0, allCards.Count);
        m_Cards[0].SetData(allCards[cardIndex]);
        m_CardsPreviouslyShown.Add(allCards[cardIndex]);
        allCards.RemoveAt(cardIndex);

        cardIndex = Random.Range(0, allCards.Count);
        m_Cards[1].SetData(allCards[cardIndex]);
        m_CardsPreviouslyShown.Add(allCards[cardIndex]);
    }

    public void CardClicked(Card card)
    {
        Debug.Log($"Card {card} clicked!");

        if (scoreboard == null)
        {
            Debug.LogError("ScoreboardUI not found!");
            return;
        }
        if (card.CardData == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        if (scoreboard != null && card.CardData != null)
        {
            scoreboard.ApplyCardEffects(card.CardData);
        }

        // Play the hide animation
        m_CardAnim.SetBool("visible", false);
    }
}
