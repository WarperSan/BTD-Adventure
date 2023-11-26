using BTD_Mod_Helper.Extensions;
using BTDAdventure.Managers;
using Il2Cpp;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Entities;

public class EnemyEntity : Entity
{
    private readonly EnemyCard Model;

    public EnemyEntity(GameObject enemyObject, int position, EnemyCard model) : base(enemyObject)
    {
        Model = model;

        Position = position;

        if (enemyObject != null)
            SetUpToModel(enemyObject);
    }
    
    //public string innate;
    //public int innateValue;
    //public bool isOriginal;

    private readonly int Position;

    #region Reward

    /// <returns>Reward earned when this entity is killed.</returns>
    public virtual Reward GetReward() => Model.GetReward();

    #endregion

    #region Intent

    private HorizontalLayoutGroup? _intentContainer;
    private Image? _intentIcon;
    private NK_TextMeshProUGUI? _intentText;
    private Animator? _intentAnimator;

    private EnemyAction? NextIntent;

    /// <summary>
    /// Advances to the next intent.
    /// </summary>
    internal void AdvanceIntent()
    {
        SetIntent(Model.GetNextAction(GameManager.Instance.TurnCount, this));
        UpdateIntent();
    }

    /// <summary>
    /// Sets the next intent to <paramref name="enemyAction"/>
    /// </summary>
    internal void SetIntent(EnemyAction enemyAction) => NextIntent = enemyAction;

    /// <summary>
    /// Called to set up UI references.
    /// </summary>
    protected void SetUpIntentUI(GameObject root)
    {
        Transform? intentContainerT = root.transform.Find("Intent");

        if (LogNull(intentContainerT, "Intent Transform"))
            return;

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

#nullable disable
        if (!LogNull(_intentText, "Intent Text"))
        {
            string text = NextIntent.GetText(this);
            _intentText.text = text;
            _intentText.gameObject.SetActive(!string.IsNullOrEmpty(text));

            if (!_intentText.gameObject.activeInHierarchy)
                containerLeftPadding = 0;
        }

        if (!LogNull(_intentContainer, "Intent Container"))
        {
            _intentContainer.padding.left = containerLeftPadding;
        }


        if (!LogNull(_intentIcon, "Intent Icon"))
        {
            _intentIcon.SetSprite(NextIntent.Icon);
        }
#nullable restore
    }

    /// <returns>Length of the played animation.</returns>
    internal float ExecuteIntent()
    {
        float animationLength = 0;

        if (Model == null || GameManager.Instance.Player == null || NextIntent == null)
            return animationLength;

        NextIntent.OnAction(this, Instance.Player);

        // Skip animation if wanted
        if (Settings.GetSettingValue(Settings.SETTING_SKIP_ENEMY_ANIMATION, false))
            return animationLength;

        // Play sound if definedd
        if (NextIntent.SoundName != null)
            SoundManager.PlaySound(NextIntent.SoundName, SoundManager.GeneralGroup);

        string? animationName = NextIntent.AnimationName;

        if (string.IsNullOrEmpty(animationName) || _intentAnimator == null)
            return animationLength;

        var clips = _intentAnimator.runtimeAnimatorController.animationClips;
        foreach (var item in clips)
        {
            if (item.name != animationName)
                continue;

            animationLength = item.length;
            break;
        }

        _intentAnimator.Play(animationName, 0);

        return animationLength;
    }

    #endregion Intent

    #region Portrait

    private Image? EnemyPortrait;

    private void SetPortrait(string? value = null)
    {
        if (LogNull(EnemyPortrait, nameof(EnemyPortrait)))
            return;

        EnemyPortrait.SetSprite(value ?? Model?.Portrait);
    }

    #endregion Portrait

    #region Other Setups

    protected override sealed void SetUpExtraUI(GameObject root)
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

    #endregion Other Setups

    // --- Entity ---
    #region Health

    protected Slider? HealthSlider;

    protected override sealed void SetUpHealthUI(GameObject root)
    {
        Transform? hpSliderT = root.transform.Find("HP/HealthBar");

        if (!LogNull(hpSliderT, "Health Slider"))
        {
            HealthSlider = hpSliderT.GetComponent<Slider>();
        }

        Transform? hpTextT = root.transform.Find("HP/TextHolder/Text");

        if (!LogNull(hpTextT, "Health Text"))
        {
            HealthText = hpTextT.gameObject.GetComponent<NK_TextMeshProUGUI>();
        }
    }

    protected override void UpdateExtraHealthUI()
    {
        if (HealthSlider == null)
            return;

        HealthSlider.maxValue = MaxHealth;
        //HealthSlider.minValue = Math.Min(HealthSlider.minValue, Health);
        HealthSlider.value = Math.Max(0, Health);
    }

    protected override void OnDeath()
    {
        Instance.KillEnemy(Position); // they kill themselves :(
        this.RemoveAllEffects();
    }

    #endregion Health

    #region Shield

    protected override sealed void SetUpShieldUI(GameObject root)
    {
        ShieldImg = root.transform.Find("Shield")?.gameObject;

        // Setting the sprite in the prefab doesn't work
        ShieldImg?.GetComponent<Image>().SetSprite(UIManager.ICON_SHIELD);

        if (LogNull(ShieldImg, "Shield Image"))
            return;

#nullable disable
        Transform textT = ShieldImg.transform.Find("Text");
#nullable restore

        ShieldText = textT.GetComponent<NK_TextMeshProUGUI>() ?? UIManager.InitializeText(textT, 25);
    }

    #endregion Shield

    #region Effects

    protected override sealed void SetUpEffectUI(GameObject root)
    {
        EffectHolder = root.transform.Find("Status")?.gameObject;
        EffectVisual = root.transform.Find("EffectVisual");
    }

    protected override void OnEffectUpdate(Effect effect) => UpdateIntent();

    #endregion Effects

}