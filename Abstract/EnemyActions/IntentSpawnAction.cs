using BTDAdventure.Entities;
using BTDAdventure.Managers;

namespace BTDAdventure.Abstract.EnemyActions;

public abstract class IntentSpawnAction : IntentAttackAction
{
    public override string? Icon => UIManager.ICON_SPAWN;
    public override string? GetText(EnemyEntity source) => null;
}