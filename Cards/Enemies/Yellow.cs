using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Cards.EnemyCards;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

abstract class Yellow : RegularBloon
{
    protected Yellow(GameObject? enemyObject, int position, params string[] actions) : base(enemyObject, position, actions) { }

    protected override int DefaultMaxHP => 44;
    protected override int Damage => 5;
    protected override string? Portrait => VanillaSprites.Yellow;
}

internal class Yellow1 : Yellow
{
    public Yellow1(GameObject? enemyObject, int position)
        : base(enemyObject, position, Attack, Wait, Attack, Attack, Wait, DoubleAttack, Wait, DoubleAttack, DoubleAttack, Wait)
    { } // a|p|a|a|p|da|p|da|da|p
}

internal class Yellow2 : Yellow
{
    public Yellow2(GameObject? enemyObject, int position)
        : base(enemyObject, position, Wait, Attack, Wait, Attack, Attack, Wait, DoubleAttack, Wait, DoubleAttack, DoubleAttack)
    { } // p|a|p|a|a|p|da|p|da|da
}