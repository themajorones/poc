using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CupHeadClone.Prototype
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private bool passiveMode;
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private TMP_Text bossLabel;
        [SerializeField] private TMP_Text moveLabel;
        [SerializeField] private TMP_Text breakLabel;
        [SerializeField] private Image bossHpFill;
        [SerializeField] private Image bossPoiseFill;
        [SerializeField] private Image rageFill;
        [SerializeField] private Image[] hpPips;
        [SerializeField] private RectTransform hpPipRoot;
        [SerializeField] private Image hpPipTemplate;

        private GameController _game;
        private string _cachedBossLabel;
        private string _cachedMoveLabel;
        private string _cachedBreakLabel;
        private float _cachedBossHpFill = -1f;
        private float _cachedBossPoiseFill = -1f;
        private float _cachedRageFill = -1f;
        private Color _cachedPoiseColor = Color.clear;
        private float _displayBossHp;
        private float _displayBossPoise;
        private float _displayRage;
        private Tween _breakTween;
        private Tween _rageTween;

        public void Initialize(GameController game)
        {
            _game = game;
            if (rootGroup == null)
            {
                rootGroup = GetComponent<CanvasGroup>();
                if (rootGroup == null)
                {
                    rootGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (passiveMode)
            {
                rootGroup.alpha = 0f;
                rootGroup.blocksRaycasts = false;
                rootGroup.interactable = false;
                return;
            }

            PolishLayout();
            EnsureHpPipGroup();
            RefreshState();
        }

        public void Tick(float dt)
        {
            if (_game == null || passiveMode)
            {
                return;
            }

            if (_game.Boss != null && _game.Boss.Active)
            {
                _displayBossHp = Mathf.Lerp(_displayBossHp, _game.Boss.MaxHp <= 0f ? 0f : _game.Boss.CurrentHp / _game.Boss.MaxHp, 1f - Mathf.Exp(-dt * 10f));
                _displayBossPoise = Mathf.Lerp(_displayBossPoise, _game.Boss.MaxPoise <= 0f ? 0f : _game.Boss.CurrentPoise / _game.Boss.MaxPoise, 1f - Mathf.Exp(-dt * 10f));
                SetFillIfChanged(bossHpFill, _displayBossHp, ref _cachedBossHpFill);
                SetFillIfChanged(bossPoiseFill, _displayBossPoise, ref _cachedBossPoiseFill);
                SetColorIfChanged(bossPoiseFill, _game.Boss.IsRecoveringPoise
                    ? PrototypeVisualUtility.WeakGold
                    : PrototypeVisualUtility.ParryPurple, ref _cachedPoiseColor);
                SetTextIfChanged(moveLabel, $"Move: {(_game.Boss.BreakTimer > 0f ? "BREAK" : BossPatternDefinitions.GetLabel(_game.Boss.CurrentMove))}", ref _cachedMoveLabel);
                SetTextIfChanged(breakLabel, _game.Boss.BreakTimer > 0f ? $"BREAK {_game.Boss.BreakTimer:0.0}s" : string.Empty, ref _cachedBreakLabel);
            }
            else
            {
                SetTextIfChanged(breakLabel, string.Empty, ref _cachedBreakLabel);
                SetTextIfChanged(moveLabel, "Move: REST", ref _cachedMoveLabel);
            }

            _displayRage = Mathf.Lerp(_displayRage, _game.RageSystem.Current / Mathf.Max(1f, _game.RageSystem.Max), 1f - Mathf.Exp(-dt * 10f));
            SetFillIfChanged(rageFill, _displayRage, ref _cachedRageFill);
            ApplyDynamicPolish();
        }

        public void RefreshState()
        {
            if (_game == null)
            {
                return;
            }

            if (passiveMode)
            {
                if (rootGroup != null)
                {
                    rootGroup.alpha = 0f;
                    rootGroup.blocksRaycasts = false;
                    rootGroup.interactable = false;
                }

                return;
            }

            var visible = _game.State == GameController.RunState.Playing;
            if (rootGroup != null)
            {
                rootGroup.alpha = visible ? 1f : 0f;
                rootGroup.blocksRaycasts = false;
                rootGroup.interactable = false;
            }

            if (_game.Boss != null && _game.Boss.Active)
            {
                SetTextIfChanged(bossLabel, $"BOSS {_game.Boss.BossIndex + 1}/{_game.BossRushController.BossCount}  {_game.Boss.BossName}", ref _cachedBossLabel);
                _displayBossHp = _game.Boss.MaxHp <= 0f ? 0f : _game.Boss.CurrentHp / _game.Boss.MaxHp;
                _displayBossPoise = _game.Boss.MaxPoise <= 0f ? 0f : _game.Boss.CurrentPoise / _game.Boss.MaxPoise;
                SetFillIfChanged(bossHpFill, _displayBossHp, ref _cachedBossHpFill);
                SetFillIfChanged(bossPoiseFill, _displayBossPoise, ref _cachedBossPoiseFill);
                SetColorIfChanged(bossPoiseFill, _game.Boss.IsRecoveringPoise
                    ? PrototypeVisualUtility.WeakGold
                    : PrototypeVisualUtility.ParryPurple, ref _cachedPoiseColor);
                SetTextIfChanged(moveLabel, $"Move: {(_game.Boss.BreakTimer > 0f ? "BREAK" : BossPatternDefinitions.GetLabel(_game.Boss.CurrentMove))}", ref _cachedMoveLabel);
                SetTextIfChanged(breakLabel, _game.Boss.BreakTimer > 0f ? $"BREAK {_game.Boss.BreakTimer:0.0}s" : string.Empty, ref _cachedBreakLabel);
            }
            else
            {
                SetTextIfChanged(bossLabel, "BOSS --/--", ref _cachedBossLabel);
                SetTextIfChanged(moveLabel, "Move: REST", ref _cachedMoveLabel);
                SetTextIfChanged(breakLabel, string.Empty, ref _cachedBreakLabel);
                SetFillIfChanged(bossHpFill, 0f, ref _cachedBossHpFill);
                SetFillIfChanged(bossPoiseFill, 0f, ref _cachedBossPoiseFill);
            }

            _displayRage = _game.RageSystem.Current / Mathf.Max(1f, _game.RageSystem.Max);
            SetFillIfChanged(rageFill, _displayRage, ref _cachedRageFill);
            EnsureHpPips(_game.CurrentPlayerMaxHp);
            for (var i = 0; i < hpPips.Length; i++)
            {
                hpPips[i].gameObject.SetActive(i < _game.CurrentPlayerMaxHp);
                hpPips[i].color = i < _game.Player.CurrentHp
                    ? PrototypeVisualUtility.HealMint
                    : new Color(1f, 1f, 1f, 0.1f);
            }

            ApplyDynamicPolish();
        }

        private void PolishLayout()
        {
            var texts = GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in texts)
            {
                text.color = text == breakLabel ? PrototypeVisualUtility.WeakGold : text == moveLabel ? PrototypeVisualUtility.TextMuted : PrototypeVisualUtility.TextPrimary;
                text.fontStyle = FontStyles.Bold;
            }

            foreach (var image in GetComponentsInChildren<Image>(true))
            {
                switch (image.name)
                {
                    case "TopPanel":
                    case "BottomPanel":
                        image.color = PrototypeVisualUtility.Panel;
                        break;
                    case "BossHp_Back":
                    case "BossPoise_Back":
                    case "Rage_Back":
                        image.color = new Color(1f, 1f, 1f, 0.08f);
                        break;
                }
            }

            if (bossHpFill != null)
            {
                bossHpFill.color = PrototypeVisualUtility.BossRose;
            }

            if (bossPoiseFill != null)
            {
                bossPoiseFill.color = PrototypeVisualUtility.ParryPurple;
            }

            if (rageFill != null)
            {
                rageFill.color = PrototypeVisualUtility.CounterGold;
            }

            _breakTween = breakLabel != null
                ? breakLabel.transform.DOScale(1.06f, 0.26f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)
                : null;
            _rageTween = rageFill != null
                ? rageFill.transform.DOScaleY(1.08f, 0.32f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine)
                : null;
        }

        private void ApplyDynamicPolish()
        {
            if (breakLabel != null)
            {
                var breakActive = _game.Boss != null && _game.Boss.BreakTimer > 0f;
                breakLabel.color = breakActive ? PrototypeVisualUtility.WeakGold : PrototypeVisualUtility.TextMuted.WithAlpha(0f);
                if (_breakTween != null)
                {
                    _breakTween.timeScale = breakActive ? 1f : 0f;
                }
            }

            if (_rageTween != null)
            {
                _rageTween.timeScale = _game.RageSystem.IsFull ? 1f : 0f;
            }

            if (moveLabel != null)
            {
                moveLabel.color = _game.Boss != null && _game.Boss.Phase == BossController.BossPhase.Telegraph
                    ? PrototypeVisualUtility.BossGlow
                    : PrototypeVisualUtility.TextMuted;
            }
        }

        private void OnDestroy()
        {
            _breakTween?.Kill();
            _rageTween?.Kill();
        }

        private static void SetTextIfChanged(TMP_Text target, string value, ref string cache)
        {
            if (target == null || cache == value)
            {
                return;
            }

            cache = value;
            target.text = value;
        }

        private static void SetFillIfChanged(Image target, float value, ref float cache)
        {
            if (target == null || Mathf.Abs(cache - value) < 0.001f)
            {
                return;
            }

            cache = value;
            target.fillAmount = value;
        }

        private static void SetColorIfChanged(Image target, Color value, ref Color cache)
        {
            if (target == null || cache == value)
            {
                return;
            }

            cache = value;
            target.color = value;
        }

        private void EnsureHpPips(int targetCount)
        {
            EnsureHpPipGroup();
            if (hpPips == null || hpPips.Length == 0 || targetCount <= hpPips.Length)
            {
                return;
            }

            var template = hpPipTemplate != null ? hpPipTemplate : hpPips[hpPips.Length - 1];
            var parent = template != null ? template.transform.parent : null;
            if (template == null || parent == null)
            {
                return;
            }

            var next = new Image[targetCount];
            for (var i = 0; i < hpPips.Length; i++)
            {
                next[i] = hpPips[i];
            }

            for (var i = hpPips.Length; i < targetCount; i++)
            {
                var clone = Instantiate(template.gameObject, parent).GetComponent<Image>();
                clone.name = $"Hp_{i}";
                var rt = clone.rectTransform;
                rt.anchorMin = template.rectTransform.anchorMin;
                rt.anchorMax = template.rectTransform.anchorMax;
                rt.pivot = template.rectTransform.pivot;
                rt.sizeDelta = template.rectTransform.sizeDelta;
                rt.anchoredPosition = new Vector2(70f + i * 34f, -108f);
                next[i] = clone;
            }

            hpPips = next;
            if (hpPipRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(hpPipRoot);
            }
        }

        private void EnsureHpPipGroup()
        {
            if (hpPips == null || hpPips.Length == 0)
            {
                return;
            }

            if (hpPipRoot == null)
            {
                var anchor = hpPips[0] != null ? hpPips[0].transform.parent as RectTransform : null;
                if (anchor == null)
                {
                    return;
                }

                hpPipRoot = new GameObject("HpPipGroup", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
                hpPipRoot.SetParent(anchor, false);
                hpPipRoot.anchorMin = new Vector2(0f, 1f);
                hpPipRoot.anchorMax = new Vector2(0f, 1f);
                hpPipRoot.pivot = new Vector2(0f, 1f);
                hpPipRoot.anchoredPosition = new Vector2(58f, -96f);
                hpPipRoot.sizeDelta = new Vector2(260f, 34f);

                var group = hpPipRoot.GetComponent<HorizontalLayoutGroup>();
                group.childAlignment = TextAnchor.UpperLeft;
                group.spacing = 10f;
                group.childControlHeight = false;
                group.childControlWidth = false;
                group.childForceExpandHeight = false;
                group.childForceExpandWidth = false;
            }

            for (var i = 0; i < hpPips.Length; i++)
            {
                if (hpPips[i] == null)
                {
                    continue;
                }

                hpPips[i].transform.SetParent(hpPipRoot, false);
                var rt = hpPips[i].rectTransform;
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(24f, 24f);
            }

            hpPipTemplate ??= hpPips[0];
            if (hpPipRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(hpPipRoot);
            }
        }
    }
}
