using CruiserTerminal.Methods;
using CruiserTerminal.CTerminal;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal.Patches
{
    public static class CTPatches
    {
        public static CruiserTerminalScript cterminal;

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        static void AddPrefabsToNetwork()
        {
            CTPlugin.terminalPrefab = CTPlugin.mainAssetBundle.LoadAsset<GameObject>("CruiserTerminal.prefab");
            CTPlugin.terminalPrefab.AddComponent<CruiserTerminalScript>();
            NetworkManager.Singleton.AddNetworkPrefab(CTPlugin.terminalPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "Awake")]
        static void AwakePatch(VehicleController __instance)
        {
            if (__instance.vehicleID != 0)
                return;

            CTMethods.Init();
            CTMethods.Spawn();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "DestroyCarServerRpc")]
        static void DestroyCarServerRpcPatch(VehicleController __instance) //i prob could use UnityEvent or Event but whatever
        {
            if (__instance.vehicleID != 0)
                return;

            cterminal.TerminalExplosionServerRpc(cterminal.health);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "DealDamageServerRpc")]
        static void DealDamageServerRpcPatch(VehicleController __instance)
        {
            if (__instance.vehicleID != 0)
                return;

            cterminal.TerminalExplosionServerRpc(CTConfig.cruiserDamage.Value);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "OnDisable")]
        static void OnDisablePatch(VehicleController __instance)
        {
            try
            {
                if (__instance.vehicleID != 0)
                    return;

                CTMethods.Despawn();
            }
            catch
            {
                CTPlugin.mls.LogError("Tried to despawn the terminal but it doesn't exist. If nothing broke then everything is ok =)");
            }

        }

        [HarmonyPostfix, HarmonyPatch(typeof(Terminal), "SetTerminalInUseClientRpc")]
        static void SetTerminalInUsePatch(ref bool ___terminalInUse)
        {
            if (cterminal == null)
                return;

            cterminal.SetTerminalBusyServerRpc(___terminalInUse);
            CTPlugin.mls.LogInfo("cruiser terminal interactable:" + !___terminalInUse);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(ManualCameraRenderer), "MeetsCameraEnabledConditions"), HarmonyPriority(Priority.Last)]
        static void MeetsCameraEnabledConditionsPatch(ref bool __result)
        {
            if (StartOfRound.Instance == null || cterminal == null)
                return;

            if (cterminal.cruiserTerminalInUse)
                __result = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "ShipHasLeft")]
        static void ShipHasLeftPatch()
        {
            if (cterminal == null) return;

            cterminal.SetTerminalBusyServerRpc(false);
            cterminal.ResetHp();
        }
    }
}
