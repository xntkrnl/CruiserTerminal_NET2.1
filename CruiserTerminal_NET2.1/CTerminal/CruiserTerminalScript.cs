using CruiserTerminal.Methods;
using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CruiserTerminal.CTerminal
{
    public class CruiserTerminalScript : NetworkBehaviour, IHittable
    {
        private int maxHealth;
        internal int health;
        private float invTime;
        private bool canBeHit;
        private bool canDestroy;
        internal bool isDestroyed;
        private bool punishment;
        private float penalty;

        internal bool shipPowerSurge = false;

        public bool cruiserTerminalInUse;
        private InteractTrigger interactTrigger;
        private Transform canvasMainContainer;
        private float timeSinceLastKeyboardPress;

        private PlayerActions playerActions;

        private Transform cruiserTerminal;
        private Transform cruiserTerminalPos;
        private Transform terminal;

        private Terminal terminalScript;
        private InteractTrigger terminalInteractTrigger;

        private AudioSource audioSource;
        private AudioClip enterTerminalAudioClip;
        private AudioClip exitTerminalAudioClip;
        private AudioClip[] keyboardAudioClips;
        private Light terminalLight;

        private VehicleController cruiserController;

        bool IHittable.Hit(int force, Vector3 hitDirection, GameNetcodeStuff.PlayerControllerB playerWhoHit, bool playHitSFX, int hitID)
        {
            if (isDestroyed)
                return true;

            CTPlugin.mls.LogInfo($"Terminal hit! Force: {force}");
            TerminalExplosionServerRpc(force);
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        internal void TerminalExplosionServerRpc(int force)
        {
            if (force <= 0 || !canBeHit || isDestroyed || !canDestroy)
                return;

            if (canBeHit)
            {
                health -= force;
                StartCoroutine(InvulnerabilityTime());
            }

            if (health <= 0 && !isDestroyed)
            {
                TerminalExplosionClientRpc(punishment);
                if (punishment)
                {
                    StartOfRound.Instance.companyBuyingRate -= penalty;
                    StartOfRound.Instance.SyncCompanyBuyingRateServerRpc();
                }
            }
        }

        [ClientRpc]
        private void TerminalExplosionClientRpc(bool punish)
        {
            if (cruiserTerminalInUse)
                QuitCruiserTerminal();

            StartCoroutine(TerminalMalfunction());
            if (punish)
                HUDManager.Instance.DisplayTip("Cruiser Terminal", "The Company's property was damaged. You will be punished for this.");
            isDestroyed = true;
        }

        private IEnumerator TerminalMalfunction()
        {
            //explosions are cool
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

        internal void ResetHp()
        {
            maxHealth = CTConfig.maxHealth.Value;
            health = maxHealth;
            invTime = CTConfig.invTime.Value;
            canBeHit = true;
            canDestroy = CTConfig.canDestroy.Value;
            isDestroyed = false;
            punishment = CTConfig.enablePenalty.Value;
            penalty = CTConfig.penalty.Value;

            shipPowerSurge = false;
    }

        private void Start()
        {
            CTConfig.ConfigCheck();

            cruiserTerminal = base.gameObject.transform;
            playerActions = new PlayerActions();
            playerActions.Movement.Enable();
            cruiserController = FindAnyObjectByType<VehicleController>();

            interactTrigger = cruiserTerminal.Find("TerminalTrigger").gameObject.GetComponent<InteractTrigger>();
            //interactTrigger.onInteract.AddListener(BeginUsingCruiserTerminal);
            interactTrigger.onInteractEarly.AddListener(BeginUsingCruiserTerminal);
            interactTrigger.onCancelAnimation.AddListener(SetTerminalNoLongerInUse);

            terminalScript = FindAnyObjectByType<Terminal>();
            terminalInteractTrigger = terminalScript.gameObject.GetComponent<InteractTrigger>();
            terminal = terminalScript.transform.parent.parent;
            cruiserTerminalPos = FindAnyObjectByType<CruiserTerminalPosition>().transform;

            cruiserTerminalInUse = false;
            canvasMainContainer = terminal.Find("Canvas"); //not that bad as GameObject.Find() ig but this is Start() so not that much dif?

            audioSource = cruiserTerminal.Find("TerminalTrigger/TerminalAudio").gameObject.GetComponent<AudioSource>();
            enterTerminalAudioClip = terminalScript.enterTerminalSFX;
            exitTerminalAudioClip = terminalScript.leaveTerminalSFX;
            keyboardAudioClips = terminalScript.keyboardClips;

            terminalLight = cruiserTerminal.Find("terminalLight").GetComponent<Light>();

            ResetHp();
        }

        private void Update()
        {
            cruiserTerminal.position = cruiserTerminalPos.position;
            cruiserTerminal.rotation = cruiserTerminalPos.rotation;

            if (cruiserTerminalInUse)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    if (timeSinceLastKeyboardPress > 0.07f)
                    {
                        timeSinceLastKeyboardPress = 0f;
                        RoundManager.PlayRandomClip(audioSource, keyboardAudioClips);
                    }
                }
                timeSinceLastKeyboardPress += Time.deltaTime;
            }
        }

        public void BeginUsingCruiserTerminal(PlayerControllerB nullPlayer)
        {
            playerActions.Movement.OpenMenu.performed += PressESC; //start listen esc key

            if (isDestroyed)
                return;

            audioSource.PlayOneShot(enterTerminalAudioClip);
            SetTerminalBusyServerRpc(true, false);
            cruiserTerminalInUse = true;
            StartCoroutine(waitUntilFrameEndAndParent(true));
            terminalScript.BeginUsingTerminal();
            CTPlugin.mls.LogInfo($"Begin using cruiser terminal.");
        }

        public void QuitCruiserTerminal()
        {
            playerActions.Movement.OpenMenu.performed -= PressESC; //stop listen esc key
            if (!isDestroyed)
            {
                terminalScript.QuitTerminal();
            }
            audioSource.PlayOneShot(exitTerminalAudioClip);
            interactTrigger.StopSpecialAnimation();
        }

        public void SetTerminalNoLongerInUse(PlayerControllerB nullPlayer)
        {
            cruiserTerminalInUse = false;
            SetTerminalBusyServerRpc(false, shipPowerSurge);
            StartCoroutine(waitUntilFrameEndAndParent(false));
            CTPlugin.mls.LogInfo($"Stop using cruiser terminal.");
        }

        private void PressESC(InputAction.CallbackContext context)
        {
            if (context.performed)
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

        [ServerRpc(RequireOwnership = false)]
        internal void SetTerminalBusyServerRpc(bool busy, bool powerSurge = false)
        {
            SetTerminalBusyClientRpc(busy, powerSurge);
        }

        [ClientRpc]
        internal void SetTerminalBusyClientRpc(bool busy, bool powerSurge)
        {
            


            CTPlugin.mls.LogInfo($"Set interaction: {busy}");
            if (powerSurge)
            {
                interactTrigger.interactable = false;
                terminalInteractTrigger.interactable = false;
                terminalLight.enabled = false;
            }
            else
            {
                interactTrigger.interactable = !busy;
                terminalInteractTrigger.interactable = !busy;
                terminalLight.enabled = busy;
            }


        }
    }
}