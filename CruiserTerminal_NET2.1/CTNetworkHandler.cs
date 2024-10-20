using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal
{
    public class CTNetworkHandler : NetworkBehaviour
    {
        internal static CTNetworkHandler Instance { get; private set; }
        public CruiserTerminal cruiserTerminal;


        private void Start()
        {
            cruiserTerminal = base.gameObject.GetComponent<CruiserTerminal>();
        }

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetCruiserTerminalInUseServerRpc(bool inUse)
        {
            CTPlugin.mls.LogMessage("sending inUse to clients: " + inUse);
            SetCruiserTerminalInUseClientRpc(inUse);
        }

        [ClientRpc]
        public void SetCruiserTerminalInUseClientRpc(bool inUse)
        {
            CTPlugin.mls.LogMessage("cruiser terminal in use: " + inUse);

            cruiserTerminal.terminalLight.enabled = inUse;
            if (inUse)
            {
                cruiserTerminal.cruiserTerminalAudio.PlayOneShot(cruiserTerminal.enterTerminalSFX);
            }
            else
            {
                cruiserTerminal.cruiserTerminalAudio.PlayOneShot(cruiserTerminal.leaveTerminalSFX);
            }
            cruiserTerminal.interactTrigger.interactable = !inUse;
            cruiserTerminal.terminalInteractTrigger.interactable = !inUse;
        }


    }
}
