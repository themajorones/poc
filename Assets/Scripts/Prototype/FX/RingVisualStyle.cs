using UnityEngine;

namespace CupHeadClone.Prototype
{
    public enum RingVisualState
    {
        Telegraph,
        Active,
        Impact
    }

    public readonly struct RingVisualStyle
    {
        public readonly float EdgeAlpha;
        public readonly float BodyAlpha;
        public readonly float GlowAlpha;
        public readonly float TrailAlpha;
        public readonly float ThicknessScale;
        public readonly float GlowWidthScale;
        public readonly float TrailInsetScale;
        public readonly float PulseSpeed;
        public readonly float WobbleAmplitude;
        public readonly float WobbleFrequency;
        public readonly int SparkCount;
        public readonly float SparkSpeed;
        public readonly float SparkScale;
        public readonly float SparkAlpha;
        public readonly float TelegraphGlowBoost;
        public readonly float ImpactFlashBoost;
        public readonly Color EdgeColor;
        public readonly Color BodyColor;
        public readonly Color GlowColor;
        public readonly Color TrailColor;

        public RingVisualStyle(
            float edgeAlpha,
            float bodyAlpha,
            float glowAlpha,
            float trailAlpha,
            float thicknessScale,
            float glowWidthScale,
            float trailInsetScale,
            float pulseSpeed,
            float wobbleAmplitude,
            float wobbleFrequency,
            int sparkCount,
            float sparkSpeed,
            float sparkScale,
            float sparkAlpha,
            float telegraphGlowBoost,
            float impactFlashBoost,
            Color edgeColor,
            Color bodyColor,
            Color glowColor,
            Color trailColor)
        {
            EdgeAlpha = edgeAlpha;
            BodyAlpha = bodyAlpha;
            GlowAlpha = glowAlpha;
            TrailAlpha = trailAlpha;
            ThicknessScale = thicknessScale;
            GlowWidthScale = glowWidthScale;
            TrailInsetScale = trailInsetScale;
            PulseSpeed = pulseSpeed;
            WobbleAmplitude = wobbleAmplitude;
            WobbleFrequency = wobbleFrequency;
            SparkCount = sparkCount;
            SparkSpeed = sparkSpeed;
            SparkScale = sparkScale;
            SparkAlpha = sparkAlpha;
            TelegraphGlowBoost = telegraphGlowBoost;
            ImpactFlashBoost = impactFlashBoost;
            EdgeColor = edgeColor;
            BodyColor = bodyColor;
            GlowColor = glowColor;
            TrailColor = trailColor;
        }

        public static RingVisualStyle DefensiveField => new(
            edgeAlpha: 1f,
            bodyAlpha: 0.5f,
            glowAlpha: 0.11f,
            trailAlpha: 0.18f,
            thicknessScale: 1.18f,
            glowWidthScale: 2.35f,
            trailInsetScale: 0.48f,
            pulseSpeed: 5.8f,
            wobbleAmplitude: 0f,
            wobbleFrequency: 0f,
            sparkCount: 1,
            sparkSpeed: 0.85f,
            sparkScale: 0.8f,
            sparkAlpha: 0.42f,
            telegraphGlowBoost: 1.12f,
            impactFlashBoost: 1.06f,
            edgeColor: new Color(0.78f, 1f, 0.98f, 1f),
            bodyColor: new Color(0.5f, 0.95f, 1f, 1f),
            glowColor: new Color(0.18f, 0.82f, 1f, 1f),
            trailColor: new Color(0.62f, 0.98f, 1f, 1f));

        public static RingVisualStyle GlobalWave => new(
            edgeAlpha: 1f,
            bodyAlpha: 0.54f,
            glowAlpha: 0.1f,
            trailAlpha: 0.18f,
            thicknessScale: 1.2f,
            glowWidthScale: 2.5f,
            trailInsetScale: 0.42f,
            pulseSpeed: 4.8f,
            wobbleAmplitude: 0f,
            wobbleFrequency: 0f,
            sparkCount: 0,
            sparkSpeed: 0.62f,
            sparkScale: 1.08f,
            sparkAlpha: 0.68f,
            telegraphGlowBoost: 1.08f,
            impactFlashBoost: 1.03f,
            edgeColor: new Color(0.98f, 0.8f, 1f, 1f),
            bodyColor: new Color(0.88f, 0.56f, 1f, 1f),
            glowColor: new Color(0.52f, 0.18f, 1f, 1f),
            trailColor: new Color(0.82f, 0.56f, 1f, 1f));

        public static RingVisualStyle BossShockwave => new(
            edgeAlpha: 1f,
            bodyAlpha: 0.56f,
            glowAlpha: 0.12f,
            trailAlpha: 0.2f,
            thicknessScale: 1.22f,
            glowWidthScale: 2.6f,
            trailInsetScale: 0.46f,
            pulseSpeed: 4.6f,
            wobbleAmplitude: 0f,
            wobbleFrequency: 0f,
            sparkCount: 0,
            sparkSpeed: 0.92f,
            sparkScale: 0.92f,
            sparkAlpha: 0.7f,
            telegraphGlowBoost: 1.1f,
            impactFlashBoost: 1.05f,
            edgeColor: new Color(0.97f, 0.74f, 1f, 1f),
            bodyColor: new Color(0.86f, 0.5f, 1f, 1f),
            glowColor: new Color(0.45f, 0.12f, 1f, 1f),
            trailColor: new Color(0.75f, 0.43f, 1f, 1f));

        public static RingVisualStyle TransientImpact => new(
            edgeAlpha: 1f,
            bodyAlpha: 0.52f,
            glowAlpha: 0.14f,
            trailAlpha: 0.22f,
            thicknessScale: 1.12f,
            glowWidthScale: 2.45f,
            trailInsetScale: 0.52f,
            pulseSpeed: 5.6f,
            wobbleAmplitude: 0f,
            wobbleFrequency: 0f,
            sparkCount: 1,
            sparkSpeed: 1.2f,
            sparkScale: 0.72f,
            sparkAlpha: 0.34f,
            telegraphGlowBoost: 1.1f,
            impactFlashBoost: 1.14f,
            edgeColor: new Color(1f, 0.92f, 0.98f, 1f),
            bodyColor: new Color(1f, 0.62f, 0.9f, 1f),
            glowColor: new Color(1f, 0.28f, 0.74f, 1f),
            trailColor: new Color(1f, 0.72f, 0.94f, 1f));

        public static RingVisualStyle WeakZone => new(
            edgeAlpha: 0.88f,
            bodyAlpha: 0.18f,
            glowAlpha: 0.12f,
            trailAlpha: 0.08f,
            thicknessScale: 1.08f,
            glowWidthScale: 1.6f,
            trailInsetScale: 0.38f,
            pulseSpeed: 4.2f,
            wobbleAmplitude: 0f,
            wobbleFrequency: 0f,
            sparkCount: 2,
            sparkSpeed: 0.46f,
            sparkScale: 0.84f,
            sparkAlpha: 0.44f,
            telegraphGlowBoost: 1.34f,
            impactFlashBoost: 1f,
            edgeColor: new Color(1f, 0.94f, 0.58f, 1f),
            bodyColor: new Color(1f, 0.82f, 0.2f, 1f),
            glowColor: new Color(1f, 0.6f, 0.08f, 1f),
            trailColor: new Color(1f, 0.9f, 0.38f, 1f));

        public static RingVisualStyle DefensiveFieldResolved => FromPresetOrDefault(RingVisualPresetRuntime.DefensiveField, DefensiveField);
        public static RingVisualStyle GlobalWaveResolved => FromPresetOrDefault(RingVisualPresetRuntime.GlobalWave, GlobalWave);
        public static RingVisualStyle BossShockwaveResolved => FromPresetOrDefault(RingVisualPresetRuntime.BossShockwave, BossShockwave);
        public static RingVisualStyle TransientImpactResolved => FromPresetOrDefault(RingVisualPresetRuntime.TransientImpact, TransientImpact);
        public static RingVisualStyle WeakZoneResolved => FromPresetOrDefault(RingVisualPresetRuntime.WeakZone, WeakZone);

        private static RingVisualStyle FromPresetOrDefault(RingVisualPreset preset, RingVisualStyle fallback)
        {
            return preset != null ? preset.ToStyle() : fallback;
        }
    }
}
