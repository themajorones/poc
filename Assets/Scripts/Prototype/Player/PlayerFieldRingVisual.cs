using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class PlayerFieldRingVisual : MonoBehaviour
    {
        private const int SegmentCount = 128;
        private const int MaxSparkCount = 4;

        private enum RingLayerKind
        {
            Glow,
            Trail,
            Body,
            Edge
        }

        private static Material s_glowMaterial;
        private static Material s_trailMaterial;
        private static Material s_bodyMaterial;
        private static Material s_edgeMaterial;
        private static readonly Vector3[] s_unitCircle = BuildUnitCircle();

        private readonly Vector3[] _ringPositions = new Vector3[SegmentCount];

        private LineRenderer _edgeRenderer;
        private LineRenderer _bodyRenderer;
        private LineRenderer _glowRenderer;
        private LineRenderer _trailRenderer;
        private LineRenderer _legacyRootRenderer;
        private SpriteRenderer[] _sparks;
        private int _sortingOrder = 16;

        private void Awake()
        {
            EnsureVisuals();
            Clear();
        }

        private void OnEnable()
        {
            Clear();
        }

        public void Render(GameReferences references, Vector2 centerLogical, float radius, float thickness, Color color)
        {
            Render(references, centerLogical, radius, thickness, color, RingVisualStyle.GlobalWave, RingVisualState.Active, 0f);
        }

        // The main circle geometry stays perfectly round. Styling only changes alpha/width/highlights.
        public void Render(
            GameReferences references,
            Vector2 centerLogical,
            float radius,
            float thickness,
            Color color,
            RingVisualStyle style,
            RingVisualState state,
            float normalizedTime)
        {
            if (references == null)
            {
                return;
            }

            EnsureVisuals();

            var pixelsPerUnit = Mathf.Max(1f, references.PixelsPerUnit);
            var clampedRadius = Mathf.Max(0.04f, radius);
            var width = Mathf.Max(0.016f, thickness / pixelsPerUnit * Mathf.Max(0.1f, style.ThicknessScale));
            var pulse = 0.94f + Mathf.Sin(Time.time * style.PulseSpeed) * 0.06f;

            var glowWidth = width * style.GlowWidthScale;
            var bodyWidth = width;
            var edgeWidth = Mathf.Max(0.012f, width * 0.3f);
            var trailWidth = Mathf.Max(0.01f, width * 0.56f);

            var glowAlpha = style.GlowAlpha;
            var bodyAlpha = style.BodyAlpha;
            var edgeAlpha = style.EdgeAlpha;
            var trailAlpha = style.TrailAlpha;

            if (state == RingVisualState.Telegraph)
            {
                glowAlpha *= style.TelegraphGlowBoost;
                bodyAlpha *= 0.68f;
                edgeAlpha *= 0.62f;
                trailAlpha *= 0.42f;
                glowWidth *= 1.12f;
            }
            else if (state == RingVisualState.Impact)
            {
                var flash = 1f + (1f - normalizedTime) * style.ImpactFlashBoost;
                glowAlpha *= flash;
                edgeAlpha *= flash;
                bodyAlpha *= 1.05f;
                edgeWidth *= 1.16f;
            }

            BuildCirclePositions(references, centerLogical, clampedRadius);

            ApplyRenderer(_glowRenderer, glowWidth, Tint(color, style.GlowColor, 1.05f, color.a * glowAlpha * pulse), _ringPositions);
            ApplyRenderer(_trailRenderer, trailWidth, Tint(color, style.TrailColor, 0.78f, color.a * trailAlpha), _ringPositions);
            ApplyRenderer(_bodyRenderer, bodyWidth, Tint(color, style.BodyColor, 0.94f, color.a * bodyAlpha), _ringPositions);
            ApplyRenderer(_edgeRenderer, edgeWidth, Tint(color, style.EdgeColor, 1.28f, color.a * edgeAlpha), _ringPositions);
            RenderSparks(references, centerLogical, clampedRadius, thickness, color, style, state, normalizedTime);
        }

        public void Clear()
        {
            EnsureVisuals();
            DisableRenderer(_legacyRootRenderer);
            DisableRenderer(_glowRenderer);
            DisableRenderer(_trailRenderer);
            DisableRenderer(_bodyRenderer);
            DisableRenderer(_edgeRenderer);

            if (_sparks == null)
            {
                return;
            }

            for (var i = 0; i < _sparks.Length; i++)
            {
                if (_sparks[i] != null)
                {
                    _sparks[i].enabled = false;
                }
            }
        }

        public void SetSortingOrder(int sortingOrder)
        {
            _sortingOrder = sortingOrder;
            EnsureVisuals();
            ApplySorting();
        }

        private void RenderSparks(
            GameReferences references,
            Vector2 centerLogical,
            float radius,
            float thickness,
            Color color,
            RingVisualStyle style,
            RingVisualState state,
            float normalizedTime)
        {
            if (_sparks == null)
            {
                return;
            }

            var activeSparkCount = Mathf.Clamp(style.SparkCount, 0, _sparks.Length);
            var sparkAlpha = color.a * style.SparkAlpha;
            if (state == RingVisualState.Telegraph)
            {
                sparkAlpha *= 0.55f;
            }
            else if (state == RingVisualState.Impact)
            {
                sparkAlpha *= 1.08f + (1f - normalizedTime) * 0.18f;
            }

            for (var i = 0; i < _sparks.Length; i++)
            {
                var spark = _sparks[i];
                if (spark == null)
                {
                    continue;
                }

                if (i >= activeSparkCount)
                {
                    spark.enabled = false;
                    continue;
                }

                var phase = i / Mathf.Max(1f, activeSparkCount) * Mathf.PI * 2f;
                var angle = phase + Time.time * style.SparkSpeed * (state == RingVisualState.Telegraph ? -0.65f : 1f);
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var logicalPosition = centerLogical + direction * radius;
                var tangentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
                var sparkScale = Mathf.Max(0.032f, thickness / references.PixelsPerUnit * 1.8f * style.SparkScale);
                var alphaPulse = 0.74f + Mathf.Sin(Time.time * (style.PulseSpeed + i * 0.45f) + phase) * 0.26f;

                spark.enabled = true;
                spark.transform.position = references.LogicalToWorld(logicalPosition);
                spark.transform.rotation = Quaternion.Euler(0f, 0f, tangentAngle);
                spark.transform.localScale = new Vector3(sparkScale, sparkScale * 0.68f, 1f);
                spark.color = Tint(color, style.EdgeColor, 1.22f, sparkAlpha * alphaPulse);
            }
        }

        private void BuildCirclePositions(GameReferences references, Vector2 centerLogical, float radius)
        {
            for (var i = 0; i < SegmentCount; i++)
            {
                var logical = centerLogical + new Vector2(s_unitCircle[i].x, s_unitCircle[i].y) * radius;
                _ringPositions[i] = references.LogicalToWorld(logical);
            }
        }

        private void EnsureVisuals()
        {
            _legacyRootRenderer ??= GetComponent<LineRenderer>();
            if (_legacyRootRenderer != null)
            {
                _legacyRootRenderer.enabled = false;
            }

            _glowRenderer ??= FindExistingLayer("Glow");
            _trailRenderer ??= FindExistingLayer("Trail");
            _bodyRenderer ??= FindExistingLayer("Body");
            _edgeRenderer ??= FindExistingLayer("Edge");

            if (_glowRenderer == null || _trailRenderer == null || _bodyRenderer == null || _edgeRenderer == null)
            {
                if (Application.isPlaying)
                {
                    // Avoid hierarchy mutation on pooled instances during gameplay.
                    _edgeRenderer ??= _legacyRootRenderer;
                    if (_edgeRenderer != null)
                    {
                        ConfigureRenderer(_edgeRenderer, RingLayerKind.Edge);
                    }
                }
                else
                {
                    BuildMissingLayers();
                }
            }

            if (_sparks == null || _sparks.Length == 0)
            {
                _sparks = new SpriteRenderer[MaxSparkCount];
            }

            for (var i = 0; i < _sparks.Length; i++)
            {
                if (_sparks[i] != null)
                {
                    continue;
                }

                var sparkTransform = transform.Find($"Spark_{i}");
                if (sparkTransform == null)
                {
                    if (Application.isPlaying)
                    {
                        continue;
                    }

                    var sparkObject = new GameObject($"Spark_{i}");
                    sparkObject.transform.SetParent(transform, false);
                    sparkTransform = sparkObject.transform;
                }

                var spark = sparkTransform.GetComponent<SpriteRenderer>();
                if (spark == null)
                {
                    spark = sparkTransform.gameObject.AddComponent<SpriteRenderer>();
                }

                spark.sprite = PrototypeVisualUtility.ArcSprite;
                spark.material = GetLayerMaterial(RingLayerKind.Edge);
                spark.enabled = false;
                _sparks[i] = spark;
            }

            ApplySorting();
        }

        private LineRenderer FindExistingLayer(string name)
        {
            var child = transform.Find(name);
            return child != null ? child.GetComponent<LineRenderer>() : null;
        }

        private void BuildMissingLayers()
        {
            _glowRenderer ??= CreateLayer("Glow", 0);
            _trailRenderer ??= CreateLayer("Trail", 1);
            _bodyRenderer ??= CreateLayer("Body", 2);
            _edgeRenderer ??= CreateLayer("Edge", 3);
        }

        private void ApplyRenderer(LineRenderer renderer, float width, Color color, Vector3[] positions)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.enabled = true;
            renderer.startWidth = width;
            renderer.endWidth = width;
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.SetPositions(positions);
        }

        private void ApplySorting()
        {
            if (_glowRenderer != null) _glowRenderer.sortingOrder = _sortingOrder;
            if (_trailRenderer != null) _trailRenderer.sortingOrder = _sortingOrder + 1;
            if (_bodyRenderer != null) _bodyRenderer.sortingOrder = _sortingOrder + 2;
            if (_edgeRenderer != null) _edgeRenderer.sortingOrder = _sortingOrder + 3;

            if (_sparks == null)
            {
                return;
            }

            for (var i = 0; i < _sparks.Length; i++)
            {
                if (_sparks[i] != null)
                {
                    _sparks[i].sortingOrder = _sortingOrder + 4;
                }
            }
        }

        private static void DisableRenderer(Renderer renderer)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        private static Color Tint(Color baseColor, Color layerTint, float brightness, float alpha)
        {
            var tint = new Color(
                Mathf.Clamp01(baseColor.r * layerTint.r * brightness),
                Mathf.Clamp01(baseColor.g * layerTint.g * brightness),
                Mathf.Clamp01(baseColor.b * layerTint.b * brightness),
                Mathf.Clamp01(alpha));
            return tint;
        }

        private static Color Tint(Color color, float brightness, float alpha)
        {
            return new Color(
                Mathf.Clamp01(color.r * brightness),
                Mathf.Clamp01(color.g * brightness),
                Mathf.Clamp01(color.b * brightness),
                Mathf.Clamp01(alpha));
        }

        private LineRenderer CreateLayer(string name, int sortingOffset)
        {
            var child = transform.Find(name);
            if (child == null)
            {
                child = new GameObject(name).transform;
                child.SetParent(transform, false);
            }

            var renderer = child.GetComponent<LineRenderer>();
            if (renderer == null)
            {
                renderer = child.gameObject.AddComponent<LineRenderer>();
            }

            var layerKind = name switch
            {
                "Glow" => RingLayerKind.Glow,
                "Trail" => RingLayerKind.Trail,
                "Body" => RingLayerKind.Body,
                _ => RingLayerKind.Edge
            };

            ConfigureRenderer(renderer, layerKind);
            renderer.sortingOrder = _sortingOrder + sortingOffset;
            renderer.enabled = false;
            return renderer;
        }

        private void ConfigureRenderer(LineRenderer renderer, RingLayerKind layerKind)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.loop = true;
            renderer.useWorldSpace = true;
            renderer.positionCount = SegmentCount;
            renderer.textureMode = LineTextureMode.Stretch;
            renderer.alignment = LineAlignment.View;
            renderer.numCapVertices = 12;
            renderer.numCornerVertices = 12;
            renderer.material = GetLayerMaterial(layerKind);
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Material GetLayerMaterial(RingLayerKind layerKind)
        {
            return layerKind switch
            {
                RingLayerKind.Glow => s_glowMaterial ??= CreateLayerMaterial("RingGlowMat", CreateProfileTexture("RingGlowTex", 0.02f, 0.28f, 0.72f, 0.28f, 0.02f)),
                RingLayerKind.Trail => s_trailMaterial ??= CreateLayerMaterial("RingTrailMat", CreateProfileTexture("RingTrailTex", 0f, 0.08f, 0.58f, 0.22f, 0f)),
                RingLayerKind.Body => s_bodyMaterial ??= CreateLayerMaterial("RingBodyMat", CreateProfileTexture("RingBodyTex", 0f, 0.42f, 0.92f, 0.42f, 0f)),
                _ => s_edgeMaterial ??= CreateLayerMaterial("RingEdgeMat", CreateProfileTexture("RingEdgeTex", 0f, 0.1f, 1f, 0.1f, 0f))
            };
        }

        private static Material CreateLayerMaterial(string materialName, Texture2D texture)
        {
            var material = new Material(Shader.Find("Sprites/Default"))
            {
                name = materialName
            };
            material.mainTexture = texture;
            return material;
        }

        private static Texture2D CreateProfileTexture(string textureName, params float[] profile)
        {
            var height = Mathf.Max(8, profile.Length * 16);
            var texture = new Texture2D(8, height, TextureFormat.RGBA32, false)
            {
                name = textureName,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < height; y++)
            {
                var t = height <= 1 ? 0f : y / (float)(height - 1);
                var alpha = SampleProfile(profile, t);
                var color = new Color(1f, 1f, 1f, alpha);
                for (var x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        private static float SampleProfile(float[] profile, float t)
        {
            if (profile == null || profile.Length == 0)
            {
                return 1f;
            }

            if (profile.Length == 1)
            {
                return profile[0];
            }

            var scaled = Mathf.Clamp01(t) * (profile.Length - 1);
            var index = Mathf.FloorToInt(scaled);
            var next = Mathf.Min(profile.Length - 1, index + 1);
            var lerp = scaled - index;
            return Mathf.Lerp(profile[index], profile[next], lerp);
        }

        private static Vector3[] BuildUnitCircle()
        {
            var points = new Vector3[SegmentCount];
            for (var i = 0; i < SegmentCount; i++)
            {
                var t = i / (float)SegmentCount * Mathf.PI * 2f;
                points[i] = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f);
            }

            return points;
        }
    }
}
