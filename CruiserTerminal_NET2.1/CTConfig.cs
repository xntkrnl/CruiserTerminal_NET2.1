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
        internal static ConfigEntry<bool> enablePenalty;
        internal static ConfigEntry<float> penalty;

        internal static void Config(ConfigFile cfg)
        {
            canDestroy = cfg.Bind("Health", "Can be destroyed", true, "Enables cruiser terminal damage");
            maxHealth = cfg.Bind("Health", "Maximum terminal health", 2, "Maximum terminal health.\nIf the value is below 1, it will set the value to 2.");
            invTime = cfg.Bind("Health", "Terminal invulnerability time", 1f, "The terminal's invulnerability time after receiving a hit.\n" +
                "Setting 0 will result in the possibility of one hit being triggered multiple times.\n" +
                "If the value is below 0, it will set the value to 1.");
            enablePenalty = cfg.Bind("Health", "Enable penalty for destruction", true);
            penalty = cfg.Bind("Health", "Penalty amount", 0.05f, "Penalty amount.\nIf the value is below 0.01 or above 0.33, it will set the value to 0.05.");

            ConfigCheck();
        }

        private static void ConfigCheck()
        {
            if(maxHealth.Value < 1)
                maxHealth.Value = 2;

            if(invTime.Value < 0)
                invTime.Value = 1f;

            if (penalty.Value < 0.01f || penalty.Value > 0.33f)
                penalty.Value = 0.05f;
        }
    }
}
