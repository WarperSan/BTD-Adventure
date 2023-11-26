namespace BTDAdventure.Effects.Classes;

internal class ResistanceEffect : Effect, IShieldEffect
{
    protected override string Name => "Resistance";
    protected override string? Image => "Ui[BTDAdventure-icon_resistance]";

    public void ModifyAmount(ref int amount)
    {
        amount++;
    }
}
