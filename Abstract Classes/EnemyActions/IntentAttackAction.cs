namespace BTDAdventure.Abstract_Classes.EnemyActions;

/// <summary>
/// Action that uses the IntentAttack animation
/// </summary>
public abstract class IntentAttackAction : EnemyAction
{
    protected IntentAttackAction(string tag, uint order, string? icon = null) : base(tag, order, "IntentAttack", icon) { }
}