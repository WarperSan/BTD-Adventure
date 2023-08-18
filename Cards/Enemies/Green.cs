﻿using BTD_Mod_Helper.Api.Enums;

namespace BTDAdventure.Cards.Enemies;

abstract class Green : RegularBloon
{
    public override int MaxHP => 32;
    public override int Damage => 4;
    public override string? Portrait => VanillaSprites.Green;
}

internal class Green1 : Green
{
    public override string[]? Intents => new string[]
    {
        Attack, Wait, Wait, PlaceHolder, Attack, Wait, Wait, PlaceHolder, Attack, Wait
    }; // a|p|p|wa|a|p|p|wa|a|p
}

internal class Green2 : Green
{
    public override string[]? Intents => new string[]
    {
        Wait, Wait, PlaceHolder, Attack, Wait, Wait, PlaceHolder, Attack, Wait, Wait
    }; // p|p|wa|a|p|p|wa|a|p|p
}