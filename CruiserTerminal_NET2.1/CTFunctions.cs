using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal
{
    public static class CTFunctions
    {
        //playerpos -5 -0.4 5.5
        public static void Spawn()
        {
            if (GameObject.Find("Cruiser Terminal"))
                return;

            ///Cruiser Terminal
            var cruiser = GameObject.Find("CompanyCruiser(Clone)");
            var terminal = GameObject.Instantiate(CTPlugin.terminalPrefab);
            terminal.name = "Cruiser Terminal";

            var cruiserNO = cruiser.GetComponent<NetworkObject>();

            CTPlugin.mls.LogInfo("cruiserNO is null = " + (cruiserNO == null));

            ///terminalPosition
            GameObject terminalPosition = GameObject.Instantiate(CTPlugin.mainAssetBundle.LoadAsset("terminalPosition.prefab") as GameObject);
            terminalPosition.name = "terminalPosition";

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                terminal.GetComponent<NetworkObject>().Spawn();

            if (NetworkManager.Singleton.IsClient)
                try
                {
                    GameObject.Destroy(GameObject.Find("Cruiser Terminal(Clone)")); //strange duplication bug
                }
                catch
                {
                    
                }

            terminalPosition.transform.SetParent(cruiser.transform);
            terminalPosition.transform.localPosition = new Vector3(1.293f, 0.938f, -3.274f);
        }
    }
}