using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class PoisonEffect : Effect, ITurnEffect
{
    protected override string Name => "Poison";
    protected override string? Image => UIManager.PoisonIcon;

    void ITurnEffect.OnPreTurn(Entity entity)
    {
        entity.ReceiveDamage(null, Level);
        entity.PlayEffectVisual("PoisonEffect");
        SoundManager.PlaySound(SoundManager.SOUND_POISON_TICK, SoundManager.GeneralGroup, 0.2f);
    }
}