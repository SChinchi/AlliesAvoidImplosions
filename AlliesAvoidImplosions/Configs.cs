using BepInEx.Bootstrap;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AlliesAvoidImplosions;

internal class Configs
{
    private const string RETREAT_SECTION = "1. Retreat";
    private const string IMMUNITY_SECTION = "2. Immunity";
    private const string GENERAL_SECTION = "3. General";

    internal static ConfigEntry<string> AdditionalBackupEntries { get; private set; }
    internal static ConfigEntry<string> BlacklistedBackupEntries { get; private set; }
    internal static ConfigEntry<string> AdditionalImmuneEntries { get; private set; }
    internal static ConfigEntry<string> BlacklistedImmuneEntries { get; private set; }
    internal static ConfigEntry<bool> ImmuneToVoidDeath { get; private set; }
    internal static ConfigEntry<float> EvasionDistance { get; private set; }

    internal static void Init(ConfigFile config, string assemblyLocation)
    {
        AdditionalBackupEntries = config.Bind(RETREAT_SECTION, "Additional Retreat Entities", "EngiWalkerTurretMaster",
            "Additional characters that back away from implosions. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
        BlacklistedBackupEntries = config.Bind(RETREAT_SECTION, "Blacklisted Retreat Entities", "", "Any characters to be blacklisted from exhibiting this behaviour. Mostly because another mod also modifies the AISkillDrivers to use the custom target and it creates an incompatibility. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
        ImmuneToVoidDeath = config.Bind(IMMUNITY_SECTION, "Immune To Void Death", false, "Allies affected by this mod are immune to void death. A sure-fire survival alternative.");
        AdditionalImmuneEntries = config.Bind(IMMUNITY_SECTION, "Additional Immune Entities", "",
            "Additional characters to exhibit this behaviour. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
        BlacklistedImmuneEntries = config.Bind(IMMUNITY_SECTION, "Blacklisted Immune Entries", "", "Any characters to be blacklisted from exhibiting this behaviour. Use the master name of the desired entities separated by comma. You can get a list of all masters with `list_ai` from DebugToolkit.");
        EvasionDistance = config.Bind(GENERAL_SECTION, "Evasion Distance", 35f, "The distance to back up from an implosion. The longer the distance, the safer the minion but the longer it will override its behaviour and prevent it from engaging other enemies. For reference, a Void Devastator's implosion has a radius of 22.5 m.");
        EvasionDistance.SettingChanged += UpdateEvasionDistance;

        if (Chainloader.PluginInfos.ContainsKey(AlliesAvoidImplosions.RiskOfOptionsGUID))
        {
            RiskOfOptionsInit(assemblyLocation);
        }
    }

    private static void UpdateEvasionDistance(object sender, System.EventArgs e)
    {
        Hooks.UpdateBackUpDistance();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void RiskOfOptionsInit(string assemblyLocation)
    {
        FileInfo iconFile = null;
        var files = new DirectoryInfo(Path.GetDirectoryName(assemblyLocation)).GetFiles("icon.png", SearchOption.TopDirectoryOnly);
        if (files != null && files.Length > 0)
        {
            iconFile = files[0];
        }
        if (iconFile != null)
        {
            var name = $"{AlliesAvoidImplosions.PluginName}Icon";
            var texture = new Texture2D(256, 256);
            texture.name = name;
            if (texture.LoadImage(File.ReadAllBytes(iconFile.FullName)))
            {
                var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                sprite.name = name;
                ModSettingsManager.SetModIcon(sprite, AlliesAvoidImplosions.PluginGUID, AlliesAvoidImplosions.PluginName);
            }
        }

        ModSettingsManager.AddOption(new CheckBoxOption(ImmuneToVoidDeath));
        ModSettingsManager.AddOption(new FloatFieldOption(EvasionDistance));
    }
}