using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;

namespace BTDAdventure.Cards.Enemies;

internal abstract class Red : RegularBloon
{
    public override string? Portrait => VanillaSprites.Red;
}

internal class Red1 : Red
{
    public override int MaxHP => 24;
    public override int Damage => 4;
    public override int Armor => 4;

    // Attack
    // Shield
    // Attack
    // Shield
    // Wait
    // Double
    // Shield
    // Double
    // Shield
    // Wait
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 10) switch
    {
        0 or 2 => new AttackAction(),
        1 or 3 or 6 or 8 => new ShieldAction(),
        5 or 7 => new DoubleAttackAction(),
        _ => new WaitAction(), // 4 & 9
    };
}

internal class Red2 : Red
{
    public override int MaxHP => 16;
    public override int Damage => 3;
    public override int Armor => 0;

    // Wait
    // Attack
    // Wait
    // Attack
    // Wait
    // Attack
    // Attack
    // Critical
    // Wait
    // Wait
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 10) switch
    {
        1 or 3 or 5 or 6 => new AttackAction(),
        7 => new DoubleAttackAction(),
        _ => new WaitAction(), // 0, 2, 4, 8, 9
    };
}

internal class Red3 : Red
{
    public override int MaxHP => 16;
    public override int Damage => 3;
    public override int Armor => 0;

    // Attack
    // Wait
    // Wait
    // Attack
    // Attack
    // Wait
    // Critical
    // Wait
    // Attack
    // Critical
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 10) switch
    {
        0 or 3 or 4 or 8 => new AttackAction(),
        6 or 9 => new DoubleAttackAction(),
        _ => new WaitAction(), // 1, 2, 5, 7
    };
}