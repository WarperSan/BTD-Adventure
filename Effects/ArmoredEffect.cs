using BTDAdventure.Abstract_Classes;
using System;

namespace BTDAdventure.Effects;


public class ArmoredEffect : Effect, IShieldEffect
{
    protected override string? Image => "Ui[BTDAdventure-icon_permashield]";
    public override string DisplayName => "Armored";

    public int ModifyAmount(int amount) => amount;

    public bool ShouldKeepShield() => true;
}
