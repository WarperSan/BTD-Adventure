using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;

namespace BTDAdventure.Cards.Enemies;

internal abstract class Blue : RegularBloon
{
    public override string? Portrait => VanillaSprites.Blue;
}

internal class Blue1 : Blue
{
    public override int MaxHP => 30;
    public override int Damage => 5;

    //Attack
    //Wait
    //Attack
    //Attack
    //Wait
    //Critical
    //Wait
    //Critical
    //Critical
    //Wait

    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 10) switch
    {
        0 or 2 or 3 => new AttackAction(),
        5 or 7 or 8 => new DoubleAttackAction(),
        _ => new WaitAction(), // 1, 4, 6, 9
    };
}

internal class Blue2 : Blue
{
    public override int MaxHP => 30;
    public override int Damage => 5;

    //Wait
    //Attack
    //Wait
    //Attack
    //Attack
    //Wait
    //Critical
    //Wait
    //Critical
    //Critical
    //Wait

    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 10) switch
    {
        1 or 3 or 4 => new AttackAction(),
        6 or 8 or 9 => new DoubleAttackAction(),

        _ => new WaitAction(), // 0, 2, 5, 7
    };
}
