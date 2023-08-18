using BTDAdventure.Abstract_Classes;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class EmptyAction : EnemyAction
{
    public EmptyAction() : base("", 0, null, null) { }

    public override string? GetText(EnemyEntity source) => null;
}
