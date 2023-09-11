using BTDAdventure.Abstract.EnemyActions;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class AttackAction : IntentAttackAction
{
    public AttackAction() : base(DamageIcon) { }

    public override string? GetText(EnemyEntity source) => source.GetAttack().ToString();
    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AttackTarget(player);
    }
}