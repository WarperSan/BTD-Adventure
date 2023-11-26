using BTDAdventure.Managers;

namespace BTDAdventure.Effects.Classes;

internal class ThornsEffect : Effect, IAttackedEffect
{
    protected override string Name => "Thorns";
    protected override string? Image => UIManager.ICON_THORNS;

    protected override int GetReduceAmount(Entity origin) => Level - LowestLevel;

    public void OnPostAttacked(Entity source, Entity attacker) => source.AttackTarget(Level, attacker);
}