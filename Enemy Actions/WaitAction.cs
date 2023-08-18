using BTDAdventure.Abstract_Classes;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class WaitAction : EnemyAction
{
    public WaitAction() : base(Wait, 0, "IntentWait", WaitIcon) { }

    public override string? GetText(EnemyEntity source) => null;

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
#if DEBUG
        Log("Wait action activate ...");
#endif
    }
}

