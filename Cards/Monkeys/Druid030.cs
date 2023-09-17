using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Effects;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

public class Druid030 : HeroCard
{
    public override string DisplayName => "Druid of the Jungle";
    public override string Portrait => VanillaSprites.Druid030;
    public override TowerSet? Type => TowerSet.Magic;

    public override string Description => $"Deals {GetEffectPlayer<ThornsEffect>()} damage and applies 1 Thorns";
    public override string RewardDescription => $"Deals X damage to the selected enemy, where X is the number of Thorns active";

    internal override void PlayCard()
    {
        AttackEnemy(GetEffectPlayer<ThornsEffect>());
        AddPermanentLevelPlayer<ThornsEffect>(1);
    }
}