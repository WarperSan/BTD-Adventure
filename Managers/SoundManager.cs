using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts.Unity;
using UnityEngine;

namespace BTDAdventure.Managers;

public class SoundManager
{
    #region Constants

    public const string GeneralGroup = "TowerGenericSounds";

    public const string SOUND_SHUFFLE = "shuffle";

    public const string SOUND_ATTACK = "swish_attack";
    public const string SOUND_SHIELD_GAINED = "swish_shield";
    public const string SOUND_POISON_TICK = "damage_poison";
    public const string SOUND_SHIELD = "shield";
    public const string SOUND_WAIT_TICK = "tic_tac";
    public const string SOUND_DEBUFF_APPLIED = "debuff";

    public const string SOUND_FIGHT_STARTED = "laugh";
    public const string SOUND_FIGHT_WON = "victory";
    public const string SOUND_FIGHT_LOST = "defeat";

    #endregion Constants

    public static void PlaySound(string name, string groupId, float volume = 1, bool loop = false) => PlaySound<Main>(name, groupId, volume, loop);

    public static void PlaySound<T>(string name, string groupId, float volume = 1, bool loop = false)
        where T : BloonsMod
    {
        if (volume <= 0f || !Settings.GetSettingValue(Settings.SETTING_PLAY_SOUNDS, false))
            return;

        var sound = LoadAsset<AudioClip>(name);

        if (sound == null/* && typeof(T) != typeof(Main)*/)
        {
            // Check another bundle
            sound = ModContent.GetAudioClip<T>(name);
        }

        if (sound == null)
        {
#if DEBUG
            Log($"No sound named \'{name}\' was found.");
#endif
            return;
        }

        Game.instance.audioFactory.PlaySoundFromUnity(
            audioClip: sound,
            assetId: sound.GetName(),
            groupId: groupId,
            groupLimit: 0,
            volume: volume,
            loop: loop);
    }
}