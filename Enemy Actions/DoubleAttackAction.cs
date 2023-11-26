using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class DoubleAttackAction : AttackAction, IAttackEffect
{
    public override string? Icon => UIManager.ICON_DOUBLE_DAMAGE;

    public void ModifyDamage(Entity entity, ref Damage damage)
    {
        damage.Amount *= 2;
    }
}