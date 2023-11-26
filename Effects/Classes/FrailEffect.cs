using BTDAdventure.Managers;

namespace BTDAdventure.Effects.Classes;

internal class FrailEffect : Effect, IShieldEffect
{
    protected override string Name => "Frail";
    protected override string? Image => UIManager.ICON_FRAIL;

    public void ModifyAmount(ref int amount) => amount /= 2;
}