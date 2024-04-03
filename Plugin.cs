using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TilesEdition.Utils;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using TMPro;

namespace TilesEdition
{
    public static class PluginInformation
    {
        public const string PLUGIN_NAME = "TilesEdition";
        public const string PLUGIN_VERSION = "1.0.0";
        public const string PLUGIN_GUID = "io.github.nelertile.TilesEdition";
    }



    [BepInPlugin(PluginInformation.PLUGIN_GUID, PluginInformation.PLUGIN_NAME, PluginInformation.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony(PluginInformation.PLUGIN_GUID);

        private static Plugin Instance;
        public static ConfigEntry<bool> configDisplayCustomMainMenuLogo;
        public static ConfigEntry<bool> configReformatWeight;
        public static ConfigEntry<bool> configReformatTime;
        public static AssetBundle assets;

        public static ManualLogSource Log;

        public static Texture2D mainLogo;
        private void Awake()
        {

            if (Instance == null) Instance = this;

            Log = base.Logger;
            InitConfig();

            // Plugin startup logic
            try
            {
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Log.LogError("Failed to patch: " + e);
            }

            Log.LogInfo("Loading TilesEdition...");

            AssetBundle bundle = BundleUtilities.LoadBundleFromInternalAssembly("tilesedition.assets", Assembly.GetExecutingAssembly());
            mainLogo = bundle.LoadPersistentAsset<Texture2D>("lethaltilesedition.png");



            Log.LogInfo($"Plugin {PluginInformation.PLUGIN_GUID} is loaded!");
        }

        private void InitConfig()
        {
            configDisplayCustomMainMenuLogo = Config.Bind(
                "General",
                "DisplayCustomMainMenuLogo",
                true,
                "Toggle main menu logo changes"
            );

            configReformatWeight = Config.Bind(
                "General.Reformat",
                "ReformatWeight",
                true,
                "Toggle weight reformatting"
            );

            configReformatTime = Config.Bind(
                "General.Reformat",
                "ReformatTime",
                true,
                "Toggle time reformatting"
            );
        }
    }

    [HarmonyPatch(typeof(MenuManager), "Awake")]
    public static class MenuManagerLogoOverridePatch
    {
        public static void Postfix(MenuManager __instance)
        {
            if (Plugin.configDisplayCustomMainMenuLogo.Value)
            {
                try
                {
                    GameObject parent = __instance.transform.parent.gameObject;

                    Sprite logoImage = Sprite.Create(Plugin.mainLogo, new Rect(0, 0, Plugin.mainLogo.width, Plugin.mainLogo.height), new Vector2(0.5f, 0.5f));

                    Transform mainLogo = parent.transform.Find("MenuContainer/MainButtons/HeaderImage");
                    if (mainLogo != null)
                    {
                        mainLogo.gameObject.GetComponent<Image>().sprite = logoImage;
                    }
                    Transform loadingScreen = parent.transform.Find("MenuContainer/LoadingScreen");
                    if (loadingScreen != null)
                    {
                        loadingScreen.localScale = new Vector3(1.02f, 1.06f, 1.02f);
                        Transform loadingLogo = loadingScreen.Find("Image");
                        if (loadingLogo != null)
                        {
                            loadingLogo.GetComponent<Image>().sprite = logoImage;
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
            }

        }
    }

    [HarmonyPatch(typeof(HUDManager), "Update")]
    public static class HUDManagerWeightFormatPatch
    {
        [HarmonyPostfix]
        private static void SetClock(ref TextMeshProUGUI ___weightCounter, ref Animator ___weightCounterAnimator)
        {
            if (Plugin.configReformatWeight.Value)
            {
                float num = Mathf.RoundToInt(Mathf.Clamp((GameNetworkManager.Instance.localPlayerController.carryWeight - 1f) * 0.4535f, 0f, 100f) * 105f);
                float num2 = Mathf.RoundToInt(Mathf.Clamp(GameNetworkManager.Instance.localPlayerController.carryWeight - 1f, 0f, 100f) * 105f);
                ((TMP_Text)___weightCounter).text = $"{num} kg";
                ___weightCounterAnimator.SetFloat("weight", num2 / 130f);
            }
        }
    }

    [HarmonyPatch(typeof(HUDManager), "SetClock")]
    public static class HUDManagerClockFormatPatch
    {
        [HarmonyPrefix]
        private static bool SetClock(ref TextMeshProUGUI ___clockNumber, ref float timeNormalized, ref float numberOfHours)
        {
            if (Plugin.configReformatTime.Value)
            {
                int num = (int)(timeNormalized * (60f * numberOfHours)) + 360;
                int num2 = (int)Mathf.Floor((float)(num / 60));
                int num3 = num % 60;
                ((TMP_Text)___clockNumber).text = $"{num2:00}:{num3:00}".TrimStart(new char[1] { '0' });
                return false;
            }
            return true;
        }
    }

}

