using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CruiserTerminal.CTerminal
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

        public bool cruiserTerminalInUse;
        public InteractTrigger interactTrigger;
        private Transform canvasMainContainer;

        private PlayerActions playerActions;

        private Transform cruiserTerminal;
        private Transform cruiserTerminalPos;
        private Transform terminal;

        bool IHittable.Hit(int force, Vector3 hitDirection, GameNetcodeStuff.PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            if (isDestroyed)
                return true;

            CTPlugin.mls.LogInfo($"Terminal hit! Force: {force}");
            TerminalExplosionServerRPC(force);
            return true;
        }

        [ServerRpc]
        private void TerminalExplosionServerRPC(int force)
        {
            if (force <= 0 || !canBeHit || isDestroyed || !canDestroy)
                return;

            //TODO: On cruiser damage
            if (canBeHit)
            {
                health -= force;
                StartCoroutine(InvulnerabilityTime());
            }

            if (health <= 0 && !isDestroyed)
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
        private void TerminalExplosionClientRPC(bool punish)
        {
            StartCoroutine(TerminalMalfunction());
            if (punish)
                HUDManager.Instance.DisplayTip("Cruiser Terminal", "The Company's property was damaged. You will be punished for this.");
            isDestroyed = true;
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
            terminal = FindAnyObjectByType<Terminal>().transform.parent.parent;
            cruiserTerminal = base.gameObject.transform;
            cruiserTerminalPos = FindAnyObjectByType<CruiserTerminalPosition>().transform;

            maxHealth = CTConfig.maxHealth.Value;
            health = maxHealth;
            invTime = CTConfig.invTime.Value;
            canBeHit = true;
            canDestroy = CTConfig.canDestroy.Value;
            isDestroyed = false;
            punishment = CTConfig.enablePenalty.Value;
            penalty = CTConfig.penalty.Value;

            cruiserTerminalInUse = false;
            canvasMainContainer = terminal.Find("Canvas"); //not that bad as GameObject.Find() ig but this is Start() so not that much dif?
        }

        private void Update()
        {
            cruiserTerminal.position = cruiserTerminalPos.position;
            cruiserTerminal.rotation = cruiserTerminalPos.rotation;
        }

        public void BeginUsingCruiserTerminal()
        {
            playerActions.Movement.OpenMenu.performed += PressESC; //start listen esc key
            cruiserTerminalInUse = true;
        }

        public void QuitCruiserTerminal()
        {
            playerActions.Movement.OpenMenu.performed -= PressESC; //stop listen esc key
            cruiserTerminalInUse = false;
            interactTrigger.StopSpecialAnimation();
        }

        private void PressESC(InputAction.CallbackContext context)
        {
            if (context.performed && cruiserTerminalInUse)
            {
                QuitCruiserTerminal();
            }
        }

        private IEnumerator waitUntilFrameEndAndParent(bool active)
        {
            if (active)
            {
                canvasMainContainer.SetParent(cruiserTerminal);
                canvasMainContainer.localPosition = new Vector3(-0.03f, 1.4f, 0.011f);
                canvasMainContainer.localScale = new Vector3(0.004f, 0.0043f, 0.0016f);
                canvasMainContainer.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
            }
            else
            {
                canvasMainContainer.SetParent(terminal); //terminal
                canvasMainContainer.localPosition = new Vector3(-0.516f, 0.284f, 1.284f);
                canvasMainContainer.localScale = new Vector3(0.0015f, 0.0015f, 0.0016f);
                canvasMainContainer.localRotation = Quaternion.Euler(new Vector3(0f, 78.0969f, 90f));
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
