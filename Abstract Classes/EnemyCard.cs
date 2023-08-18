using UnityEngine;

namespace BTDAdventure.Cards.EnemyCards;

public abstract class EnemyCard
{
    /// <summary>
    /// Defines the GUID of the image used for the portrait.
    /// </summary>
    public abstract string? Portrait { get; }

    /// <summary>
    /// Defines the actions that the enemy will execute.
    /// </summary>
    public abstract string[]? Intents { get; }

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
    #endregion

    /// <summary>
    /// Defines the size of the portrait
    /// </summary>
    public virtual Vector2 Size { get; } = new(2, 2);

    public virtual uint CoinsGiven { get; } = 2;

    public virtual string? GetBackgroundGUID() => null;

    //public virtual string? World { get; }
}

public abstract class RegularBloon : EnemyCard
{
    public override Vector2 Size => new(4, 4);
}