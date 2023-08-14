using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Cards.EnemyCards;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

internal class RedRegrow : RegularBloon
{
    public RedRegrow(GameObject? enemyObject, int position) :
        base(enemyObject, position, Shield, Shield, PlaceHolder, Wait, Wait, Shield, PlaceHolder, Shield, PlaceHolder, Attack)
    { } // s|s|wa|p|p|s|wa|s|wa|a

    protected override int DefaultMaxHP => 12;
    protected override int Damage => 2;
    protected override string? Portrait => VanillaSprites.Red;
}