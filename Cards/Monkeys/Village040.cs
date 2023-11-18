using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Village040 : HeroCard
{
    public override string DisplayName => "Call To Arms";
    public override string Portrait => VanillaSprites.MonkeyVillage040;
    public override string Description => "Adds 14 Shield. Gives 1 Mana";
    public override TowerSet? Type => TowerSet.Support;

    internal override void PlayCard()
    {
        AddShield(14);
        AddMana(1);
    }
}