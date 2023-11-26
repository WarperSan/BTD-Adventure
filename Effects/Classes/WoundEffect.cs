using BTDAdventure.Managers;

namespace BTDAdventure.Effects.Classes;

internal class WoundEffect : Effect, IActionEffect
{
    protected override string Name => "Wound";
    protected override string? Image => UIManager.ICON_WOUND;

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