using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal
{
    public static class CTPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "Start")]
        static void StartPatch()
        {
            CTFunctions.Spawn();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        static void AddPrefabsToNetwork()
        {
            CTPlugin.terminalPrefab = CTPlugin.mainAssetBundle.LoadAsset<GameObject>("CruiserTerminal.prefab");
            NetworkManager.Singleton.AddNetworkPrefab(CTPlugin.terminalPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "OnDisable")]
        static void TerminalOnDisablePatch()
        {
            GameObject.Destroy(GameObject.Find("Cruiser Terminal"));
            GameObject.Destroy(GameObject.Find("terminalPosition"));
        }
    }
}
