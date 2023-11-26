using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class WaitAction : EnemyAction
{
    public override string? Icon => UIManager.ICON_WAIT;
    public override string? AnimationName => ANIMATION_WAIT;
    public override string? SoundName => SoundManager.SOUND_WAIT_TICK;
}