using BTDAdventure.Abstract_Classes;
using BTDAdventure.Abstract_Classes.EnemyActions;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class AttackAction : IntentAttackAction
{
    public AttackAction() : base(Attack, 0, DamageIcon) { }

    public override string? GetText(EnemyEntity source) => source.GetAttack().ToString();
    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AttackTarget(player);
    }
}