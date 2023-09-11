using BTDAdventure.Entities;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class WaitAction : EnemyAction
{
    public WaitAction() : base("IntentWait", WaitIcon) { }

    public override string? GetText(EnemyEntity source) => null;

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        SoundManager.PlaySound("tic_tac", SoundManager.GeneralGroup);

#if DEBUG
        Log("Wait action activate ...");
#endif
    }
}

