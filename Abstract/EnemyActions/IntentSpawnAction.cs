using BTDAdventure.Entities;
using BTDAdventure.Managers;

namespace BTDAdventure.Abstract.EnemyActions;

public abstract class IntentSpawnAction : IntentAttackAction
{
    protected IntentSpawnAction() : base(UIManager.SpawnIcon) { }

    public override string? GetText(EnemyEntity source) => null;
}