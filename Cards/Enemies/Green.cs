using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Cards.EnemyCards;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

abstract class Green : RegularBloon
{
    protected Green(GameObject? enemyObject, int position, params string[] actions) : base(enemyObject, position, actions) { }

    protected override int DefaultMaxHP => 32;
    protected override int Damage => 4;
    protected override string? Portrait => VanillaSprites.Green;
}

internal class Green1 : Green
{
    public Green1(GameObject? enemyObject, int position)
        : base(enemyObject, position, Attack, Wait, Wait, PlaceHolder, Attack, Wait, Wait, PlaceHolder, Attack, Wait)
    { } // a|p|p|wa|a|p|p|wa|a|p
}

internal class Green2 : Green
{
    public Green2(GameObject? enemyObject, int position)
        : base(enemyObject, position, Wait, Wait, PlaceHolder, Attack, Wait, Wait, PlaceHolder, Attack, Wait, Wait)
    { } // p|p|wa|a|p|p|wa|a|p|p
}