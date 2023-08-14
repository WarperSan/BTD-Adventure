using BTDAdventure.Cards.EnemyCards;

namespace BTDAdventure;

public abstract class EnemyAction : System.IComparable<EnemyAction>
{
    #region Constants
    public const string PlaceHolder = "placeholder";

    public const string Wait = "wait";
    
    public const string Attack = "attack";
    public const string DoubleAttack = "doubleattack";
    
    public const string Shield = "shield";
    public const string ShieldAll = "shieldall";

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

    /// <summary>
    /// Used to override existing actions.
    /// </summary>
    public uint Order { get; }
    public string? Icon { get; }
    public string Tag { get; }

    public string? AnimationName { get; }

    public EnemyAction(string tag, uint order, string? animationName, string? icon = null)
    {
        this.Tag = tag;
        this.Order = order;
        this.Icon = icon;
        this.AnimationName = animationName;
    }

    public virtual void OnAction(EnemyCard source) { }
    public abstract string? GetText(EnemyCard source);

    /// The biggest moves before the smallest
    public int CompareTo(EnemyAction? other) => this.Order.CompareTo(other?.Order);
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