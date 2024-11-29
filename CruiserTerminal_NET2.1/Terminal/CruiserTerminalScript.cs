using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace CruiserTerminal.Terminal
{
    public class CruiserTerminalScript : NetworkBehaviour, IHittable
    {
        //TODO: make config
        public int health;
        private Transform cruiserTerminal;
        private Transform cruiserTerminalPos;

        bool IHittable.Hit(int force, Vector3 hitDirection, GameNetcodeStuff.PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            CTPlugin.mls.LogInfo("Terminal hit!");
            TerminalExplosionServerRPC();
            return true;
        }

        [ServerRpc]
        void TerminalExplosionServerRPC()
        {
            health--;
            if (health == 0)
            {
                TerminalExplosionClientRPC();

                //base.gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }

        [ClientRpc]
        void TerminalExplosionClientRPC()
        {
            StartCoroutine(TerminalMalfunction());
        }

        private IEnumerator TerminalMalfunction()
        {
            yield return new WaitForSeconds(0.1f);
            Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 2, 5, 1);
            yield return new WaitForSeconds(0.6f);
            Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 3, 5, 1);
            yield return new WaitForSeconds(1.6f);
            Landmine.SpawnExplosion(gameObject.transform.position, true, 0, 3, 5, 1);
        }

        private void Start()
        {
            health = 2;
            cruiserTerminal = base.gameObject.transform;
            cruiserTerminalPos = FindAnyObjectByType<CruiserTerminalPosition>().transform;

        }

        private void Update()
        {
            cruiserTerminal.position = cruiserTerminalPos.position;
            cruiserTerminal.rotation = cruiserTerminalPos.rotation;
        }
    }
}
