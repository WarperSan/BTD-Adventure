using BTDAdventure.Abstract;

namespace BTDAdventure.Effects;


internal class ArmoredEffect : Effect, IShieldEffect
{
    protected override string Name => "Armored";
    protected override string? Image => "Ui[BTDAdventure-icon_permashield]";

    public int ModifyAmount(int amount) => amount;

    public bool ShouldKeepShield() => true;
}
