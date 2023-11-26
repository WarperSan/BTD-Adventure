namespace BTDAdventure.Effects.Interfaces;

public interface IBlockCardEffect : IEffect
{
    public virtual void OnPlay(ref bool blocked, HeroCard card) { }

    public virtual void OnDraw(ref bool blocked, HeroCard card) { }
}