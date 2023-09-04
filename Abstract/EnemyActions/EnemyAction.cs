using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;

namespace BTDAdventure.Abstract;

public abstract class EnemyAction : ModContent, System.IComparable<EnemyAction>
{
    #region Constants
    public const string PlaceHolder = "placeholder";

    public const string Wait = "wait";

    public const string Attack = "attack";
    public const string DoubleAttack = "doubleattack";

    public const string Shield = "shield";
    public const string ShieldAll = "shieldall";

    public const string Weakness = "weakness";

    /*
    h = HEAL
    ha = HEAL ALL
    e = EVOKE
    cc = CURSE
    bd = BLIND
    bk = BLOCK
    sl = SILENCE
    wa = FRAIL
    wd = WOUND
    ws = Weakness
    */
    #endregion

    public string? Icon { get; }
    public string Tag { get; }

    public string? AnimationName { get; }

    public EnemyAction(string tag, string? animationName, string? icon = null)
    {
        this.Tag = tag;
        this.Icon = icon;
        this.AnimationName = animationName;
    }

    public virtual void OnAction(EnemyEntity source, PlayerEntity player) { }
    public abstract string? GetText(EnemyEntity source);

    //public abstract int PlayAnimation();


    /// The biggest moves before the smallest
    public int CompareTo(EnemyAction? other) => this.Order.CompareTo(other?.Order);

    public override void Register()
    {
#if DEBUG
        Log($"{Name} registered with tag \'{Tag}\'.");
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