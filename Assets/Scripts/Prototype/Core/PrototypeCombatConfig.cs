using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    [CreateAssetMenu(
        fileName = "PrototypeCombatConfig",
        menuName = "ParryShooter/Prototype Combat Config")]
    public sealed class PrototypeCombatConfig : ScriptableObject
    {
        public const float LogicalWidth = 450f;
        public const float LogicalHeight = 800f;

        [Serializable]
        public struct PlayerSettings
        {
            public float hitboxRadius;
            public float spriteRadius;
            public float parryOuterRadius;
            public float pointerShipOffsetY;
            public int maxHp;
            public float moveLerp;
            public float speedClamp;
            public float hitInvuln;
        }

        [Serializable]
        public struct AutoShotSettings
        {
            public float interval;
            public float speed;
            public float damage;
            public float poiseDamage;
        }

        [Serializable]
        public struct ParrySettings
        {
            public float threshold;
            public float travelThreshold;
            public float travelWindow;
            public float burstHold;
            public float burstDistance;
            public float burstDuration;
            public float activeMinUpSpeed;
            public float postGrace;
            public float sameTargetRepeatCooldown;
            public float cooldown;
            public float parryWindow;
            public float successInvuln;
            public float counterBulletSpeed;
            public float counterDamage;
            public float counterPoiseDamage;
            public float rageGain;
            public float counterHitRageGain;
        }

        [Serializable]
        public struct BossSettings
        {
            public float contactRadiusX;
            public float contactRadiusY;
            public float bossY;
            public float moveSpeed;
            public float restTime;
            public float telegraphTime;
            public float breakDuration;
            public float weakZoneRadius;
            public float weakZoneRagePerSecond;
            public float poiseRecoverDelay;
            public float poiseRecoverRate;
        }

        [Serializable]
        public struct VfxSettings
        {
            public int maxActiveEffects;
            public float burstCountScale;
            public float afterimageInterval;
        }

        [Serializable]
        public struct PoolSettings
        {
            public int playerProjectilePreload;
            public int bossProjectilePreload;
            public int counterProjectilePreload;
            public int effectPreload;
            public int hardLimitPerPool;
        }

        [Serializable]
        public struct SkillSettings
        {
            public float max;
            public float duration;
            public float recoveryInvuln;
            public float laserWidth;
            public float laserDps;
            public float laserPoiseDps;
            public float laneHitAllowance;
        }

        [Serializable]
        public struct PatternSettings
        {
            public int aimedFanCount;
            public float aimedFanSpeed;
            public float aimedFanSpread;
        }

        [Serializable]
        public sealed class BossDefinition
        {
            public string name;
            public float hp;
            public float poise;
            public GameObject bossPrefab;
            public List<BossMoveDefinition> moveDefinitions = new();
            public List<string> moveQueue = new();

            public int MoveCount => moveDefinitions != null && moveDefinitions.Count > 0 ? moveDefinitions.Count : moveQueue.Count;

            public BossMoveDefinition GetMoveDefinition(int index)
            {
                if (moveDefinitions == null || moveDefinitions.Count == 0)
                {
                    return null;
                }

                return moveDefinitions[index % moveDefinitions.Count];
            }

            public string GetMoveId(int index)
            {
                var moveDefinition = GetMoveDefinition(index);
                if (moveDefinition != null)
                {
                    return moveDefinition.MoveId;
                }

                return moveQueue[index % moveQueue.Count];
            }
        }

        [FoldoutGroup("Playfield")] public float logicalWidth = LogicalWidth;
        [FoldoutGroup("Playfield")] public float logicalHeight = LogicalHeight;
        [FoldoutGroup("Playfield")] public float pixelsPerUnit = 100f;
        [FoldoutGroup("Authoring")] public GameObject playerPrefab;
        [FoldoutGroup("Authoring")] public PlayerLoadoutDefinition playerLoadout;
        [FoldoutGroup("Authoring")] public PlayerCharacterRoster characterRoster;
        [FoldoutGroup("Authoring"), AssetsOnly] public TutorialBluePlayerMarker tutorialBluePlayerPrefab;
        [FoldoutGroup("Presentation")] public Sprite bossTelegraphSprite;

        [FoldoutGroup("Player")] public PlayerSettings player = DefaultPlayer();
        [FoldoutGroup("Auto Shot")] public AutoShotSettings autoShot = DefaultAutoShot();
        [FoldoutGroup("Parry")] public ParrySettings parry = DefaultParry();
        [FoldoutGroup("Boss")] public BossSettings boss = DefaultBoss();
        [FoldoutGroup("Skill")] public SkillSettings skill = DefaultSkill();
        [FoldoutGroup("Patterns")] public PatternSettings patterns = DefaultPatterns();
        [FoldoutGroup("VFX")] public VfxSettings vfx = DefaultVfx();
        [FoldoutGroup("Pooling")] public PoolSettings pooling = DefaultPooling();

        [FoldoutGroup("Boss Rush"), ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<BossDefinition> bosses = DefaultBosses();

        [Button(ButtonSizes.Medium)]
        public void ResetToDefaults()
        {
            logicalWidth = LogicalWidth;
            logicalHeight = LogicalHeight;
            pixelsPerUnit = 100f;
            player = DefaultPlayer();
            autoShot = DefaultAutoShot();
            parry = DefaultParry();
            boss = DefaultBoss();
            skill = DefaultSkill();
            patterns = DefaultPatterns();
            vfx = DefaultVfx();
            pooling = DefaultPooling();
            bosses = DefaultBosses();
        }

        private void OnValidate()
        {
            if (bosses == null || bosses.Count == 0)
            {
                bosses = DefaultBosses();
            }

            if (boss.poiseRecoverDelay <= 0f)
            {
                boss.poiseRecoverDelay = DefaultBoss().poiseRecoverDelay;
            }

            if (boss.poiseRecoverRate <= 0f)
            {
                boss.poiseRecoverRate = DefaultBoss().poiseRecoverRate;
            }

            if (vfx.maxActiveEffects <= 0)
            {
                vfx.maxActiveEffects = DefaultVfx().maxActiveEffects;
            }

            if (vfx.burstCountScale <= 0f)
            {
                vfx.burstCountScale = DefaultVfx().burstCountScale;
            }

            if (vfx.afterimageInterval <= 0f)
            {
                vfx.afterimageInterval = DefaultVfx().afterimageInterval;
            }

            if (pooling.playerProjectilePreload <= 0)
            {
                pooling.playerProjectilePreload = DefaultPooling().playerProjectilePreload;
            }

            if (pooling.bossProjectilePreload <= 0)
            {
                pooling.bossProjectilePreload = DefaultPooling().bossProjectilePreload;
            }

            if (pooling.counterProjectilePreload <= 0)
            {
                pooling.counterProjectilePreload = DefaultPooling().counterProjectilePreload;
            }

            if (pooling.effectPreload <= 0)
            {
                pooling.effectPreload = DefaultPooling().effectPreload;
            }

            if (pooling.hardLimitPerPool <= 0)
            {
                pooling.hardLimitPerPool = DefaultPooling().hardLimitPerPool;
            }
        }

        private static PlayerSettings DefaultPlayer() => new()
        {
            hitboxRadius = 11f,
            spriteRadius = 22f,
            parryOuterRadius = 28f,
            pointerShipOffsetY = 52f,
            maxHp = 3,
            moveLerp = 0.23f,
            speedClamp = 18f,
            hitInvuln = 1.5f
        };

        private static AutoShotSettings DefaultAutoShot() => new()
        {
            interval = 0.12f,
            speed = 670f,
            damage = 3.5f,
            poiseDamage = 0.7f
        };

        private static ParrySettings DefaultParry() => new()
        {
            threshold = 720f,
            travelThreshold = 24f,
            travelWindow = 0.12f,
            burstHold = 0.125f,
            burstDistance = 74f,
            burstDuration = 0.12f,
            activeMinUpSpeed = 120f,
            postGrace = 0.25f,
            sameTargetRepeatCooldown = 0.85f,
            cooldown = 0f,
            parryWindow = 0.12f,
            successInvuln = 0.5f,
            counterBulletSpeed = 920f,
            counterDamage = 30f,
            counterPoiseDamage = 32f,
            rageGain = 18f,
            counterHitRageGain = 10f
        };

        private static BossSettings DefaultBoss() => new()
        {
            contactRadiusX = 68f,
            contactRadiusY = 38f,
            bossY = 128f,
            moveSpeed = 78f,
            restTime = 0.62f,
            telegraphTime = 0.72f,
            breakDuration = 2f,
            weakZoneRadius = 42f,
            weakZoneRagePerSecond = 46f,
            poiseRecoverDelay = 1.4f,
            poiseRecoverRate = 26f
        };

        private static SkillSettings DefaultSkill() => new()
        {
            max = 100f,
            duration = 1f,
            recoveryInvuln = 0.5f,
            laserWidth = 18f,
            laserDps = 190f,
            laserPoiseDps = 30f,
            laneHitAllowance = 56f
        };

        private static PatternSettings DefaultPatterns() => new()
        {
            aimedFanCount = 9,
            aimedFanSpeed = 270f,
            aimedFanSpread = 1f
        };

        private static VfxSettings DefaultVfx() => new()
        {
            maxActiveEffects = 96,
            burstCountScale = 0.65f,
            afterimageInterval = 0.04f
        };

        private static PoolSettings DefaultPooling() => new()
        {
            playerProjectilePreload = 64,
            bossProjectilePreload = 180,
            counterProjectilePreload = 24,
            effectPreload = 96,
            hardLimitPerPool = 256
        };

        private static List<BossDefinition> DefaultBosses() => new()
        {
            new BossDefinition { name = "Violet Manta", hp = 760f, poise = 108f, moveQueue = new List<string> { "aimedFan", "staggerWave", "offsetRain", "laneBarrage" } },
            new BossDefinition { name = "Cerulean Halberd", hp = 900f, poise = 118f, moveQueue = new List<string> { "twinLance", "sweepBloom", "crossBurst", "checkerDrop" } },
            new BossDefinition { name = "Amber Widow", hp = 1040f, poise = 132f, moveQueue = new List<string> { "pinwheel", "sideSnakes", "wedgePress", "splitCurtain" } },
            new BossDefinition { name = "Prism Leviathan", hp = 1180f, poise = 146f, moveQueue = new List<string> { "cometCurtain", "pulseGrid", "orbitMinefall", "prismFork" } },
            new BossDefinition { name = "Eclipse Sovereign", hp = 1380f, poise = 164f, moveQueue = new List<string> { "crownRain", "crushColumns", "helixGate", "finalConvergence" } }
        };
    }
}
