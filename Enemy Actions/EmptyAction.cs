using BTDAdventure.Cards.EnemyCards;

namespace BTDAdventure.Enemy_Actions;

internal class EmptyAction : EnemyAction
{
    public EmptyAction() : base("", 0, null, null) { }

    public override string? GetText(EnemyCard source) => null;
}
