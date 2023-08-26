using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class WoundExtremeEffect : Effect, IPostActionEffect
{
    protected override string Name => "Extreme Wound";
    protected override string? Image => UIManager.WoundExtremeIcon;

    public bool CheckEntityType => false;

    void IPostActionEffect.OnEntityPlayed(Entity entity)
    {
        entity.ReceiveDamage(null, new Damage()
        {
            Amount = Level,
            IgnoresShield = true,
        });
    }
}
