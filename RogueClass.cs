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
            new Test1(),
            new Test2(),
            new Test3(),
            new Test4(),
            new Test1(),
            new Test2(),
            new Test3(),
            new Test4(),
        };
    }
}