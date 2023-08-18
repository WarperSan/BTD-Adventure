using BTDAdventure.Entities;
using BTDAdventure.Managers;
using System;
using System.Collections.Generic;

namespace BTDAdventure.Abstract_Classes.EnemyActions;

public abstract class IntentEffectAction : IntentAttackAction
{
    protected IntentEffectAction(uint order) : base(Weakness, order, UIManager.CurseIcon) { }

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        foreach (var item in Effects)
        {
            if (GameManager.Instance.IsTypeAnEffect(item.Key))
                player.AddLevel(item.Key, item.Value);
        }
    }

    /// <summary>
    /// List of effects to apply during the action, with their level
    /// </summary>
    protected abstract Dictionary<Type, int> Effects { get; }

    public override string? GetText(EnemyEntity source)
    {
        string? text = "";

        int i = 0;
        foreach (var (_, lvl) in Effects)
        {
            // "1-2-3"

            text += lvl;

            if (i + 1 < Effects.Count)
                text += "-";

            i++;
        }

        return text;
    }
}