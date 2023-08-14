using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Enemy_Actions;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Cards.EnemyCards;

public abstract class EnemyCard
{
    private readonly GameObject? uiObject;

    public EnemyCard(
        GameObject? enemyObject,
        int position,
        Vector2? portraitSize = null,
        params string[] actions)
    {
        uiObject = enemyObject;

        Position = position;

        #region UI
        if (uiObject != null)
        {
            #region Portrait
            Transform _enemyPortraitTransform = uiObject.transform.Find("Display/Portrait");
            _enemyPortraitTransform.localScale = portraitSize ?? Vector2.one * 2; // Auto-scaling ?

            EnemyPortrait = _enemyPortraitTransform.GetComponent<Image>();

            SetPortrait(Portrait);
            #endregion

            #region Background
            string? bgGUID = GetBackgroundGUID();

            if (!string.IsNullOrEmpty(bgGUID))
                uiObject.transform.Find("Display/Background")?.GetComponent<Image>().SetSprite(GetBackgroundGUID());
            #endregion

            #region Set Up UIs
            SetUpIntentUI();
            SetUpHealthUI();
            #endregion

            #region Health
            MaxHealth = DefaultMaxHP;
            Health = MaxHealth;
            UpdateHealthUI();
            #endregion

            #region Button
            uiObject.GetComponent<Button>().onClick.SetListener(new Function(() =>
            {
                Instance.SelectEnemy(Position);
            }));
            #endregion


        }
        else
            Log("Enemy object was not given.");
        #endregion

        // BG

        Intents = actions;
    }

    //public int blind;
    //public int block;
    //public int curse;
    //public TextView enemyDamage;
    //public int frail;
    //public int heal;
    //public int id;
    //public string innate;
    //public int innateValue;
    public bool isOriginal;

    //public string name;
    //public String sequences;
    //public int silence;
    public int weakness;
    //public String world;
    //public int wound;

    private readonly int Position;

    public readonly uint coinsGiven = 2;

    #region Health
    protected abstract int DefaultMaxHP { get; }

    private int Health;
    private int MaxHealth;

    private NK_TextMeshProUGUI? HealthText;
    private Slider? HealthSlider;

    private void UpdateHealthUI()
    {
        if (HealthText != null)
        {
            HealthText.text = Health + " / " + MaxHealth;
        }

        if (HealthSlider != null)
        {
            HealthSlider.maxValue = MaxHealth;
            HealthSlider.minValue = Math.Min(HealthSlider.minValue, Health);
            HealthSlider.value = Health;
        }
    }

    protected virtual void SetUpHealthUI()
    {
        if (uiObject == null)
        {
            Log("UI Object was not defined.");
            return;
        }

        Transform? hpSliderT = uiObject.transform.Find("HP/HealthBar");

        if (hpSliderT != null)
            HealthSlider = hpSliderT.GetComponent<Slider>();
        else
            Log("Health Slider was not found.");

        Transform? hpTextT = uiObject.transform.Find("HP/TextHolder/Text");

        if (hpTextT != null)
            HealthText = hpTextT.gameObject.GetComponent<NK_TextMeshProUGUI>();
        else
            Log("Health Text was not found.");
    }

    public void ReceiveDamage(Damage damage)
    {
        // Check for own skills to modify damage
        // Check for own ability to modify damage

        if (damage.IgnoresShield)
        {
            RemoveHealth(damage.Amount);
        }
        else
        {
            ShieldDamage(damage);
        }
    }

    private void RemoveHealth(int amount) => ModifyHealth(amount, true);

    private void ModifyHealth(int amount, bool isRemoving)
    {
        if (isRemoving)
        {
            amount = OnHealthDamage(amount);
            Health -= amount;
        }
        else
        {
            amount = OnHealthGain(amount);
            Health = Math.Min(Health + amount, MaxHealth);
        }

        UpdateHealthUI();

        if (Health <= 0)
        {
            Instance.KillEnemy(Position); // they kill themselves :(
        }
    }

    /// <summary>
    /// Called whenever the enemy receives direct damage.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    protected virtual int OnHealthDamage(int amount) => amount;

    protected virtual int OnHealthGain(int amount) => amount;
    #endregion

    #region Shield
    private int Shield;

    private void ShieldDamage(Damage damage)
    {
        OnShieldDamage(ref damage);

        if (Shield < damage.Amount)
        {
            RemoveHealth(damage.Amount - Shield);
            Shield = 0;
        }
        else
            Shield -= damage.Amount;
    }

    /// <summary>
    /// Called whenever the enemy receives a damage that will change its shield.
    /// </summary>
    /// <param name="damage"></param>
    protected virtual void OnShieldDamage(ref Damage damage) { }
    #endregion

    #region Intent
    private HorizontalLayoutGroup? _intentContainer;
    private Image? _intentIcon;
    private NK_TextMeshProUGUI? _intentText;

    private readonly string[]? Intents;

    public string? Intent { get; private set; } = null;
    private int _currentIndex = -1;

    public void MoveIntent()
    {
        if (Intents == null)
        {
            Log("No intents defined.");
            return;
        }

        _currentIndex++;

        if (_currentIndex >= Intents.Length)
            _currentIndex -= Intents.Length;

        SetIntent(Intents[_currentIndex]);
        UpdateIntent();
    }

    public void SetIntent(string? value)
    {
        Intent = value;
    }

    protected virtual void SetUpIntentUI()
    {
        if (uiObject == null) // To make VS happy
        {
            Log("UI Object was not defined.");
            return;
        }

        Transform? intentContainerT = uiObject.transform.Find("Intent");

        if (intentContainerT != null)
            _intentContainer = intentContainerT.GetComponent<HorizontalLayoutGroup>();
        else
            Log("Intent container was not found.");

        Transform? intentIconT = uiObject.transform.Find("Intent/IconHolder/Icon");

        if (intentIconT != null)
            _intentIcon = intentIconT.GetComponent<Image>();
        else
            Log("Intent Icon was not found.");

        Transform? intentTextT = uiObject.transform.Find("Intent/Text");

        if (intentTextT != null)
        {
            _intentText = intentTextT.gameObject.GetComponent<NK_TextMeshProUGUI>();
        }
        else
            Log("Intent Text was not found.");
    }

    private void UpdateIntent()
    {
        EnemyAction action = Instance.GetIntentAction(Intent) ?? new EmptyAction();

        if (Intent == PlaceHolder)
        {
            // typeof(this) will give EnemyCard, not Red or Blue (e.g.)
            Log($"A placeholder action was found in the enemy card with the portrait \'{this.Portrait}\'.");
        }

        int containerLeftPadding = 30;

        if (_intentText != null)
        {
            string? text = action.GetText(this);
            _intentText.text = text;
            _intentText.gameObject.SetActive(!string.IsNullOrEmpty(text));

            if (!_intentText.gameObject.activeInHierarchy)
                containerLeftPadding = 0;
        }
        else
            Log("Intent Text was not found.");

        if (_intentContainer != null)
            _intentContainer.padding.left = containerLeftPadding;
        else
            Log("Intent container was not found.");

        if (_intentIcon != null)
            _intentIcon.SetSprite(action.Icon);
        else
            Log("Intent Icon was not found.");
    }

    internal void ExecuteIntent()
    {
        EnemyAction? action = Instance.GetIntentAction(Intent);

        if (action != null)
        {
            action.OnAction(this);

            if (!string.IsNullOrEmpty(action.AnimationName))
                uiObject?.GetComponent<Animator>().Play(action.AnimationName, 0);
        }
        else
            Log($"No action was registered with the tag \'{Intent ?? "null"}\'");
    }
    #endregion

    #region Attack
    /// <summary>
    /// Base amount of damage 
    /// </summary>
    protected abstract int Damage { get; }

    /// <returns>Amount of damage that an attack would deal</returns>
    internal int GetAttack()
    {
        return Damage;
    }

    #endregion

    #region Background
    protected virtual string? GetBackgroundGUID() => null;
    #endregion

    #region Portrait
    private readonly Image? EnemyPortrait;
    protected abstract string? Portrait { get; }

    protected void SetPortrait(string? value = null)
    {
        if (EnemyPortrait == null)
        {
            Log($"Tried to access {nameof(EnemyPortrait)}, but it was not defined.");
            return;
        }

        EnemyPortrait.SetSprite(value ?? Portrait);
    }
    #endregion
}

public abstract class RegularBloon : EnemyCard
{
    protected RegularBloon(GameObject? enemyObject, int position, params string[] actions)
        : base(enemyObject, position, new Vector2(4, 4), actions) { }
}