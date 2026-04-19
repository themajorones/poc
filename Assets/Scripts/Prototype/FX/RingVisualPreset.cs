using UnityEngine;

namespace CupHeadClone.Prototype
{
    [CreateAssetMenu(fileName = "RingVisualPreset", menuName = "ParryShooter/VFX/Ring Visual Preset")]
    public sealed class RingVisualPreset : ScriptableObject
    {
        [Header("Brightness")]
        [SerializeField] private float edgeAlpha = 1f;
        [SerializeField] private float bodyAlpha = 0.5f;
        [SerializeField] private float glowAlpha = 0.1f;
        [SerializeField] private float trailAlpha = 0.18f;
        [SerializeField] private float thicknessScale = 1f;
        [SerializeField] private float glowWidthScale = 2.4f;
        [SerializeField] private float trailInsetScale = 0.45f;

        [Header("Motion")]
        [SerializeField] private float pulseSpeed = 5f;
        [SerializeField] private int sparkCount = 0;
        [SerializeField] private float sparkSpeed = 0.8f;
        [SerializeField] private float sparkScale = 0.8f;
        [SerializeField] private float sparkAlpha = 0.3f;

        [Header("State Boosts")]
        [SerializeField] private float telegraphGlowBoost = 1.1f;
        [SerializeField] private float impactFlashBoost = 1.08f;

        [Header("Layer Colors")]
        [ColorUsage(false, true)]
        [SerializeField] private Color edgeColor = Color.white;
        [ColorUsage(false, true)]
        [SerializeField] private Color bodyColor = Color.white;
        [ColorUsage(false, true)]
        [SerializeField] private Color glowColor = Color.white;
        [ColorUsage(false, true)]
        [SerializeField] private Color trailColor = Color.white;

        public RingVisualStyle ToStyle()
        {
            return new RingVisualStyle(
                edgeAlpha,
                bodyAlpha,
                glowAlpha,
                trailAlpha,
                thicknessScale,
                glowWidthScale,
                trailInsetScale,
                pulseSpeed,
                0f,
                0f,
                sparkCount,
                sparkSpeed,
                sparkScale,
                sparkAlpha,
                telegraphGlowBoost,
                impactFlashBoost,
                edgeColor,
                bodyColor,
                glowColor,
                trailColor);
        }
    }
}
