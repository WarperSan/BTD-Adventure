using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;

namespace BTDAdventure.Cards.Enemies;

internal abstract class Yellow : RegularBloon
{
    public override int MaxHP => 140;
    public override int Damage => 5;
    public override string? Portrait => VanillaSprites.Yellow;
}

internal class Yellow1 : Yellow
{
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source)
    {
        return new WaitAction();
    }
}

internal class Yellow2 : Yellow
{
    public override int Damage => 30;
    public override int MaxHP => 200;

    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 9) switch
    {
        8 => new AttackAction(),
        _ => new WaitAction(),
    };
}