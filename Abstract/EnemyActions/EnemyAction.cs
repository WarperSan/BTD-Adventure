using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;

namespace BTDAdventure.Abstract;

public abstract class EnemyAction : ModContent
{
    #region Constants

    public const string ANIMATION_WAIT = "IntentWait";
    public const string ANIMATION_SHIELD = ANIMATION_WAIT;
    public const string ANIMATION_ATTACK = "IntentAttack";

    #endregion Constants

    /// <summary>
    /// GUID of the image to use when this action is used.
    /// </summary>
    public virtual string? Icon => null;

    /// <summary>
    /// Name of the animation to play whenever this action is executed.
    /// </summary>
    public virtual string? AnimationName => null;

    /// <summary>
    /// Name of the audio to play whenever this action is executed.
    /// </summary>
    public virtual string? SoundName => null;

    /// <summary>
    /// Called whenever this action is executed.
    /// </summary>
    public virtual void OnAction(EnemyEntity source, PlayerEntity player) { }

    /// <returns>Text to display when this action is used.</returns>
    public virtual string? GetText(EnemyEntity source) => null;

    /// <inheritdoc/>
    public override sealed void Register()
    {
#if DEBUG
        Log($"{Name} registered.");
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