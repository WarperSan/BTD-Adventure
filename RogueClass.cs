using BTDAdventure.Cards.HeroCard;
using System.Collections.Generic;

namespace BTDAdventure;

public abstract class RogueClass
{
    public abstract List<HeroCard> InitialCards();

    //public virtual void OnCardPlayed(PlayerEntity origin, Card card) { }
}

public class WarriorClass : RogueClass
{
    public override List<HeroCard> InitialCards()
    {
        return new List<HeroCard>()
        {
            new MonkeyAce000(),
            new MonkeyAce000(),
            new MonkeyAce000(),
            new MonkeyAce000(),
            new MonkeyAce000(),
            new MonkeyAce000(),
            new MonkeyAce000(),
            new MonkeyAce000(),
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