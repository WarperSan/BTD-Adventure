using BTDAdventure.Cards.EnemyCards;
using BTDAdventure.Managers;

namespace BTDAdventure.Enemy_Actions;

internal class ShieldAction : EnemyAction
{
    public ShieldAction() : base(Shield, 0, null, UIManager.ShieldIcon) { }

    public override string? GetText(EnemyCard source) => "TEMP";
}
