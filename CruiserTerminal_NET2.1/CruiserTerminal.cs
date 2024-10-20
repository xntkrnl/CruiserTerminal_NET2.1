using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputRemoting;

namespace CruiserTerminal
{
    public class CruiserTerminal : MonoBehaviour
    {
        public bool cruiserTerminalInUse;
        public GameObject canvasMainContainer;
        public Terminal terminalScript;
        public GameObject cruiserTerminal;
        public InteractTrigger interactTrigger;
        public InteractTrigger terminalInteractTrigger;
        public AudioSource cruiserTerminalAudio;
        public AudioClip[] cruiserKeyboardClips;
        public AudioClip[] cruiserSyncedAudios;
        public PlayerActions playerActions;
        public GameObject terminalPos;
        public AudioClip enterTerminalSFX;
        public AudioClip leaveTerminalSFX;
        public Light terminalLight;
        public VehicleController cruiserController;

        private float timeSinceLastKeyboardPress;
        private CTNetworkHandler CTNH;

        private void Awake()
        {
            playerActions = new PlayerActions();
            playerActions.Movement.Enable();
        }

        private void Start()
        {
            cruiserTerminalInUse = false;

            canvasMainContainer = CloneCanvas();
            canvasMainContainer.SetActive(false);

            terminalScript = GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").GetComponent<Terminal>();

            cruiserTerminal = base.gameObject;

            interactTrigger = GameObject.Find("Cruiser Terminal/TerminalTrigger/Trigger").GetComponent<InteractTrigger>();
            //interactTrigger.onInteractEarly.AddListener(BeginUsingCruiserTerminal);
            //interactTrigger.onCancelAnimation.AddListener(SetCruiserTerminalNoLongerInUse);
            CTNH = base.gameObject.GetComponent<CTNetworkHandler>();

            terminalInteractTrigger = terminalScript.gameObject.GetComponent<InteractTrigger>();

            cruiserTerminalAudio = GameObject.Find("Cruiser Terminal/TerminalTrigger/TerminalAudio").GetComponent<AudioSource>();
            cruiserKeyboardClips = terminalScript.keyboardClips;
            cruiserSyncedAudios = terminalScript.syncedAudios;

            terminalPos = GameObject.Find("terminalPosition");

            enterTerminalSFX = terminalScript.enterTerminalSFX;
            leaveTerminalSFX = terminalScript.leaveTerminalSFX;

            terminalLight = GameObject.Find("Cruiser Terminal/terminalLight").GetComponent<Light>();

            cruiserController = GameObject.Find("CompanyCruiser(Clone)").GetComponent<VehicleController>();

            /*if (!NetworkManager.Singleton.IsHost)
            {
                interactTrigger.onInteractEarly.RemoveAllListeners();
                interactTrigger.onCancelAnimation.RemoveAllListeners();

                interactTrigger.onInteractEarly.AddListener(BeginUsingCruiserTerminal);
                interactTrigger.onCancelAnimation.AddListener(SetCruiserTerminalNoLongerInUse);
            }*/
        }

        private void Update()
        {
            cruiserTerminal.transform.position = terminalPos.transform.position;
            cruiserTerminal.transform.rotation = terminalPos.transform.rotation;

            if (cruiserTerminalInUse)
            {
                Destroy(canvasMainContainer);
                canvasMainContainer = CloneCanvas();

                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    if (timeSinceLastKeyboardPress > 0.07f)
                    {
                        timeSinceLastKeyboardPress = 0f;
                        RoundManager.PlayRandomClip(cruiserTerminalAudio, cruiserKeyboardClips);
                    }
                }
                timeSinceLastKeyboardPress += Time.deltaTime;
            }
        }

        internal GameObject CloneCanvas()
        {
            return Instantiate(GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer"), GameObject.Find("Cruiser Terminal/Canvas").transform);
        }

        //PlayerControllerB is not needed for scripts, but i need it to .AddListener() at Start()
        public void BeginUsingCruiserTerminal()
        {
            StartCoroutine(waitUntilFrameEndToSetActive(true));
            cruiserController.SetVehicleCollisionForPlayer(false, GameNetworkManager.Instance.localPlayerController);
            terminalScript.BeginUsingTerminal();
            CTNH.SetCruiserTerminalInUseServerRpc(true);
            cruiserTerminalInUse = true;

            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Clock, 1f, 0f, 0f);
        }

        public void QuitCruiserTerminal()
        {
            SetCruiserTerminalNoLongerInUse();
            terminalScript.QuitTerminal();
            interactTrigger.StopSpecialAnimation();
        }

        public void SetCruiserTerminalNoLongerInUse()
        {
            CTPlugin.mls.LogMessage("terminal no longer in use!!!");
            CTNH.SetCruiserTerminalInUseServerRpc(false);
            cruiserController.SetVehicleCollisionForPlayer(true, GameNetworkManager.Instance.localPlayerController);
            cruiserTerminalInUse = false;
            StartCoroutine(waitUntilFrameEndToSetActive(false));
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Clock, 0f, 1f, 1f);
        }

        private void OnEnable()
        {
            playerActions.Movement.OpenMenu.performed += PressESC;
        }

        private void OnDisable()
        {
            CTPlugin.mls.LogInfo("Terminal disabled, disabling ESC key listener");
            playerActions.Movement.OpenMenu.performed -= PressESC;
        }

        private void PressESC(InputAction.CallbackContext context)
        {
            interactTrigger.StopSpecialAnimation();
            if (context.performed && cruiserTerminalInUse)
            {
                QuitCruiserTerminal();
            }
        }

        private IEnumerator waitUntilFrameEndToSetActive(bool active)
        {
            yield return new WaitForEndOfFrame();
            canvasMainContainer.SetActive(active);
        }
    }
}
