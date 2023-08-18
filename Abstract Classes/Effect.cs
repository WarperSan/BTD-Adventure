using BTD_Mod_Helper.Extensions;
using BTDAdventure.Entities;
using BTDAdventure.Managers;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Abstract_Classes;

/// <summary>
/// Defines what any effect needs.
/// </summary>
public abstract class Effect
{
    private GameObject? root;

    public abstract string DisplayName { get; }

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
                return;
            }
        }

        UpdateLevelText();
    }

    /// <summary>
    /// Called whenever the effect gets removed
    /// </summary>
    internal void RemoveEffect()
    {
        if (root == null)
            return;

        GameObject.Destroy(root);
    }
}

/// <summary>
/// Defines all the effects that modify the damage output
/// </summary>
public interface IAttackEffect
{
    public abstract Damage ModifyDamage(Entity entity, Damage damage);
}

public interface IShieldEffect
{
    public abstract int ModifyAmount(int amount);

    public bool ShouldKeepShield() => false;
}