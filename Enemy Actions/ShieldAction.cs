using BTDAdventure.Entities;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class ShieldAction : EnemyAction
{
    public override string? Icon => UIManager.ICON_SHIELD;
    public override string? AnimationName => ANIMATION_SHIELD;

    public override string? SoundName => SoundManager.SOUND_SHIELD;

    public override string? GetText(EnemyEntity source) => source.GetNextShield().ToString();

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.PlayEffectVisual("ShieldGainVisual");
        source.AddShield(source.GetNextShield(), source);
    }
}