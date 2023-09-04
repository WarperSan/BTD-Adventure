using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class WoundExtremeEffect : Effect, IActionEffect
{
    protected override string Name => "Extreme Wound";
    protected override string? Image => UIManager.WoundExtremeIcon;

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
