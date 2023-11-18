using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Village030 : HeroCard
{
    public override string DisplayName => "Monkey Intelligence Bureau";
    public override string Portrait => VanillaSprites.MonkeyVillage030;
    public override string Description => "Adds 6 Shield";
    public override TowerSet? Type => TowerSet.Support;

    internal override void PlayCard()
    {
        AddShield(6);
    }
}