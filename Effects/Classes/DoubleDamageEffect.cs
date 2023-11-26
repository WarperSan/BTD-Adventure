using BTDAdventure.Managers;
using System;

namespace BTDAdventure.Effects.Classes;

/// <summary>
/// Doubles the power of all attacks
/// </summary>
internal class DoubleDamageEffect : Effect, IAttackEffect
{
    protected override string Name => throw new NotImplementedException();
    protected override string? Image => UIManager.ICON_DOUBLE_DAMAGE;

    public void ModifyDamage(Entity entity, ref Damage damage)
    {
        damage.Amount *= Level + 1;
    }
}