using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CruiserTerminal.Compatibility;
using CruiserTerminal.Patches;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CruiserTerminal
{
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.zealsprince.malfunctions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(modGUID, modName, modVersion)]
    public class CTPlugin : BaseUnityPlugin
    {
        // Mod Details
        private const string modGUID = "mborsh.CruiserTerminal";
        private const string modName = "CruiserTerminal";
        private const string modVersion = "1.1.3";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static ManualLogSource mls;

        public static AssetBundle mainAssetBundle;

        private static CTPlugin Instance;

        public static GameObject terminalPrefab;

        private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

        void Awake()
        {
            NetcodePatcher();

            Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource("Cruiser Terminal");
            mls = Logger;

            var cfg = new ConfigFile(Path.Combine(Paths.ConfigPath, "mborsh.CruiserTerminal.cfg"), true);
            CTConfig.Config(cfg);

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
                LethalConfigCompat.LethalConfigSetup();

            if (!LoadAssetBundle())
            {
                mls.LogError("Failed to load asset bundle! Abort mission!");
                return;
            }

            mls.LogInfo("Cruiser Terminal loaded. Patching.");

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.zealsprince.malfunctions"))
                harmony.PatchAll(typeof(MalfunctionsCompat));

            harmony.PatchAll(typeof(CTPatches));

            bool LoadAssetBundle()
            {
                mls.LogInfo("Loading AssetBundle...");
                string sAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                mainAssetBundle = AssetBundle.LoadFromFile(Path.Combine(sAssemblyLocation, "CruiserTerminal"));

                if (mainAssetBundle == null)
                    return false;

                mls.LogInfo($"AssetBundle {mainAssetBundle.name} loaded from {sAssemblyLocation}.");
                return true;
            }
        }
    }
}
