using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class BurnEffect : Effect, IPostTurnEffect
{
    protected override string Name => "Burn";
    protected override string? Image => UIManager.BurnIcon;

    void IPostTurnEffect.OnEffect(Entity entity)
    {
        entity.ReceiveDamage(null, Level);
    }
}