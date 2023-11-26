using BTDAdventure.Effects.Interfaces;

namespace BTDAdventure.Effects.Classes;

/// <summary>
/// Effect that allows the entity to keep its shield.
/// </summary>
public class ArmoredEffect : Effect, IShieldEffect
{
    protected override string Name => "Armored";
    protected override string? Image => "Ui[BTDAdventure-icon_permashield]";

    public void ModifyAmount(ref int amount)
    { }

    public void ShouldKeepShield(ref bool keepShield) => keepShield = true;
}