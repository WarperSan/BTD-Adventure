using UnityEngine;

namespace BTDAdventure.Effects;

/// <summary>
/// Reduces the power of all attacks by 50% (rounded down)
/// </summary>
public class WeaknessEffect : Effect, IAttackEffect
{
    protected override string Name => "Weakness";
    protected override string? Image => "Ui[BTDAdventure-icon_enemy_weak]";

    public void ModifyDamage(Entity entity, ref Damage damage)
    {
        if (Level == 0)
            return;

        float percentage = 1 - Mathf.Pow(Level, 0.9f) / 100;

        damage.Amount = Mathf.FloorToInt(damage.Amount * percentage);
    }
}
