using BepInEx.Configuration;

namespace AlliesAvoidImplosions
{
    internal class Configuration
    {
        internal static ConfigEntry<string>
            additionalBackupEntries,
            blacklistedBackupEntries,
            additionalImmuneEntries,
            blacklistedImmuneEntries;

        internal static ConfigEntry<bool> immuneToVoidDeath;
        internal static ConfigEntry<float> evasionDistance;

        internal static void Init(ConfigFile config)
        {
            additionalBackupEntries = config.Bind("1. Retreat", "Additional Retreat Entities", "EngiWalkerTurretMaster",
                "Additional characters that back away from implosions. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
            blacklistedBackupEntries = config.Bind("1. Retreat", "Blacklisted Retreat Entities", "EmergencyDroneMaster", "Any characters to be blacklisted from exhibiting this behaviour. Mostly because another mod also modifies the AISkillDrivers to use the custom target and it creates an incompatibility. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
            immuneToVoidDeath = config.Bind("2. Immunity", "Immune To Void Death", false, "Allies affected by this mod are immune to void death. A sure-fire survival alternative.");
            additionalImmuneEntries = config.Bind("2. Immunity", "Additional Immune Entities", "",
                "Additional characters to exhibit this behaviour. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
            blacklistedImmuneEntries = config.Bind("2. Immunity", "Blacklisted Immune Entries", "", "Any characters to be blacklisted from exhibiting this behaviour. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
            evasionDistance = config.Bind("3. General", "Evasion Distance", 35f, "The distance to back up from an implosion. The longer the distance, the safer the minion but the longer it will override its behaviour and prevent it from engaging other enemies. For reference, a Void Devastator's implosion has a radius of 22.5 m.");
        }
    }
}