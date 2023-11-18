using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Boomerang400 : HeroCard
{
    public override string DisplayName => "M.O.A.R. Richochet";

    public override string Portrait => VanillaSprites.BoomerangMonkey400;
    public override string Description => $"Deals {CalculateDamage(5)} damage {GetCounter(Boomerang300.BoomerangUsageCounter)} times";

    public override string RewardDescription => "Deals 5 damage X times, where X is increased by 1 after each use";

    public override TowerSet? Type => TowerSet.Primary;

    internal override void PlayCard()
    {
        int count = GetCounter(Boomerang300.BoomerangUsageCounter);
        for (int i = 0; i < count; i++)
        {
            AttackEnemy(5);
        }
        AddCounter(Boomerang300.BoomerangUsageCounter, 1);
    }
}