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
        public static AssetBundle assets;

        public static ManualLogSource Log;

        public static Texture2D mainLogo;
        private void Awake()
        {

            if (Instance == null) Instance = this;

            Log = base.Logger;

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
    }

    [HarmonyPatch(typeof(MenuManager), "Awake")]
    public static class MenuManagerLogoOverridePatch
    {
        public static void Postfix(MenuManager __instance)
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
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }
    }
}

