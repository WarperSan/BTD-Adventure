using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Boomerang300 : HeroCard
{
    internal const string BoomerangUsageCounter = "BoomerangUsageCounter";
    public override string DisplayName => "Glaives Ricochet";
    public override string Portrait => VanillaSprites.BoomerangMonkey300;
    public override string Description => $"Deals {CalculateDamage(2)} damage {GetCounter(BoomerangUsageCounter)} times";
    public override string RewardDescription => "Deals 2 damage X times, where X is increased by 1 after each use";
    public override TowerSet? Type => TowerSet.Primary;

    internal override void PlayCard()
    {
        int count = GetCounter(BoomerangUsageCounter);
        for (int i = 0; i < count; i++)
        {
            AttackEnemy(2);
        }
        AddCounter(BoomerangUsageCounter, 1);
    }
}
