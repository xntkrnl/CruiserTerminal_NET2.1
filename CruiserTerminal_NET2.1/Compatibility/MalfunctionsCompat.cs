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
            if (State.MalfunctionPower.Triggered && CTPatches.cterminal != null)
            {
                CTPatches.cterminal.isDestroyed = true;
                CTPatches.cterminal.shipPowerSurge = true;

                if (CTPatches.cterminal.cruiserTerminalInUse)
                    CTPatches.cterminal.QuitCruiserTerminal();

            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeOfDayPatches), "CheckMalfunctionDistortionTrigger")]
        static void ElectroMagneticDistortionEvent()
        {
            if (State.MalfunctionDistortion.Triggered && CTPatches.cterminal != null)
            {
                CTPatches.cterminal.shipPowerSurge = true;
                CTPatches.cterminal.isDestroyed = true;

                if (CTPatches.cterminal.cruiserTerminalInUse)
                    CTPatches.cterminal.QuitCruiserTerminal();
            }
        }
    }
}
