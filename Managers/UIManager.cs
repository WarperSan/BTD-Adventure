using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Entities;
using Il2Cpp;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
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

    public const string WoundIcon = "Ui[BTDAdventure-icon_wound]";
    public const string WoundExtremeIcon = "Ui[BTDAdventure-innate_wounded_hit]";
    public const string PoisonIcon = "Ui[BTDAdventure-icon_poison]";
    public const string BurnIcon = "Ui[BTDAdventure-icon_burn]";
    public const string FrailIcon = "Ui[BTDAdventure-icon_frail]";
    public const string ThornsIcon = "Ui[BTDAdventure-icon_thorns]";

    public const float TopValueFontSize = 125;
    #endregion

    private Transform? EnemyCardsHolder;
    private Transform? PlayerCardsHolder;
    private readonly GameObject?[] PlayerCards = new GameObject?[MaxPlayerCardCount];

    private Button? EndTurnBtn;

    private GameObject? LoadingScreen;

    public Transform? SetUpMainUI()
    {
        SetActiveUiRect(false);

        GameObject? mainUIprefab = GameManager.LoadAsset<GameObject>("BTDAdventureUI");

        if (mainUIprefab == null)
            return null;

        Transform mainUI = UnityEngine.Object.Instantiate(mainUIprefab, InGame.instance.GetInGameUI().transform).transform;

        LoadingScreen = mainUI.Find("LoadingUI")?.gameObject;
        LoadingScreen?.transform.Find("Wheel").GetComponent<Image>().SetSprite(VanillaSprites.LoadingWheel);

        SetUpVictoryUI(mainUI);
        SetUpRewardUI(mainUI);

        EnemyCardsHolder = mainUI.Find("GameUI/EnemyCardsHolder");
        PlayerCardsHolder = mainUI.Find("GameUI/PlayerCards/Holder");

        EndTurnBtn = mainUI.Find("Overlay/Button").GetComponent<Button>();
        EndTurnBtn.onClick.AddListener(new Function(GameManager.Instance.EndTurn));

        // Enemies
        EnemyCardPrefab = InitializeEnemyPrefab();

        // Create enemies objects
        for (int i = 0; i < EnemiesObject.Length; i++)
        {
            EnemiesObject[i] = UnityEngine.Object.Instantiate(EnemyCardPrefab, EnemyCardsHolder);
        }
        // ---

        mainUI.Find("Overlay/TopPanel/Settings").GetComponent<Image>().SetSprite(VanillaSprites.SettingsBtn);

        return mainUI;
    }

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
    #endregion

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
        text.text = initialText;

        return text;
    }

    #region Player Cards
    internal void InitPlayerCards()
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
        root.transform.Find("Portrait").GetComponent<Image>().SetSprite(template.Portrait);
        root.transform.Find("Background").GetComponent<Image>().SetSprite(template.GetBackgroundGUID());
    }
    #endregion

    #region Enemy
    private GameObject? EnemyCardPrefab;

    private readonly GameObject?[] EnemiesObject = new GameObject?[MaxEnemiesCount];

    internal EnemyEntity? AddEnemy(Type t, int position)
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

            return new EnemyEntity(enemyObject, position, t);
        }
        catch (Exception e)
        {
            Log(e.Message);
            throw;
        }
    }
    internal void SetEnemyState(bool state, int position) => EnemiesObject[position]?.SetActive(state);

    internal void KillEnemy(int position) => MelonLoader.MelonCoroutines.Start(EnemyDies(position));

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

    #region Static
    private static void SetLockState(bool state, GameObject card) => card.transform.Find("WaitFrame").gameObject.SetActive(state);

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

    IEnumerator EnemyDies(int position)
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
        }
        else
            yield return new WaitForEndOfFrame();
    }
    #endregion
}