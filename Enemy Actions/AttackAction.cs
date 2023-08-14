using BTDAdventure.Cards.EnemyCards;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class AttackAction : EnemyAction
{
    public AttackAction() : base(Attack, 0, "IntentAttack", UIManager.DamageIcon) { }

    public override string? GetText(EnemyCard source) => source.GetAttack().ToString();
    public override void OnAction(EnemyCard source)
    {
        GameManager.Instance.DamagePlayer(new Damage(source.GetAttack()));
    }
}