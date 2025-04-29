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
    RecyclingPlant2,
    WasteDump,
    WasteDump2,

    Incinerator,
    CompostPlant,
    WasteSorter,

    LakeFilter,

    Count
}

public enum DependencyType
{
    CardPlayed,
    HasBuildingOfType,
    PlayOnlyNTimes
}

[System.Serializable]
public struct CardDependency
{
    public DependencyType Type;

    public CardData CardRequired;
    public BuildingType BuildingRequired;
    public int MaxTimesPlayed;
}


[CreateAssetMenu(fileName = "CardData", menuName = "Wasting Co/CardData")]
public class CardData : ScriptableObject
{
    public CardArchetype Archetype;

    public CardDependency[] Dependencies;

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
        return Resources.LoadAll<CardData>("FinalCards/");
    }

    // Can this card be played yet?
    // Checks unlock status/building placement/etc.
    public bool IsPlayable()
    {
        foreach (var dep in Dependencies)
        {
            switch (dep.Type)
            {
                case DependencyType.CardPlayed:
                    if (!GameManager.Instance.HasCardBeenPlayed(dep.CardRequired))
                        return false;
                    break;
                case DependencyType.HasBuildingOfType:
                    if (GameManager.Instance.GetBuildingCountInTown(dep.BuildingRequired) == 0)
                        return false;
                    break;
                case DependencyType.PlayOnlyNTimes:
                    if (GameManager.Instance.GetTimesCardPlayed(this) >= dep.MaxTimesPlayed)
                        return false;
                    break;
            }
        }

        return true;
    }
}
