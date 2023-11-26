using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;
using System.Collections.Generic;

namespace BTDAdventure.Abstract;

public abstract class RogueClass : ModContent
{
    public abstract List<HeroCard> InitialCards();

    /// <summary>
    /// Called before a card is played
    /// </summary>
    /// <param name="player"></param>
    /// <param name="card">Card playerd</param>
    public virtual void OnPreCardPlay(PlayerEntity player, HeroCard card)
    { }

    /// <summary>
    /// Called after a card is played
    /// </summary>
    /// <param name="player"></param>
    /// <param name="card">Card playerd</param>
    /// <param name="target"></param>
    public virtual void OnCardPlayed(PlayerEntity player, HeroCard card, EnemyCard target)
    { }

    public virtual int GetDefaultHealth() => 50;
    public virtual int GetDefaultMaxHealth() => GetDefaultHealth();

    public override void Register()
    {
#if DEBUG
        Log(Name + " registered");
#endif
    }
}