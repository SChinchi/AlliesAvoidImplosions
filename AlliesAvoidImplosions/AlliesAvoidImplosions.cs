using BepInEx;
using R2API.Utils;
using RoR2;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace AlliesAvoidImplosions
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class AlliesAvoidImplosions : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Chinchi";
        public const string PluginName = "AlliesAvoidImplosions";
        public const string PluginVersion = "1.0.0";

        internal static new BepInEx.Logging.ManualLogSource Logger;
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
            Logger = base.Logger;
            Configuration.Init(Config);
            RoR2Application.onLoad += Hooks.LoadDataAndPatch;
        }

        private void OnEnable()
        {
            Hooks.Apply();
            Hooks.CollectCurrentProjectiles();
        }

        private void OnDisable()
        {
            Hooks.Undo();
            Hooks.implosions.Clear();
        }
    }
}