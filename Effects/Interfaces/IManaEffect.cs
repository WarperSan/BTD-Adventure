namespace BTDAdventure.Effects.Interfaces;

/// <summary>
/// Determines all the effects that change mana gain.
/// </summary>
public interface IManaEffect : IEffect
{
    public void OnManaGained(ref uint amount);
    public void OnManaReset(ref uint amount);
}