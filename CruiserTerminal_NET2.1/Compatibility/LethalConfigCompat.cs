using LethalConfig;
using LethalConfig.ConfigItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace CruiserTerminal.Compatibility
{
    internal class LethalConfigCompat
    {
        internal static void LethalConfigSetup()
        {
            var boolCanDestroy = new BoolCheckBoxConfigItem(CTConfig.canDestroy);
            var intMaxHealth = new IntInputFieldConfigItem(CTConfig.maxHealth);
            var floatInvTime = new FloatInputFieldConfigItem(CTConfig.invTime);
            var boolEnablePenalty = new BoolCheckBoxConfigItem(CTConfig.enablePenalty);
            var floatPenalty = new FloatInputFieldConfigItem(CTConfig.penalty);
            var boolCanCruiserDamage = new BoolCheckBoxConfigItem(CTConfig.canCruiserDamage);
            var intCruiserDamage = new IntInputFieldConfigItem(CTConfig.cruiserDamage);

            LethalConfigManager.AddConfigItem(boolCanDestroy);
            LethalConfigManager.AddConfigItem(intMaxHealth);
            LethalConfigManager.AddConfigItem(floatInvTime);
            LethalConfigManager.AddConfigItem(boolEnablePenalty);
            LethalConfigManager.AddConfigItem(floatPenalty);
            LethalConfigManager.AddConfigItem(boolCanCruiserDamage);
            LethalConfigManager.AddConfigItem(intCruiserDamage);
        }
    }
}
