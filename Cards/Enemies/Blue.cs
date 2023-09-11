using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Abstract.EnemyActions;
using BTDAdventure.Effects;
using BTDAdventure.Enemy_Actions;
using BTDAdventure.Entities;
using UnityEngine;

namespace BTDAdventure.Cards.Enemies;

internal abstract class Blue : RegularBloon
{
    public override string? Portrait => VanillaSprites.Blue;

    public override int MaxHP => 74;
    public override int Damage => 5;
    public override int Armor => 14;
}

// Focus Shield
internal class Blue1 : Blue
{
    public override int Armor => 30;

    // Prioritize shield but mostly random
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source)
    {
        float rdm = Random.value;

        if (source.GetEffectLevel<ArmoredEffect>() < 2) return new BlueArmored();

        if (source.GetCurrentShield() == 0) return new ShieldAction();

        // If 60%
        if (rdm < 0.6f) return new BlueAttackShield();

        // 40%
        return new ShieldAction();
    }
}

// Focus Strength
internal class Blue2 : Blue
{
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source)
    {
        if (source.GetEffectLevel<StrengthEffect>() == 0) return new BlueStrengthEffect();

        float rdm = Random.value;

        if (rdm < 0.3f) return new WaitAction();

        if (source.GetHealth() < MaxHP / 3) return new DoubleAttackAction();

        return new AttackAction();
    }
}

// Focus Spawn
internal class Blue3 : Blue
{
    public override int MaxHP => 80;

    // Spawn 0
    // Wait 1
    // Wait 2
    // Attack 3
    // Spawn 4
    public override EnemyAction GetNextAction(uint roundCount, EnemyEntity source) => (roundCount % 5) switch
    {
        0 or 4 => new RedSpawn(),
        3 => new AttackAction(),
        _ => new WaitAction(),
    };
}

internal class BlueArmored : IntentEffectAction
{
    public override string? GetText(EnemyEntity source) => "3";

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AddLevel<ArmoredEffect>(3);
    }
}

internal class BlueAttackShield : IntentAttackAction
{
    public override string? GetText(EnemyEntity source)
    {
        return source.GetCurrentShield().ToString();
    }

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AttackTarget(new Damage()
        {
            Amount = source.GetCurrentShield()
        }, player);
    }
}

internal class BlueStrengthEffect : IntentEffectAction
{
    public override string? GetText(EnemyEntity source) => "3";

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AddLevel<StrengthEffect>(3);
    }
}