using BTDAdventure.Abstract.EnemyActions;
using System.Collections.Generic;
using BTDAdventure.Effects;

namespace BTDAdventure.Enemy_Actions;

public class WeaknessAction : IntentEffectAction
{
    public WeaknessAction() : base() { }

    protected override Dictionary<System.Type, int> Effects => new()
    {
        [typeof(WeaknessEffect)] = 1,
    };
}
