using BTD_Mod_Helper.Api.Enums;
using BTDAdventure.Effects;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Wizard030 : HeroCard
{
    public override string DisplayName => "Dragon's Breath";
    public override string Portrait => VanillaSprites.Wizard030;
    public override string Description => $"Deals {CalculateDamage(3)} damage to all and adds 2 Burn to the target";
    public override TowerSet? Type => TowerSet.Magic;

    internal override void PlayCard()
    {
        AttackAllEnemies(3);
        AddLevelEnemy<BurnEffect>(2);
    }
}