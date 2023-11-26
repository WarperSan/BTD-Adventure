using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using BTDAdventure.Managers;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BTDAdventure.Entities;

public class PlayerEntity : Entity
{
    public PlayerEntity(GameObject root, RogueClass rogueClass) : base(root)
    {
        SetRogueClass(rogueClass);
    }

    #region UI

    public void UpdatePlayerUI()
    {
        UpdateHealthUI();
        UpdateCoinsUI();
        UpdateBloonjaminsUI();
        UpdateShieldText();
    }

    #endregion

    #region Coins

    internal uint coins;

    private NK_TextMeshProUGUI? CashText;

    internal void AddCoins(uint amount)
    {
        coins += amount;
        UpdateCoinsUI();
    }

    private void UpdateCoinsUI() => CashText?.UpdateText(coins);

    #endregion Coins

    #region Bloonjamins

    internal uint bjms;

    private NK_TextMeshProUGUI? BloonjaminsText;

    internal void AddBloonjamins(uint amount)
    {
        bjms += amount;
        UpdateBloonjaminsUI();
    }

    private void UpdateBloonjaminsUI() => BloonjaminsText?.UpdateText(bjms);

    #endregion Bloonjamins

    #region Mana

    public uint Mana { get; internal set; }
    internal uint MaxMana = 3;

    private NK_TextMeshProUGUI? ManaText;

    internal void AddMana(uint amount)
    {
        OnManaGain?.Invoke(ref amount);
        SetMana(Mana + amount);
    }

    internal void RemoveMana(uint amount) => SetMana(Mana - amount);

    internal void ResetMana()
    {
        uint amount = MaxMana;

        OnManaReset?.Invoke(ref amount);

        SetMana(amount);
    }

    private void SetMana(uint amount)
    {
#if DEBUG
        Log($"Setting mana from {Mana} to {amount}.");
#endif

        Mana = amount;
        ManaText?.UpdateText(Mana + "/" + MaxMana);

        /*if (this.player.player_silence > 0 && i > 0 && !z)
        {
            showMessage(getString(R.string.silenced), false, 0, 0, 0, 0, 800L);
            return;
        }*/
    }

    // --- Mana Events ---
    private delegate void ManaGainEvent(ref uint amount);
    private delegate void ManaResetEvent(ref uint amount);

    private event ManaGainEvent? OnManaGain;
    private event ManaResetEvent? OnManaReset;

    #endregion Mana

    #region Piles

    private NK_TextMeshProUGUI? DrawPileText;
    private NK_TextMeshProUGUI? DiscardPileText;

    internal void UpdatePiles(CardManager cardManager)
    {
        DrawPileText?.UpdateText(cardManager.DrawPileCount);
        DiscardPileText?.UpdateText(cardManager.DiscardPileCount);
    }

    #endregion Piles

    #region Cards

    internal bool AreCardsLocked = false;

    internal bool BlockCardOnPlay(HeroCard card)
    {
        bool isBlocked = false;
        OnCardPlay?.Invoke(ref isBlocked, card);
        return isBlocked;
    }

    internal bool BlockCardOnDraw(HeroCard card)
    {
        bool isBlocked = false;
        OnCardDraw?.Invoke(ref isBlocked, card);
        return isBlocked;
    }

    // --- Cards Events ---
    private delegate void CardPlayEvent(ref bool blocked, HeroCard card);

    private event CardPlayEvent? OnCardPlay;
    private event CardPlayEvent? OnCardDraw;

    #endregion

    #region Rogue Class
#nullable disable
    internal RogueClass RogueClass;
#nullable restore

    private void SetRogueClass(RogueClass rogueClass)
    {
        RogueClass = rogueClass;
        MaxHealth = rogueClass.GetDefaultMaxHealth();
        Health = rogueClass.GetDefaultHealth();
    }

    #endregion Rogue Class

    #region Events

    protected override void ChildrenSubscribe(Effect effect)
    {
        if (effect is IBlockCardEffect blockCardEffect)
        {
            OnCardPlay += blockCardEffect.OnPlay;
            OnCardDraw += blockCardEffect.OnDraw;
        }

        if (effect is IManaEffect manaGainEffect)
        {
            OnManaGain += manaGainEffect.OnManaGained;
            OnManaReset += manaGainEffect.OnManaReset;
        }
    }

    protected override void ChildrenUnsubscribe(Effect effect)
    {
        if (effect is IBlockCardEffect blockCardEffect)
        {
            OnCardPlay -= blockCardEffect.OnPlay;
            OnCardDraw -= blockCardEffect.OnDraw;
        }

        if (effect is IManaEffect manaGainEffect)
        {
            OnManaGain -= manaGainEffect.OnManaGained;
            OnManaReset -= manaGainEffect.OnManaReset;
        }
    }

    #endregion

    // --- Entity ---
    #region Health

    protected override bool ScaleUpMaxHP => true;

    protected override void SetUpHealthUI(GameObject root)
    {
        GameObject health = root.transform.Find(UIManager.OVERLAY_VALUES_PATH + "/Health").gameObject;
        HealthImg = health.transform.Find("Shield/Health").gameObject;
        HealthImg.GetComponentInChildren<Image>().SetSprite(VanillaSprites.LivesIcon);
        HealthText = UIManager.InitializeText(health.transform.Find("Text"), UIManager.FONT_SIZE_TOP_VALUES);
    }

    protected override void OnDeath() => GameManager.Instance.OnDefeat();

    #endregion

    #region Shield

    protected override void SetUpShieldUI(GameObject root)
    {
        const string shieldPath = UIManager.OVERLAY_VALUES_PATH + "/Health/Shield";

        ShieldImg = root.transform.Find(shieldPath + "/Icon")?.gameObject;

        if (ShieldImg != null)
            ShieldText = UIManager.InitializeText(ShieldImg.transform.Find("Text"), UIManager.FONT_SIZE_TOP_VALUES / 2);
    }

    #endregion

    #region Damage

    internal void SetDamage(int amount) => Damage = amount;

    #endregion Damage

    #region UI

    protected override void SetUpExtraUI(GameObject root)
    {
        // Cash (coins)
        GameObject cash = root.transform.Find(UIManager.OVERLAY_VALUES_PATH + "/Cash").gameObject;
        cash.GetComponentInChildren<Image>().SetSprite(VanillaSprites.CoinIcon);
        CashText = UIManager.InitializeText(cash.transform.Find("Text"), UIManager.FONT_SIZE_TOP_VALUES);

        // Bloonjamins (gems)
        GameObject bloonjamins = root.transform.Find(UIManager.OVERLAY_VALUES_PATH + "/Bloonjamins").gameObject;
        bloonjamins.GetComponentInChildren<Image>().SetSprite(VanillaSprites.BloonjaminsIcon);
        BloonjaminsText = UIManager.InitializeText(bloonjamins.transform.Find("Text"), UIManager.FONT_SIZE_TOP_VALUES);

        // Mana
        ManaText = UIManager.InitializeText(root.transform.Find("GameUI/Mana/Text"), 110, font: Fonts.Btd6FontBody);

        // Bottom group
        Transform bottomGroup = root.transform.Find("GameUI/BottomGroup");
        bottomGroup.Find("Background").GetComponent<Image>().SetSprite(VanillaSprites.MainBGPanelBlueNotchesShadow);
        bottomGroup.Find("Status").GetComponent<Image>().SetSprite(VanillaSprites.BlueInsertPanelRound);

        // Draw pile
        DrawPileText = UIManager.InitializeText(bottomGroup.Find("DrawPile/Text"), 75, TextAlignmentOptions.Left);

        // Discard pile
        DiscardPileText = UIManager.InitializeText(bottomGroup.Find("DiscardPile/Text"), 75, TextAlignmentOptions.Right);
    }

    #endregion UI

    #region Effect

    protected override void SetUpEffectUI(GameObject root)
    {
        EffectHolder = root.transform.Find("GameUI/BottomGroup/Status/Scroll View/Viewport/Content")?.gameObject;
    }

    #endregion Effect
}