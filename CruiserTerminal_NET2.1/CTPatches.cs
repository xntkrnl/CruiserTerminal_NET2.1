using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal
{
    public static class CTPatches
    {
        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "Awake")]
        static void StartPatch()
        {
            CTFunctions.Spawn();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        static void AddPrefabsToNetwork()
        {
            CTPlugin.terminalPrefab = CTPlugin.mainAssetBundle.LoadAsset<GameObject>("CruiserTerminal.prefab");
            CTPlugin.terminalPrefab.AddComponent<CruiserTerminal>();
            NetworkManager.Singleton.AddNetworkPrefab(CTPlugin.terminalPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "OnDisable"), HarmonyPatch(typeof(VehicleController), "DestroyCar"), HarmonyPatch(typeof(VehicleController), "OnDisable")]
        static void TerminalOnDisablePatch()
        {
            CTPlugin.mls.LogInfo("Destroying the terminal in a cruiser");
            GameObject.Destroy(GameObject.Find("Cruiser Terminal"));
            GameObject.Destroy(GameObject.Find("terminalPosition"));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "SetTerminalInUseClientRpc")]
        static void SetTerminalInUsePatch(ref bool ___terminalInUse)
        {
            var cruiserTerminalTrigger = GameObject.Find("Cruiser Terminal");
            if (cruiserTerminalTrigger != null)
            {
                cruiserTerminalTrigger.GetComponent<CTNetworkHandler>().SetCruiserTerminalInUseServerRpc(___terminalInUse);
                CTPlugin.mls.LogInfo("cruiser terminal interactable = " + ___terminalInUse);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ManualCameraRenderer), "MeetsCameraEnabledConditions")]
        static void MeetsCameraEnabledConditionsPatch(ref bool __result)
        {
            if (StartOfRound.Instance == null)
                return;

            if (!StartOfRound.Instance.inShipPhase && !__result)
                __result = true;
        }
    }
}
