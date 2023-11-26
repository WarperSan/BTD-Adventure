using BTDAdventure.Managers;

namespace BTDAdventure.Effects.Classes;

internal class WoundExtremeEffect : Effect, IActionEffect
{
    protected override string Name => "Extreme Wound";
    protected override string? Image => UIManager.ICON_EXTREME_WOUND;

    public bool CheckEntityType => false;

    void IActionEffect.OnEntityPlay(Entity entity)
    {
        entity.ReceiveDamage(null, new Damage()
        {
            Amount = Level,
            IgnoresShield = true,
        });
    }
}