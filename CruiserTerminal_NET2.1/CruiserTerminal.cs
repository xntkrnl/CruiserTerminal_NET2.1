using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

            canvasMainContainer = GameObject.Find("Environment/HangarShip/Terminal/Canvas");

            terminalScript = GameObject.Find("Environment/HangarShip/Terminal/TerminalTrigger/TerminalScript").GetComponent<Terminal>();

            cruiserTerminal = base.gameObject;

            interactTrigger = GameObject.Find("CruiserTerminal(Clone)/TerminalTrigger/Trigger").GetComponent<InteractTrigger>();
            CTNH = base.gameObject.GetComponent<CTNetworkHandler>();

            terminalInteractTrigger = terminalScript.gameObject.GetComponent<InteractTrigger>();

            cruiserTerminalAudio = GameObject.Find("CruiserTerminal(Clone)/TerminalTrigger/TerminalAudio").GetComponent<AudioSource>();
            cruiserKeyboardClips = terminalScript.keyboardClips;
            cruiserSyncedAudios = terminalScript.syncedAudios;

            terminalPos = GameObject.Find("terminalPosition");

            enterTerminalSFX = terminalScript.enterTerminalSFX;
            leaveTerminalSFX = terminalScript.leaveTerminalSFX;

            terminalLight = GameObject.Find("CruiserTerminal(Clone)/terminalLight").GetComponent<Light>();

            cruiserController = GameObject.Find("CompanyCruiser(Clone)").GetComponent<VehicleController>();
        }

        private void Update()
        {
            cruiserTerminal.transform.position = terminalPos.transform.position;
            cruiserTerminal.transform.rotation = terminalPos.transform.rotation;

            if (cruiserTerminalInUse && !cruiserController.carDestroyed)
            {
                //Destroy(canvasMainContainer);
                //canvasMainContainer = CloneCanvas();

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

        internal GameObject MoveCanvas()
        {
            return Instantiate(GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer"), GameObject.Find("CruiserTerminal(Clone)/Canvas").transform);
        }

        public void BeginUsingCruiserTerminal()
        {
            cruiserController.SetVehicleCollisionForPlayer(false, GameNetworkManager.Instance.localPlayerController);

            if (cruiserController.carDestroyed)
                return;

            StartCoroutine(waitUntilFrameEndToSetActive(true));
            terminalScript.BeginUsingTerminal();
            CTNH.SetCruiserTerminalInUseServerRpc(true);
            cruiserTerminalInUse = true;

            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Clock, 1f, 1f, 0.2f);
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
            if (active)
            {
                canvasMainContainer.transform.SetParent(cruiserTerminal.transform);
                canvasMainContainer.transform.localPosition = new Vector3(-0.03f, 1.4f, 0.011f);
                canvasMainContainer.transform.localScale = new Vector3(0.004f, 0.0043f, 0.0016f);
                canvasMainContainer.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
                
            }
            else
            {
                canvasMainContainer.transform.SetParent(terminalScript.gameObject.transform.parent.parent); //terminal
                canvasMainContainer.transform.localPosition = new Vector3(-0.516f, 0.284f, 1.284f);
                canvasMainContainer.transform.localScale = new Vector3(0.0015f, 0.0015f, 0.0016f);
                canvasMainContainer.transform.localRotation = Quaternion.Euler(new Vector3(0f, 78.0969f, 90f));
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
