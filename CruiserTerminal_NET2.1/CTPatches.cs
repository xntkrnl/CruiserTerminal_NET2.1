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
            //CruiserTerminal.cruiserTerminalEvent += ReceivedEventFromServer;
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
            //CruiserTerminal.cruiserTerminalEvent -= ReceivedEventFromServer;
        }

        ///uuuh
/*
        static void ReceivedEventFromServer(bool inUse)
        {
            CruiserTerminal.Instance.interactTrigger.interactable = !inUse;
        }

        static void SendEventToClients(bool inUse)
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
                return;

            CruiserTerminal.Instance.SetCruiserTerminalInUseClientRpc(inUse);
        }*/
    }
}
