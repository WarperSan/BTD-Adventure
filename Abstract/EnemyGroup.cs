using BTD_Mod_Helper.Api;

namespace BTDAdventure.Abstract;

public abstract class EnemyGroup : ModContent
{
    /// <summary>
    /// List of enemies in the group
    /// </summary>
    public abstract EnemyCard[] Enemies { get; }

    /// <summary>
    /// Type of the group
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    /// World in which the enemy group will appear.
    /// </summary>
    protected abstract World World { get; }

    /// <inheritdoc/>
    protected override sealed int Order => 2; // Load after World

    /// <inheritdoc/>
    public override sealed void Register()
    {
#if DEBUG
        Log(Name + " registered !");
#endif

        if (World == null)
            Log($"The enemy group \'{this.GetType().Name}\' is not associated with any world or the specifiec world does not exist.");
        else
            World.AddEnemyGroup(this);
    }
}

/// <typeparam name="T">World in which the enemy group will appear.</typeparam>
/// <inheritdoc/>
public abstract class EnemyGroup<T> : EnemyGroup where T : World
{
    protected override sealed World World => ModContent.GetInstance<T>();
}

/// <summary>
/// Determines all enemy groups that are of type <see cref="Components.MapGenerator.NODE_TYPE_NORMAL"/>.
/// </summary>
/// <inheritdoc/>
public abstract class NormalEnemyGroup<T> : EnemyGroup<T> where T : World
{
    public override sealed string Type => Components.MapGenerator.NODE_TYPE_NORMAL;
}

/// <summary>
/// Determines all enemy groups that are of type <see cref="Components.MapGenerator.NODE_TYPE_ELITE"/>.
/// </summary>
/// <inheritdoc/>
public abstract class EliteEnemyGroup<T> : EnemyGroup<T> where T : World
{
    public override sealed string Type => Components.MapGenerator.NODE_TYPE_ELITE;
}

/// <summary>
/// Determines all enemy groups that are of type <see cref="Components.MapGenerator.NODE_TYPE_BOSS"/>.
/// </summary>
/// <inheritdoc/>
public abstract class BossEnemyGroup<T> : EnemyGroup<T> where T : World
{
    public override sealed string Type => Components.MapGenerator.NODE_TYPE_BOSS;
}