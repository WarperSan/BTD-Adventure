using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Effects;
using BTDAdventure.Managers;
using Il2CppAssets.Scripts.Models.TowerSets;
using Color = System.Drawing.Color;

namespace BTDAdventure.Cards.HeroCard;

public abstract class HeroCard : ModContent
{
    public abstract string DisplayName { get; }

    /// <summary>
    /// GUID of the portrait sprite
    /// </summary>
    public abstract string Portrait { get; }

    public virtual string Description => "No description provided.";
    public virtual string RewardDescription => Description;

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

    internal virtual void PlayCard()
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

    protected string CalculateDamage(int amount, bool changeColor)
    {
        int realAmount = GameManager.Instance.GetPlayerDamage(amount);

        // If no color must be applied or the amount are the same
        if (!changeColor || realAmount == amount)
            return realAmount.ToString();
        return $"<color={GetDamageColor(realAmount - amount)}>" + realAmount + "</color>";
    }

    protected string GetDamageColor(int difference)
    {
        // If not override, + => green; - => red
        Color color = difference > 0 ? (GetPlusAttackColor(difference) ?? Color.FromArgb(0xFF, 0x00, 0xFF, 0x00)) :
            (GetMinusAttackColor(difference) ?? Color.FromArgb(0xFF, 0xA5, 0x2A, 0x2A));
        Log(color);
        return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
    }

    protected virtual Color? GetPlusAttackColor(int difference) => null;
    protected virtual Color? GetMinusAttackColor(int difference) => null;
    #endregion

    #region Effect
    protected static void AddLevelPlayer<T>(int amount) where T : Effect => GameManager.Instance.AddLevelPlayer<T>(amount);
    protected static void AddLevelEnemy<T>(int amount) where T : Effect => GameManager.Instance.AddLevelEnemy<T>(amount);
    protected static void AddLevelEnemyAll<T>(int amount) where T : Effect => GameManager.Instance.AddLevelToAll<T>(amount);

    protected static void AddPermanentLevelPlayer<T>(int amount) where T : Effect => GameManager.Instance.AddPermanentLevelPlayer<T>(amount);

    protected static int GetEffectPlayer<T>() where T : Effect => GameManager.Instance.GetEffectLevelPlayer<T>();
    #endregion

    #region Shield
    protected static void AddShield(int amount) => GameManager.Instance.Player?.AddShield(amount);
    #endregion

    #region Frame

    #endregion

    public override void Register()
    {
#if DEBUG
        Log(Name + " registered !");
#endif
    }
}

public class DartMonkey000 : HeroCard
{
    public override string Portrait => VanillaSprites.DartMonkey000;
    public override TowerSet? Type => TowerSet.Primary;

    public override string DisplayName => "Base Dart Monkey";

    public override string Description => $"Deals {CalculateDamage(6, true)} damage to the selected enemy";

    internal override void PlayCard()
    {
        AttackEnemy(6);
        AddLevelEnemy<PoisonEffect>(5);
        //AttackAllEnemies(50);
    }
}

public class WizardMonkey000 : HeroCard
{
    public override string Portrait => VanillaSprites.Wizard000;
    public override TowerSet? Type => TowerSet.Magic;

    public override string DisplayName => "Base Wizard Monkey";

    public override string Description => $"Applies {3} Weakness and deals {CalculateDamage(3, true)} damage";

    internal override void PlayCard()
    {
        AddLevelEnemy<WeaknessEffect>(3);
        AttackEnemy(3);
    }
}

public class Druid030 : HeroCard
{
    public override string DisplayName => "Druid 030";
    public override string Portrait => VanillaSprites.Druid030;
    public override TowerSet? Type => TowerSet.Magic;

    public override string Description => $"Deals {GetEffectPlayer<ThornsEffect>()} damage and applies {2} Thorns";
    public override string RewardDescription => $"Deals X damage to the selected enemy, where X is the number of Thorns active";

    internal override void PlayCard()
    {
        AttackEnemy(GetEffectPlayer<ThornsEffect>());
        AddPermanentLevelPlayer<ThornsEffect>(2);
    }
}

public class MonkeyVillage000 : HeroCard
{
    public override string Portrait => VanillaSprites.MonkeyVillage000;
    public override TowerSet? Type => TowerSet.Support;

    public override string DisplayName => "Base Monkey Village";

    public override string Description => $"Adds {6} shield";

    internal override void PlayCard()
    {
        AddShield(6);
    }
}

public class MonkeyAce000 : HeroCard
{
    public override string Portrait => VanillaSprites.MonkeyAce000;
    public override TowerSet? Type => TowerSet.Military;
    public override string DisplayName => "Base Monkey Ace";

    public override string Description => $"Deals {CalculateDamage(3, true)} to all enemies";

    internal override void PlayCard()
    {
        AttackAllEnemies(3);
    }
}

public class BoomerangMonkey000 : HeroCard
{
    public override string Portrait => VanillaSprites.BoomerangMonkey000;
    public override TowerSet? Type => TowerSet.Primary;
    public override string DisplayName => "Base Boomerang Monkey";

    public override string Description => $"Deals {CalculateDamage(2, true)} damage to the enemy and an extra {CalculateDamage(3, true)} to all enemies";

    internal override void PlayCard()
    {
        AttackEnemy(2);
        AttackAllEnemies(3);
    }
}

public class GlueGunner000 : HeroCard
{
    public override string Portrait => VanillaSprites.GlueGunner000;
    public override TowerSet? Type => TowerSet.Primary;
    public override string DisplayName => "Base Glue Gunner";

    public override string Description => $"Gives Double Damage to the player until the next turn";

    internal override void PlayCard()
    {
        //AddLevelEnemyAll<DoubleDamageEffect>(1);
        AddLevelPlayer<DoubleDamageEffect>(1);
    }
}