using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CruiserTerminal
{
    internal class CTConfig
    {
        internal static ConfigEntry<bool> canDestroy;
        internal static ConfigEntry<int> maxHealth;
        internal static ConfigEntry<float> invTime;

        internal static void Config(ConfigFile cfg)
        {
            canDestroy = cfg.Bind("Health", "Can be destroyed", true, "Enables cruiser terminal damage");
            maxHealth = cfg.Bind("Health", "Maximum terminal health", 2, "Maximum terminal health.\nIf the value is set below 1, then the value is set to 1.");
            invTime = cfg.Bind("Health", "Terminal invulnerability time", 1f, "The terminal's invulnerability time after receiving a hit.\n" +
                "Setting 0 will result in the possibility of one hit being triggered multiple times.\n" +
                "If the value is below 0, it will set the value to 1.");

            ConfigCheck();
        }

        private static void ConfigCheck()
        {
            if(maxHealth.Value < 1)
                maxHealth.Value = 1;

            if(invTime.Value < 0)
                invTime.Value = 1;
        }
    }
}
