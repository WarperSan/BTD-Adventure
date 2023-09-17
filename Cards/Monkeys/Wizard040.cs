using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Effects;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Wizard040 : HeroCard
{
    public override string DisplayName => "Summon Phoenix";
    public override string Portrait => VanillaSprites.Wizard040;
    public override string Description => $"Deals {CalculateDamage(6)} damage to all and adds 5 Burn to the target";
    public override TowerSet? Type => TowerSet.Magic;

    internal override void PlayCard()
    {
        AttackAllEnemies(6);
        AddLevelEnemy<BurnEffect>(5);
    }
}