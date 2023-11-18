namespace BTDAdventure.Effects;

public class ImmuneEffect : Effect, IHealthEffect
{
    protected override string Name => "Immune";
    protected override string? Image => "Ui[BTDAdventure-icon_immune]";

    public void ModifyAmount(ref int amount) => amount = 0;
}