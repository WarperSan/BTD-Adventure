using BTDAdventure.Managers;

namespace BTDAdventure.Abstract.EnemyActions;

/// <summary>
/// Action that uses the IntentAttack animation
/// </summary>
public abstract class IntentAttackAction : EnemyAction
{
    public override string? Icon => UIManager.ICON_DAMAGE;
    public override sealed string? AnimationName => ANIMATION_ATTACK;
}