namespace BTDAdventure.Effects.Interfaces;

/// <summary>
/// Defines all the effects that modify the shield output.
/// </summary>
public interface IShieldEffect : IEffect
{
    /// <summary>
    /// Called whenever a shield is gained.
    /// </summary>
    /// <param name="amount">Amount of shield that was going to be gained</param>
    /// <returns>Amount of shield that will be gained</returns>
    public abstract void ModifyAmount(ref int amount);

    /// <summary>
    /// Determines if the effect causes the entity to keep it's shield or not.
    /// </summary>
    public void ShouldKeepShield(ref bool keepShield) => keepShield |= false;
}
