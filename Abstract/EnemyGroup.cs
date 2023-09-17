using BTD_Mod_Helper.Api;

namespace BTDAdventure.Abstract;

public abstract class EnemyGroup : ModContent
{
    public abstract EnemyCard[] Enemies { get; }
    public abstract string Type { get; }

    protected abstract World World { get; }

    internal bool IsWorld(World world) => World == world;

    public override void Register()
    {
#if DEBUG
        Log(Name + " registered !");
#endif
    }
}

public abstract class EnemyGroup<T> : EnemyGroup where T : World
{
    protected override World World => ModContent.GetInstance<T>();
}

public abstract class NormalEnemyGroup<T> : EnemyGroup<T> where T : World
{
    public sealed override string Type => Components.MapGenerator.NormalNode;
}