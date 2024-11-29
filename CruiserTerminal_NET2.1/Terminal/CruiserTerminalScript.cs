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
        private int maxHealth; //config
        private int health;

        private float invTime; //config
        private bool canBeHit;
        private bool canDestroy;
        private bool isDestroyed;

        private Transform cruiserTerminal;
        private Transform cruiserTerminalPos;


        bool IHittable.Hit(int force, Vector3 hitDirection, GameNetcodeStuff.PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            if (!canDestroy)
                return true;

            CTPlugin.mls.LogInfo("Terminal hit!");
            TerminalExplosionServerRPC();
            return true;
        }

        [ServerRpc]
        void TerminalExplosionServerRPC()
        {
            //TODO: On cruiser damage
            if (canBeHit)
            {
                health--;
                StartCoroutine(InvulnerabilityTime());
            }

            if (health == 0)
            {
                TerminalExplosionClientRPC();
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

        private IEnumerator InvulnerabilityTime()
        {
            canBeHit = false;
            yield return new WaitForSeconds(invTime);
            canBeHit = true;
        }

        private void Start()
        {
            maxHealth = 2;
            health = maxHealth;
            invTime = 1f;
            canBeHit = true;
            canDestroy = true;
            isDestroyed = false;

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
