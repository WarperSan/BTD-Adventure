using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Ace300 : HeroCard
{
    public override string DisplayName => "Fighter Plane";
    public override string Portrait => VanillaSprites.MonkeyAce300;
    public override string Description => $"Deals {CalculateDamage(1)} damage to all 8 times";
    public override TowerSet? Type => TowerSet.Military;

    internal override void PlayCard()
    {
        for (int i = 0; i < 8; i++)
        {
            AttackAllEnemies(1);
        }
    }
}