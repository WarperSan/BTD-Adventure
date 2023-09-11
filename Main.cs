global using BTDAdventure.Abstract;
global using BTDAdventure.Cards.EnemyCards;
global using BTDAdventure.Cards.HeroCard;
global using static BTDAdventure.Abstract.EnemyAction;
global using static BTDAdventure.Managers.GameManager;
global using static BTDAdventure.Managers.UIManager;
global using IEnumerator = System.Collections.IEnumerator;

using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using BTDAdventure;
using BTDAdventure.Components;
using BTDAdventure.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[assembly: MelonLoader.MelonInfo(typeof(Main), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonLoader.MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BTDAdventure;

public class Main : BloonsTD6Mod
{
    internal static Material? blurMat = null;
    public static ModSettingHotkey ShowDescription = new(KeyCode.R);

    public override void OnApplicationStart()
    {
        GameManager.Instance.Initialize();
        InjectTypes();
    }

    private void InjectTypes()
    {
        Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<ShaderEngine_CameraBehavior>();
        Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<MapNode>();
        //Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<MapGenerator.Path>();
        Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<MapGenerator>();
        Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<DeleteObject>();
    }

    public override void OnUpdate()
    {
        if (InGame.instance == null)
            return;

        if (ShowDescription.JustPressed())
        {
            var c = GameManager.Instance.GetCard();

            if (c == null)
                Log("No card is selected");
            else
                UIManager.CreatePopupCard(c);
        }
    }

    private bool _wasFromMe = false;

    public override void OnMatchStart()
    {
        if (_wasFromMe)
        {
            GameManager.Instance.StartGame();

            _wasFromMe = false; // Reset
        }
        else
        {
            GameObject.Find("Engine")?.transform.Find("Objects")?.gameObject.SetActive(true);

            // Remove every camera that has ShaderEngine_CameraBehavior
            if (Camera.allCameras.Count > 0)
            {
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.gameObject.HasComponent<ShaderEngine_CameraBehavior>(out var shader))
                    {
                        shader.UseShader = false;
                    }
                }
            }
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "PlaySocialUI")
        {
            OnPlaySocialUI();
        }
    }
    private void OnPlaySocialUI()
    {
        Scene playSocialScene = SceneManager.GetSceneByName("PlaySocialUI");

        if (playSocialScene.rootCount < 1)
        {
            ModHelper.Error<Main>("Expected at least one root gameobject in PlaySocialUI");
            return;
        }

        Transform rootItem = playSocialScene.GetRootGameObjects()[0].transform;

        Transform contentBrowserContainer = rootItem.Find("PlaySocialScreen/BottomGroup/ContentBrowser");
        contentBrowserContainer.GetComponent<RectTransform>().sizeDelta = new(3_726, 812);
        // x = (padding * (# - 1) + size * #) * 1.35 = (280 * 3 + 480 * 4) * 1.35 = 3_726

        Transform buttonHolder = contentBrowserContainer.Find("Buttons");
        GameObject? buttonOG = null;

        // Check for all buttons (safe check)
        for (int i = 0; i < buttonHolder.childCount; i++)
        {
            Transform btnChild = buttonHolder.GetChild(i);

            if (!btnChild.gameObject.activeInHierarchy)
                continue;

            if (btnChild.GetComponent<Button>() != null)
            {
                buttonOG = btnChild.gameObject;
                break;
            }
        }

        if (buttonOG == null)
        {
            ModHelper.Error<Main>("No button has been found.");
            return;
        }

        GameObject btdAdBtn = GameObject.Instantiate(buttonOG, buttonHolder);
        btdAdBtn.name = "BTD Adventure BTN";
        btdAdBtn.GetComponentInChildren<NK_TextMeshProUGUI>().localizeKey = "KYS";
        //btdAdBtn.GetComponentInChildren<Image>().SetSprite(VanillaSprites.LoadingCloudPuff);

        // Change Icon

        btdAdBtn.GetComponent<Button>().AddOnClick(new Function(() =>
        {
            // Load game
            // https://github.com/Void-n-Null/QuickGame/blob/main/QuickGame.cs W
            InGameData.Editable.selectedMap = "MiddleOfTheRoad"; // Map type
            InGameData.Editable.selectedMode = "Easy"; // Removes the popup
            
            // Does not matter
            InGameData.Editable.selectedDifficulty = "Easy";
            // ---

            _wasFromMe = true;
            UI.instance.LoadGame();
        }));
    }

    #region Settings
    internal static ModSettingDouble EnemySpeed = new(0.25)
    {
        min = 0,
        max = 2,
        slider = true,
        displayName = "Enemy Delay",
        description = "Determines the amount of seconds the enemy will have to wait before acting"
    };

    internal static ModSettingBool ShowCardCursor = new(true)
    {
        displayName = "Card Cursor",
        description = "Determines if the card cursor should be showed or not"
    };
    #endregion
}