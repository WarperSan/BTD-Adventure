using BTD_Mod_Helper.Api.Enums;
using Il2CppAssets.Scripts.Models.TowerSets;

namespace BTDAdventure.Cards.Heroes;

internal class Dart100 : HeroCard.HeroCard
{
    internal const string Dart100Counter = "Dart100Counter";
    public override string DisplayName => "Dart 100";
    public override string Portrait => VanillaSprites.Dartmonkey100;
    public override TowerSet? Type => TowerSet.Primary;

    public override string Description => $"Deals {CalculateDamage(2 + GetCounter(Dart100Counter), true)} to all.\nIncreases the strength of the attack by 1 for each use.";
    public override string RewardDescription => $"Deals 2 + X to all, where X is the number of times this card was used during this fight.\nIncreases the strength of the attack by 1 for each use.";

    internal override void PlayCard()
    {
        AttackAllEnemies(2 + GetCounter(Dart100Counter));

        AddCounter(Dart100Counter, 1);
    }

    protected override string GetDamageColor(int difference) => 
        base.GetDamageColor(difference + GetCounter(Dart100Counter));
}