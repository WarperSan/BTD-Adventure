using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class FrailEffect : Effect, IShieldEffect
{
    protected override string Name => "Frail";
    protected override string? Image => UIManager.FrailIcon;

    public void ModifyAmount(ref int amount) => amount /= 2;
}