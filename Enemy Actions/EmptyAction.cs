using BTDAdventure.Abstract;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class EmptyAction : EnemyAction
{
    public EmptyAction() : base("", null, null) { }

    public override string? GetText(EnemyEntity source) => null;
}
