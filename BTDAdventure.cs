using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Bloons;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;
using BTDAdventure;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Utils;
using MelonLoader;
using System;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(BTDAdventure.BTDAdventure), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BTDAdventure;

// Order of actions
// Fill hand
// Select card
// Select enemy
// Play card
// Place card in discard or exile
// Refill if needed
// Remove mana
// Repeat until end or no more mana
// Lock all cards
// Enemies play
// Remove hand
// Repeat until end of fight or death


// Things
// Hand
// Draw pile
// Discard pile
// Exile pile
// Player HP
// Max HP
// Enemy HP
// Mana
// Enemy intent
// Buffs
// Status Effects
// Debuffs

public class BTDAdventure : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<BTDAdventure>("BTDAdventure loaded!");
    }
}

public abstract class RogueClass
{
    public abstract List<Card> InitialCards();
}
public class WarriorClass : RogueClass
{
    public override List<Card> InitialCards()
    {
        List<Card> hand = new();

        Card.AddCard<FighterCard>(hand, 4);
        Card.AddCard<GuardCard>(hand, 3);
        Card.AddCard<MageCard>(hand, 1);

        return hand;
    }
}

public abstract class Card : ModContent
{
    /// <summary>
    /// Name displayed on the card render
    /// </summary>
    public abstract string DisplayName { get; }
    public virtual string? Portrait { get; }

    public virtual string? Type { get; private set; } // Combat / Magic
    public virtual string Race { get; private set; } = "Neutral";
    public virtual CardRarity Rarity { get; private set; } = CardRarity.Common;

    public enum CardRarity
    {
        Common, Uncommon, Rare, Epic, Legenday, Other
    }

    public virtual bool GoesInExile { get; } = false;

    public override void Register()
    {
        //this.mod.LoggerInstance.Msg(this);
    }

    public override string ToString()
        => $"(Name: \'{DisplayName}\', Type: \'{Type}\', Race: \'{Race}\', Rarity: \'{Rarity}\')";

    public static void AddCard<T>(List<Card> cards, uint count = 1) where T : Card
    {
        if (typeof(T) == typeof(Card))
        {
            ModHelper.Error<BTDAdventure>($"Invalid type: {typeof(T)}");
            return;
        }

        try
        {
            for (uint i = 0; i < count; i++)
            {
                cards.Add(Activator.CreateInstance<T>());
            }
        }
        catch (Exception e)
        {
            ModHelper.Error<BTDAdventure>(e.Message);
            throw;
        }
    }
}

public class HighWitch : Card
{
    public override string DisplayName => "A";
    public override CardRarity Rarity => CardRarity.Rare;
}

public class FighterCard : Card
{
    public override string DisplayName => "Fighter";
}

public class GuardCard : Card
{
    public override string DisplayName => "Guard";
}

public class MageCard : Card
{
    public override string DisplayName => "Mage";
}