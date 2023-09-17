using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Ace400 : HeroCard
{
    public override string DisplayName => "Operation: Dart Storm";
    public override string Portrait => VanillaSprites.MonkeyAce400;
    public override string Description => $"Deals {CalculateDamage(1)} damage to all 24 times";
    public override TowerSet? Type => TowerSet.Military;

    internal override void PlayCard()
    {
        for (int i = 0; i < 24; i++)
        {
            AttackAllEnemies(1);
        }
    }
}