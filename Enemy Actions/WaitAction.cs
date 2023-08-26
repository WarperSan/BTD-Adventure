using BTDAdventure.Abstract;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class WaitAction : EnemyAction
{
    public WaitAction() : base(Wait, "IntentWait", WaitIcon) { }

    public override string? GetText(EnemyEntity source) => null;

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
#if DEBUG
        Log("Wait action activate ...");
#endif
    }
}

