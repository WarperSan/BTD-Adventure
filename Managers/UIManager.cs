using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Cards;
using BTDAdventure.Cards.HeroCard;
using Il2Cpp;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppTMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BTDAdventure.Managers;

internal class UIManager
{
    #region Constants
    const string ValuesPath = "Overlay/TopPanel/Values";
    public const string DamageIcon = "Ui[BTDAdventure-intent_damage]";
    public const string DoubleDamageIcon = "Ui[BTDAdventure-intent_damage_double]";
    public const string CurseIcon = "Ui[BTDAdventure-negate]";
    public const string WaitIcon = "Ui[BTDAdventure-intent_pause]";
    public const string ShieldIcon = "Ui[BTDAdventure-shield-1]";

    public const float TopValueFontSize = 125;
    #endregion

    private Transform? EnemyCardsHolder;
    private Transform? PlayerCardsHolder;
    private readonly GameObject?[] PlayerCards = new GameObject?[MaxPlayerCardCount];

    private Button? EndTurnBtn;

    private GameObject? LoadingScreen;

    public void SetUpMainUI()
    {
        SetActiveUiRect(false);

        GameObject? mainUIprefab = GameManager.LoadAsset<GameObject>("BTDAdventureUI");

        if (mainUIprefab == null)
            return;

        Transform mainUI = UnityEngine.Object.Instantiate(mainUIprefab, InGame.instance.GetInGameUI().transform).transform;

        LoadingScreen = mainUI.Find("LoadingUI")?.gameObject;
        LoadingScreen?.transform.Find("Wheel").GetComponent<Image>().SetSprite(VanillaSprites.LoadingWheel);

        SetUpVictoryUI(mainUI);
        SetUpRewardUI(mainUI);

        EnemyCardsHolder = mainUI.Find("GameUI/EnemyCardsHolder");
        PlayerCardsHolder = mainUI.Find("GameUI/PlayerCards/Holder");

        EndTurnBtn = mainUI.Find("Overlay/Button").GetComponent<Button>();
        EndTurnBtn.onClick.AddListener(new Function(GameManager.Instance.EndTurn));

        // Create enemies objects
        for (int i = 0; i < EnemiesObject.Length; i++)
        {
            EnemiesObject[i] = UnityEngine.Object.Instantiate(GameManager.Instance.EnemyCardPrefab, EnemyCardsHolder);
        }

        mainUI.Find("Overlay/TopPanel/Settings").GetComponent<Image>().SetSprite(VanillaSprites.SettingsBtn);

        SetUpTexts(mainUI);
    }

    #region Victory UI
    internal GameObject? VictoryUI;
    internal Button? VictoryBtn;

    private void SetUpVictoryUI(Transform mainUI)
    {
        VictoryUI = mainUI.Find("VictoryUI").gameObject;
        VictoryBtn = VictoryUI?.GetComponent<Button>();
        VictoryBtn?.onClick.SetListener(new Function(GameManager.Instance.VictoryUIClose));
    }
    #endregion

    #region Reward UI
    internal GameObject? RewardUI;
    internal Transform? RewardHolder;

    private void SetUpRewardUI(Transform mainUI)
    {
        RewardUI = mainUI.Find("RewardUI")?.gameObject;
        RewardHolder = RewardUI?.transform.Find("Rewards/Viewport/Content");

        RewardUI?.transform.Find("Background").GetComponent<Image>().SetSprite(VanillaSprites.TrophyStoreTiledBG);

        Transform? title = RewardUI?.transform.Find("TitleHolder/Title");

        if (title != null)
        {
            InitializeText(title, 125, initialText: "CHOOSE YOUR REWARD");
        }
    }

    internal static GameObject? InitializeRewardPrefab()
    {
        GameObject? RewardCardPrefab = GameManager.Instance.RewardCardPrefab ?? Object.Instantiate(LoadAsset<GameObject>("Reward"));

        if (RewardCardPrefab != null)
        {
            Transform? bannerT = RewardCardPrefab.transform.Find("Banner");
            if (bannerT != null)
            {
                //bannerT.GetComponent<Image>().SetSprite(VanillaSprites.TrophyStoreBanner);

                InitializeText(bannerT.Find("TextHolder/Text"), 50, initialText: "GrahamKracker the chad");
            }

            Transform? buttonT = RewardCardPrefab.transform.Find("Button");

            if (buttonT != null)
            {
                buttonT.gameObject.GetComponent<Image>().SetSprite(VanillaSprites.ParagonBtnLong);
                InitializeText(buttonT.Find("Text"), 75, font: Fonts.Btd6FontBody, initialText: "SELECT");
            }

            RewardCardPrefab.transform.Find("InfoBtn").GetComponent<Image>().SetSprite(VanillaSprites.InfoBtn2);
        }
        return RewardCardPrefab;
    }
    #endregion

    #region Texts
    private NK_TextMeshProUGUI? HealthText;
    private NK_TextMeshProUGUI? CashText;
    private NK_TextMeshProUGUI? BloonjaminsText;
    private NK_TextMeshProUGUI? ManaText;

    /// <summary>
    /// Causes the health text to update itself
    /// </summary>
    public void UpdateHealthText() => UpdateText(GameManager.Instance.Health + " / " + GameManager.Instance.MaxHealth, HealthText);

    /// <summary>
    /// Causes the cash text to update itself
    /// </summary>
    public void UpdateCashText() => UpdateText(GameManager.Instance.coins, CashText);

    /// <summary>
    /// Causes the bloonjamins text to update itself
    /// </summary>
    public void UpdateBloonjaminsText() => UpdateText(GameManager.Instance.bjms, BloonjaminsText);

    /// <summary>
    /// Causes the mana text to update itself
    /// </summary>
    public void UpdateManaText() => UpdateText(GameManager.Instance.Mana + "/" + GameManager.Instance.MaxMana, ManaText);

    private static void UpdateText(object? content, NK_TextMeshProUGUI? text)
    {
        if (text == null)
        {
            Log("The given text component is null.");
            return;
        }

        text.text = content == null ? "null" : content.ToString();
    }

    private void SetUpTexts(Transform mainUI)
    {
        //Fonts.Btd6FontTitle
        // Health
        GameObject health = mainUI.Find(ValuesPath + "/Health").gameObject;
        health.GetComponentInChildren<Image>().SetSprite(VanillaSprites.LivesIcon);
        HealthText = InitializeText(health.transform.Find("Text"), TopValueFontSize);

        // Cash (coins)
        GameObject cash = mainUI.Find(ValuesPath + "/Cash").gameObject;
        cash.GetComponentInChildren<Image>().SetSprite(VanillaSprites.CoinIcon);
        CashText = InitializeText(cash.transform.Find("Text"), TopValueFontSize);

        // Bloonjamins (gems)
        GameObject bloonjamins = mainUI.Find(ValuesPath + "/Bloonjamins").gameObject;
        bloonjamins.GetComponentInChildren<Image>().SetSprite(VanillaSprites.BloonjaminsIcon);
        BloonjaminsText = InitializeText(bloonjamins.transform.Find("Text"), TopValueFontSize);

        // Mana
        ManaText = InitializeText(mainUI.Find("GameUI/Mana/Text"), 125, font: Fonts.Btd6FontBody);
    }

    internal static NK_TextMeshProUGUI? InitializeText(
        Transform @object,
        float fontSize,
        TextAlignmentOptions? textAlignment = null,
        TMP_FontAsset? font = null,
        string? initialText = null)
    {
        NK_TextMeshProUGUI text = @object.gameObject.AddComponent<NK_TextMeshProUGUI>();

        // Often because a Text component already exists
        if (text == null)
        {
            Log($"Could not add the component NK_TextMeshProUGUI to the gameobject \'{@object.name}\'");
            return null;
        }

        text.fontSize = fontSize;
        text.font = font ?? Fonts.Btd6FontTitle;
        text.alignment = textAlignment ?? TextAlignmentOptions.Center;
        text.text = initialText;

        return text;
    }
    #endregion

    #region Player
    public void InitPlayerCards()
    {
        if (GameManager.Instance.PlayerCardPrefab == null)
            return;

        int targetX = Mathf.CeilToInt(MaxEnemiesCount / 2f);

        for (int i = 0; i < MaxPlayerCardCount; i++)
        {
            if (i == targetX)
            {
                GameObject manaSpacer = new("ManaSpacer");
                manaSpacer.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                manaSpacer.transform.parent = PlayerCardsHolder;
            }


            GameObject newCard = GameObject.Instantiate(GameManager.Instance.PlayerCardPrefab, PlayerCardsHolder);

            int value = i;

            newCard.GetComponent<Button>().onClick.AddListener(new Function(() =>
            {
                GameManager.Instance.SelectCard(value);
            }));

            PlayerCards[i] = newCard;
        }
    }

    public void SetUpPlayerCard(int index, HeroCard heroCard, bool swingAnimation)
    {
        if (index < 0 || index >= PlayerCards.Length)
        {
            Log($"The given index \'{index}\' is outside the valid range [0;{PlayerCards.Length - 1}].");
            return;
        }

        GameObject? selectedCard = PlayerCards[index];

        if (selectedCard == null)
        {
            Log($"No card resides at index \'{index}\'.");
            return;
        }

        selectedCard.name = heroCard.DisplayName;

        selectedCard.transform.Find("Portrait").GetComponent<Image>().SetSprite(heroCard.Portrait);
        selectedCard.transform.Find("Background").GetComponent<Image>().SetSprite(heroCard.GetBackgroundGUID());

        if (swingAnimation)
            MelonLoader.MelonCoroutines.Start(SwingCard(selectedCard));
    }

    public void SetLockState(bool state)
    {
        foreach (var item in PlayerCards)
        {
            if (item == null)
                continue;

            SetLockState(state, item);
        }
    }
    #endregion

    #region Enemy
    private readonly GameObject?[] EnemiesObject = new GameObject?[MaxEnemiesCount];

    internal EnemyCard? AddEnemy(Type t, int position)
    {
        if (!GameManager.Instance.IsEnemyTypeValid(t))
            return null;

        if (EnemyCardsHolder == null)
        {
            Log("\'EnemyCardsHolder\' was not defined.");
            return null;
        }

        try
        {
            GameObject? enemyObject = EnemiesObject[position];

            if (enemyObject == null)
            {
                Log($"Enemy Object at \'{position}\' was not defined.");
                return null;
            }

            enemyObject.SetActive(true);

            return (EnemyCard?)Activator.CreateInstance(t, enemyObject, position);
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
    }
    internal void KillEnemy(int position) => EnemiesObject[position]?.SetActive(false);

    internal static GameObject? InitializeEnemyPrefab()
    {
        GameObject? EnemyCardPrefab = GameManager.Instance.EnemyCardPrefab ?? Object.Instantiate(LoadAsset<GameObject>("EnemyCard"));

        if (EnemyCardPrefab != null)
        {
            InitializeText(EnemyCardPrefab.transform.Find("Intent/Text"), 50, initialText: "yuh uh");
            InitializeText(EnemyCardPrefab.transform.Find("HP/TextHolder/Text"), 25, initialText: "nuh uh");
        }
        return EnemyCardPrefab;
    }
    #endregion

    #region Static
    private static void SetLockState(bool state, GameObject card) => card.transform.Find("WaitFrame").gameObject.SetActive(state);

    internal static void SetActiveUiRect(bool isActive)
    {
        InGame.instance.uiRect.gameObject.SetActive(isActive);
#if DEBUG
        Log($"InGameUI is now {(isActive ? "active" : "inactive")}.");
#endif
    }
    #endregion

    #region Coroutines
    public void UseLoading(Action? preLoad = null, Action? postLoad = null) => MelonLoader.MelonCoroutines.Start(UseLoadingCoroutine(preLoad, postLoad));

    private IEnumerator UseLoadingCoroutine(Action? preLoad = null, Action? postLoad = null)
    {
        LoadingScreen?.SetActive(true);

        Animator? animator = LoadingScreen?.GetComponent<Animator>();

        preLoad?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        if (animator != null)
            animator.Play("LoadingFadeIn");
        yield return new WaitForSeconds(0.75f); // Wait for fade in

        postLoad?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        if (animator != null)
            animator.Play("LoadingFadeOut");
        yield return new WaitForSeconds(0.75f); // Wait for fade out

        LoadingScreen?.SetActive(false);
    }

    static IEnumerator SwingCard(GameObject card)
    {
        Animator animator = card.GetComponent<Animator>();

        animator.Play("PlayerCardLeftSwig");

        yield return new WaitForEndOfFrame(); // Wait for load
        yield return new WaitForSeconds(0.1f); // Wait for animation

        SetLockState(false, card);

        animator.Play("PlayerCardRightSwig");
    }
    #endregion
}