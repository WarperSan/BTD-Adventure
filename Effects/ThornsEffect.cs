using BTDAdventure.Managers;

namespace BTDAdventure.Effects;

internal class ThornsEffect : Effect, IAttackedEffect
{
    protected override string Name => "Thorns";
    protected override string? Image => UIManager.ThornsIcon;
    protected override int GetReduceAmount(Entity origin) => Level - LowestLevel;

    public void OnEffect(Entity source, Entity attacker)
    {
        source.AttackTarget(Level, attacker);
    }
}