namespace BTDAdventure.Abstract.EnemyActions;

/// <summary>
/// Action that uses the IntentAttack animation
/// </summary>
public abstract class IntentAttackAction : EnemyAction
{
    protected IntentAttackAction(string tag, string? icon = null) : base(tag, "IntentAttack", icon) { }
}