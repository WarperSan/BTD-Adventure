namespace BTDAdventure.Effects;

internal class ArmoredEffect : Effect, IShieldEffect
{
    protected override string Name => "Armored";
    protected override string? Image => "Ui[BTDAdventure-icon_permashield]";

    public void ModifyAmount(ref int amount)
    { }

    public void ShouldKeepShield(ref bool keepShield) => keepShield = true;
}