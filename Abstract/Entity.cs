using BTDAdventure.Effects.Interfaces;
using BTDAdventure.Entities;
using BTDAdventure.Managers;
using Il2Cpp;
using Il2CppNinjaKiwi.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Abstract;

public abstract class Entity
{
    public Entity(GameObject @object)
    {
        SetUpHealthUI(@object);
        SetUpShieldUI(@object);
        SetUpEffectUI(@object);
        SetUpExtraUI(@object);
    }

    #region Health

    /// <summary>
    /// The amount of HP remaining for this entity.
    /// </summary>
    protected int Health;

    /// <summary>
    /// Is this entity still alive ?
    /// </summary>
    public bool IsAlive => Health > 0;

    // --- Health Modification ---
    /// <inheritdoc cref="ModifyHealth(int, bool, Entity?)"/>
    protected int RemoveHealth(int amount, Entity? source) => ModifyHealth(amount, true, source);

    /// <inheritdoc cref="ModifyHealth(int, bool, Entity?)"/>
    protected int AddHealth(int amount, Entity? source) => ModifyHealth(amount, false, source);

    /// <summary>
    /// Modifies the current amount of health by <paramref name="amount"/> by <paramref name="source"/>.
    /// </summary>
    private int ModifyHealth(int amount, bool isRemoving, Entity? source)
    {
        OnHealthModify?.Invoke(ref amount);

        if (isRemoving)
        {
            if (source != null)
                SoundManager.PlaySound(SoundManager.SOUND_ATTACK, SoundManager.GeneralGroup);

            Health -= amount;
        }
        else
        {
            int newHealth = Health + amount;

            Health = Settings.GetSettingValue(Settings.SETTING_CAP_HEALTH_GAIN, true) ?
                Math.Min(Health + amount, MaxHealth) :
                newHealth;
        }

        UpdateHealthUI();

        if (Health <= 0)
        {
            OnHealthDeath?.Invoke();
            OnDeath();
        }
        return amount;
    }

    /// <summary>
    /// Deals an attack defined by <paramref name="damage"/> from <paramref name="source"/> to this entity.
    /// </summary>
    public int ReceiveDamage(Entity? source, Damage damage)
    {
        int damageAmount = 0;

        // Check for own skills to modify damage
        // Check for own ability to modify damage

        if (source != null)
            OnPreAttacked?.Invoke(this, source);

        if (damage.IgnoresShield)
            damageAmount = RemoveHealth(damage.Amount, source);
        else
            damageAmount = RemoveShield(Mathf.FloorToInt(damage.Amount * damage.ShieldMultiplier), source);

        if (source != null)
            OnPostAttacked?.Invoke(this, source);

        return damageAmount;
    }

    public void ReceiveDamage(Entity? source, int amount) => ReceiveDamage(source, new Damage(amount));

    // --- Health UI ---
    /// <summary>
    /// Text used to display the current health and the maximum health of this entity.
    /// </summary>
    protected NK_TextMeshProUGUI? HealthText;

    /// <summary>
    /// Image used to display the health icon.
    /// </summary>
    protected GameObject? HealthImg;

    /// <summary>
    /// Updates the health UI of the entity.
    /// </summary>
    protected void UpdateHealthUI()
    {
        int displayedHealth =
            Settings.GetSettingValue(Settings.SETTING_SHOW_NEGATIVE_OVERFLOW, true) ?
            Health :
            Math.Max(Health, 0);

        HealthText?.UpdateText(displayedHealth + " / " + MaxHealth);

        UpdateExtraHealthUI();
    }

    /// <summary>
    /// Called whenever the health UI gets updated.
    /// </summary>
    protected virtual void UpdateExtraHealthUI()
    { }

    /// <summary>
    /// Called in order to set every element for the health UI.
    /// </summary>
    protected virtual void SetUpHealthUI(GameObject root)
    { }

    // --- Health Events ---
    private delegate void HealthModifyEvent(ref int amount);
    private delegate void HealthDeathEvent();

    private event HealthModifyEvent? OnHealthModify;
    private event HealthDeathEvent? OnHealthDeath;

    /// <summary>
    /// Called whenever this entity dies.
    /// </summary>
    protected virtual void OnDeath()
    { }

    #endregion

    #region Max Health

    /// <summary>
    /// The maximum amount of HP this entity can have.
    /// </summary>
    protected int MaxHealth = 10;

    /// <summary>
    /// Determines if, when this entity increases its maximum HP, its current HP should scale up proportionally.
    /// </summary>
    protected virtual bool ScaleUpMaxHP => false;

    // --- Max HP Modification ---
    /// <summary>
    /// Modifies the current amount of maximum health by <paramref name="amount"/> by <paramref name="source"/>.
    /// </summary>
    private void ModifyMaxHealth(int amount, bool isRemoving, Entity? source)
    {
        if (isRemoving)
        {
            MaxHealth = Math.Max(MaxHealth + amount, 0);

            if (Health > MaxHealth)
                ModifyHealth(Health - MaxHealth, true, null);

            UpdateHealthUI();
        }
        else
        {
            int oldMH = MaxHealth;

            MaxHealth += amount;

            // Scale up
            if (ScaleUpMaxHP)
                AddHealth(Mathf.CeilToInt(MaxHealth * Health / (float)oldMH), this);
        }
    }

    #endregion

    #region Shield

    /// <summary>
    /// The amount of shield remaining for this entity.
    /// </summary>
    private int Shield;

    protected int BaseShield = 1;

    internal int GetNextShield()
    {
        int amount = BaseShield;

        // Apply skills
        OnShieldModify?.Invoke(ref amount);

        return amount;
    }

    // --- Shield UI ---
    protected NK_TextMeshProUGUI? ShieldText;
    protected GameObject? ShieldImg;

    /// <summary>
    /// Updates the shield UI of the entity.
    /// </summary>
    protected void UpdateShieldText()
    {
        bool showShield = Shield > 0;

        ShieldImg?.SetActive(showShield);
        ShieldText?.gameObject.SetActive(showShield);

        HealthImg?.SetActive(!showShield);

        ShieldText?.UpdateText(Shield);
    }

    /// <summary>
    /// Called in order to set every element for the shield UI.
    /// </summary>
    protected virtual void SetUpShieldUI(GameObject root)
    { }

    // --- Shield Modification ---
    /// <inheritdoc cref="ModifyShield(int, bool, Entity?)"/>
    internal void AddShield(int amount, Entity? source) => ModifyShield(amount, false, source);

    /// <inheritdoc cref="ModifyShield(int, bool, Entity?)"/>
    internal int RemoveShield(int amount, Entity? source) => ModifyShield(amount, true, source);

    internal void RemoveShield(Entity? source) => RemoveShield(Shield, source);

    /// <summary>
    /// Modifies the current shield by <paramref name="amount"/> by <paramref name="source"/>.
    /// </summary>
    private int ModifyShield(int amount, bool isRemoving, Entity? source)
    {
        int damageAmount = 0;

        if (isRemoving)
        {
            if (amount > Shield)
            {
                damageAmount = RemoveHealth(amount - Shield, source);
                Shield = 0;

                OnShieldClear?.Invoke(true);
            }
            else
                Shield -= amount;
        }
        else
            Shield += amount;

        UpdateShieldText();

        return damageAmount;
    }

    /// <summary>
    /// Called whenever this entity should clear its shield at the start of its turn.
    /// </summary>
    private void ClearShield()
    {
        if (ShouldKeepShield())
            return;

        Shield = 0;
        UpdateShieldText();

        OnShieldClear?.Invoke(false);
    }

    /// <returns>Should this entity keep its shield when its turn starts ?</returns>
    private bool ShouldKeepShield()
    {
        bool result = false;

        OnShieldKeep?.Invoke(ref result);

        return result;
    }

    // --- Shield Events ---
    private delegate void ShieldKeepEvent(ref bool keepShield);
    private delegate void ShieldModifyEvent(ref int amount);
    private delegate void ShieldClearEvent(bool clearFromEntity);

    private event ShieldKeepEvent? OnShieldKeep;
    private event ShieldModifyEvent? OnShieldModify;
    private event ShieldClearEvent? OnShieldClear;

    #endregion

    #region Attack

    protected int? Damage;

    /// <returns>The amount of damage that an attack by this entity would deal.</returns>
    internal int CalculateDamage(int? amount = null)
    {
        Damage damage = new(amount ?? Damage ?? 0);

        if (this is not PlayerEntity)
            damage.Amount += GameManager.Instance.FightDifficulty;

        // Apply skills

        // Apply effects
        OnDamageModify?.Invoke(this, ref damage);

        return damage.Amount;
    }

    // Normal attack => AttackTarget(Entity)
    // Thorns attack => AttackTarget(int, Entity)
    // % attack => AttackTarget(Damage, Entity)

    /// <summary>
    /// Attacks <paramref name="target"/> with a default attack.
    /// </summary>
    public void AttackTarget(Entity target) => AttackTarget(this.CalculateDamage(), target);

    /// <summary>
    /// Attacks <paramref name="target"/> with a default attack dealing <paramref name="amount"/> damage.
    /// </summary>
    /// <remarks>
    /// This attack will deal fixed amount of damage.
    /// </remarks>
    public void AttackTarget(int amount, Entity target) => AttackTarget(new Damage(amount), target);

    /// <summary>
    /// Attacks <paramref name="target"/> with <paramref name="damage"/>.
    /// </summary>
    public int AttackTarget(Damage damage, Entity target) => target.ReceiveDamage(this, damage);

    // --- Attack Events ---
    private delegate void AttackedEvent(Entity source, Entity attacker);
    private delegate void DamageModifyEvent(Entity entity, ref Damage amount);

    private event AttackedEvent? OnPreAttacked;
    private event AttackedEvent? OnPostAttacked;
    private event DamageModifyEvent? OnDamageModify;

    #endregion

    #region Effects

    private readonly List<Effect> _effects = new();

    // --- Effect UI ---
    protected GameObject? EffectHolder;

    protected virtual void SetUpEffectUI(GameObject root)
    { }

    // --- Effect Getter ---
    private Effect? GetEffect<T>() where T : Effect
    {
        foreach (var item in _effects)
        {
            if (item is T)
                return item;
        }
        return null;
    }

    /// <returns><see cref="Effect.Level"/> of the effect</returns>
    public int GetEffectLevel<T>() where T : Effect => GetEffect<T>()?.Level ?? 0;

    // --- Effect Addition ---
    private Effect GetOrCreateEffect<T>() where T : Effect
    {
        Effect? effect = GetEffect<T>();

        if (effect != null)
            return effect;

        effect = Activator.CreateInstance<T>();

        if (effect == null)
            throw new NullReferenceException();

        GameObject? effectObject = EffectHolder != null ? UIManager.GetEffectSlot(EffectHolder.transform) : null;

        effect.SetUpUI(this, effectObject);

        Subscribe(effect);
        _effects.Add(effect);

        OnEffectUpdate(effect);
        return effect;
    }

    /// <summary>
    /// Adds the <paramref name="amount"/> to <see cref="Effect.Level"/> of <paramref name="effect"/>.
    /// </summary>
    /// <returns>The amount of levels of the effect after the modification.</returns>
    private int AddLevel(Effect effect, int amount)
    {
        effect.Level += amount;

        // Update level
        effect.UpdateLevelText();

        OnEffectUpdate(effect);

        return effect.Level;
    }

    /// <summary>
    /// Adds <paramref name="amount"/> levels to the effect of type <typeparamref name="T"/>.
    /// </summary>
    /// <inheritdoc cref="AddLevel(Effect, int)"/>
    public int AddLevel<T>(int amount) where T : Effect => AddLevel(GetOrCreateEffect<T>(), amount);

    /// <summary>
    /// Adds the <paramref name="amount"/> to the <see cref="Effect.LowestLevel"/> of <paramref name="effect"/>.
    /// </summary>
    /// <returns>The amount of permanent levels of the effect after the modification.</returns>
    private int AddPermanentLevel(Effect effect, int amount)
    {
        effect.LowestLevel += amount;

        if (effect.Level < effect.LowestLevel)
            AddLevel(effect, effect.LowestLevel - effect.Level);

        return effect.LowestLevel;
    }

    /// <summary>
    /// Adds <paramref name="amount"/> permanent levels to the effect of type <typeparamref name="T"/>.
    /// </summary>
    /// <inheritdoc cref="AddPermanentLevel(Effect, int)"/>
    public int AddPermanentLevel<T>(int amount) where T : Effect => AddPermanentLevel(GetOrCreateEffect<T>(), amount);

    // --- Effect Reduction ---
    /// <summary>
    /// Updates the counter of all effects and removes effects if necessary
    /// </summary>
    internal void UpdateEffects()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
            _effects[i].UpdateEffect(this);
    }

    /// <summary>
    /// Removes the given effect
    /// </summary>
    /// <returns><inheritdoc cref="List{T}.Remove(T)"/></returns>
    public bool RemoveEffect(Effect effect)
    {
        Unsubscribe(effect);
        OnEffectUpdate(effect);
        return _effects.Remove(effect);
    }

    /// <summary>
    /// Removes the effect of the give type
    /// </summary>
    /// <returns>Removed effect</returns>
    public Effect? RemoveEffect<T>() where T : Effect
    {
        Effect? effect = GetEffect<T>();

        if (effect != null)
            RemoveEffect(effect);
        return effect;
    }

    /// <summary>
    /// Removes all effects
    /// </summary>
    public Effect[] RemoveAllEffects() => RemoveAllMatching(new Predicate<Effect>(_ => true));

    /// <summary>
    /// Removes all effects matching the given condition
    /// </summary>
    public Effect[] RemoveAllMatching(Predicate<Effect> condition)
    {
        List<Effect> effectsRemoved = new();

        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var effect = _effects[i];

            if (condition.Invoke(effect))
            {
                effectsRemoved.Add(effect);
                effect.RemoveEffect();
                this.RemoveEffect(effect);
            }
        }
        return effectsRemoved.ToArray();
    }

    // --- Effect Events ---
    /// <summary>
    /// Called whenever an effect of this entity is updated.
    /// </summary>
    protected virtual void OnEffectUpdate(Effect effect)
    { }

    #endregion
    
    #region Visuals

    protected Transform? EffectVisual;

    internal void PlayEffectVisual(string assetName)
    {
        if (!Settings.GetSettingValue(Settings.SETTING_SHOW_VISUALS, true))
            return;

        PlayEffectVisual(LoadAsset<GameObject>(assetName));
    }

    /// <summary>
    /// Instantiates <paramref name="effect"/>
    /// </summary>
    public void PlayEffectVisual(GameObject? effect)
    {
        if (!Settings.GetSettingValue(Settings.SETTING_SHOW_VISUALS, true))
            return;

        if (effect == null || EffectVisual == null)
            return;

        GameObject.Instantiate(effect, EffectVisual);
    }

    public void DestroyAllVisual()
    {
        EffectVisual?.DestroyAllChildren();
    }

    #endregion

    protected virtual void SetUpExtraUI(GameObject root)
    { }

    #region Turn Event

    private delegate void TurnEvent(Entity entity);

    private event TurnEvent? OnPreTurn;
    private event TurnEvent? OnPostTurn;

    internal void PreTurn()
    {
        this.ClearShield();
        OnPreTurn?.Invoke(this);
    }

    internal void PostTurn()
    {
        this.UpdateEffects();
        OnPostTurn?.Invoke(this);
    }

    #endregion Turn Event

    #region Action Event

    private delegate void ActionEvent(Entity source, HeroCard? card);

    private event ActionEvent? OnPreAction;
    private event ActionEvent? OnPostAction;

    internal void PreAction(HeroCard? card) => OnPreAction?.Invoke(this, card);
    internal void PostAction(HeroCard? card) => OnPostAction?.Invoke(this, card);

    #endregion Action Event

    private void Subscribe(Effect effect)
    {
        if (effect is IShieldEffect shieldEffect)
        {
            OnShieldKeep += shieldEffect.ShouldKeepShield;
            OnShieldModify += shieldEffect.ModifyAmount;
        }

        if (effect is IAttackedEffect attackedEffect)
        {
            OnPreAttacked += attackedEffect.OnPreAttacked;
            OnPostAttacked += attackedEffect.OnPostAttacked;
        }

        if (effect is IAttackEffect attackEffect)
        {
            OnDamageModify += attackEffect.ModifyDamage;
        }

        if (effect is ITurnEffect turnEffect)
        {
            OnPreTurn += turnEffect.OnPreTurn;
            OnPostTurn += turnEffect.OnPostTurn;
        }

        if (effect is IActionEffect actionEffect)
        {
            OnPreAction += actionEffect.OnAction;
            OnPostAction += actionEffect.OnAction;
        }

        if (effect is IHealthEffect healthEffect)
        {
            OnHealthModify += healthEffect.ModifyAmount;
        }

        ChildrenSubscribe(effect);
    }

    private void Unsubscribe(Effect effect)
    {
        if (effect is IShieldEffect shieldEffect)
        {
            OnShieldKeep -= shieldEffect.ShouldKeepShield;
            OnShieldModify -= shieldEffect.ModifyAmount;
        }

        if (effect is IAttackedEffect attackedEffect)
        {
            OnPreAttacked -= attackedEffect.OnPreAttacked;
            OnPostAttacked -= attackedEffect.OnPostAttacked;
        }

        if (effect is IAttackEffect attackEffect)
        {
            OnDamageModify -= attackEffect.ModifyDamage;
        }

        if (effect is ITurnEffect turnEffect)
        {
            OnPreTurn -= turnEffect.OnPreTurn;
            OnPostTurn -= turnEffect.OnPostTurn;
        }

        if (effect is IActionEffect actionEffect)
        {
            OnPreAction -= actionEffect.OnAction;
            OnPostAction -= actionEffect.OnAction;
        }

        if (effect is IHealthEffect healthEffect)
        {
            OnHealthModify -= healthEffect.ModifyAmount;
        }

        ChildrenUnsubscribe(effect);
    }

    protected virtual void ChildrenSubscribe(Effect effect)
    { }

    protected virtual void ChildrenUnsubscribe(Effect effect)
    { }
}