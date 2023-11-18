using BTD_Mod_Helper.Extensions;
using BTDAdventure.Abstract.EnemyActions;
using BTDAdventure.Managers;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Entities;

public class EnemyEntity : Entity
{
    private readonly EnemyCard Model;

    public EnemyEntity(GameObject? enemyObject, int position, EnemyCard model) : base(enemyObject)
    {
        Model = model;

        Position = position;

        if (enemyObject != null)
            SetUpToModel(enemyObject);
    }

    protected override void SetUpExtraUI(GameObject root)
    {
        // Portrait
        EnemyPortrait = root.transform.Find("Display/Portrait")?.GetComponent<Image>();
        // ---

        // Set Up UIs
        SetUpIntentUI(root);
        // ---
    }

    private void SetUpToModel(GameObject root)
    {
        // Portrait
        Transform _enemyPortraitTransform = root.transform.Find("Display/Portrait");
        _enemyPortraitTransform.localScale = Model?.Size ?? Vector2.one; // Auto-scaling ?

        SetPortrait(Model?.Portrait);
        // ---

        // Background
        string? bgGUID = Model?.GetBackgroundGUID();

        if (!string.IsNullOrEmpty(bgGUID))
            root.transform.Find("Display/Background")?.GetComponent<Image>().SetSprite(bgGUID);
        // ---

        // Health
        MaxHealth = Model?.MaxHP ?? MaxHealth;
        Health = MaxHealth;
        UpdateHealthUI();
        // ---

        // Damage
        Damage = Model?.Damage ?? Damage;
        // ---

        // Shield
        BaseShield = Model?.Armor ?? BaseShield;
        // ---

        // Button
        root.GetComponent<Button>().onClick.SetListener(new Function(() =>
        {
            Instance.SelectEnemy(Position);
        }));
        // ---
    }

    //public string innate;
    //public int innateValue;
    //public bool isOriginal;

    private readonly int Position;

    public uint GetCoinsGiven() => Model?.CoinsGiven ?? 0;

    #region Health

    protected Slider? HealthSlider;

    protected override void SetUpHealthUI(GameObject root)
    {
        Transform? hpSliderT = root.transform.Find("HP/HealthBar");

        if (hpSliderT != null)
            HealthSlider = hpSliderT.GetComponent<Slider>();
        else
            Log("Health Slider was not found.");

        Transform? hpTextT = root.transform.Find("HP/TextHolder/Text");

        if (hpTextT != null)
            HealthText = hpTextT.gameObject.GetComponent<NK_TextMeshProUGUI>();
        else
            Log("Health Text was not found.");
    }

    protected override void UpdateExtraHealthUI()
    {
        if (HealthSlider != null)
        {
            HealthSlider.maxValue = MaxHealth;
            //HealthSlider.minValue = Math.Min(HealthSlider.minValue, Health);
            HealthSlider.value = Math.Max(0, Health);
        }
    }

    protected override void OnDeath()
    {
        Instance.KillEnemy(Position); // they kill themselves :(
        this.RemoveAllEffects();
    }

    #endregion Health

    #region Shield

    protected override void SetUpShieldUI(GameObject root)
    {
        ShieldImg = root.transform.Find("Shield")?.gameObject;

        // Setting the sprite in the prefab doesn't work
        ShieldImg?.GetComponent<Image>().SetSprite(UIManager.ShieldIcon);

        if (ShieldImg != null)
        {
            Transform textT = ShieldImg.transform.Find("Text");

            ShieldText = textT.GetComponent<NK_TextMeshProUGUI>() ?? InitializeText(textT, 25);
        }
    }

    #endregion Shield

    #region Intent

    private HorizontalLayoutGroup? _intentContainer;
    private Image? _intentIcon;
    private NK_TextMeshProUGUI? _intentText;
    private Animator? _intentAnimator;

    private EnemyAction NextIntent;

    /// <summary>
    /// Moves to the next intent
    /// </summary>
    internal void MoveIntent()
    {
        SetIntent(Model.GetNextAction(GameManager.Instance.TurnCount, this));
        UpdateIntent();
    }

    /// <summary>
    /// Sets the next intent to <paramref name="enemyAction"/>
    /// </summary>
    internal void SetIntent(EnemyAction enemyAction) => NextIntent = enemyAction;

    protected virtual void SetUpIntentUI(GameObject root)
    {
        Transform? intentContainerT = root.transform.Find("Intent");

        if (intentContainerT == null)
        {
            Log("Intent Transform was not found.");
            return;
        }

        intentContainerT.SafeGetComponent(out _intentContainer);
        intentContainerT.Find("IconHolder/Icon").SafeGetComponent(out _intentIcon);
        intentContainerT.Find("Text").SafeGetComponent(out _intentText);
        root.transform.SafeGetComponent(out _intentAnimator);
    }

    private void UpdateIntent()
    {
        int containerLeftPadding = 30;

        if (Model == null)
            return;

        if (_intentText != null)
        {
            string? text = NextIntent.GetText(this);
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
            _intentIcon.SetSprite(NextIntent.Icon);
        else
            Log("Intent Icon was not found.");
    }

    internal float ExecuteIntent()
    {
        if (Model != null && Instance.Player != null)
        {
            if (NextIntent is IntentEffectAction)
                SoundManager.PlaySound(SoundManager.SOUND_DEBUFF_APPLIED, SoundManager.GeneralGroup);

            NextIntent.OnAction(this, Instance.Player);

            string? animationName = NextIntent.AnimationName;

            if (!string.IsNullOrEmpty(animationName) && _intentAnimator != null)
            {
                _intentAnimator.Play(animationName, 0);

                var clips = _intentAnimator.runtimeAnimatorController.animationClips;
                foreach (var item in clips)
                {
                    if (item.name == animationName)
                        return item.length;
                }
            }
        }
        return 0;
    }

    #endregion Intent

    #region Portrait

    private Image? EnemyPortrait;

    protected void SetPortrait(string? value = null)
    {
        if (EnemyPortrait == null)
        {
            Log($"Tried to access {nameof(EnemyPortrait)}, but it was not defined.");
            return;
        }

        EnemyPortrait.SetSprite(value ?? Model?.Portrait);
    }

    #endregion Portrait

    #region Effects

    protected override void SetUpEffectUI(GameObject root)
    {
        EffectHolder = root.transform.Find("Status")?.gameObject;
        EffectVisual = root.transform.Find("EffectVisual");
    }

    protected override void OnEffectUpdate(Effect effect)
    {
        UpdateIntent();
    }

    #endregion Effects
}