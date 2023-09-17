using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class NailSentry : HeroCard
{
    public override string DisplayName => "Nail Sentry";
    public override string Portrait => VanillaSprites.SentryPortrait;
    public override string Description => $"Deals {CalculateDamage(10)} damage. Gives 1 Mana. Exile";

    public override TowerSet? Type => TowerSet.Military;

    public override bool CanBeReward => false;
    public override bool ExileOnPlay => true;

    internal override void PlayCard()
    {
        AttackEnemy(10);
        AddMana(1);
    }
}