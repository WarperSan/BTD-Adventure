using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Abstract_Classes;
using BTDAdventure.Effects;
using BTDAdventure.Managers;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;

namespace BTDAdventure.Cards.HeroCard;

public abstract class HeroCard
{
    public abstract string DisplayName { get; }

    /// <summary>
    /// GUID of the portrait sprite
    /// </summary>
    public abstract string Portrait { get; }

    /// <summary>
    /// Determines in which category (primary, magic, military) the tower is in
    /// </summary>
    public virtual TowerSet? Type { get; } = null;

    #region Background
    public string? GetBackgroundGUID() => Type switch
    {
        TowerSet.Primary => VanillaSprites.TowerContainerPrimary,
        TowerSet.Military => VanillaSprites.TowerContainerMilitary,
        TowerSet.Magic => VanillaSprites.TowerContainerMagic,
        TowerSet.Support => VanillaSprites.TowerContainerSupport,
        TowerSet.Hero => VanillaSprites.TowerContainerHero,
        TowerSet.Items => VanillaSprites.PowerContainer,
        TowerSet.Paragon => VanillaSprites.TowerContainerParagonLarge,
        _ => GetCustomBackgroundGUID(),
    };

    protected virtual string? GetCustomBackgroundGUID() => null;
    #endregion

    public virtual void PlayCard()
    {
#if DEBUG
        Log($"The card \'{DisplayName}\' was played, but it has no effect.");
#endif
    }

    #region Attack
    protected static void AttackEnemy(int amount) => AttackEnemy(new Damage(amount));
    protected static void AttackEnemy(Damage damage) => GameManager.Instance.AttackEnemy(damage);
    protected static void AttackAllEnemies(int amount) => AttackAllEnemies(new Damage(amount));
    protected static void AttackAllEnemies(Damage damage) => GameManager.Instance.AttackAllEnemies(damage);
    #endregion

    #region Effect
    protected static void AddLevelPlayer<T>(int amount) where T : Effect => GameManager.Instance.AddLevelPlayer(typeof(T), amount);
    protected static void AddLevelEnemy<T>(int amount) where T : Effect => GameManager.Instance.AddLevelEnemy(typeof(T), amount);
    #endregion

    #region Shield
    protected static void AddShield(int amount) => GameManager.Instance.Player?.AddShield(amount);
    #endregion
}

public class DartMonkey000 : HeroCard
{
    public override string Portrait => VanillaSprites.DartMonkey000;
    public override TowerSet? Type => TowerSet.Primary;

    public override string DisplayName => "Base Dart Monkey";

    public override void PlayCard()
    {
        //AddLevelPlayer<DoubleDamageEffect>(4);
        AttackEnemy(6);
    }
}

public class WizardMonkey000 : HeroCard
{
    public override string Portrait => VanillaSprites.Wizard000;
    public override TowerSet? Type => TowerSet.Magic;

    public override string DisplayName => "Base Wizard Monkey";

    public override void PlayCard()
    {
        AddLevelEnemy<WeaknessEffect>(3);
        AttackEnemy(3);
    }
}

public class MonkeyVillage000 : HeroCard
{
    public override string Portrait => VanillaSprites.MonkeyVillage000;
    public override TowerSet? Type => TowerSet.Support;

    public override string DisplayName => "Base Monkey Village";

    public override void PlayCard()
    {
        AddShield(6);
    }
}