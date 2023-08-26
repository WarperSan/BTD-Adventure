using BTDAdventure.Entities;
using BTDAdventure.Managers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BTDAdventure.Abstract.EnemyActions;

public abstract class IntentEffectAction : IntentAttackAction
{
    protected IntentEffectAction() : base(Weakness, UIManager.CurseIcon) { }

    public override void OnAction(EnemyEntity source, PlayerEntity player)
    {
        var method = typeof(PlayerEntity).GetMethod(nameof(PlayerEntity.AddLevel));

        if (method == null)
            return;

        foreach (var item in Effects)
        {
            if (GameManager.Instance.IsTypeAnEffect(item.Key))
            {
                method.MakeGenericMethod(item.Key).Invoke(player, new object[] { item.Value });
            }
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