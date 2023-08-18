using BTDAdventure.Abstract_Classes;
using BTDAdventure.Managers;
using System;

namespace BTDAdventure.Effects;

/// <summary>
/// Doubles the power of all attacks
/// </summary>
public class DoubleDamageEffect : Effect, IAttackEffect
{
    protected override string? Image => throw new NotImplementedException();
    public override string DisplayName => throw new NotImplementedException();

    public Damage ModifyDamage(Entity entity, Damage damage)
    {
        damage.Amount *= 2;
        return damage;
    }
}