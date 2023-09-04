using BTDAdventure.Abstract;
using BTDAdventure.Entities;
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
        damage.Amount = entity is EnemyEntity ?
            Mathf.CeilToInt(damage.Amount * 0.7f) :
            Mathf.FloorToInt(damage.Amount * 0.5f);
    }
}
