using BTD_Mod_Helper.Api.Enums;

namespace BTDAdventure.Cards.Enemies;

abstract class Yellow : RegularBloon
{
    public override int MaxHP => 44;
    public override int Damage => 5;
    public override string? Portrait => VanillaSprites.Yellow;
    public override string? World => "forest";
    public override uint RiskValue => 10;
}

internal class Yellow1 : Yellow
{
    public override string[]? Intents => new string[]
    {
        Attack, Wait, Attack, Attack, Wait, DoubleAttack, Wait, DoubleAttack, DoubleAttack, Wait
    }; // a|p|a|a|p|da|p|da|da|p
}

internal class Yellow2 : Yellow
{
    public override string[]? Intents => new string[]
    {
        Wait, Attack, Wait, Attack, Attack, Wait, DoubleAttack, Wait, DoubleAttack, DoubleAttack
    }; // p|a|p|a|a|p|da|p|da|da
}