using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum CardArchetype
{
    EcoFriendly,
    BusinessSavvy,
    Monetary,
    Welfare,
}

public enum CardAction
{
    None,
    UnlockBuilding,
    PlaceBuilding,
    TriggerDisaster
}

[CreateAssetMenu(fileName = "CardData", menuName = "Wasting Co/CardData")]
public class CardData : ScriptableObject
{
    public CardArchetype Archetype;

    public Image Icon;
    public string Name;
    public string Description;
    public string HintText;

    // Estimated impact of this card.
    // The actual impact will be determined by the action performed by the card.
    public int EstimatedHappiness = 0;
    public int EstimatedPollution = 0;

    public int Cost = 1000;

    public CardAction Action;
    public GameObject BuildingToUnlock;
    public GameObject BuildingToPlace;

    public static CardData[] GetAll()
    {
        return Resources.LoadAll<CardData>("Cards/");
    }
}
