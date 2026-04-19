using UnityEngine;

namespace CupHeadClone.Prototype
{
    public static class PrototypeVisualUtility
    {
        public static readonly Color BackgroundTop = new(0.031f, 0.086f, 0.149f, 1f);
        public static readonly Color BackgroundMid = new(0.027f, 0.067f, 0.122f, 1f);
        public static readonly Color BackgroundBottom = new(0.016f, 0.03f, 0.075f, 1f);
        public static readonly Color Panel = new(0.039f, 0.071f, 0.125f, 0.78f);
        public static readonly Color PanelBorder = new(0.478f, 0.667f, 1f, 0.18f);
        public static readonly Color TextPrimary = new(0.906f, 0.945f, 1f, 1f);
        public static readonly Color TextMuted = new(0.61f, 0.706f, 0.847f, 1f);
        public static readonly Color PlayerCyan = new(0.341f, 0.858f, 1f, 1f);
        public static readonly Color PlayerCore = Color.white;
        public static readonly Color BossRose = new(1f, 0.49f, 0.62f, 1f);
        public static readonly Color BossGlow = new(1f, 0.58f, 0.78f, 1f);
        public static readonly Color EnemyBlue = new(0.325f, 0.667f, 1f, 1f);
        public static readonly Color ParryPurple = new(0.769f, 0.424f, 1f, 1f);
        public static readonly Color CounterGold = new(1f, 0.71f, 0.3f, 1f);
        public static readonly Color WeakGold = new(1f, 0.831f, 0.392f, 1f);
        public static readonly Color LaserCore = new(1f, 0.945f, 0.678f, 1f);
        public static readonly Color LaserEdge = new(1f, 0.561f, 0.365f, 1f);
        public static readonly Color HealMint = new(0.553f, 0.969f, 0.776f, 1f);

        private static Sprite _square;
        private static Sprite _circle;
        private static Sprite _arc;
        private static Sprite _ring;

        public static Sprite SquareSprite => _square ??= CreateSquareSprite();
        public static Sprite CircleSprite => _circle ??= CreateCircleSprite();
        public static Sprite ArcSprite => _arc ??= CreateArcSprite();
        public static Sprite RingSprite => _ring ??= CreateRingSprite();

        public static SpriteRenderer EnsureSpriteChild(Transform parent, string name, Sprite sprite, Color color, int sortingOrder)
        {
            var child = parent.Find(name);
            GameObject go;
            if (child == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = child.gameObject;
            }

            var renderer = go.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = go.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Sprite CreateSquareSprite()
        {
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            {
                name = "PrototypeSquare",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[64];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 8f);
        }

        private static Sprite CreateCircleSprite()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "PrototypeCircle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.48f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var index = y * size + x;
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(radius - 2f, radius, distance));
                    pixels[index] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateArcSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "PrototypeArc",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            var center = new Vector2((size - 1) * 0.5f, size * 0.56f);
            var outerRadius = size * 0.46f;
            var innerRadius = outerRadius * 0.82f;
            const float arcHalfAngle = 62f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var index = y * size + x;
                    var delta = new Vector2(x, y) - center;
                    var distance = delta.magnitude;
                    if (distance < innerRadius || distance > outerRadius)
                    {
                        pixels[index] = Color.clear;
                        continue;
                    }

                    var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
                    var angleFromUp = Mathf.DeltaAngle(90f, angle);
                    if (Mathf.Abs(angleFromUp) > arcHalfAngle)
                    {
                        pixels[index] = Color.clear;
                        continue;
                    }

                    var outerAlpha = Mathf.Clamp01(1f - Mathf.InverseLerp(outerRadius - 2f, outerRadius, distance));
                    var innerAlpha = Mathf.Clamp01(Mathf.InverseLerp(innerRadius, innerRadius + 2f, distance));
                    pixels[index] = new Color(1f, 1f, 1f, Mathf.Min(outerAlpha, innerAlpha));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.35f), size);
        }

        private static Sprite CreateRingSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "PrototypeRing",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var outerRadius = size * 0.47f;
            var innerRadius = outerRadius * 0.9f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var index = y * size + x;
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    if (distance < innerRadius || distance > outerRadius)
                    {
                        pixels[index] = Color.clear;
                        continue;
                    }

                    var outerAlpha = Mathf.Clamp01(1f - Mathf.InverseLerp(outerRadius - 2.5f, outerRadius, distance));
                    var innerAlpha = Mathf.Clamp01(Mathf.InverseLerp(innerRadius, innerRadius + 2.5f, distance));
                    pixels[index] = new Color(1f, 1f, 1f, Mathf.Min(outerAlpha, innerAlpha));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
