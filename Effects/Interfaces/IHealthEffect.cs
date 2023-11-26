namespace BTDAdventure.Effects.Interfaces;

/// <summary>
/// Determines all effects that modify the amount of health modified.
/// </summary>
public interface IHealthEffect : IEffect
{
    public void ModifyAmount(ref int amount);
}

