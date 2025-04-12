using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CardOverlay : MonoBehaviour
{
    public Animator m_CardAnim;
    public Card[] m_Cards;

    private HashSet<CardData> m_CardsPreviouslyShown = new();

    private ScoreboardUI scoreboard;

    private bool m_bCardsVisible = false;
    public bool CardsVisible => m_bCardsVisible;

    private void Start()
    {
        scoreboard = FindObjectOfType<ScoreboardUI>();

        if (scoreboard == null)
        {
            Debug.LogError("Could not find ScoreboardUI in the scene!");
        }
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


    public void ShowCards()
    {
        Debug.Log("Showing cards.");

        // Don't show cards if a set is already visible!
        if (m_bCardsVisible)
            return;

        SetRandomCards();

        m_CardAnim.SetBool("visible", true);
        m_bCardsVisible = true;
    }

    public void CardClicked(Card card)
    {
        if (!m_bCardsVisible)
            return;

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
        m_bCardsVisible = false;

        GameManager.Instance.IncrementRound();
    }
}
