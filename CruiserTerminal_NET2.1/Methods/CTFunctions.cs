using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal.Methods
{
    public static class CTFunctions
    {
        //playerpos -5 -0.4 5.5
        public static void Spawn()
        {
            if (GameObject.Find("CruiserTerminal(Clone)"))
                return;

            ///Cruiser Terminal
            var cruiser = GameObject.Find("CompanyCruiser(Clone)");
            var cruiserNO = cruiser.GetComponent<NetworkObject>();

            CTPlugin.mls.LogInfo("cruiserNO is null = " + (cruiserNO == null));

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var terminal = Object.Instantiate(CTPlugin.terminalPrefab);
                terminal.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}