using BTDAdventure.Entities;

namespace BTDAdventure.Effects.Interfaces;

/// <summary>
/// Defines all the effects that get called whenever the entity does an action.
/// </summary>
public interface IActionEffect : IEffect
{
    /// <summary>
    /// Determines if the effect should call the method specific of the entity type or the generic one.
    /// If true, the effect will either call <see cref="OnCardPlay(PlayerEntity, HeroCard)"/> or
    /// <see cref="OnEntityPlay(Entity)"/>; Otherwise, <see cref="OnEntityPlay(Entity)"/> will be called.
    /// </summary>
    bool CheckEntityType { get; }

    sealed void OnAction(Entity source, HeroCard? card)
    {
        if (CheckEntityType)
        {
            if (source is PlayerEntity player)
            {
                if (card != null)
                {
                    OnCardPlay(player, card);
                    return;
                }
            }
            else if (source is EnemyEntity enemy)
            {
                OnEnemyPlay(enemy);
                return;
            }
            else
                Log($"The entity {source} is neither a {typeof(PlayerEntity).Name}, nor an {typeof(EnemyEntity).Name}.");
        }
        OnEntityPlay(source);
    }

    protected void OnCardPlay(PlayerEntity player, HeroCard cardPlayed) { }

    protected void OnEnemyPlay(EnemyEntity enemy) { }

    protected void OnEntityPlay(Entity entity) { }
}