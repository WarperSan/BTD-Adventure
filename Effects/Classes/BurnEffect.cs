using BTDAdventure.Effects.Interfaces;
using BTDAdventure.Managers;

namespace BTDAdventure.Effects.Classes;

internal class BurnEffect : Effect, ITurnEffect
{
    protected override string Name => "Burn";
    protected override string? Image => UIManager.ICON_BURN;

    void ITurnEffect.OnPostTurn(Entity entity)
    {
        entity.ReceiveDamage(null, Level);
    }
}