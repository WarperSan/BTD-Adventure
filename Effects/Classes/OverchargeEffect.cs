using BTDAdventure.Effects.Interfaces;
using BTDAdventure.Managers;
using System;

namespace BTDAdventure.Effects.Classes;

internal class OverchagedEffect : Effect, IManaEffect
{
    protected override string Name => "Overcharged";
    protected override string? Image => UIManager.ICON_OVERCHARGED;

    public void OnManaGained(ref uint amount)
    { amount = 0; }

    public void OnManaReset(ref uint amount)
    {
        throw new NotImplementedException();
    }
}
