using BTDAdventure.Abstract;
using System;

namespace BTDAdventure.Effects;

/// <summary>
/// Each point of Strength increases the damage by 1.
/// </summary>
public class StrengthEffect : Effect, IAttackEffect
{
    protected override string Name => "Strength";
    protected override string? Image => throw new NotImplementedException();

    public Damage ModifyDamage(Entity entity, Damage damage)
    {
        damage.Amount += Level;
        return damage;
    }
}
