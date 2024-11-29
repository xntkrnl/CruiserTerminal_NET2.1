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
        private int maxHealth;
        private int health;
        private float invTime;
        private bool canBeHit;
        private bool canDestroy;
        private bool isDestroyed;
        private bool punishment;
        private float penalty;

        private Transform cruiserTerminal;
        private Transform cruiserTerminalPos;

        bool IHittable.Hit(int force, Vector3 hitDirection, GameNetcodeStuff.PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            if (!canDestroy || isDestroyed)
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
                TerminalExplosionClientRPC(punishment);
                if (punishment)
                {
                    StartOfRound.Instance.companyBuyingRate -= penalty;
                    StartOfRound.Instance.SyncCompanyBuyingRateServerRpc();
                }
            }
        }

        [ClientRpc]
        void TerminalExplosionClientRPC(bool punish)
        {
            StartCoroutine(TerminalMalfunction());
            if (punish)
                HUDManager.Instance.DisplayTip("Cruiser Terminal not responding", "The Сompany's property was damaged. You will be punished for this.");
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
            maxHealth = CTConfig.maxHealth.Value;
            health = maxHealth;
            invTime = CTConfig.invTime.Value;
            canBeHit = true;
            canDestroy = CTConfig.canDestroy.Value;
            isDestroyed = false;
            punishment = CTConfig.enablePenalty.Value;
            penalty = CTConfig.penalty.Value;

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
