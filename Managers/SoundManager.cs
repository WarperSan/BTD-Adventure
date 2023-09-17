using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using Il2CppAssets.Scripts.Unity;
using UnityEngine;

namespace BTDAdventure.Managers;

public class SoundManager
{
    #region Constants
    public const string GeneralGroup = "TowerGenericSounds";
    #endregion

    internal static void PlaySound(string name, string groupId, float volume = 1, bool loop = false) => PlaySound<Main>(name, groupId, volume, loop);

    internal static void PlaySound<T>(string name, string groupId, float volume = 1, bool loop = false) where T : BloonsMod
    {
        return;
        var sound = LoadAsset<AudioClip>(name);

        if (sound == null)
        {
            // Check another bundle
            sound = ModContent.GetAudioClip<T>(name);

#if DEBUG
            Log($"No sound named \'{name}\' was found.");
#endif
        }

        if (sound == null)
            return;

        Game.instance.audioFactory.PlaySoundFromUnity(
            audioClip: sound,
            assetId: sound.GetName(),
            groupId: groupId,
            groupLimit: 0,
            volume: volume,
            loop: loop);
    }
}
