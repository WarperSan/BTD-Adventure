namespace BTDAdventure.Effects.Interfaces;

/// <summary>
/// Defines all the effects that modify the damage output of entities.
/// </summary>
public interface IAttackEffect : IEffect
{
    /// <summary>
    /// Called whenever an attack is dealt.
    /// </summary>
    /// <param name="entity">Entity that has the effect active</param>
    /// <param name="damage">Attack to modify</param>
    public abstract void ModifyDamage(Entity entity, ref Damage damage);
}

