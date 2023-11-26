namespace BTDAdventure.Effects.Classes;

/// <summary>
/// Each point of Strength increases the damage by 1.
/// </summary>
public class StrengthEffect : Effect, IAttackEffect
{
    protected override string Name => "Strength";
    protected override string? Image => "Ui[BTDAdventure-icon_strength]";

    public void ModifyDamage(Entity entity, ref Damage damage)
    {
        damage.Amount += Level;
    }
}