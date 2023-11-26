using BTDAdventure.Managers;

namespace BTDAdventure.Abstract.EnemyActions;

public abstract class IntentEffectAction : IntentAttackAction
{
    public override string? Icon => UIManager.ICON_CURSE;
    public override string? SoundName => SoundManager.SOUND_DEBUFF_APPLIED;
}