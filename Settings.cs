using BTD_Mod_Helper.Api.Data;
using BTD_Mod_Helper.Api.ModOptions;

namespace BTDAdventure;

internal class Settings : ModSettings
{
    internal static T? GetSettingValue<T>(string settingName, T? defaultValue = default)
    {
        ModSetting? modSetting = settingName switch
        {
            SETTING_ENEMY_SPEED => EnemySpeed,
            SETTING_SHOW_CARD_CURSOR => ShowCardCursor,
            SETTING_RANDOM_MAP_OFFSET => RandomMapOffset,
            SETTING_CAP_HEALTH_GAIN => CapHealthGain,
            SETTING_SHOW_NEGATIVE_OVERFLOW => ShowNegativeOverflow,
            SETTING_PLAY_SOUNDS => PlaySFX,
            SETTING_SHOW_VISUALS => ShowVisuals,
            SETTING_SKIP_ENEMY_ANIMATION => SkipEnemyAnimation,
            _ => null
        };

        if (modSetting == null)
        {
            Log($"No setting associated with the name \'{settingName}\'.");
            return defaultValue;
        }

        object value = modSetting.GetValue();

        try
        {
            return (T?)value;
        }
        catch (System.Exception e)
        {
            Log(e.Message);
        }
        return default;
    }

    #region UI Tweaks

    internal const string SETTING_SHOW_CARD_CURSOR = "ShowCardCursor";
    internal const string SETTING_RANDOM_MAP_OFFSET = "RandomMapOffset";
    internal const string SETTING_SHOW_NEGATIVE_OVERFLOW = "ShowNegativeOverflow";

    private static readonly ModSettingCategory UITweaks = new("UI Tweaks")
    {
        collapsed = true,
    };

    private static readonly ModSettingBool ShowCardCursor = new(true)
    {
        displayName = "Card Cursor",
        description = "Determines if the card cursor should be showed or not.",
        category = UITweaks,
    };
    private static readonly ModSettingBool RandomMapOffset = new(false)
    {
        displayName = "Random Map Offset",
        description = "Determines if the nodes should have a random offset or not.",
        category = UITweaks,
    };
    private static readonly ModSettingBool ShowNegativeOverflow = new(true)
    {
        displayName = "Show Negative Overflow",
        description = "Determines if the mod should display the health in the negative if an attack overkills an entity.",
        category = UITweaks,
    };

    #endregion
    #region Sound Tweaks

    internal const string SETTING_PLAY_SOUNDS = "PlaySFX";

    private static readonly ModSettingCategory SoundTweaks = new("Sound Tweaks")
    {
        collapsed = true,
    };

    private static readonly ModSettingBool PlaySFX = new(true)
    {
        displayName = "Play SFXs",
        description = "Determines if the mod should play the sound effects.",
        category = SoundTweaks,
    };

    #endregion
    #region In-Game Tweaks

    internal const string SETTING_ENEMY_SPEED = "EnemySpeed";
    internal const string SETTING_SHOW_VISUALS = "ShowVisuals";
    internal const string SETTING_SKIP_ENEMY_ANIMATION = "SkipEnemyAnimation";

    private static readonly ModSettingCategory InGameTweaks = new("In-Game Tweaks")
    {
        collapsed = true,
    };

    private static readonly ModSettingDouble EnemySpeed = new(0.25)
    {
        min = 0,
        max = 2,
        slider = true,
        displayName = "Enemy Delay",
        description = "Determines the amount of seconds the enemy will have to wait before acting.",
        category = InGameTweaks,
    };
    private static readonly ModSettingBool SkipEnemyAnimation = new(false)
    {
        displayName = "Skip Enemy Animation",
        description = "Determines if the enemies should do their animations or not.",
        category = InGameTweaks,
    };
    private static readonly ModSettingBool ShowVisuals = new(true)
    {
        displayName = "Show Visuals",
        description = "Determines if the mod should show the visuals created by attacks/effects.",
        category = InGameTweaks,
    };

    #endregion
    #region Cheats

    internal const string SETTING_CAP_HEALTH_GAIN = "CapHealthGain";

    private static readonly ModSettingCategory CheatsCategory = new("Cheats")
    {
        collapsed = true,
        order = int.MaxValue,
    };

    private static readonly ModSettingBool CapHealthGain = new(false)
    {
        displayName = "Cap Health Gain",
        description = "Determines entites can heal past their maximum health. This affects both the player and the enemies.",
        category = CheatsCategory,
    };

    #endregion
}
