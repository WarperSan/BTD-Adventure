using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;

namespace BTDAdventure.Abstract;

public abstract class EnemyAction : ModContent
{
    public string? Icon { get; }
    public string? AnimationName { get; }

    public EnemyAction(string? animationName, string? icon = null)
    {
        this.Icon = icon;
        this.AnimationName = animationName;
    }

    public abstract void OnAction(EnemyEntity source, PlayerEntity player);

    public abstract string? GetText(EnemyEntity source);

    public override void Register()
    {
#if DEBUG
        Log($"{Name} registered");
#endif
    }
}

public struct Damage
{
    public int Amount = 0;

    /// <summary>
    /// Determines wheter or not the damage will pass through the shield
    /// </summary>
    public bool IgnoresShield = false;

    /// <summary>
    /// Determines by how much the damage will be multiplied if it hits a shield. Won't be considered if <see cref="IgnoresShield"/> is set to true.
    /// </summary>
    public float ShieldMultiplier = 1;

    public Damage(int Amount)
    {
        this.Amount = Amount;
    }
}