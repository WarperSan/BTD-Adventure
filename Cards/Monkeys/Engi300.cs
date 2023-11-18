using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Monkeys;

internal class Engi300 : HeroCard
{
    private const int sentryCount = 4;

    public override string DisplayName => "Sprockets";
    public override string Portrait => VanillaSprites.EngineerMonkey300;
    public override string Description => $"Adds {sentryCount} Sentries to your deck. Exile";
    public override TowerSet? Type => TowerSet.Support;

    public override bool ExileOnPlay => true;

    internal override void PlayCard()
    {
        for (int i = 0; i < sentryCount; i++)
        {
            AddCard(new NailSentry());
        }
    }
}