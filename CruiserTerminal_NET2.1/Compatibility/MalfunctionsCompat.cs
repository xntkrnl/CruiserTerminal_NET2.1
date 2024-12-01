using CruiserTerminal.Patches;
using HarmonyLib;
using Malfunctions;
using Malfunctions.Patches;



namespace CruiserTerminal.Compatibility
{
    internal class MalfunctionsCompat
    {
        //electromagnetic dist + power surge
        //publicized

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRoundPatches), "HandleLevelStart")]
        static void PowerSurgeEvent()
        {
            if (State.MalfunctionPower.Triggered)
            {
                if (CTPatches.cterminal.cruiserTerminalInUse)
                    CTPatches.cterminal.QuitCruiserTerminal();

                CTPatches.cterminal.isDestroyed = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeOfDayPatches), "CheckMalfunctionDistortionTrigger")]
        static void ElectroMagneticDistortionEvent()
        {
            if (State.MalfunctionDistortion.Triggered)
            {
                if (CTPatches.cterminal.cruiserTerminalInUse)
                    CTPatches.cterminal.QuitCruiserTerminal();

                CTPatches.cterminal.isDestroyed = true;
            }
        }
    }
}
