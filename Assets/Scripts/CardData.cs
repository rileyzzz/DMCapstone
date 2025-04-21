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
    UpgradeBuilding,
    ContributeToDisaster
}

public enum BuildingType
{
    None,
    RecyclingPlant,
    WasteDump,
}

public enum UpgradeType
{
}


[CreateAssetMenu(fileName = "CardData", menuName = "Wasting Co/CardData")]
public class CardData : ScriptableObject
{
    public CardArchetype Archetype;

    public CardData[] Dependencies;

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
    public BuildingType BuildingToUnlock;
    public BuildingType BuildingToPlace;

    public static IReadOnlyList<CardData> GetAll()
    {
        //return Resources.LoadAll<CardData>("Cards/");
        return Resources.LoadAll<CardData>("TestCards/");
    }

    // Can this card be played yet?
    // Checks unlock status/building placement/etc.
    public bool IsPlayable()
    {
        return true;
    }
}
