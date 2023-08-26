using BTD_Mod_Helper.Api.Enums;

namespace BTDAdventure.Cards.Enemies;

internal class Blue : RegularBloon
{
    public override string[]? Intents => new string[] 
    { 
        // Attack
        Weakness, Shield, Attack, Shield, Wait, DoubleAttack, Shield, DoubleAttack, Wait 
    };
    // a|s|a|s|p|da|s|da|s|p

    public override string? Portrait => VanillaSprites.Blue;

    public override int MaxHP => 24;
    public override int Damage => 4;
    public override int Armor => 4;

    public override string? World => "forest";

    public override uint RiskValue => 2;
}