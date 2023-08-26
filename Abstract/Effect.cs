using BTD_Mod_Helper.Extensions;
using BTDAdventure.Entities;
using BTDAdventure.Managers;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Abstract;

/// <summary>
/// Defines what any effect needs.
/// </summary>
public abstract class Effect
{
    private GameObject? root;

    /// <summary>
    /// Name of the effect
    /// </summary>
    protected abstract string Name { get; }

    /// <summary>
    /// Name displayed when the effect is checked. By default, the value will be '<see cref="Name"/> <see cref="Level"/>'.
    /// </summary>
    public virtual string DisplayName => Name + " " + Level;

    //public abstract uint LiveCost { get; }
    //public String id;
    protected abstract string? Image { get; }

    public int Level { get; set; }

    // Permanent lvl
    public int LowestLevel { get; set; }
    protected virtual bool ShowLevel { get; } = true;

    protected virtual int GetReduceAmount(Entity origin) => 1;

    //public String text;

    private NK_TextMeshProUGUI? LevelText;

    internal void SetUpUI(Entity entity, GameObject? @object)
    {
        root = @object;

        if (root == null)
            return;

        root.GetComponent<Image>().SetSprite(Image); // Set icon

        if (ShowLevel)
        {
            LevelText = UIManager.InitializeText(root.transform.Find("Text"), entity is PlayerEntity ? 50 : 25, Il2CppTMPro.TextAlignmentOptions.Bottom);
        }
    }

    public void UpdateLevelText() => LevelText?.UpdateText(Level);

    internal void UpdateEffect(Entity origin)
    {
        Level = Math.Max(Level - GetReduceAmount(origin), LowestLevel);

        if (Level <= 0)
        {
            if (root != null)
            {
                GameObject.Destroy(root);
                origin.RemoveEffect(this);
                return; // Can't update if isn't applied
            }
        }

        UpdateLevelText();
    }

    /// <summary>
    /// Called whenever the effect gets removed
    /// </summary>
    internal void RemoveEffect()
    {
        // Called fade in effect
        // Destroy after animation

        root?.Destroy();
    }
}

public interface IEffect { }

/// <summary>
/// Defines all the effects that are called before the entity's turn begins
/// </summary>
public interface IPreTurnEffect : IEffect
{
    public void OnEffect(Entity entity);
}

/// <summary>
/// Defines all the effects that are called when an action is execute
/// </summary>
public interface IPreActionEffect : IEffect
{
    /// <summary>
    /// Determines if the effect should call the method specific of the entity type or the generic one.
    /// If true, the effect will either call <see cref="OnCardPlay(PlayerEntity, HeroCard)"/> or 
    /// <see cref="OnEntityPlay(Entity)"/>; Otherwise, <see cref="OnEntityPlay(Entity)"/> will be called.
    /// </summary>
    bool CheckEntityType { get; }

    internal static void OnAction(IPreActionEffect effect, Entity target, HeroCard? card)
    {
        if (effect.CheckEntityType)
        {
            if (target is PlayerEntity player && card != null)
                effect.OnCardPlay(player, card);
            else if (target is EnemyEntity enemy)
                effect.OnEnemyPlay(enemy);
            else
                Log($"The entity {target} is neither a {typeof(PlayerEntity).Name}, nor an {typeof(EnemyEntity).Name}.");
        }
        else
            effect.OnEntityPlay(target);
    }

    protected void OnCardPlay(PlayerEntity player, HeroCard cardPlayed) { }
    protected void OnEnemyPlay(EnemyEntity enemy) { }
    protected void OnEntityPlay(Entity entity) { }
}

/// <summary>
/// Defines all the effects that are called after an action is execute
/// </summary>
public interface IPostActionEffect : IEffect
{
    /// <summary>
    /// Determines if the effect should call the method specific of the entity type or the generic one.
    /// If true, the effect will either call <see cref="OnCardPlay(PlayerEntity, HeroCard)"/> or 
    /// <see cref="OnEntityPlay(Entity)"/>; Otherwise, <see cref="OnEntityPlay(Entity)"/> will be called.
    /// </summary>
    bool CheckEntityType { get; }

    internal static void OnAction(IPostActionEffect effect, Entity target, HeroCard? card)
    {
        if (effect.CheckEntityType)
        {
            if (target is PlayerEntity player && card != null)
                effect.OnCardPlayed(player, card);
            else if (target is EnemyEntity enemy)
                effect.OnEnemyPlayed(enemy);
            else
                Log($"The entity {target} is neither a {typeof(PlayerEntity).Name}, nor an {typeof(EnemyEntity).Name}.");
        }
        else
            effect.OnEntityPlayed(target);
    }

    protected void OnCardPlayed(PlayerEntity player, HeroCard cardPlayed) { }
    protected void OnEnemyPlayed(EnemyEntity enemy) { }
    protected void OnEntityPlayed(Entity entity) { }
}

/// <summary>
/// Defines all the effects that modify the damage output
/// </summary>
public interface IAttackEffect : IEffect
{
    /// <summary>
    /// Called whenever a damage amount is dealt
    /// </summary>
    /// <param name="entity">Entity that has the effect active</param>
    /// <param name="damage">Attack to modify</param>
    /// <returns>Modified attack</returns>
    public abstract Damage ModifyDamage(Entity entity, Damage damage);
}

/// <summary>
/// Defines all the effects taht modify the shield output
/// </summary>
public interface IShieldEffect : IEffect
{
    /// <summary>
    /// Called whenever a shield is gained
    /// </summary>
    /// <param name="amount">Amount of shield that was going to be gained</param>
    /// <returns>Amount of shield that will be gained</returns>
    public abstract int ModifyAmount(int amount);

    /// <summary>
    /// Determines if the effect causes the entity to keep it's shield
    /// </summary>
    public bool ShouldKeepShield() => false;
}

public interface IAttackedEffect : IEffect
{
    public void OnEffect(Entity source, Entity attacker);
}

/// <summary>
/// Defines all the effects that are called after the entity's turn ends
/// </summary>
public interface IPostTurnEffect : IEffect
{
    public void OnEffect(Entity entity);
}