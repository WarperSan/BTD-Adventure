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
    internal bool AreCardsLocked = false;

    public PlayerEntity(GameObject? root, int maxHealth, RogueClass rogueClass) : base(root)
    {
        MaxHealth = maxHealth;
        Health = maxHealth;
        RogueClass = rogueClass;
    }

    #region Health

    protected override bool ScaleUpMaxHP => true;

    protected override void SetUpHealthUI(GameObject root)
    {
        GameObject health = root.transform.Find(ValuesPath + "/Health").gameObject;
        HealthImg = health.transform.Find("Shield/Health").gameObject;
        HealthImg.GetComponentInChildren<Image>().SetSprite(VanillaSprites.LivesIcon);
        HealthText = InitializeText(health.transform.Find("Text"), TopValueFontSize);
    }

    protected override void OnDeath() => GameManager.Instance.OnDefeat();

    #endregion Health

    #region Shield

    protected override void SetUpShieldUI(GameObject root)
    {
        const string shieldPath = UIManager.ValuesPath + "/Health/Shield";

        ShieldImg = root.transform.Find(shieldPath + "/Icon")?.gameObject;

        if (ShieldImg != null)
            ShieldText = InitializeText(ShieldImg.transform.Find("Text"), TopValueFontSize / 2);
    }

    #endregion Shield

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

        // Check for mana effects

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

    #region Damage

    internal void SetDamage(int amount) => Damage = amount;

    #endregion Damage

    #region Effect

    #region Card Block

    private delegate void CardPlayEvent(ref bool blocked, HeroCard card);

    private event CardPlayEvent OnCardPlay;

    internal bool BlockCardOnPlay(HeroCard card)
    {
        bool isBlocked = false;
        OnCardPlay?.Invoke(ref isBlocked, card);
        return isBlocked;
    }

    private event CardPlayEvent OnCardDraw;

    internal bool BlockCardOnDraw(HeroCard card)
    {
        bool isBlocked = false;
        OnCardDraw?.Invoke(ref isBlocked, card);
        return isBlocked;
    }

    #endregion Card Block

    #region Mana Gain

    private delegate void ManaGainEvent(ref uint amount);

    private event ManaGainEvent OnManaGain;

    #endregion Mana Gain

    protected override void SetUpEffectUI(GameObject root)
    {
        EffectHolder = root.transform.Find("GameUI/BottomGroup/Status/Scroll View/Viewport/Content")?.gameObject;
    }

    protected override void ChildrenSubscribe(Effect effect)
    {
        if (effect is IBlockCardEffect blockCardEffect)
        {
            OnCardPlay += blockCardEffect.OnPlay;
            OnCardDraw += blockCardEffect.OnDraw;
        }

        if (effect is IManaGainEffect manaGainEffect)
        {
            OnManaGain += manaGainEffect.OnManaGained;
        }
    }

    protected override void ChildrenUnsubscribe(Effect effect)
    {
        if (effect is IBlockCardEffect blockCardEffect)
        {
            OnCardPlay -= blockCardEffect.OnPlay;
            OnCardDraw -= blockCardEffect.OnDraw;
        }

        if (effect is IManaGainEffect manaGainEffect)
        {
            OnManaGain -= manaGainEffect.OnManaGained;
        }
    }

    #endregion Effect

    #region UI

    protected override void SetUpExtraUI(GameObject root)
    {
        // Cash (coins)
        GameObject cash = root.transform.Find(ValuesPath + "/Cash").gameObject;
        cash.GetComponentInChildren<Image>().SetSprite(VanillaSprites.CoinIcon);
        CashText = InitializeText(cash.transform.Find("Text"), TopValueFontSize);

        // Bloonjamins (gems)
        GameObject bloonjamins = root.transform.Find(ValuesPath + "/Bloonjamins").gameObject;
        bloonjamins.GetComponentInChildren<Image>().SetSprite(VanillaSprites.BloonjaminsIcon);
        BloonjaminsText = InitializeText(bloonjamins.transform.Find("Text"), TopValueFontSize);

        // Mana
        ManaText = InitializeText(root.transform.Find("GameUI/Mana/Text"), 110, font: Fonts.Btd6FontBody);

        // Bottom group
        Transform bottomGroup = root.transform.Find("GameUI/BottomGroup");
        bottomGroup.Find("Background").GetComponent<Image>().SetSprite(VanillaSprites.MainBGPanelBlueNotchesShadow);
        bottomGroup.Find("Status").GetComponent<Image>().SetSprite(VanillaSprites.BlueInsertPanelRound);

        // Draw pile
        DrawPileText = InitializeText(bottomGroup.Find("DrawPile/Text"), 75, TextAlignmentOptions.Left);

        // Discard pile
        DiscardPileText = InitializeText(bottomGroup.Find("DiscardPile/Text"), 75, TextAlignmentOptions.Right);
    }

    public void UpdatePlayerUI()
    {
        UpdateHealthUI();
        UpdateCoinsUI();
        UpdateBloonjaminsUI();
        UpdateShieldText();
    }

    #endregion UI

    #region Rogue Class

    internal RogueClass RogueClass;

    #endregion Rogue Class
}