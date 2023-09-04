using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class BurnEffect : Effect, ITurnEffect
{
    protected override string Name => "Burn";
    protected override string? Image => UIManager.BurnIcon;

    void ITurnEffect.OnPostTurn(Entity entity)
    {
        entity.ReceiveDamage(null, Level);
    }
}