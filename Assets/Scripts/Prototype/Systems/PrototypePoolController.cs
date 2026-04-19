using Sirenix.OdinInspector;
using DarkTonic.PoolBoss;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PrototypePoolController : MonoBehaviour
    {
        public enum EffectTemplateKind
        {
            Burst,
            Afterimage,
            Break,
            Heal
        }

        [TitleGroup("Projectile Pools")]
        [SerializeField] private Transform playerProjectileTemplate;
        [TitleGroup("Projectile Pools")]
        [SerializeField] private Transform bossProjectileTemplate;
        [TitleGroup("Projectile Pools")]
        [SerializeField] private Transform counterProjectileTemplate;
        [TitleGroup("Projectile Pools")]
        [SerializeField] private Transform ringVisualTemplate;
        [TitleGroup("Projectile Pools")]
        [SerializeField] private Transform molotovFireZoneTemplate;

        [TitleGroup("Effect Pools")]
        [SerializeField] private Transform burstEffectTemplate;
        [TitleGroup("Effect Pools")]
        [SerializeField] private Transform afterimageEffectTemplate;
        [TitleGroup("Effect Pools")]
        [SerializeField] private Transform breakEffectTemplate;
        [TitleGroup("Effect Pools")]
        [SerializeField] private Transform healEffectTemplate;

        private GameController _game;

        public void Initialize(GameController game)
        {
            _game = game;
            ResolveTemplatesFromPoolBoss();
        }

        public PlayerProjectile SpawnPlayerProjectile(Vector3 worldPosition)
        {
            return SpawnFromTemplate<PlayerProjectile>(playerProjectileTemplate, worldPosition, "PlayerProjectile");
        }

        public BossProjectile SpawnBossProjectile(Vector3 worldPosition)
        {
            return SpawnFromTemplate<BossProjectile>(bossProjectileTemplate, worldPosition, "BossProjectile");
        }

        public CounterProjectile SpawnCounterProjectile(Vector3 worldPosition)
        {
            return SpawnFromTemplate<CounterProjectile>(counterProjectileTemplate, worldPosition, "CounterProjectile");
        }

        public PlayerFieldRingVisual SpawnRingVisual(Vector3 worldPosition)
        {
            return SpawnFromTemplate<PlayerFieldRingVisual>(ringVisualTemplate, worldPosition, "GameplayRing");
        }

        public MolotovFireZone SpawnMolotovFireZone(Vector3 worldPosition)
        {
            return SpawnFromTemplate<MolotovFireZone>(molotovFireZoneTemplate, worldPosition, "MolotovFireZone");
        }

        public SpriteRenderer SpawnEffect(Vector3 worldPosition)
        {
            return SpawnEffect(EffectTemplateKind.Burst, worldPosition);
        }

        public SpriteRenderer SpawnEffect(EffectTemplateKind kind, Vector3 worldPosition)
        {
            var template = kind switch
            {
                EffectTemplateKind.Afterimage => afterimageEffectTemplate,
                EffectTemplateKind.Break => breakEffectTemplate,
                EffectTemplateKind.Heal => healEffectTemplate,
                _ => burstEffectTemplate
            };

            return SpawnFromTemplate<SpriteRenderer>(template, worldPosition, kind.ToString());
        }

        public void Despawn(Transform transformToDespawn)
        {
            if (transformToDespawn == null || !PoolBoss.IsReady)
            {
                return;
            }

            transformToDespawn.Despawn();
        }

        [Button(ButtonSizes.Medium)]
        public void ResolveTemplatesFromPoolBoss()
        {
            var poolBoss = PoolBoss.Instance;
            if (poolBoss == null)
            {
                return;
            }

            playerProjectileTemplate ??= FindTemplateInPoolItems("PlayerProjectile");
            bossProjectileTemplate ??= FindTemplateInPoolItems("BossProjectile");
            counterProjectileTemplate ??= FindTemplateInPoolItems("CounterProjectile");
            ringVisualTemplate ??= FindTemplateInPoolItems("GameplayRing");
            molotovFireZoneTemplate ??= FindTemplateInPoolItems("MolotovFireZone");
            burstEffectTemplate ??= FindTemplateInPoolItems("GenericBurstFX");
            afterimageEffectTemplate ??= FindTemplateInPoolItems("AfterimageFX");
            breakEffectTemplate ??= FindTemplateInPoolItems("BreakBurstFX");
            healEffectTemplate ??= FindTemplateInPoolItems("HealBurstFX");
        }

        private T SpawnFromTemplate<T>(Transform template, Vector3 worldPosition, string debugName) where T : Component
        {
            if (template == null)
            {
                ResolveTemplatesFromPoolBoss();
                template = debugName switch
                {
                    "PlayerProjectile" => playerProjectileTemplate,
                    "BossProjectile" => bossProjectileTemplate,
                    "CounterProjectile" => counterProjectileTemplate,
                    "GameplayRing" => ringVisualTemplate,
                    "MolotovFireZone" => molotovFireZoneTemplate,
                    "Burst" => burstEffectTemplate,
                    _ => template
                };
            }

            if (template == null)
            {
                Debug.LogError($"PoolBoss template missing for '{debugName}'. Assign it in PrototypePoolController or add the prefab to PoolBoss poolItems with the expected name.");
                return null;
            }

            if (!PoolBoss.IsReady)
            {
                return null;
            }

            var spawned = template.SpawnOutsidePool(worldPosition, Quaternion.identity);
            return spawned != null ? spawned.GetComponent<T>() : null;
        }

        private Transform FindTemplateInPoolItems(string exactName)
        {
            if (PoolBoss.Instance == null)
            {
                return null;
            }

            for (var i = 0; i < PoolBoss.Instance.poolItems.Count; i++)
            {
                var item = PoolBoss.Instance.poolItems[i];
                if (item?.prefabTransform == null)
                {
                    continue;
                }

                if (item.prefabTransform.name == exactName)
                {
                    return item.prefabTransform;
                }
            }

            var childTemplate = PoolBoss.Instance.transform.Find(exactName);
            if (childTemplate != null)
            {
                return childTemplate;
            }

            return null;
        }
    }
}
