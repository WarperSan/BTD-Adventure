using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class PoisonEffect : Effect, IPreTurnEffect
{
    protected override string Name => "Poison";
    protected override string? Image => UIManager.PoisonIcon;

    void IPreTurnEffect.OnEffect(Entity entity)
    {
        entity.ReceiveDamage(null, Level);
    }
}
