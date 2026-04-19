using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    [CreateAssetMenu(fileName = "PlayerLoadout", menuName = "ParryShooter/Player/Loadout")]
    public sealed class PlayerLoadoutDefinition : ScriptableObject
    {
        [TitleGroup("Loadout Slots", "Equip each gameplay piece separately on the player.")]
        [BoxGroup("Loadout Slots/Primary")]
        [LabelText("Primary Shot")]
        [PropertyOrder(0)]
        [SerializeField] private PlayerShotDefinition primaryShot;

        [BoxGroup("Loadout Slots/Parry")]
        [LabelText("Parry Visual Effect")]
        [PropertyOrder(1)]
        [Tooltip("Visual-only feedback for a successful parry: burst, ring flash, hit stop, camera shake.")]
        [SerializeField] private PlayerParryEffectDefinition parryEffect;

        [BoxGroup("Loadout Slots/Rage")]
        [LabelText("Rage Skill")]
        [PropertyOrder(2)]
        [Tooltip("Active skill that requires a full rage bar. Example: Expanding Global Ring.")]
        [SerializeField] private PlayerSkillDefinition skill;

        [BoxGroup("Loadout Slots/Parry")]
        [LabelText("Parry Special")]
        [PropertyOrder(3)]
        [Tooltip("Gameplay special triggered by a successful parry. Examples: counter projectile, Defensive Ring.")]
        [SerializeField] private PlayerCounterShotDefinition counterShot;

        public PlayerShotDefinition PrimaryShot => primaryShot;
        public PlayerCounterShotDefinition CounterShot => counterShot;
        public PlayerSkillDefinition Skill => skill;
        public PlayerParryEffectDefinition ParryEffect => parryEffect;
    }
}
