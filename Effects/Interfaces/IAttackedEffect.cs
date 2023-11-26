namespace BTDAdventure.Effects.Interfaces;

/// <summary>
/// Determines all effects that get called whenever an entity is being attacked.
/// </summary>
public interface IAttackedEffect : IEffect
{
    public void OnPreAttacked(Entity source, Entity attacker) { }

    public void OnPostAttacked(Entity source, Entity attacker);
}

