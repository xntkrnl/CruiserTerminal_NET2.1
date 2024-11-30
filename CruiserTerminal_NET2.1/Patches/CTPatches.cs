using CruiserTerminal.Methods;
using CruiserTerminal.CTerminal;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal.Patches
{
    public static class CTPatches
    {
        internal static CruiserTerminalScript cterminal;

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        static void AddPrefabsToNetwork()
        {
            CTPlugin.terminalPrefab = CTPlugin.mainAssetBundle.LoadAsset<GameObject>("CruiserTerminal.prefab");
            CTPlugin.terminalPrefab.AddComponent<CruiserTerminalScript>();
            NetworkManager.Singleton.AddNetworkPrefab(CTPlugin.terminalPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "Awake")]
        static void AwakePatch()
        {
            CTMethods.Init();
            CTMethods.Spawn();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "DestroyCarServerRpc")]
        static void DestroyCarServerRpcPatch() //i prob could use UnityEvent or Event but whatever
        {
            cterminal.TerminalExplosionServerRpc(2147483647); //im so smart
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VehicleController), "DealDamageServerRpc")]
        static void DealDamageServerRpcPatch()
        {

            cterminal.TerminalExplosionServerRpc(1);
        }

        
    }
}
