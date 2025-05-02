using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CardOverlay : MonoBehaviour
{
    public Animator m_CardAnim;
    public Card[] m_Cards;

    public AudioSource m_audioSource;

    public AudioClip m_revealSFX;
    public AudioClip m_playCardSFX;

    //private ScoreboardUI scoreboard;

    private bool m_bCardsVisible = false;
    public bool CardsVisible => m_bCardsVisible;

    private void Start()
    {
    }

    private void SetRandomCards()
    {
        GameManager.Instance.ChooseCardsFromDeck(out CardData card0, out CardData card1);
        m_Cards[0].SetData(card0);
        m_Cards[1].SetData(card1);
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

        m_audioSource.PlayOneShot(m_revealSFX);
    }

    public void CardClicked(Card card)
    {
        if (!m_bCardsVisible)
            return;

        Debug.Log($"Card {card} clicked!");

        //if (scoreboard == null)
        //{
        //    Debug.LogError("ScoreboardUI not found!");
        //    return;
        //}
        if (card.CardData == null)
        {
            Debug.LogError("CardData is null!");
            return;
        }

        // Play the hide animation
        m_CardAnim.SetBool("visible", false);
        m_bCardsVisible = false;

        GameManager.Instance.SelectCard(card.CardData);

        m_audioSource.PlayOneShot(m_playCardSFX);
    }
}
