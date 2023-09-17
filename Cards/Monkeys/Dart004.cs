using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Dart004 : HeroCard
{
    public override string DisplayName => "Sharp Shooter";
    public override string Portrait => VanillaSprites.Dartmonkey004;
    public override string Description => $"Deals {CalculateDamage(21)} damage to the target";

    public override TowerSet? Type => TowerSet.Primary;

    internal override void PlayCard()
    {
        AttackEnemy(21);
    }
}
