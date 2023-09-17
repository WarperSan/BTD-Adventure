using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Dart003 : HeroCard
{
    public override string DisplayName => "Crossbow";
    public override string Portrait => VanillaSprites.Dartmonkey003;
    public override string Description => $"Deals {CalculateDamage(6)} damage to the target";
    public override TowerSet? Type => TowerSet.Primary;

    internal override void PlayCard()
    {
        AttackEnemy(6);
    }
}