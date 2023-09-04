using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class WoundEffect : Effect, IActionEffect
{
    protected override string Name => "Wound";
    protected override string? Image => UIManager.WoundIcon;

    public bool CheckEntityType => false;

    void IActionEffect.OnEntityPlay(Entity entity)
    {
        entity.ReceiveDamage(null, new Damage()
        {
            Amount = 1,
            IgnoresShield = true,
        });
    }
}
