using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Cards.EnemyCards;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

internal class Blue : RegularBloon
{
    public Blue(GameObject? enemyObject, int position)
        : base(enemyObject, position, Attack, Shield, Attack, Shield, Wait, DoubleAttack, Shield, DoubleAttack, Wait)
    { } // a|s|a|s|p|da|s|da|s|p

    protected override int DefaultMaxHP => 24;
    protected override int Damage => 4;
    protected override string? Portrait => VanillaSprites.Blue;
}