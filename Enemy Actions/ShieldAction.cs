using BTDAdventure.Entities;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class ShieldAction : EnemyAction
{
    public ShieldAction() : base("IntentWait", ShieldIcon)
    {
    }

    public override string? GetText(EnemyEntity source) => source.GetShield().ToString();

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        SoundManager.PlaySound(SoundManager.SOUND_SHIELD, SoundManager.GeneralGroup);

        source.AddShield(source.GetShield(), source);
    }
}