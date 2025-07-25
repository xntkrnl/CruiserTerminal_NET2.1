﻿using CruiserTerminal.Patches;
using HarmonyLib;
using Malfunctions;
using Malfunctions.Patches;



namespace CruiserTerminal.Compatibility
{
    internal class MalfunctionsCompat
    {
        //electromagnetic dist + power surge

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRoundPatches), "HandleLevelStart")]
        static void PowerSurgeEvent()
        {
            if (State.MalfunctionPower.Triggered && CTPatches.cterminal != null)
            {
                CTPatches.cterminal.shipPowerSurge = true;

                if (CTPatches.cterminal.cruiserTerminalInUse)
                    CTPatches.cterminal.QuitCruiserTerminal();

                CTPatches.cterminal.isDestroyed = true;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TimeOfDayPatches), "CheckMalfunctionDistortionTrigger")]
        static void ElectroMagneticDistortionEvent()
        {
            if (State.MalfunctionDistortion.Triggered && CTPatches.cterminal != null)
            {
                CTPatches.cterminal.shipPowerSurge = true;

                if (CTPatches.cterminal.cruiserTerminalInUse)
                    CTPatches.cterminal.QuitCruiserTerminal();


                CTPatches.cterminal.isDestroyed = true;
            }
        }
    }
}
