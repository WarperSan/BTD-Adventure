using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Managers;
using Il2CppAssets.Scripts.Models.TowerSets;
using Color = System.Drawing.Color;

namespace BTDAdventure.Cards.Monkeys;

public abstract class HeroCard : ModContent
{
    /// <summary>
    /// Name to display when the card is displayed.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// GUID of the portrait sprite.
    /// </summary>
    public abstract string Portrait { get; }

    /// <summary>
    /// Description of the card at run time
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Description used when viewing the card in the reward UI.
    /// </summary>
    public virtual string RewardDescription => Description;

    /// <summary>
    /// Determines in which category (primary, magic, military) the tower is in.
    /// </summary>
    public abstract TowerSet Type { get; }

    public virtual bool ExileOnPlay => false;

    public virtual bool CanBeReward => true;

    #region Background

    /// <returns>GUID of the background used for <see cref="HeroCard.Type"/> of this card.</returns>
    public virtual string? GetBackgroundGUID() => Type switch
    {
        TowerSet.Military => VanillaSprites.TowerContainerMilitary,
        TowerSet.Magic => VanillaSprites.TowerContainerMagic,
        TowerSet.Support => VanillaSprites.TowerContainerSupport,
        TowerSet.Hero => VanillaSprites.TowerContainerHero,
        TowerSet.Items => VanillaSprites.PowerContainer,
        TowerSet.Paragon => VanillaSprites.TowerContainerParagonLarge,
        _ => VanillaSprites.TowerContainerPrimary,
    };

    #endregion Background

    internal abstract void PlayCard();

    #region Attack

    protected static int AttackEnemy(int amount) => AttackEnemy(new Damage(amount));

    protected static int AttackEnemy(Damage damage) => GameManager.Instance.AttackEnemy(damage);

    protected static void AttackAllEnemies(int amount) => AttackAllEnemies(new Damage(amount));

    protected static void AttackAllEnemies(Damage damage) => GameManager.Instance.AttackAllEnemies(damage);

    protected string CalculateDamage(int amount, bool changeColor = true)
    {
        int realAmount = GameManager.Instance.GetPlayerDamage(amount);

        // If no color must be applied or the amount are the same
        if (!changeColor || realAmount == amount)
            return realAmount.ToString();
        return $"<color={GetDamageColor(realAmount - amount)}>" + realAmount + "</color>";
    }

    protected virtual string GetDamageColor(int difference)
    {
        // If not override, + => green; - => red
        Color color = difference > 0 ? (GetPlusAttackColor(difference) ?? Color.FromArgb(0xFF, 0x00, 0xFF, 0x00)) :
            (GetMinusAttackColor(difference) ?? Color.FromArgb(0xFF, 0xA5, 0x2A, 0x2A));
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
    }

    /// <returns>Color of the text when the attack is increased.</returns>
    protected virtual Color? GetPlusAttackColor(int difference) => null;

    /// <returns>Color of the text when the attack is reduced.</returns>
    protected virtual Color? GetMinusAttackColor(int difference) => null;

    #endregion Attack

    #region Effect

    protected static void AddLevelPlayer<T>(int amount) where T : Effect => GameManager.Instance.AddLevelPlayer<T>(amount);

    /// <inheritdoc cref="GameManager.AddLevelEnemy{T}(int)"/>
    protected static void AddLevelEnemy<T>(int amount) where T : Effect => GameManager.Instance.AddLevelEnemy<T>(amount);

    /// <inheritdoc cref="GameManager.AddLevelToAll{T}(int)"/>
    protected static void AddLevelEnemyAll<T>(int amount) where T : Effect => GameManager.Instance.AddLevelToAll<T>(amount);

    /// <inheritdoc cref="GameManager.AddPermanentLevelPlayer{T}(int)"/>
    protected static void AddPermanentLevelPlayer<T>(int amount) where T : Effect => GameManager.Instance.AddPermanentLevelPlayer<T>(amount);

    protected static int GetEffectPlayer<T>() where T : Effect => GameManager.Instance.GetEffectLevelPlayer<T>();

    protected static int GetEffectEnemy<T>() where T : Effect => GameManager.Instance.GetEffectLevelEnemy<T>();

    #endregion Effect

    #region Shield

    protected static void AddShield(int amount) => GameManager.Instance.Player?.AddShield(amount, null);

    #endregion Shield

    #region Cards

    /// <inheritdoc cref="GameManager.AddCard(HeroCard)"/>
    protected static void AddCard(HeroCard card) => GameManager.Instance.AddCard(card);

    #endregion Cards

    #region Mana

    protected static void AddMana(uint amount) => GameManager.Instance.GainMana(amount);

    #endregion Mana

    #region Counter

    protected static int GetCounter(string name) => GameManager.Instance.GetCounter(name);

    protected static int AddCounter(string name, int value) => GameManager.Instance.AddCounter(name, value);

    #endregion Counter

    public override void Register()
    {
#if DEBUG
        Log(Name + " registered !");
#endif
    }
}

//public abstract class MonkeyCard : HeroCard
//{
//    /// <inheritdoc/>
//    public sealed override string DisplayName => _displayName;

//    /// <inheritdoc/>
//    public sealed override string Portrait => _portrait;

//    /// <inheritdoc/>
//    public sealed override TowerSet? Type => _type;

//    /// <summary>
//    /// Model from which the informations will be selected
//    /// </summary>
//    protected abstract TowerModel? TowerModel { get; }

//    readonly string _displayName = string.Empty;
//    readonly string _portrait = string.Empty;
//    readonly TowerSet _type = TowerSet.None;

//    public MonkeyCard()
//    {
//        if (TowerModel == null)
//            return;

//        // Portrait
//        _portrait = TowerModel.portrait.guidRef;

//        // Tower Type
//        _type = TowerModel.towerSet;

//        // Upgrade name
//        int maxValue = TowerModel.tiers.Max();

//        _displayName = TowerModel.GetUpgrade(TowerModel.tiers.IndexOf(maxValue), maxValue).LocsKey;
//    }
//}