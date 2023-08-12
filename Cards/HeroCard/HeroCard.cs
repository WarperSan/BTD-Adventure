using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Managers;
using Il2CppAssets.Scripts.Models.TowerSets;

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

    // Race
    public void AddRaceCounter()
    {

    }
}

public class Test1 : HeroCard
{
    public override string Portrait => VanillaSprites.DartMonkey000;
    public override TowerSet? Type => TowerSet.Primary;

    public override string DisplayName => "Test 1";

    public override void PlayCard()
    {
        GameManager.Instance.AttackEnemy(1);
    }
}

public class Test2 : HeroCard
{
    public override string Portrait => VanillaSprites.SuperMonkey000;
    public override TowerSet? Type => TowerSet.Magic;

    public override string DisplayName => "Test 2";

    public override void PlayCard()
    {
        GameManager.Instance.AttackAllEnemies(3);
    }
}

public class Test3 : HeroCard
{
    public override string Portrait => VanillaSprites.MonkeyVillage000;
    public override TowerSet? Type => TowerSet.Support;

    public override string DisplayName => "Test 3";
}

public class Test4 : HeroCard
{
    public override string Portrait => VanillaSprites.MonkeyAce000;
    public override TowerSet? Type => TowerSet.Military;

    public override string DisplayName => "Test 4";

    public override void PlayCard()
    {
        GameManager.Instance.AttackAllEnemies(8);
    }
}