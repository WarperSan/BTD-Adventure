using BTDAdventure.Abstract_Classes;
using BTDAdventure.Abstract_Classes.EnemyActions;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class DoubleAttackAction : IntentAttackAction
{
    public DoubleAttackAction() : base(DoubleAttack, 0, DoubleDamageIcon) { }

    public override string? GetText(EnemyEntity source) => (source.GetAttack() * 2).ToString();
    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AttackTarget(source.GetAttack() * 2, player);
    }
}