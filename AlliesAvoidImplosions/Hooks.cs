using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace AlliesAvoidImplosions
{
    internal class Hooks
    {
        internal static readonly HashSet<GameObject> implosions = [];
        private static readonly HashSet<int> implosionProjectiles = [];
        private static readonly HashSet<MasterCatalog.MasterIndex> backupMasterIndices = [];
        private static readonly HashSet<MasterCatalog.MasterIndex> immuneMasterIndices = [];

        internal static void Apply()
        {
            On.RoR2.Projectile.ProjectileController.Awake += OnSpawnProjectile;
            On.RoR2.Projectile.ProjectileController.OnDestroy += OnDestroyProjectile;
            if (Configuration.immuneToVoidDeath.Value)
            {
                IL.RoR2.HealthComponent.TakeDamage += IgnoreVoidDeathForAllies;
            }
        }

        internal static void Undo()
        {
            On.RoR2.Projectile.ProjectileController.Awake -= OnSpawnProjectile;
            On.RoR2.Projectile.ProjectileController.OnDestroy -= OnDestroyProjectile;
            if (Configuration.immuneToVoidDeath.Value)
            {
                IL.RoR2.HealthComponent.TakeDamage -= IgnoreVoidDeathForAllies;
            }
        }

        private static void IgnoreVoidDeathForAllies(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(
                x => x.MatchLdfld<CharacterBody>("bodyFlags"),
                x => x.MatchLdcI4(0x800)))
            {
                AlliesAvoidImplosions.Logger.LogError("Failed to patch HealthComponent.TakeDamage");
                return;
            }
            c.Index += 1;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<CharacterBody.BodyFlags, HealthComponent, CharacterBody.BodyFlags>>((bodyFlags, healthComponent) =>
            {
                var master = healthComponent.body.master;
                if (master && immuneMasterIndices.Contains(master.masterIndex) && Configuration.immuneToVoidDeath.Value)
                {
                    bodyFlags |= CharacterBody.BodyFlags.ImmuneToVoidDeath;
                }
                return bodyFlags;
            });
        }

        private static void OnSpawnProjectile(On.RoR2.Projectile.ProjectileController.orig_Awake orig, ProjectileController self)
        {
            orig(self);
            if (NetworkServer.active && implosionProjectiles.Contains(self.catalogIndex))
            {
                implosions.Add(self.gameObject);
                GTFOHController.EnableAll();
            }
        }

        private static void OnDestroyProjectile(On.RoR2.Projectile.ProjectileController.orig_OnDestroy orig, ProjectileController self)
        {
            if (NetworkServer.active && implosions.Contains(self.gameObject))
            {
                implosions.Remove(self.gameObject);
            }
            orig(self);
        }

        internal static void CollectCurrentProjectiles()
        {
            if (NetworkServer.active)
            {
                foreach (var projectile in InstanceTracker.GetInstancesList<ProjectileController>())
                {
                    if (implosionProjectiles.Contains(projectile.catalogIndex))
                    {
                        implosions.Add(projectile.gameObject);
                    }
                }
            }
        }

        internal static void LoadDataAndPatch()
        {
            BuildImplosionProjectileList();
            BuildAllyIDs();
            PatchAllies();
        }

        private static void BuildImplosionProjectileList()
        {
            foreach (var projectile in ProjectileCatalog.projectilePrefabProjectileControllerComponents)
            {
                if (projectile.TryGetComponent<ProjectileDamage>(out var damage) && (damage.damageType & DamageType.VoidDeath) == DamageType.VoidDeath)
                {
                    implosionProjectiles.Add(projectile.catalogIndex);
                }
            }
        }

        private static void BuildAllyIDs()
        {
            BuildMasterIndices(Configuration.additionalBackupEntries, Configuration.blacklistedBackupEntries, backupMasterIndices);
            BuildMasterIndices(Configuration.additionalImmuneEntries, Configuration.blacklistedImmuneEntries, immuneMasterIndices);

        }

        private static void BuildMasterIndices(ConfigEntry<string> additionalConfig, ConfigEntry<string> blacklistedConfig, HashSet<MasterCatalog.MasterIndex> indices)
        {
            var blacklisted = new HashSet<string>(SplitConfig(blacklistedConfig));
            var allowed = new HashSet<string>();
            foreach (var name in AlliesAvoidImplosions.defaultAllies)
            {
                if (!blacklisted.Contains(name))
                {
                    allowed.Add(name);
                }
            }
            foreach (var name in SplitConfig(additionalConfig))
            {
                if (!blacklisted.Contains(name))
                {
                    allowed.Add(name);
                }
            }
            foreach (var name in allowed)
            {
                var master = MasterCatalog.FindMasterPrefab(name);
                if (master != null)
                {
                    indices.Add(master.GetComponent<CharacterMaster>().masterIndex);
                }
            }

            static IEnumerable<string> SplitConfig(ConfigEntry<string> config)
            {
                return config.Value.Split(',').Select(x => x.Trim());
            }
        }

        private static void PatchAllies()
        {
            foreach (var index in backupMasterIndices)
            {
                var master = MasterCatalog.GetMasterPrefab(index);
                if (master != null)
                {
                    var cm = master.GetComponent<CharacterMaster>();
                    // Stationary turrets can't escape so their AI shouldn't be modified
                    if (cm.bodyPrefab && (cm.bodyPrefab.TryGetComponent<CharacterMotor>(out _) || cm.bodyPrefab.TryGetComponent<RigidbodyMotor>(out _)))
                    {
                        var originalSkillDrivers = master.GetComponents<AISkillDriver>();

                        var component = master.AddComponent<AISkillDriver>();
                        component.customName = "BackUpFromImplosion";
                        component.skillSlot = SkillSlot.None;
                        component.maxDistance = Configuration.evasionDistance.Value;
                        component.moveTargetType = AISkillDriver.TargetType.Custom;
                        component.aimType = AISkillDriver.AimType.AtMoveTarget;
                        component.movementType = AISkillDriver.MovementType.FleeMoveTarget;
                        component.shouldSprint = true;
                        component.ignoreNodeGraph = true;
                        component.driverUpdateTimerOverride = 1f;

                        // The skill drivers must be in a specific order and I do not know of a better way to
                        // programmatically reorder them, so I remove and readd the ones I want at the end.
                        foreach (var skillDriver in originalSkillDrivers)
                        {
                            var copy = master.AddComponent<AISkillDriver>();
                            foreach (var field in skillDriver.GetType().GetFields())
                            {
                                field.SetValue(copy, field.GetValue(skillDriver));
                            }
                            Component.DestroyImmediate(skillDriver);
                        }

                        master.AddComponent<GTFOHController>();
                    }
                }
            }
        }
    }
}