using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Cards.EnemyCards;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

abstract class Red : RegularBloon
{
    protected Red(GameObject? enemyObject, int position, params string[] actions) : base(enemyObject, position, actions) { }

    protected override int DefaultMaxHP => 16;
    protected override int Damage => 3;
    protected override string? Portrait => VanillaSprites.Red;
}

internal class Red1 : Red
{
    public Red1(GameObject? enemyObject, int position) :
        base(enemyObject, position, Attack, Wait, Wait, Attack, Attack, Wait, DoubleAttack, Wait, Attack, DoubleAttack)
    { } // a|p|p|a|a|p|da|p|a|da
}


internal class Red2 : Red
{
    public Red2(GameObject? enemyObject, int position) :
        base(enemyObject, position, Wait, Attack, Wait, Wait, Attack, DoubleAttack, Wait, Wait)
    { } // p|a|p|a|p|a|a|da|p|p
}