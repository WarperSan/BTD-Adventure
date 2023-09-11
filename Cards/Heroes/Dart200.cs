using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Heroes;

internal class Dart200 : HeroCard.HeroCard
{
    public override string DisplayName => "Dart 200";
    public override string Portrait => VanillaSprites.Dartmonkey200;
    public override TowerSet? Type => TowerSet.Primary;

    public override string Description => $"Deals {CalculateDamage(GetDamage(), true)} to all.\nIncreases the strength of the attack by 1 for each use.";
    public override string RewardDescription => $"Deals 6 + X to all, where X is the number of times this card was used during this fight.\nIncreases the strength of the attack by 3 for each use.";

    internal override void PlayCard()
    {
        AttackAllEnemies(GetDamage());

        AddCounter(Dart100.Dart100Counter, 1);
    }

    private static int GetDamage() => 6 + 3 * GetCounter(Dart100.Dart100Counter);

    protected override string GetDamageColor(int difference) =>
        base.GetDamageColor(difference + GetCounter(Dart100.Dart100Counter));
}
