using BTD_Mod_Helper.Api.Enums;
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
    protected virtual bool ShowLevel => true;

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
            LevelText = UIManager.InitializeText(root.transform.Find("Text"), entity is PlayerEntity ? 50 : 25, Il2CppTMPro.TextAlignmentOptions.BottomRight);

            if (LevelText != null)
            {
                LevelText.textWrappingMode = Il2CppTMPro.TextWrappingModes.NoWrap;
            }
            // textWrappingMode
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
                root.Destroy();
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

public interface IEffect
{

}

public interface ITurnEffect : IEffect
{
    public void OnPreTurn(Entity entity) { }
    public void OnPostTurn(Entity entity) { }
}

public interface IActionEffect : IEffect
{
    /// <summary>
    /// Determines if the effect should call the method specific of the entity type or the generic one.
    /// If true, the effect will either call <see cref="OnCardPlay(PlayerEntity, HeroCard)"/> or 
    /// <see cref="OnEntityPlay(Entity)"/>; Otherwise, <see cref="OnEntityPlay(Entity)"/> will be called.
    /// </summary>
    bool CheckEntityType { get; }

    sealed void OnAction(Entity source, HeroCard? card)
    {
        if (CheckEntityType)
        {
            if (source is PlayerEntity player)
            {
                if (card != null)
                {
                    OnCardPlay(player, card);
                    return;
                }
            }
            else if (source is EnemyEntity enemy)
            {
                OnEnemyPlay(enemy);
                return;
            }
            else
                Log($"The entity {source} is neither a {typeof(PlayerEntity).Name}, nor an {typeof(EnemyEntity).Name}.");
        }
        OnEntityPlay(source);
    }

    protected void OnCardPlay(PlayerEntity player, HeroCard cardPlayed) { }
    protected void OnEnemyPlay(EnemyEntity enemy) { }
    protected void OnEntityPlay(Entity entity) { }
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
    public abstract void ModifyDamage(Entity entity, ref Damage damage);
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
    public abstract void ModifyAmount(ref int amount);

    /// <summary>
    /// Determines if the effect causes the entity to keep it's shield
    /// </summary>
    public void ShouldKeepShield(ref bool keepShield) => keepShield |= false;
}

public interface IAttackedEffect : IEffect
{
    public void OnPreAttacked(Entity source, Entity attacker) { }
    public void OnPostAttacked(Entity source, Entity attacker);

}

public interface IHealthEffect : IEffect
{
    public void ModifyAmount(ref int amount);
}

public interface IManaGainEffect : IEffect
{
    public void OnManaGained(ref uint amount);
}

public interface IBlockCardEffect : IEffect
{
    public virtual void OnPlay(ref bool blocked, HeroCard card) { }
    public virtual void OnDraw(ref bool blocked, HeroCard card) { }
}

class OverchagedEffect : Effect, IManaGainEffect
{
    protected override string Name => "Overcharged";
    protected override string? Image => UIManager.OverchargedIcon;

    public void OnManaGained(ref uint amount) { amount = 0; }
}

class NoPrimaryEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Primary";
    protected override string? Image => VanillaSprites.PrimaryMonkeyIcon;
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Primary;
}

class NoMagicEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Magic";
    protected override string? Image => throw new NotImplementedException();
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Magic;
}

class NoMilitaryEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Military";
    protected override string? Image => throw new NotImplementedException();
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Military;
}

class NoSupportEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Support";
    protected override string? Image => throw new NotImplementedException();
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Support;
}

class NoHeroEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Hero";
    protected override string? Image => throw new NotImplementedException();
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Hero;
}

class NoItemsEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Items";
    protected override string? Image => throw new NotImplementedException();
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Items;
}

class NoParagoncEffect : Effect, IBlockCardEffect
{
    protected override string Name => "No Paragon";
    protected override string? Image => throw new NotImplementedException();
    protected override bool ShowLevel => false;

    public void OnPlay(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Paragon;
}

class BlockPrimaryEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Primary";
    protected override string? Image => VanillaSprites.PrimaryKnowledgeBtn;

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Primary;
}

class BlockMagicEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Magic";
    protected override string? Image => throw new NotImplementedException();

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Magic;
}

class BlockMilitaryEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Military";
    protected override string? Image => throw new NotImplementedException();

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Military;
}

class BlockSupportEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Support";
    protected override string? Image => throw new NotImplementedException();

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Support;
}

class BlockHeroEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Hero";
    protected override string? Image => throw new NotImplementedException();

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Hero;
}
class BlockParagonEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Paragon";
    protected override string? Image => throw new NotImplementedException();

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Paragon;
}

class BlockItemsEffect : Effect, IBlockCardEffect
{
    protected override string Name => "Block Items";
    protected override string? Image => throw new NotImplementedException();

    void IBlockCardEffect.OnDraw(ref bool blocked, HeroCard card) => blocked |= card.Type == Il2CppAssets.Scripts.Models.TowerSets.TowerSet.Items;
}