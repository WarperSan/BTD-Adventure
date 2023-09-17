using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Components;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Unity.UI_New.ChallengeEditor;
using Il2CppNinjaKiwi.Common;
using UnityEngine;

namespace BTDAdventure.Ui;

internal class StartUpUi : ModGameMenu<ExtraSettingsScreen>
{
    /// <inheritdoc/>
    public override bool OnMenuOpened(Il2CppSystem.Object data)
    {
        CommonForegroundHeader.SetText("BTD Adventure");

        var panelTransform = GameMenu.gameObject.GetComponentInChildrenByName<RectTransform>("Panel");
        var panel = panelTransform.gameObject;
        panel.DestroyAllChildren();

        var mainPanel = panel.AddModHelperPanel(new Info("MainPanel", 3600, 1900), VanillaSprites.BluePanelSmall);

        var btnStart = mainPanel.AddButton(new Info("btnStart", 0, -750, 750, 250), VanillaSprites.GreenBtnSquare,
            new System.Action(() =>
        {
            GetInstance<Main>().LoadGame("Sanctuary");
        }));
        //bossMenu.AddSlider(new Info("HPSlider", 2500, 250), 50, 1, 250, 1, Vector2.one);
        btnStart.AddText(new Info("btnText", 750, 250), "START", 75);
        return false;
    }
}