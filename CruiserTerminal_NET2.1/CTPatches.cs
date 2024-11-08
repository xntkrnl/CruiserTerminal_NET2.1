using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal
{
    public static class CTPatches
    {
        private static CruiserTerminal cterminal;

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "Awake")]
        static void StartPatch()
        {
            CTFunctions.Spawn();

            GameObject terminalPosition = GameObject.Instantiate(CTPlugin.mainAssetBundle.LoadAsset("terminalPosition.prefab") as GameObject);
            terminalPosition.name = "terminalPosition";
            terminalPosition.transform.SetParent(GameObject.Find("CompanyCruiser(Clone)").transform);
            terminalPosition.transform.localPosition = new Vector3(1.293f, 0.938f, -3.274f);

            cterminal = GameObject.Find("CruiserTerminal(Clone)").GetComponent<CruiserTerminal>();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        static void AddPrefabsToNetwork()
        {
            CTPlugin.terminalPrefab = CTPlugin.mainAssetBundle.LoadAsset<GameObject>("CruiserTerminal.prefab");
            CTPlugin.terminalPrefab.AddComponent<CTNetworkHandler>();
            NetworkManager.Singleton.AddNetworkPrefab(CTPlugin.terminalPrefab);
        }

        [HarmonyPrefix, HarmonyPatch(typeof(VehicleController), "DestroyCar")]
        static void DestroyCarPatch()
        {
            if (cterminal.cruiserTerminalInUse)
                cterminal.QuitCruiserTerminal();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "OnDisable"), HarmonyPatch(typeof(VehicleController), "OnDisable")]
        static void TerminalOnDisablePostfix()
        {
            CTPlugin.mls.LogInfo("Destroying the terminal in a cruiser");
            GameObject.Destroy(GameObject.Find("CruiserTerminal(Clone)"));
            GameObject.Destroy(GameObject.Find("terminalPosition"));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "SetTerminalInUseClientRpc")]
        static void SetTerminalInUsePatch(ref bool ___terminalInUse)
        {
            if (cterminal != null)
            {
                cterminal.gameObject.GetComponent<CTNetworkHandler>().SetCruiserTerminalInteractableServerRpc(!___terminalInUse);
                CTPlugin.mls.LogInfo("cruiser terminal interactable = " + !___terminalInUse);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ManualCameraRenderer), "MeetsCameraEnabledConditions")]
        static void MeetsCameraEnabledConditionsPatch(ref bool __result)
        {
            if (StartOfRound.Instance == null || cterminal == null)
                return;

            if (cterminal.cruiserTerminalInUse)
                __result = true;
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "KickOffTerminalClientRpc")]
        static void KickOffTerminalClientRpcPatch()
        {
            if (cterminal == null) return;

            if (cterminal.cruiserTerminalInUse)
                cterminal.QuitCruiserTerminal();
        }
    }
}
