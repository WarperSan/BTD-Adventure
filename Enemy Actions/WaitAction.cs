using BTDAdventure.Cards.EnemyCards;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class WaitAction : EnemyAction
{
    public WaitAction() : base(Wait, 0, "IntentWait", UIManager.WaitIcon) { }

    public override string? GetText(EnemyCard source) => null;

    public override void OnAction(EnemyCard source)
    {
#if DEBUG
        Log("Wait action activate ...");
#endif
    }
}

