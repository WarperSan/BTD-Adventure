using BTD_Mod_Helper.Api.Enums;

namespace BTDAdventure.Cards.Enemies;

internal class RedRegrow : RegularBloon
{
    public override int MaxHP => 12;
    public override int Damage => 2;
    public override string? Portrait => VanillaSprites.Red;

    public override string[]? Intents => new string[]
    {
        Shield, Shield, PlaceHolder, Wait, Wait, Shield, PlaceHolder, Shield, PlaceHolder, Attack
    }; // s|s|wa|p|p|s|wa|s|wa|a
}