using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Abstract.EnemyActions;
using BTDAdventure.Effects;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;

namespace BTDAdventure.Cards.Enemies;

internal abstract class Green : RegularBloon
{
    public override int MaxHP => 100;
    public override int Damage => 4;
    public override string? Portrait => VanillaSprites.Green;
}

internal class Green1 : Green
{
    // Attack 0 9
    // Attack 1 10
    // Attack 2 11

    // Wait 3 12
    // Attack 4 13
    // Wait 5 14

    // Wait 6 15
    // Double 7 16
    // Immune 8 17
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 9) switch
    {
        0 or 1 or 2 or 4 => new AttackAction(),
        7 => new DoubleAttackAction(),
        8 => new GreenImmune(),
        _ => new WaitAction()
    };
}

internal class Green2 : Green
{
    // Strength
    // Attack
    // Wait
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 3) switch
    {
        0 => new GreenStrength(),
        1 => new AttackAction(),
        _ => new WaitAction(),
    };
}

internal class Green3 : Green
{
    // Wait
    // Attack
    // Strength
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 3) switch
    {
        0 => new WaitAction(),
        1 => new AttackAction(),
        _ => new GreenStrength(),
    };
}

internal class GreenImmune : IntentEffectAction
{
    public override string? GetText(EnemyEntity source) => "4";

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AddLevel<ImmuneEffect>(4);
    }
}

internal class GreenStrength : IntentEffectAction
{
    public override string? GetText(EnemyEntity source) => "3";

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AddLevel<StrengthEffect>(3);
    }
}