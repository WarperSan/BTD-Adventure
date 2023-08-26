using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class WoundEffect : Effect, IPostActionEffect
{
    protected override string Name => "Wound";
    protected override string? Image => UIManager.WoundIcon;

    public bool CheckEntityType => false;

    void IPostActionEffect.OnEntityPlayed(Entity entity)
    {
        entity.ReceiveDamage(null, new Damage()
        {
            Amount = 1,
            IgnoresShield = true,
        });
    }
}
