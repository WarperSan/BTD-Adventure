using BTD_Mod_Helper.Api.Enums;

namespace BTDAdventure.Cards.Enemies;

abstract class Red : RegularBloon
{
    public override int MaxHP => 16;
    public override int Damage => 3;
    public override string? Portrait => VanillaSprites.Red;
}

internal class Red1 : Red
{
    public override string[]? Intents => new string[]
    {
        Attack, Wait, Wait, Attack, Attack, Wait, DoubleAttack, Wait, Attack, DoubleAttack
    }; // a|p|p|a|a|p|da|p|a|da
}

internal class Red2 : Red
{
    public override string[]? Intents => new string[]
    {
        Wait, Attack, Wait, Wait, Attack, DoubleAttack, Wait, Wait
    }; // p|a|p|a|p|a|a|da|p|p
}