using CruiserTerminal.Patches;
using HarmonyLib;
using LittleCompany.patches;

namespace CruiserTerminal.Compatibility
{
    internal class LittleCompanyCompat
    {
        [HarmonyPrefix, HarmonyPatch(typeof(TerminalPatch), "BeginUsingTerminal"), HarmonyPatch(typeof(TerminalPatch), "QuitTerminal")]
        internal static bool LittleCompanyBeginUsingTerminalPatch()
        {
            if (CTPatches.cterminal != null && CTPatches.cterminal.cruiserTerminalInUse)
            {
                return false;
            }
            return true;
        }
    }
}
