using BTDAdventure.Entities;
using BTDAdventure.Managers;

namespace BTDAdventure.Abstract.EnemyActions;

public abstract class IntentEffectAction : IntentAttackAction
{
    protected IntentEffectAction() : base(UIManager.CurseIcon) { }
}