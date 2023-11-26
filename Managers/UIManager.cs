using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Components;
using BTDAdventure.Entities;
using Il2Cpp;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppTMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Managers;

internal class UIManager
{
    #region Constants

    public const string OVERLAY_VALUES_PATH = "Overlay/TopPanel/Values";
    public const float FONT_SIZE_TOP_VALUES = 125;

    // --- Animation ---
    private const string ANIMATION_ENEMY_DIE = "EnemyDie";
    private const string ANIMATION_CARD_SWING_LEFT = "PlayerCardLeftSwig";
    private const string ANIMATION_CARD_SWING_RIGHT = "PlayerCardRightSwig";
    private const string ANIMATION_LOADING_FADE_IN = "LoadingFadeIn";
    private const string ANIMATION_LOADING_FADE_OUT = "LoadingFadeOut";

    // --- Action ---
    public const string ICON_DAMAGE = "Ui[BTDAdventure-intent_damage]";
    public const string ICON_DOUBLE_DAMAGE = "Ui[BTDAdventure-intent_damage_double]";
    public const string ICON_CURSE = "Ui[BTDAdventure-intent_negate]";
    public const string ICON_WAIT = "Ui[BTDAdventure-intent_pause]";
    public const string ICON_SHIELD = "Ui[BTDAdventure-intent_shield-1]";
    public const string ICON_SPAWN = "Ui[BTDAdventure-intent_evoke]";

    // --- Status ---
    public const string ICON_WOUND = "Ui[BTDAdventure-icon_wound]";
    public const string ICON_EXTREME_WOUND = "Ui[BTDAdventure-innate_wounded_hit]";
    public const string ICON_POISON = "Ui[BTDAdventure-icon_poison]";
    public const string ICON_BURN = "Ui[BTDAdventure-icon_burn]";
    public const string ICON_FRAIL = "Ui[BTDAdventure-icon_frail]";
    public const string ICON_THORNS = "Ui[BTDAdventure-icon_thorns]";
    public const string ICON_OVERCHARGED = "Ui[BTDAdventure-icon_overcharged]";
    // ---

    #endregion Constants

    internal static Transform? SetUpMainUI()
    {
        GameObject? mainUIprefab = GameManager.LoadAsset<GameObject>("BTDAdventureUI");

        if (mainUIprefab == null)
            return null;

        Transform mainUI = UnityEngine.Object.Instantiate(mainUIprefab, InGame.instance.GetInGameUI().transform).transform;

        // Engine Object
        var engineObject = GameObject.Find("Engine").transform;
        menuManager = engineObject.Find("Game")?.GetComponent<MenuManager>();
        engineObject.Find("Objects")?.gameObject.SetActive(false); // Disable track arrows
        // ---

        //// Pause btn
        //var b = InGame.instance.uiRect.Find("PauseButton").GetComponent<ButtonExtended>();
        //mainUIprefab.transform.Find("Settings").GetComponent<Button>().onClick = b.onClick;
        //// ---

        SetUpLoadingUI(mainUI);
        SetUpVictoryUI(mainUI);
        SetUpRewardUI(mainUI);
        SetUpMapUI(mainUI);
        SetUpDeathUI(mainUI);
        SetUpGameUI(mainUI);
        
        SetUpEnemyUI();

        SetUpPlayerCardPrefab();
        SetUpEffectsPrefab();

        return mainUI;
    }

    private static void RemoveVanillaUI(Transform mainUI)
    {
        for (int i = 0; i < InGame.instance.uiRect.childCount; ++i)
        {
            GameObject @object = InGame.instance.uiRect.GetChild(i).gameObject;

            if (@object.name == "BlackBars(Clone)")
                continue;

            @object.SetActive(false);
        }
    }

    #region Game UI

    private static GameObject? GameUI;

    private static Button? EndTurnBtn;

    internal static void SetEndTurnBtnInteractable(bool value)
    {
        if (EndTurnBtn != null)
            EndTurnBtn.interactable = value;
    }
    internal static void SetGameUIActive(bool value) => GameUI?.SetActive(value);

    private static void SetUpGameUI(Transform mainUI)
    {
        GameUI = mainUI.Find("GameUI").gameObject;

        EnemyCardsHolder = GameUI.transform.Find("EnemyCardsHolder");
        PlayerCardsHolder = GameUI.transform.Find("PlayerCards/Holder");

        // End Btn
        var endTurnObject = GameUI.transform.Find("EndTurn");
        InitializeText(endTurnObject.Find("Text"), 20, TextAlignmentOptions.Baseline, initialText: "END TURN");
        EndTurnBtn = endTurnObject.GetComponent<Button>();
        EndTurnBtn.onClick.AddListener(new Function(() =>
        {
            EndTurnBtn.interactable = false;
            GameManager.Instance.EndTurn();
        }));
        // ---

        RemoveVanillaUI(mainUI);
    }
    #endregion

    #region Loading UI

    private static Animator? LoadingScreenAnimator;
    
    private static void SetLoadingScreenActive(bool value)
    {
        LoadingScreenAnimator?.gameObject.SetActive(value);
        SetMenuManagerActive(!value);
    }

    /// <summary>
    /// Causes the loading screen to appear
    /// </summary>
    /// <param name="preLoad">Called before the fade in starts</param>
    /// <param name="postLoad">Called before the fade out starts</param>
    public static void UseLoading(Action? preLoad = null, Action? postLoad = null) => 
        StartCoroutine(UseLoadingCoroutine(preLoad, postLoad));

    private static IEnumerator UseLoadingCoroutine(Action? preLoad, Action? postLoad)
    {
        SetLoadingScreenActive(true);

        preLoad?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        LoadingScreenAnimator?.Play(ANIMATION_LOADING_FADE_IN);
        yield return new WaitForSeconds(0.75f); // Wait for fade in

        postLoad?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        LoadingScreenAnimator?.Play(ANIMATION_LOADING_FADE_OUT);
        yield return new WaitForSeconds(0.25f); // Wait for fade out

        SetLoadingScreenActive(false);

        ClearCoroutines();
    }

    private static void SetUpLoadingUI(Transform mainUI)
    {
        var loadingScreen = mainUI.Find("LoadingUI")?.gameObject;
        loadingScreen?.transform.Find("Wheel").GetComponent<Image>().SetSprite(VanillaSprites.LoadingWheel);

        LoadingScreenAnimator = loadingScreen?.GetComponent<Animator>();
    }


    #endregion

    #region Menu Manager

    private static MenuManager? menuManager;

    private static void SetMenuManagerActive(bool value)
    {
        if (menuManager != null)
            menuManager.enabled = value;
    }

    #endregion

    #region Death UI

    private static GameObject? DeathUI;

    internal static void ShowDeathUI() => DeathUI?.SetActive(true);

    private static void SetUpDeathUI(Transform mainUI)
    {
        DeathUI = mainUI.Find("DeathUI")?.gameObject;

        if (DeathUI == null)
            return;

        DeathUI.transform.Find("DeathBloon1/Render").GetComponent<Image>().SetSprite(VanillaSprites.UnpoppedArmyZombiePhoenix);
        DeathUI.transform.Find("DeathBloon2/Render").GetComponent<Image>().SetSprite(VanillaSprites.UnpoppedArmyZombie);
        DeathUI.transform.Find("Panel/MoreInfo").GetComponent<Image>().SetSprite(VanillaSprites.InfoBtn2);

        var goBackBtn = DeathUI.transform.Find("Panel/GoBackBtn");
        goBackBtn.GetComponent<Image>().SetSprite(VanillaSprites.BlueBtnLong);
        goBackBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            InGame.instance.QuitToMainMenu();
        });

        InitializeText(goBackBtn.Find("Render"), 90, TextAlignmentOptions.Center, initialText: "Back to Home");

        var titleText = InitializeText(DeathUI.transform.Find("Panel/Title"), 160, TextAlignmentOptions.Center, initialText: "You died !");

        if (titleText != null)
        {
            titleText.color = Color.red;
            titleText.outlineColor = new Color32(255, 0, 255, 33);
        }
    }

    #endregion

    #region Map

    private static MapGenerator? MapGenerator;

    internal static void AdvanceLayer()
    {
        MapGenerator?.ProgressLayer();
        MapGenerator?.MapObjectsParent?.gameObject.SetActive(true);
    }

    internal static void InitializeWorld(int seed)
    {
        MapGenerator?.GenerateMap(seed);
        MapGenerator?.ProgressLayer();
    }

    private static void SetUpMapUI(Transform mainUI)
    {
        MapGenerator = mainUI.Find("MapGenerator").gameObject.AddComponent<MapGenerator>();

        MapGenerator.SetUp(MapGenerator.transform.Find("Scroll View/Viewport/Content/MapObjects")?.GetComponent<RectTransform>());
    }

    #endregion Map

    #region Enemy

    private static GameObject? EnemyCardPrefab;

    private static Transform? EnemyCardsHolder;
    private static GameObject?[]? EnemiesObject;

    internal static EnemyEntity? AddEnemy(EnemyCard enemyCard, int position)
    {
        if (LogNull(EnemyCardsHolder, nameof(EnemyCardsHolder)))
            return null;

        if (LogNull(EnemiesObject, nameof(EnemiesObject)))
            return null;

        try
        {
#nullable disable
            GameObject enemyObject = EnemiesObject[position];

            if (LogNull(enemyObject, nameof(enemyObject)))
                return null;

            enemyObject.GetComponent<Button>().enabled = true;
            enemyObject.GetComponent<CanvasGroup>().alpha = 1;
            enemyObject.SetActive(true);
            //SetEnemyState(true, position);

            return new EnemyEntity(enemyObject, position, enemyCard);
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
#nullable restore
    }

    internal static void SetEnemyState(bool state, int position) => EnemiesObject?[position]?.SetActive(state);

    internal static void KillEnemy(int position, EnemyEntity? entity) => StartCoroutine(EnemyDies(position, entity, 0.6667f));

    private static IEnumerator EnemyDies(int position, EnemyEntity? entity, float animationLength)
    {
        if (EnemiesObject == null)
            yield break;

        GameObject? enemy = EnemiesObject[position];

        if (enemy == null)
            yield break;

        enemy.GetComponent<Button>().enabled = false;

        Animator animator = enemy.GetComponent<Animator>();

        animator.Play(ANIMATION_ENEMY_DIE);

        yield return new WaitForEndOfFrame(); // Wait for load
        yield return new WaitForSeconds(animationLength); // Wait for animation

        SetEnemyState(false, position);
        EnemiesObject[position]?.transform.Find("Shield")?.gameObject.SetActive(false); // Disable shield on death

        entity?.DestroyAllVisual();
    }

    private static void SetUpEnemyUI()
    {
        InitializeEnemyPrefab();

        EnemiesObject = new GameObject?[COUNT_MAX_ENEMIES];

        // Create enemies objects
        for (int i = 0; i < EnemiesObject.Length; i++)
        {
            EnemiesObject[i] = UnityEngine.Object.Instantiate(EnemyCardPrefab, EnemyCardsHolder);
        }
        // ---
    }

    private static void InitializeEnemyPrefab()
    {
        EnemyCardPrefab = GameObject.Instantiate(LoadAsset<GameObject>("EnemyCard"));

        if (EnemyCardPrefab == null)
            return;

        InitializeText(EnemyCardPrefab.transform.Find("Intent/Text"), 50, initialText: "yuh uh");
        InitializeText(EnemyCardPrefab.transform.Find("HP/TextHolder/Text"), 25, initialText: "nuh uh");

        //EnemyCardPrefab.transform.Find("Shield")?.GetComponent<Image>().SetSprite(UIManager.ShieldIcon);
    }

    #endregion Enemy

    #region Player Cards

    private static GameObject? PlayerCardPrefab;
    private static Transform? PlayerCardsHolder;
    private static readonly GameObject?[] PlayerCards = new GameObject?[PLAYER_HAND_SIZE];

    internal static void InitPlayerCards()
    {
        if (LogNull(PlayerCardPrefab, "Player Card Prefab"))
            return;

        if (LogNull(PlayerCardsHolder, nameof(PlayerCardsHolder)))
            return;

        int targetX = Mathf.CeilToInt(COUNT_MAX_ENEMIES / 2f);

        for (int i = 0; i < PLAYER_HAND_SIZE; i++)
        {
            if (i == targetX)
            {
                GameObject manaSpacer = new("ManaSpacer");
                manaSpacer.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
                manaSpacer.transform.parent = PlayerCardsHolder;
            }

            GameObject? newCard = GameObject.Instantiate(PlayerCardPrefab, PlayerCardsHolder);

            if (newCard != null)
            {
                int value = i;

                newCard.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GameManager.Instance.SelectCard(value);
                });
            }

            PlayerCards[i] = newCard;
        }
    }

    internal static void SetUpPlayerCard(int index, HeroCard? heroCard, bool swingAnimation)
    {
        if (index < 0 || index >= PlayerCards.Length)
        {
            Log($"The given index \'{index}\' is outside the valid range [0; {PlayerCards.Length - 1}].");
            return;
        }

        GameObject? selectedCard = PlayerCards[index];

        if (selectedCard == null)
        {
            Log($"No card resides at index \'{index}\'.");
            return;
        }

        if (heroCard != null)
            selectedCard.name = heroCard.DisplayName;

        if (swingAnimation)
        {
            StartCoroutine(SwingCard(selectedCard, new Action(() =>
            {
                SetUpCard(selectedCard, heroCard);
            }), 0.1667f, 0.1667f));
        }
        else
            SetUpCard(selectedCard, heroCard);
    }

    private static IEnumerator SwingCard(
        GameObject card,
        Action? afterLeftSwing,
        float leftSwingLength,
        float rightSwingLength)
    {
        Animator animator = card.GetComponent<Animator>();

        animator.Play(ANIMATION_CARD_SWING_LEFT);

        yield return new WaitForEndOfFrame(); // Wait for load
        yield return new WaitForSeconds(leftSwingLength); // Wait for animation

        SetLockState(false, card);
        afterLeftSwing?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        animator.Play(ANIMATION_CARD_SWING_RIGHT);
        yield return new WaitForEndOfFrame(); // Wait for load
        yield return new WaitForSeconds(rightSwingLength); // Wait for animation
    }

    internal static void SetLockState(bool state)
    {
        foreach (var item in PlayerCards)
        {
            if (item == null)
                continue;

            SetLockState(state, item);
        }
    }
    private static void SetLockState(bool state, GameObject card)
    {
        card.GetComponent<Button>().enabled = !state;
        card.transform.Find("WaitFrame").gameObject.SetActive(state);
    }

    private static void SetUpCard(GameObject root, HeroCard? template)
    {
        bool isBlocked = template == null || (GameManager.Instance.Player?.BlockCardOnDraw(template) ?? false);


        string portrait = template?.Portrait ?? VanillaSprites.LockIcon;
        string background = template?.GetBackgroundGUID() ?? VanillaSprites.TowerContainerPrimary;

        root.transform.Find("Portrait").GetComponent<Image>().SetSprite(portrait);
        root.transform.Find("Background").GetComponent<Image>().SetSprite(background);

        root.transform.GetComponent<Button>().interactable = !isBlocked;
        root.transform.Find("BlockFrame").gameObject.SetActive(isBlocked);
    }

    internal static void CreatePopupCard(HeroCard card, bool useRewardDescription = false)
    {
        PopupScreen.instance.SafelyQueue(screen => screen.ShowOkPopup(useRewardDescription ? card.RewardDescription : card.Description));
    }

    internal static void SetCardCursorState(int index, bool state)
    {
        if (index < PlayerCards.Length && index >= 0)
            PlayerCards[index]?.transform?.Find("SelectCard")?.gameObject?.SetActive(state);
    }

    private static void SetUpPlayerCardPrefab()
    {
        PlayerCardPrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("Card"));
    }

    #endregion Player Cards

    #region Reward UI

    private static GameObject? RewardCardPrefab;

    private static GameObject? RewardUI;
    private static Transform? RewardHolder;

    internal static void SetRewardUIActive(bool value) => RewardUI?.SetActive(value);
    internal static GameObject? GetRewardCard() => GameObject.Instantiate(RewardCardPrefab, RewardHolder);

    private static void InitializeRewardPrefab()
    {
        RewardCardPrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("Reward"));

        if (RewardCardPrefab == null)
            return;

        Transform? bannerT = RewardCardPrefab.transform.Find("Banner");
        if (bannerT != null)
        {
            //bannerT.GetComponent<Image>().SetSprite(VanillaSprites.TrophyStoreBanner);

            InitializeText(bannerT.Find("TextHolder/Text"), 50, initialText: "GrahamKracker the chad");
        }

        Transform? buttonT = RewardCardPrefab.transform.Find("Button");

        if (buttonT != null)
        {
            buttonT.gameObject.GetComponent<Image>().SetSprite(VanillaSprites.GreenBtnLong);
            InitializeText(buttonT.Find("Text"), 75, font: Fonts.Btd6FontTitle, initialText: "SELECT");
        }

        RewardCardPrefab.transform.Find("InfoBtn").GetComponent<Image>().SetSprite(VanillaSprites.InfoBtn2);
    }
    private static void SetUpRewardUI(Transform mainUI)
    {
        RewardUI = mainUI.Find("RewardUI")?.gameObject;
        RewardHolder = RewardUI?.transform.Find("Rewards/Viewport/Content");

        RewardUI?.transform.Find("Background").GetComponent<Image>().SetSprite(VanillaSprites.TrophyStoreTiledBG);

        Transform? title = RewardUI?.transform.Find("TitleHolder/Title");

        if (title != null)
        {
            InitializeText(title, 125, initialText: "CHOOSE YOUR REWARD");
        }

        InitializeRewardPrefab();
    }

    #endregion Reward UI

    #region Victory UI

    private static GameObject? VictoryUI;
    private static Button? VictoryBtn;
    
    internal static void SetVictoryBtnEnable(bool value)
    {
        if (VictoryBtn != null)
            VictoryBtn.enabled = value;
    }
    internal static void SetVictoryUIActive(bool value) => VictoryUI?.SetActive(value);

    private static void SetUpVictoryUI(Transform mainUI)
    {
        VictoryUI = mainUI.Find("VictoryUI").gameObject;
        VictoryBtn = VictoryUI?.GetComponent<Button>();
        VictoryBtn?.onClick.SetListener(new Function(GameManager.Instance.VictoryUIClose));

        if (VictoryUI != null) InitializeText(VictoryUI.transform.Find("Banner/Text"), 225,
            font: Fonts.Btd6FontTitle,
            initialText: "VICTORY");
    }

    #endregion Victory UI

    #region Effects UI

    private static GameObject? EffectSlotPrefab;

    internal static GameObject? GetEffectSlot(Transform parent) => 
        GameObject.Instantiate(EffectSlotPrefab, parent);

    private static void SetUpEffectsPrefab()
    {
        EffectSlotPrefab ??= GameObject.Instantiate(LoadAsset<GameObject>("EffectSlot"));
    }

    #endregion

    #region Coroutines

    private static List<object> CoroutinesToken = new();

    internal static void StartCoroutine(System.Collections.IEnumerator routine)
    {
        CoroutinesToken.Add(MelonLoader.MelonCoroutines.Start(routine));
    }

    internal static void ClearCoroutines()
    {
        foreach (var item in CoroutinesToken)
            MelonLoader.MelonCoroutines.Stop(item);

        CoroutinesToken.Clear();
    }

    #endregion

    internal static NK_TextMeshProUGUI? InitializeText(
       Transform @object, float fontSize, TextAlignmentOptions? textAlignment = null,
       TMP_FontAsset? font = null, string? initialText = null,
       [System.Runtime.CompilerServices.CallerMemberName] string source = "")
    {
        NK_TextMeshProUGUI text = @object.gameObject.AddComponent<NK_TextMeshProUGUI>();

        // Often because a Text component already exists
        if (text == null)
        {
            Log($"Could not add the component NK_TextMeshProUGUI to the gameobject \'{@object.name}\'", source);
            return null;
        }

        text.fontSize = fontSize;
        text.font = font ?? Fonts.Btd6FontTitle;
        text.alignment = textAlignment ?? TextAlignmentOptions.Center;
        text.UpdateText(initialText);

        return text;
    }

    internal static void ReleaseResources()
    {
        if (EnemyCardPrefab != null)
        {
            GameObject.Destroy(EnemyCardPrefab);
            EnemyCardPrefab = null;
        }

        if (RewardCardPrefab != null)
        {
            GameObject.Destroy(RewardCardPrefab);
            RewardCardPrefab = null;
        }

        if (PlayerCardPrefab != null)
        {
            GameObject.Destroy(PlayerCardPrefab);
            PlayerCardPrefab = null;
        }

        if (EffectSlotPrefab != null)
        {
            GameObject.Destroy(EffectSlotPrefab);
            EffectSlotPrefab = null;
        }

        ClearCoroutines();
    }
}