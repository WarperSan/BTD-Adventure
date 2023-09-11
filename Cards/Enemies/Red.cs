using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Abstract.EnemyActions;
using BTDAdventure.Effects;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;
using BTDAdventure.Managers;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

abstract class Red : RegularBloon
{
    public override int MaxHP => 32;
    public override string? Portrait => VanillaSprites.Red;
    public override int Damage => 4;
}

internal class Red1 : Red
{
    // Wait 0 9
    // Attack 1 10
    // Wait 2 11

    // Wait 3 12
    // Attack 4 13
    // Wait 5 14

    // Attack 6 15
    // Wait 7 16
    // Double 8 17
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 9) switch
    {
        1 or 4 => new AttackAction(),
        6 => new RedStrength(),
        8 => new DoubleAttackAction(),
        _ => new WaitAction(),
    };
}

internal class Red2 : Red
{
    // Attack 0 6
    // Wait 1 7
    // Attack 2 8

    // Weak 3 9
    // Attack 4 10
    // Weak 5 11
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 6) switch
    {
        0 or 2 or 4 => new AttackAction(),
        3 or 5 => new RedWeakness(),
        _ => new WaitAction(),
    };
}

internal class Red3 : Red
{
    public override int Damage => 6;
    public override int MaxHP => 50;

    // Wait 0
    // Spawn 1
    // Wait 2

    // Double 3
    // Attack 4
    // Wait 5

    // Wait 6
    // Attack 7
    // Attack 8

    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 9) switch
    {
        1 => new RedSpawn(),
        3 => new DoubleAttackAction(),
        4 or 7 or 8 => new AttackAction(),
        _ => new WaitAction(),
    };
}

internal class RedWeakness : IntentEffectAction
{
    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        player.AddLevel<WeaknessEffect>(5);
    }

    public override string? GetText(EnemyEntity source)
    {
        return "5";
    }
}

internal class RedStrength : IntentEffectAction
{
    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AddLevel<StrengthEffect>(3);
    }

    public override string? GetText(EnemyEntity source) => "3";
}

internal class RedSpawn : IntentSpawnAction
{
    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        if (Random.value <= 0.5f)
            GameManager.Instance.AddEnemy(new Red1(), true);
        else
            GameManager.Instance.AddEnemy(new Red2(), true);
    }
}