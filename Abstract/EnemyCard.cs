using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;
using UnityEngine;

namespace BTDAdventure.Cards.EnemyCards;

public abstract class EnemyCard : ModContent
{
    /// <summary>
    /// Defines the GUID of the image used for the portrait.
    /// </summary>
    public abstract string? Portrait { get; }

    #region Stats

    /// <summary>
    /// Defines the default amount of max HP the enemy has.
    /// </summary>
    public abstract int MaxHP { get; }

    /// <summary>
    /// Base amount of damage.
    /// </summary>
    public abstract int Damage { get; }

    /// <summary>
    /// Base amount of shield.
    /// </summary>
    public virtual int Armor { get; } = 1;

    #endregion Stats

    /// <summary>
    /// Defines the size of the portrait
    /// </summary>
    public virtual Vector2 Size { get; } = new(2, 2);

    public virtual uint CoinsGiven { get; } = 2;

    public virtual string? GetBackgroundGUID() => null;

    public abstract EnemyAction GetNextAction(uint roundCount, EnemyEntity source);

    public override void Register()
    {
#if DEBUG
        Log(Name + " registered");
#endif
    }
}

public abstract class RegularBloon : EnemyCard
{
    public override Vector2 Size => new(4, 4);
}