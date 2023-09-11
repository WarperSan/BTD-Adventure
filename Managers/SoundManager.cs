using Il2CppAssets.Scripts.Unity;
using UnityEngine;

namespace BTDAdventure.Managers;

public class SoundManager
{
    #region Constants
    public const string GeneralGroup = "TowerGenericSounds";
    #endregion

    internal static void PlaySound(string name, string groupId, float volume = 1, bool loop = false)
    {
        var sound = LoadAsset<AudioClip>(name);

        if (sound == null)
        {
            // Check another bundle

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
