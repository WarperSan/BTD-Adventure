using BTDAdventure.Abstract;
using BTDAdventure.Entities;

namespace BTDAdventure.Enemy_Actions;

internal class ShieldAction : EnemyAction
{
    public ShieldAction() : base(Shield, "IntentWait", ShieldIcon) { }

    public override string? GetText(EnemyEntity source) => source.GetShield().ToString();

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        source.AddShield(source.GetShield());
    }
}