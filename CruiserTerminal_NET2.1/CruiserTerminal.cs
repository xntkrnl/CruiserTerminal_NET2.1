using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CruiserTerminal
{
    public class CruiserTerminal : NetworkBehaviour
    {
        internal static CruiserTerminal Instance { get; private set; }
        public static event Action<bool> cruiserTerminalEvent;

        public bool cruiserTerminalInUse;
        public GameObject canvasMainContainer;
        public Terminal terminalScript;
        public GameObject cruiserTerminal;
        public InteractTrigger interactTrigger;
        public AudioSource cruiserTerminalAudio;
        public AudioClip[] cruiserKeyboardClips;
        public AudioClip[] cruiserSyncedAudios;
        public PlayerActions playerActions;
        public GameObject terminalPos;
        public AudioClip enterTerminalSFX;
        public AudioClip leaveTerminalSFX;
        public Light terminalLight;
        public VehicleController cruiserController;

        public override void OnNetworkSpawn()
        {
            cruiserTerminalEvent = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        private void Awake()
        {
            playerActions = new PlayerActions();
            playerActions.Movement.Enable();
        }

        private void Start()
        {
            cruiserTerminal = GameObject.Find("Cruiser Terminal");
            terminalPos = GameObject.Find("terminalPosition");
            cruiserController = GameObject.Find("CompanyCruiser(Clone)").GetComponent<VehicleController>();

            interactTrigger = base.gameObject.GetComponent<InteractTrigger>();

            cruiserTerminalInUse = false;

            canvasMainContainer = CloneCanvas();
            canvasMainContainer.SetActive(false);

            terminalScript = GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").GetComponent<Terminal>();

            enterTerminalSFX = terminalScript.enterTerminalSFX;
            leaveTerminalSFX = terminalScript.leaveTerminalSFX;

            //cruiserTerminalAudio = terminalScript.terminalAudio;
            cruiserKeyboardClips = terminalScript.keyboardClips;
            cruiserSyncedAudios = terminalScript.syncedAudios;

            terminalLight = GameObject.Find("Cruiser Terminal/terminalLight").GetComponent<Light>();
        }

        private void Update()
        {
            cruiserTerminal.transform.position = terminalPos.transform.position;
            cruiserTerminal.transform.rotation = terminalPos.transform.rotation;

            if (cruiserTerminalInUse)
            {
                Destroy(canvasMainContainer);
                canvasMainContainer = CloneCanvas();
            }
        }

        internal GameObject CloneCanvas()
        {
            return Instantiate(GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer"), GameObject.Find("Cruiser Terminal/Canvas").transform);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetCruiserTerminalInUseServerRpc(bool inUse)
        {
            CTPlugin.mls.LogMessage("sending stuff to clients");
            EventSetCruiserTerminalInUseClientRpc(inUse);
        }

        [ClientRpc]
        public void EventSetCruiserTerminalInUseClientRpc(bool inUse)
        {
            CTPlugin.mls.LogMessage("cruiser terminal in use: " + inUse);
            cruiserTerminalInUse = inUse;
            StartCoroutine(waitUntilFrameEndToSetActive(inUse));
            terminalLight.enabled = inUse;
            if (inUse)
            {
                cruiserTerminalAudio.PlayOneShot(enterTerminalSFX);
            }
            else
            {
                cruiserTerminalAudio.PlayOneShot(leaveTerminalSFX);
            }
            cruiserTerminalEvent.Invoke(inUse);
        }

        public void BeginUsingCruiserTerminal()
        {
            if (cruiserTerminalInUse)
            {
                interactTrigger.StopSpecialAnimation();
                return;
            }

            SetCruiserTerminalInUseServerRpc(true);
            cruiserController.SetVehicleCollisionForPlayer(false, GameNetworkManager.Instance.localPlayerController);
            terminalScript.BeginUsingTerminal();
        }

        public void StopUsingCruiserTerminal()
        {
            cruiserTerminalInUse = false;
            StartCoroutine(waitUntilFrameEndToSetActive(false));
        }

        public void QuitCruiserTerminal()
        {
            SetCruiserTerminalInUseServerRpc(false);
            cruiserController.SetVehicleCollisionForPlayer(true, GameNetworkManager.Instance.localPlayerController);
            terminalScript.QuitTerminal();
            interactTrigger.StopSpecialAnimation();
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
