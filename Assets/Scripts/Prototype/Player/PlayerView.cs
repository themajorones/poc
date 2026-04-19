using DG.Tweening;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private PlayerAuthoring authoring;

        private SpriteRenderer _glow;
        private SpriteRenderer _body;
        private SpriteRenderer _core;
        private SpriteRenderer _parryRing;
        private SpriteRenderer _parryFill;
        private SpriteRenderer _skillAura;
        private SpriteRenderer _laserOuter;
        private SpriteRenderer _laserBeam;
        private SpriteRenderer _laserCore;
        private Tween _parryTween;
        private Tween _auraTween;
        private Color _bodyBaseColor;
        private Color _coreBaseColor;
        private float _laserLength;
        public SpriteRenderer BodyRenderer => _body;

        public void Build()
        {
            authoring ??= GetComponent<PlayerAuthoring>();
            _laserLength = PrototypeCombatConfig.LogicalHeight / 100f;
            var laserMuzzleOffset = authoring != null ? authoring.LaserMuzzleOffset : 0.22f;

            _glow = PrototypeVisualUtility.EnsureSpriteChild(transform, "Glow", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.PlayerCyan.WithAlpha(0.08f), 17);
            _glow.transform.localScale = Vector3.one * (authoring != null ? authoring.IdleGlowScale : 0.38f);

            _body = authoring != null && authoring.BodyRenderer != null
                ? authoring.BodyRenderer
                : CreatePart("Body", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.PlayerCyan, 20);
            if (_body.sprite == null)
            {
                _body.sprite = PrototypeVisualUtility.SquareSprite;
                _body.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                _body.transform.localScale = new Vector3(0.22f, 0.22f, 1f);
            }

            _core = authoring != null && authoring.CoreRenderer != null
                ? authoring.CoreRenderer
                : CreatePart("Core", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.PlayerCore, 22);
            if (_core.sprite == null)
            {
                _core.sprite = PrototypeVisualUtility.CircleSprite;
                _core.transform.localScale = new Vector3(0.11f, 0.11f, 1f);
            }

            _parryRing = authoring != null && authoring.ParryShieldRenderer != null
                ? authoring.ParryShieldRenderer
                : CreatePart("ParryRing", PrototypeVisualUtility.ArcSprite, PrototypeVisualUtility.ParryPurple.WithAlpha(0f), 23);
            if (_parryRing.sprite == null)
            {
                _parryRing.sprite = PrototypeVisualUtility.ArcSprite;
                _parryRing.transform.localPosition = new Vector3(0f, -0.02f, 0f);
                _parryRing.transform.localScale = new Vector3(0.7f, 0.56f, 1f);
            }
            _parryFill = PrototypeVisualUtility.EnsureSpriteChild(transform, "ParryFill", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.ParryPurple.WithAlpha(0f), 18);
            _parryFill.transform.localScale = new Vector3(0.28f, 0.22f, 1f);
            _parryFill.transform.localPosition = new Vector3(0f, -0.03f, 0f);
            _parryTween = _parryRing.transform.DOScale(new Vector3(0.76f, 0.6f, 1f), 0.22f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            _skillAura = authoring != null && authoring.SkillAuraRenderer != null
                ? authoring.SkillAuraRenderer
                : CreatePart("SkillAura", PrototypeVisualUtility.CircleSprite, PrototypeVisualUtility.CounterGold.WithAlpha(0f), 18);
            if (_skillAura.sprite == null)
            {
                _skillAura.sprite = PrototypeVisualUtility.CircleSprite;
                _skillAura.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
            }
            var auraBaseScale = _skillAura.transform.localScale;
            _auraTween = _skillAura.transform.DOScale(auraBaseScale * 1.08f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            _laserOuter = PrototypeVisualUtility.EnsureSpriteChild(transform, "LaserOuter", PrototypeVisualUtility.SquareSprite, new Color(1f, 0.95f, 0.74f, 0f), 11);
            _laserOuter.transform.localPosition = new Vector3(0f, laserMuzzleOffset + _laserLength * 0.5f, 0f);
            _laserOuter.transform.localScale = new Vector3(0.24f, _laserLength, 1f);

            _laserBeam = authoring != null && authoring.LaserBeamRenderer != null
                ? authoring.LaserBeamRenderer
                : CreatePart("LaserBeam", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.LaserEdge.WithAlpha(0f), 12);
            if (_laserBeam.sprite == null)
            {
                _laserBeam.sprite = PrototypeVisualUtility.SquareSprite;
            }
            _laserBeam.transform.localPosition = new Vector3(0f, laserMuzzleOffset + _laserLength * 0.5f, 0f);
            _laserBeam.transform.localScale = new Vector3(0.16f, _laserLength, 1f);

            _laserCore = authoring != null && authoring.LaserCoreRenderer != null
                ? authoring.LaserCoreRenderer
                : CreatePart("LaserCore", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.LaserCore.WithAlpha(0f), 13);
            if (_laserCore.sprite == null)
            {
                _laserCore.sprite = PrototypeVisualUtility.SquareSprite;
            }
            _laserCore.transform.localPosition = new Vector3(0f, laserMuzzleOffset + _laserLength * 0.5f, 0f);
            _laserCore.transform.localScale = new Vector3(0.06f, _laserLength, 1f);

            _bodyBaseColor = _body.color;
            _coreBaseColor = _core.color;
        }

        public void ApplyState(PlayerController player)
        {
            var skillDefinition = (player.SkillActive || player.ShowLaserVisual) && authoring != null && authoring.PlayerLoadout != null
                ? authoring.PlayerLoadout.Skill
                : null;
            var visibleBody = !player.IsInvulnerable || Mathf.FloorToInt(Time.time * 14f) % 2 == 0;
            var alpha = visibleBody ? 1f : 0.12f;
            var hurtTint = Mathf.Clamp01(player.HitFlash / 0.18f);
            _body.color = Color.Lerp(new Color(_bodyBaseColor.r, _bodyBaseColor.g, _bodyBaseColor.b, _bodyBaseColor.a * alpha), new Color(1f, 0.58f, 0.68f, _bodyBaseColor.a * alpha), hurtTint);
            _core.color = Color.Lerp(new Color(_coreBaseColor.r, _coreBaseColor.g, _coreBaseColor.b, _coreBaseColor.a * alpha), Color.white, hurtTint);
            _glow.color = (player.IsParrying ? PrototypeVisualUtility.ParryPurple : PrototypeVisualUtility.PlayerCyan).WithAlpha(player.IsParrying ? 0.01f : 0.005f);
            var glowTargetScale = player.IsParrying
                ? Vector3.one * (authoring != null ? authoring.ParryGlowScale : 0.48f)
                : Vector3.one * (authoring != null ? authoring.IdleGlowScale : 0.38f);
            _glow.transform.localScale = Vector3.Lerp(_glow.transform.localScale, glowTargetScale, Time.deltaTime * 10f);

            var parryColor = _parryRing.color;
            parryColor.a = player.IsParrying ? 0.9f : 0f;
            _parryRing.color = parryColor;
            _parryFill.color = PrototypeVisualUtility.ParryPurple.WithAlpha(player.IsParrying ? 0.08f : 0f);

            var auraColor = _skillAura.color;
            auraColor.a = player.SkillActive ? 0.02f : 0f;
            _skillAura.color = auraColor;

            var showLaser = player.ShowLaserVisual;
            var outerColor = _laserOuter.color;
            var laserOuterBase = skillDefinition != null ? skillDefinition.LaserOuterColor : new Color(1f, 0.95f, 0.74f, 0.28f);
            outerColor = laserOuterBase;
            outerColor.a = showLaser ? laserOuterBase.a : 0f;
            _laserOuter.color = outerColor;

            var beamBase = skillDefinition != null ? skillDefinition.LaserBeamColor : PrototypeVisualUtility.LaserEdge.WithAlpha(0.82f);
            var beamColor = _laserBeam.color;
            beamColor = beamBase;
            beamColor.a = showLaser ? beamBase.a : 0f;
            _laserBeam.color = beamColor;

            var coreBase = skillDefinition != null ? skillDefinition.LaserCoreColor : PrototypeVisualUtility.LaserCore.WithAlpha(0.96f);
            var coreColor = _laserCore.color;
            coreColor = coreBase;
            coreColor.a = showLaser ? coreBase.a : 0f;
            _laserCore.color = coreColor;

            if (showLaser)
            {
                var pulseAmplitude = skillDefinition != null ? skillDefinition.LaserPulseAmplitude : 0.02f;
                var pulseSpeed = skillDefinition != null ? skillDefinition.LaserPulseSpeed : 40f;
                var outerWidth = skillDefinition != null ? skillDefinition.LaserOuterWidth : 0.24f;
                var beamWidth = skillDefinition != null ? skillDefinition.LaserBeamWidth : 0.16f;
                var coreWidth = skillDefinition != null ? skillDefinition.LaserCoreWidth : 0.06f;
                var pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
                _laserOuter.transform.localScale = new Vector3(outerWidth + pulse * 0.45f, _laserLength, 1f);
                _laserBeam.transform.localScale = new Vector3(beamWidth + pulse * 0.7f, _laserLength, 1f);
                _laserCore.transform.localScale = new Vector3(coreWidth + pulse * 0.3f, _laserLength, 1f);
            }
            else
            {
                var outerWidth = skillDefinition != null ? skillDefinition.LaserOuterWidth : 0.24f;
                var beamWidth = skillDefinition != null ? skillDefinition.LaserBeamWidth : 0.16f;
                var coreWidth = skillDefinition != null ? skillDefinition.LaserCoreWidth : 0.06f;
                _laserOuter.transform.localScale = new Vector3(outerWidth, _laserLength, 1f);
                _laserBeam.transform.localScale = new Vector3(beamWidth, _laserLength, 1f);
                _laserCore.transform.localScale = new Vector3(coreWidth, _laserLength, 1f);
            }
        }

        private SpriteRenderer CreatePart(string name, Sprite sprite, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void OnDestroy()
        {
            _parryTween?.Kill();
            _auraTween?.Kill();
        }
    }
}
