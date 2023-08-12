using BTDAdventure.Cards;
using BTDAdventure.Managers;
using System;

namespace BTDAdventure;

public abstract class EnemyAction : IComparable<EnemyAction>
{
    #region Constants
    public const string WaitTag = "wait";
    public const string AttackTag = "attack";
    #endregion

    public uint Order { get; }
    public string? Icon { get; }
    public string Tag { get; }

    public string AnimationName { get; }

    public EnemyAction(string tag, uint order, string animationName, string? icon = null)
    {
        this.Tag = tag;
        this.Order = order;
        this.Icon = icon;
        this.AnimationName = animationName;
    }

    public virtual void OnAction(EnemyCard source) { }
    public abstract string? GetText();

    /// The biggest moves before the smallest
    public int CompareTo(EnemyAction? other) => this.Order.CompareTo(other?.Order);
}

public class WaitAction : EnemyAction
{
    public WaitAction() : base(WaitTag, 0, "IntentWait", UIManager.WaitIcon) { }

    public override string? GetText() => null;

    public override void OnAction(EnemyCard source)
    {
#if DEBUG
        Log("Wait action activate ...");
#endif
    }
}

public class AttackAction : EnemyAction
{
    public AttackAction() : base(AttackTag, 1, "IntentAttack", UIManager.DamageIcon) { }

    public override string? GetText() => "1";
    public override void OnAction(EnemyCard source)
    {

        GameManager.Instance.DamagePlayer(new Damage(1));
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