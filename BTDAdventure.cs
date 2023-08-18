﻿global using static BTDAdventure.EnemyAction;
global using static BTDAdventure.Managers.GameManager;
global using static BTDAdventure.Managers.UIManager;
global using BTDAdventure.Cards.Enemies;
global using IEnumerator = System.Collections.IEnumerator;
global using BTDAdventure.Cards.EnemyCards;
global using BTDAdventure.Cards.HeroCard;

using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using BTDAdventure;
using BTDAdventure.Managers;
using Il2Cpp;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[assembly: MelonLoader.MelonInfo(typeof(BTDAdventure.BTDAdventure), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonLoader.MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace BTDAdventure;

// Order of actions:
// Fill hand
// Select card
// Select enemy
// Play card
// Place card in discard or exile
// Refill if needed
// Remove mana
// Repeat until end or no more mana
// Lock all cards
// Enemies play
// Remove hand
// Update enemies intent
// Repeat until end of fight or death

public class BTDAdventure : BloonsTD6Mod
{
    public static ModSettingDouble EnemySpeed = new(0.25)
    {
        min = 0,
        max = 2,
        slider = true,
    };

    public override void OnApplicationStart()
    {
        GameManager.Instance.Initialize();
    }

    private bool _wasFromMe = false;

    public override void OnMatchStart()
    {
        if (_wasFromMe)
        {
            GameManager.Instance.StartGame();

            _wasFromMe = false; // Reset
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
            ModHelper.Error<BTDAdventure>("Expected at least one root gameobject in PlaySocialUI");
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
            ModHelper.Error<BTDAdventure>("No button has been found.");
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

            // Does not matter
            InGameData.Editable.selectedMode = "Clicks";
            InGameData.Editable.selectedDifficulty = "Easy";
            // ---

            _wasFromMe = true;
            UI.instance.LoadGame();
        }));
    }
}