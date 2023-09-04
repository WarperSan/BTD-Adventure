﻿using BTDAdventure.Entities;
using BTDAdventure.Managers;
using Il2Cpp;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BTDAdventure.Abstract;

public abstract class Entity
{
    public Entity(GameObject? @object)
    {
        if (@object != null)
        {
            SetUpHealthUI(@object);
            SetUpShieldUI(@object);
            SetUpEffectUI(@object);
            SetUpExtraUI(@object);
        }
    }

    #region Health
    /// <summary>
    /// Amount of HP remaining
    /// </summary>
    protected int Health;

    /// <summary>
    /// Max amount of HP that the entity can have
    /// </summary>
    protected int MaxHealth = 10;

    protected NK_TextMeshProUGUI? HealthText;
    protected GameObject? HealthImg;

    public bool IsAlive => Health > 0;

    /// <summary>
    /// Called whenever the health UI needs an update
    /// </summary>
    protected void UpdateHealthUI()
    {
        HealthText?.UpdateText(Health + " / " + MaxHealth);

        UpdateExtraHealthUI();
    }

    public void ReceiveDamage(Entity? source, Damage damage)
    {
        // Check for own skills to modify damage
        // Check for own ability to modify damage

        if (source != null)
        {
            OnPreAttacked?.Invoke(this, source);
        }

        if (damage.IgnoresShield)
        {
            RemoveHealth(damage.Amount);
        }
        else
        {
            ShieldDamage(damage);
        }

        if (source != null)
        {
            OnPostAttacked?.Invoke(this, source);
        }
    }
    public void ReceiveDamage(Entity? source, int amount) => ReceiveDamage(source, new Damage(amount));

    protected void RemoveHealth(int amount) => ModifyHealth(amount, true);
    protected void AddHealth(int amount) => ModifyHealth(amount, false);
    private void ModifyHealth(int amount, bool isRemoving)
    {
        //this.ExecuteOnEffect(new Func<IHealthEffect, bool>((effect) => { amount = effect.ModifyAmount(amount); }));

        if (isRemoving)
        {
            Health -= amount;
        }
        else
        {
            Health = Math.Min(Health + amount, MaxHealth);
        }

        UpdateHealthUI();

        if (Health <= 0)
        {
            OnDeath();
        }
    }

    protected virtual bool ScaleUpMaxHP { get; } = false;

    private void ModifyMaxHealth(int amount, bool isRemoving)
    {
        if (isRemoving)
        {
            MaxHealth = Math.Max(MaxHealth + amount, 0);

            if (Health > MaxHealth)
                ModifyHealth(Health - MaxHealth, true);

            UpdateHealthUI();
        }
        else
        {
            int oldMH = MaxHealth;

            MaxHealth += amount;

            // Scale up
            if (ScaleUpMaxHP)
                AddHealth(Mathf.CeilToInt(MaxHealth * Health / (float)oldMH));
        }
    }

    /// <summary>
    /// Called whenever the entity dies.
    /// </summary>
    protected virtual void OnDeath() { }

    protected virtual void SetUpHealthUI(GameObject root) { }
    protected virtual void UpdateExtraHealthUI() { }
    #endregion

    #region Shield
    private int Shield;

    protected int BaseShield = 1;

    protected NK_TextMeshProUGUI? ShieldText;
    protected GameObject? ShieldImg;

    private void ShieldDamage(Damage damage)
    {
        if (Shield < damage.Amount)
        {
            RemoveHealth(damage.Amount - Shield);
            ModifyShield(Shield, true); // Set to 0
        }
        else
            ModifyShield(damage.Amount, true);
    }

    public void AddShield(int amount) => ModifyShield(amount, false);
    public void RemoveShield(int amount) => ModifyShield(amount, true);
    public void RemoveShield() => RemoveShield(Shield);

    private void ModifyShield(int amount, bool isRemoving)
    {
        if (isRemoving)
        {
            if (amount > Shield)
            {
                ModifyHealth(amount - Shield, true);
                Shield = 0;
            }
            else
                Shield -= amount;
        }
        else
        {
            Shield += amount;
        }

        UpdateShieldText();
    }

    /// <summary>
    /// Updates the shield UI of the entity
    /// </summary>
    public void UpdateShieldText()
    {
        bool showShield = Shield > 0;

        ShieldImg?.SetActive(showShield);
        //ShieldText?.gameObject.SetActive(showShield);

        HealthImg?.SetActive(!showShield);

        ShieldText?.UpdateText(Shield);
    }

    internal void ClearShield()
    {
        if (ShouldKeepShield())
            return;

        Shield = 0;
        UpdateShieldText();
    }

    protected virtual void SetUpShieldUI(GameObject root) { }

    internal int GetShield()
    {
        int amount = BaseShield;

        // Apply skills
        OnShieldModify?.Invoke(ref amount);

        return amount;
    }

    private bool ShouldKeepShield()
    {
        bool result = false;

        OnShieldKeep?.Invoke(ref result);

        return result;
    }
    #endregion

    protected virtual void SetUpExtraUI(GameObject root) { }

    #region Attack
    protected int? Damage;

    /// <returns>Amount of damage that an attack would deal</returns>
    internal int GetAttack(int? amount = null)
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
    // Double attack => AttackTarget(int, Entity)
    // % attack => AttackTarget(Damage, Entity)

    /// <summary>
    /// Attacks <paramref name="target"/> with a default attack dealing <see cref="GetAttack()"/> damage.
    /// </summary>
    public void AttackTarget(Entity? target) => AttackTarget(this.GetAttack(), target);

    /// <summary>
    /// Attacks <paramref name="target"/> with a default attack dealing <paramref name="amount"/> damage.
    /// </summary>
    public void AttackTarget(int amount, Entity? target) => AttackTarget(new Damage(amount), target);

    /// <summary>
    /// Attacks <paramref name="target"/> with the attack <paramref name="damage"/>.
    /// </summary>
    public virtual void AttackTarget(Damage damage, Entity? target) => target?.ReceiveDamage(this, damage);
    #endregion

    #region Effects
    internal readonly List<Effect> _effects = new();

    protected GameObject? EffectHolder;
    internal Transform? EffectVisual;

    /// <summary>
    /// Updates the counter of all effects and removes effects if necessary
    /// </summary>
    internal void UpdateEffects()
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            _effects[i].UpdateEffect(this);
        }
    }

    /// <summary>
    /// Checks if the entity has the given effect
    /// </summary>
    public bool HasEffect<T>() where T : Effect => GetEffect<T>() != null;

    /// <returns><see cref="Effect.Level"/> of the effect</returns>
    public int GetEffectLevel<T>() where T : Effect => GetEffect<T>()?.Level ?? 0;

    private Effect? GetEffect<T>() where T : Effect
    {
        foreach (var item in _effects)
        {
            if (item is T)
                return item;
        }
        return null;
    }

    #region Add
    private Effect GetOrCreateEffect<T>() where T : Effect
    {
        Effect? effect = GetEffect<T>();

        if (effect == null)
        {
            effect = Activator.CreateInstance<T>();

            if (effect == null)
                throw new NullReferenceException();

            GameObject? effectObject = EffectHolder != null ?
                GameObject.Instantiate(GameManager.Instance.EffectSlotPrefab, EffectHolder.transform) :
                null;

            effect.SetUpUI(this, effectObject);

            Subscribe(effect);

            _effects.Add(effect);
            OnEffectAdded<T>();
        }
        return effect;
    }

    /// <summary>
    /// Adds the <paramref name="amount"/> to <see cref="Effect.Level"/> of <paramref name="effect"/>.
    /// </summary>
    /// <returns>New <see cref="Effect.Level"/> of <paramref name="effect"/>.</returns>
    private static int AddLevel(Effect effect, int amount)
    {
        effect.Level += amount;

        // Update level
        effect.UpdateLevelText();

        return effect.Level;
    }

    /// <summary>
    /// Adds the <paramref name="amount"/> to the <see cref="Effect.LowestLevel"/> of <paramref name="effect"/>.
    /// </summary>
    /// <returns>New <see cref="Effect.LowestLevel"/> of <paramref name="effect"/>.</returns>
    private static int AddPermanentLevel(Effect effect, int amount)
    {
        effect.LowestLevel += amount;

        if (effect.Level < effect.LowestLevel)
            AddLevel(effect, effect.LowestLevel - effect.Level);

        return effect.LowestLevel;
    }
    #endregion

    #region Remove
    /// <summary>
    /// Removes the given effect
    /// </summary>
    /// <returns><inheritdoc cref="List{T}.Remove(T)"/></returns>
    public bool RemoveEffect(Effect effect)
    {
        Unsubscribe(effect);
        return _effects.Remove(effect);
    }

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
    #endregion

    #region Variants
    /// <summary>
    /// Removes the effect of the give type
    /// </summary>
    /// <returns></returns>
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

    public int AddLevel<T>(int amount) where T : Effect => AddLevel(GetOrCreateEffect<T>(), amount);
    public int AddPermanentLevel<T>(int amount) where T : Effect => AddPermanentLevel(GetOrCreateEffect<T>(), amount);
    #endregion

    internal void PlayEffectVisual(string assetName) => PlayEffectVisual(LoadAsset<GameObject>(assetName));

    /// <summary>
    /// Instantiates <paramref name="effect"/>
    /// </summary>
    public void PlayEffectVisual(GameObject? effect)
    {
        if (effect != null && EffectVisual != null)
        {
            GameObject.Instantiate(effect, EffectVisual);
        }
    }

    protected virtual void SetUpEffectUI(GameObject root) { }
    protected virtual void OnEffectAdded<T>() where T : Effect { }
    #endregion

    #region Events
    private delegate void ShieldKeepEvent(ref bool keepShield);
    private event ShieldKeepEvent OnShieldKeep;

    private delegate void ShieldModifyEvent(ref int amount);
    private event ShieldModifyEvent OnShieldModify;

    private delegate void AttackedEvent(Entity source, Entity attacker);
    private event AttackedEvent OnPreAttacked;
    private event AttackedEvent OnPostAttacked;

    private delegate void DamageModifyEvent(Entity entity, ref Damage amount);
    private event DamageModifyEvent OnDamageModify;

    #region Turn Event
    private delegate void TurnEvent(Entity entity);
    private event TurnEvent OnPreTurn;
    internal void PreTurn()
    {
        this.ClearShield();
        OnPreTurn?.Invoke(this);
    }

    private event TurnEvent OnPostTurn;
    internal void PostTurn() => OnPostTurn?.Invoke(this);
    #endregion

    #region Action Event
    private delegate void ActionEvent(Entity source, HeroCard? card);
    private event ActionEvent OnPreAction;
    internal void PreAction(HeroCard? card = null) => OnPreAction?.Invoke(this, card);

    private event ActionEvent OnPostAction;
    internal void PostAction(HeroCard? card = null) => OnPostAction?.Invoke(this, card);
    #endregion

    private delegate void HealthModifyEvent(ref int amount);
    private event HealthModifyEvent OnHealthModify;

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

    protected virtual void ChildrenSubscribe(Effect effect) { }

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

    protected virtual void ChildrenUnsubscribe(Effect effect) { }
    #endregion
}