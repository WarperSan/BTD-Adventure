using BTDAdventure.Cards.EnemyCards;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class DoubleAttackAction : EnemyAction
{
    public DoubleAttackAction() : base(DoubleAttack, 0, "IntentAttack", UIManager.DoubleDamageIcon) { }

    public override string? GetText(EnemyCard source) => (source.GetAttack() << 1).ToString();
    public override void OnAction(EnemyCard source)
    {
        GameManager.Instance.DamagePlayer(new Damage(source.GetAttack() << 1));
    }
}