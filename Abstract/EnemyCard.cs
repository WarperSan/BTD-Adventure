using BTD_Mod_Helper.Api;
using BTDAdventure.Entities;
using BTDAdventure.Managers;
using UnityEngine;

namespace BTDAdventure.Cards.EnemyCards;

public abstract class EnemyCard : ModContent
{
    #region UI

    /// <summary>
    /// Defines the GUID of the image used for the portrait.
    /// </summary>
    public abstract string? Portrait { get; }

    /// <summary>
    /// Defines the size of the portrait.
    /// </summary>
    public virtual Vector2 Size { get; } = new(2, 2);

    public virtual string? GetBackgroundGUID() => null;

    #endregion

    #region Stats

    /// <summary>
    /// Defines the default amount of max HP the enemy has.
    /// </summary>
    public virtual int MaxHP { get; } = 1;

    /// <summary>
    /// Base amount of damage.
    /// </summary>
    public virtual int Damage { get; } = 1;

    /// <summary>
    /// Base amount of shield.
    /// </summary>
    public virtual int Armor { get; } = 1;

    #endregion Stats

    #region Reward

    /// <inheritdoc cref="EnemyEntity.GetReward"/>
    public virtual Reward GetReward() => new()
    {
        Cash = 2,
    };

    #endregion

    #region Intent

    public abstract EnemyAction GetNextAction(uint roundCount, EnemyEntity source);

    #endregion

    public override sealed void Register()
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