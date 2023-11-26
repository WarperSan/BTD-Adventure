namespace BTDAdventure.Effects.Interfaces;

public interface ITurnEffect : IEffect
{
    public void OnPreTurn(Entity entity) { }

    public void OnPostTurn(Entity entity) { }
}
