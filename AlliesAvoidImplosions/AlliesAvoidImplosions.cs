using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace AlliesAvoidImplosions;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(RiskOfOptionsGUID, BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
public class AlliesAvoidImplosions : BaseUnityPlugin
{
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Chinchi";
    public const string PluginName = "AlliesAvoidImplosions";
    public const string PluginVersion = "1.1.0";
    internal const string RiskOfOptionsGUID = "com.rune580.riskofoptions";

    internal static readonly string[] defaultAllies =
    [
        "Drone1Master",
        "Drone2Master",
        "DroneBackupMaster",
        "DroneMissileMaster",
        "EmergencyDroneMaster",
        "EquipmentDroneMaster",
        "FlameDroneMaster",
        "MegaDroneMaster",
        "Turret1Master",
        "DevotedLemurianMaster"
    ];

    private void Awake()
    {
        Log.Init(Logger);
        Configs.Init(Config, Info.Location);
        Hooks.Init();
        RoR2Application.onLoad += Hooks.LoadDataAndPatch;
    }
}