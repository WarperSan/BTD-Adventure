using BTD_Mod_Helper.Extensions;
using BTDAdventure.Entities;
using BTDAdventure.Managers;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Abstract;

///
/// An effect can act at multiple events
/// For example, an effect could be a mix of poison (pre turn),
/// burn (post turn) and weakness (attack)
///

//public abstract uint LiveCost { get; }

/// <summary>
/// Defines what any effect needs.
/// </summary>
public abstract class Effect
{
    private GameObject? root;

    /// <summary>
    /// Name of the effect.
    /// </summary>
    protected abstract string Name { get; }

    /// <summary>
    /// Name displayed when the effect is checked. By default, the value will be '<see cref="Name"/> <see cref="Level"/>'.
    /// </summary>
    public virtual string DisplayName => Name + " " + Level;

    /// <summary>
    /// Image used to display the effect.
    /// </summary>
    protected virtual string? Image => null;

    #region Levels

    /// <summary>
    /// Current level of this effect.
    /// </summary>
    public int Level { get; internal set; }

    /// <summary>
    /// Current permanent level of this effect.
    /// </summary>
    public int LowestLevel { get; set; }

    /// <summary>
    /// Determines if the mod should show the effect's level or not.
    /// </summary>
    protected virtual bool ShowLevel => true;

    protected virtual int GetReduceAmount(Entity origin) => 1;

    #endregion

    #region Level UI
    
    private NK_TextMeshProUGUI? LevelText;

    public void UpdateLevelText() => LevelText?.UpdateText(Level);

    #endregion

    internal void SetUpUI(Entity entity, GameObject? @object)
    {
        root = @object;

        if (root == null)
            return;

        root.GetComponent<Image>().SetSprite(Image); // Set icon

        if (!ShowLevel)
            return;

        LevelText = UIManager.InitializeText(root.transform.Find("Text"), entity is PlayerEntity ? 50 : 25, Il2CppTMPro.TextAlignmentOptions.BottomRight);

        if (LevelText != null)
        {
            LevelText.textWrappingMode = Il2CppTMPro.TextWrappingModes.NoWrap;
        }
        // textWrappingMode
    }

    internal void UpdateEffect(Entity origin)
    {
        Level = Math.Max(Level - GetReduceAmount(origin), LowestLevel);

        if (Level <= 0)
        {
            if (root != null)
            {
                root.Destroy();
                origin.RemoveEffect(this);
                return; // Can't update if isn't applied
            }
        }

        UpdateLevelText();
    }

    /// <summary>
    /// Called whenever the effect gets removed.
    /// </summary>
    internal void RemoveEffect()
    {
        // Called fade in effect
        // Destroy after animation

        root?.Destroy();
    }
}

///
/// PreTurn:
///     OnEffect(Entity)
///
/// PreAction:
///     OnAction(Entity, HeroCard?)
///     OnCardPlay(PlayerEntity, HeroCard)
///     OnEnemyPlay(EnemyEntity)
///     OnEntityPlay(Entity)
///
/// PostAction:
///     OnAction(Entity, HeroCard?)
///     OnCardPlay(PlayerEntity, HeroCard)
///     OnEnemyPlay(EnemyEntity)
///     OnEntityPlay(Entity)
///
/// Attack:
///     ModifyDamage(Entity, Damage)
///
/// Shield:
///     ModifyAmount(int)
///
/// Attacked:
///     OnEffect(Entity, Entity)
///
/// Health:
///     ModifyAmount(int)
///
/// PostTurn:
///     OnEffect(Entity)
///
///
/// Action:
/// - Turn (Pre/Post)
/// - Action (Pre/Post)
/// - Attack (Pre)
/// - Shield (Pre)
/// - Attacked (Post)
/// - Health (Pre)
///

public interface IEffect { }