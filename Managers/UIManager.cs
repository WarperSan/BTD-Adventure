using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Components;
using BTDAdventure.Entities;
using Il2Cpp;
using Il2CppAssets.Scripts.Unity.Menu;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.Popups;
using Il2CppNinjaKiwi.Common;
using Il2CppTMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Managers;

internal class UIManager
{
    #region Constants
    public const string ValuesPath = "Overlay/TopPanel/Values";
    public const string DamageIcon = "Ui[BTDAdventure-intent_damage]";
    public const string DoubleDamageIcon = "Ui[BTDAdventure-intent_damage_double]";
    public const string CurseIcon = "Ui[BTDAdventure-intent_negate]";
    public const string WaitIcon = "Ui[BTDAdventure-intent_pause]";
    public const string ShieldIcon = "Ui[BTDAdventure-intent_shield-1]";
    public const string SpawnIcon = "Ui[BTDAdventure-intent_evoke]";

    public const string WoundIcon = "Ui[BTDAdventure-icon_wound]";
    public const string WoundExtremeIcon = "Ui[BTDAdventure-innate_wounded_hit]";
    public const string PoisonIcon = "Ui[BTDAdventure-icon_poison]";
    public const string BurnIcon = "Ui[BTDAdventure-icon_burn]";
    public const string FrailIcon = "Ui[BTDAdventure-icon_frail]";
    public const string ThornsIcon = "Ui[BTDAdventure-icon_thorns]";
    public const string OverchargedIcon = "Ui[BTDAdventure-icon_overcharged]";

    public const float TopValueFontSize = 125;
    #endregion

    private Transform? EnemyCardsHolder;
    private Transform? PlayerCardsHolder;
    private readonly GameObject?[] PlayerCards = new GameObject?[MaxPlayerCardCount];

    internal Button? EndTurnBtn;

    private GameObject? LoadingScreen;

    internal Transform? SetUpMainUI()
    {
        SetActiveUiRect(false);

        GameObject? mainUIprefab = GameManager.LoadAsset<GameObject>("BTDAdventureUI");

        if (mainUIprefab == null)
            return null;

        Transform mainUI = UnityEngine.Object.Instantiate(mainUIprefab, InGame.instance.GetInGameUI().transform).transform;

        // Engine Object
        var engineObject = GameObject.Find("Engine").transform;
        menuManager = engineObject.Find("Game")?.GetComponent<MenuManager>();
        engineObject.Find("Objects")?.gameObject.SetActive(false); // Disable track arrows
        // ---

        LoadingScreen = mainUI.Find("LoadingUI")?.gameObject;
        LoadingScreen?.transform.Find("Wheel").GetComponent<Image>().SetSprite(VanillaSprites.LoadingWheel);

        SetUpVictoryUI(mainUI);
        SetUpRewardUI(mainUI);
        SetUpMapUI(mainUI);
        SetUpDeathUI(mainUI);

        // Game UI
        GameUI = mainUI.Find("GameUI").gameObject;
        // ---

        EnemyCardsHolder = GameUI.transform.Find("EnemyCardsHolder");
        PlayerCardsHolder = GameUI.transform.Find("PlayerCards/Holder");

        // End Btn
        var endTurnObject = GameUI.transform.Find("EndTurn");
        InitializeText(endTurnObject.Find("Text"), 20, TextAlignmentOptions.Baseline, initialText: "END TURN");
        EndTurnBtn = endTurnObject.GetComponent<Button>();
        EndTurnBtn.onClick.AddListener(new Function(() =>
        {
            this.EndTurnBtn.interactable = false;
            GameManager.Instance.EndTurn();
        }));
        // ---

        // Enemies
        EnemyCardPrefab ??= InitializeEnemyPrefab();

        // Create enemies objects
        for (int i = 0; i < EnemiesObject.Length; i++)
        {
            EnemiesObject[i] = UnityEngine.Object.Instantiate(EnemyCardPrefab, EnemyCardsHolder);
        }
        // ---

        mainUI.Find("Overlay/TopPanel/Settings").GetComponent<Image>().SetSprite(VanillaSprites.SettingsBtn);

        return mainUI;
    }

    internal static NK_TextMeshProUGUI? InitializeText(
        Transform @object,
        float fontSize,
        TextAlignmentOptions? textAlignment = null,
        TMP_FontAsset? font = null,
        string? initialText = null,
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

    #region GameUI
    internal GameObject? GameUI;
    private MenuManager? menuManager;
    #endregion

    #region Victory UI
    internal GameObject? VictoryUI;
    internal Button? VictoryBtn;

    private void SetUpVictoryUI(Transform mainUI)
    {
        VictoryUI = mainUI.Find("VictoryUI").gameObject;
        VictoryBtn = VictoryUI?.GetComponent<Button>();
        VictoryBtn?.onClick.SetListener(new Function(GameManager.Instance.VictoryUIClose));

        if (VictoryUI != null) InitializeText(VictoryUI.transform.Find("Banner/Text"), 225,
            font: Fonts.Btd6FontTitle,
            initialText: "VICTORY");
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
        GameObject? RewardCardPrefab = GameManager.Instance.RewardCardPrefab ?? GameObject.Instantiate(LoadAsset<GameObject>("Reward"));

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
                buttonT.gameObject.GetComponent<Image>().SetSprite(VanillaSprites.GreenBtnLong);
                InitializeText(buttonT.Find("Text"), 75, font: Fonts.Btd6FontTitle, initialText: "SELECT");
            }

            RewardCardPrefab.transform.Find("InfoBtn").GetComponent<Image>().SetSprite(VanillaSprites.InfoBtn2);
        }
        return RewardCardPrefab;
    }

    internal static void CreatePopupReward(RewardManager.Reward? reward)
    {
        if (!reward.HasValue)
            return;

        if (reward.Value.HeroCard != null)
            CreatePopupCard(reward.Value.HeroCard, true);
    }
    #endregion

    #region Player Cards
    internal void InitPlayerCards()
    {
        if (GameManager.Instance.PlayerCardPrefab == null)
        {
            Log("\'PlayerCardPrefab\' was not defined");
            return;
        }

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

    internal void SetUpPlayerCard(int index, HeroCard heroCard, bool swingAnimation)
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

        if (swingAnimation)
        {
            MelonLoader.MelonCoroutines.Start(SwingCard(selectedCard, new Action(() =>
            {
                SetUpCard(selectedCard, heroCard);
            })));
        }
        else
            SetUpCard(selectedCard, heroCard);
    }

    internal void SetLockState(bool state)
    {
        foreach (var item in PlayerCards)
        {
            if (item == null)
                continue;

            SetLockState(state, item);
        }
    }

    private static void SetUpCard(GameObject root, HeroCard template)
    {
        bool isBlocked = GameManager.Instance.Player?.BlockCardOnDraw(template) ?? false;

        root.transform.Find("Portrait").GetComponent<Image>().SetSprite(template.Portrait);
        root.transform.Find("Background").GetComponent<Image>().SetSprite(template.GetBackgroundGUID());

        root.transform.GetComponent<Button>().interactable = !isBlocked;
        root.transform.Find("BlockFrame").gameObject.SetActive(isBlocked);
    }

    internal static void CreatePopupCard(HeroCard card, bool useRewardDescription = false)
    {
        PopupScreen.instance.SafelyQueue(screen => screen.ShowOkPopup(useRewardDescription ? card.RewardDescription : card.Description));
    }

    internal void SetCardCursorState(int index, bool state)
    {
        PlayerCards[index]?.transform?.Find("SelectCard")?.gameObject?.SetActive(state);
    }
    #endregion

    #region Enemy
    private GameObject? EnemyCardPrefab;

    private readonly GameObject?[] EnemiesObject = new GameObject?[MaxEnemiesCount];

    internal EnemyEntity? AddEnemy(EnemyCard enemyCard, int position)
    {
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

            enemyObject.GetComponent<Button>().enabled = true;
            enemyObject.SetActive(true);
            //SetEnemyState(true, position);

            return new EnemyEntity(enemyObject, position, enemyCard);
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
    }
    internal void SetEnemyState(bool state, int position) => EnemiesObject[position]?.SetActive(state);

    internal void KillEnemy(int position, EnemyEntity? entity) => MelonLoader.MelonCoroutines.Start(EnemyDies(position, entity));

    internal static GameObject? InitializeEnemyPrefab()
    {
        GameObject? EnemyCardPrefab = GameObject.Instantiate(LoadAsset<GameObject>("EnemyCard"));

        if (EnemyCardPrefab != null)
        {
            InitializeText(EnemyCardPrefab.transform.Find("Intent/Text"), 50, initialText: "yuh uh");
            InitializeText(EnemyCardPrefab.transform.Find("HP/TextHolder/Text"), 25, initialText: "nuh uh");

            //EnemyCardPrefab.transform.Find("Shield")?.GetComponent<Image>().SetSprite(UIManager.ShieldIcon);
        }
        return EnemyCardPrefab;
    }
    #endregion

    #region Map
    internal MapGenerator? MapGenerator;
    private void SetUpMapUI(Transform mainUI)
    {
        MapGenerator = mainUI.Find("MapGenerator").gameObject.AddComponent<MapGenerator>();

        MapGenerator.SetUp(MapGenerator.transform.Find("Scroll View/Viewport/Content/MapObjects")?.GetComponent<RectTransform>());

        GameManager.Instance.SetWord(new Forest());
        MapGenerator.GenerateMap(UnityEngine.Random.Range(0, 100));
        MapGenerator.ProgressLayer();
    }
    #endregion

    #region Death
    private GameObject? DeathUI;
    private void SetUpDeathUI(Transform mainUI)
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

    internal void ShowDeathUI() => DeathUI?.SetActive(true);
    #endregion

    #region Static
    private static void SetLockState(bool state, GameObject card)
    {
        card.GetComponent<Button>().enabled = !state;
        card.transform.Find("WaitFrame").gameObject.SetActive(state);
    }

    internal static void SetActiveUiRect(bool isActive)
    {
        //InGame.instance.uiRect.gameObject.SetActive(isActive);

        for (int i = 0; i < InGame.instance.uiRect.childCount; ++i)
        {
            GameObject @object = InGame.instance.uiRect.GetChild(i).gameObject;

            if (@object.name == "BlackBars(Clone)")
                continue;

            @object.SetActiveRecursively(false);
        }

#if DEBUG
        Log($"InGameUI is now {(isActive ? "active" : "inactive")}.");
#endif
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Causes the loading screen to appear
    /// </summary>
    /// <param name="preLoad">Called before the fade in starts</param>
    /// <param name="postLoad">Called before the fade out starts</param>
    public void UseLoading(Action? preLoad = null, Action? postLoad = null) => MelonLoader.MelonCoroutines.Start(UseLoadingCoroutine(preLoad, postLoad));

    private IEnumerator UseLoadingCoroutine(Action? preLoad = null, Action? postLoad = null)
    {
        LoadingScreen?.SetActive(true);

        if (menuManager != null)
            menuManager.enabled = false;

        Animator? animator = LoadingScreen?.GetComponent<Animator>();

        preLoad?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        animator?.Play("LoadingFadeIn");
        yield return new WaitForSeconds(0.75f); // Wait for fade in

        postLoad?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        animator?.Play("LoadingFadeOut");

        if (menuManager != null)
            menuManager.enabled = true;

        yield return new WaitForSeconds(0.25f); // Wait for fade out

        LoadingScreen?.SetActive(false);
    }

    static IEnumerator SwingCard(GameObject card, Action? afterLeftSwing)
    {
        Animator animator = card.GetComponent<Animator>();

        animator.Play("PlayerCardLeftSwig");

        yield return new WaitForEndOfFrame(); // Wait for load
        yield return new WaitForSeconds(0.1667f); // Wait for animation

        SetLockState(false, card);
        afterLeftSwing?.Invoke();
        yield return new WaitForEndOfFrame(); // Wait for load

        animator.Play("PlayerCardRightSwig");
        yield return new WaitForEndOfFrame(); // Wait for load
        yield return new WaitForSeconds(0.1667f); // Wait for animation
    }

    IEnumerator EnemyDies(int position, EnemyEntity? entity)
    {
        GameObject? enemy = EnemiesObject[position];

        if (enemy != null)
        {
            enemy.GetComponent<Button>().enabled = false;

            Animator animator = enemy.GetComponent<Animator>();

            animator.Play("EnemyDie");

            yield return new WaitForEndOfFrame(); // Wait for load
            yield return new WaitForSeconds(0.6667f); // Wait for animation

            SetEnemyState(false, position);
            EnemiesObject[position]?.transform.Find("Shield")?.gameObject.SetActive(false); // Disable shield on death

            entity?.EffectVisual.DestroyAllChildren();
        }
        else
            yield return new WaitForEndOfFrame();
    }
    #endregion
}