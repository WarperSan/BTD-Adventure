using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;
using System.Collections.Generic;

namespace BTDAdventure;

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

    public override void Register()
    {
#if DEBUG
        Log(Name + " registered");
#endif
    }
}

public class WarriorClass : RogueClass
{
    public override List<HeroCard> InitialCards()
    {
        return new List<HeroCard>()
        {
            new Dart003(),
            new Dart003(),
            new Dart003(),
            new Dart003(),
            new Dart003(),
            new Dart003(),
            new Dart003(),
            new Dart003(),
            //new Dart000(),
            //new Dart000(),
            //new Dart000(),
            //new Dart000(),
            //new Boomerang000(),
            //new Ice000(),
            //new Ice000(),
            //new Ice000(),
            /*
            new DartMonkey000(),
            new DartMonkey000(),
            new DartMonkey000(),
            new DartMonkey000(),

            new MonkeyVillage000(),
            new MonkeyVillage000(),

            new SuperMonkey000(),

            new MonkeyAce000(),*/
        };
    }
}