using CruiserTerminal.CTerminal;
using CruiserTerminal.Patches;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal.Methods
{
    internal class CTMethods
    {
        private static bool isSpawned;
        private static bool isHostOrServer;

        internal static void Init()
        {
            isHostOrServer = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
            isSpawned = false;
        }

        internal static void Spawn()
        {
            if (isSpawned) return; //just in case

            if (isHostOrServer)
            {
                var terminalGO = GameObject.Instantiate(CTPlugin.terminalPrefab);
                CTPatches.cterminal = terminalGO.GetComponent<CruiserTerminalScript>();
                var terminalNO = terminalGO.GetComponent<NetworkObject>();

                terminalNO.Spawn();
            }

            // move/rotate terminalPosition if you want move/rotate cruiser terminal for some reason
            var terminalPosition = GameObject.Instantiate(CTPlugin.mainAssetBundle.LoadAsset("terminalPosition.prefab") as GameObject);
            terminalPosition.name = "terminalPosition";
            terminalPosition.transform.SetParent(GameObject.Find("CompanyCruiser(Clone)").transform);
            terminalPosition.transform.localPosition = new Vector3(1.293f, 0.938f, -3.274f);

            isSpawned = true;
        }
    }
}
