using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.Prototype
{
    public sealed class TutorialOverlayController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup rootGroup;
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text tipsText;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;

        private Tween _cardTween;

        public void Initialize()
        {
            if (rootGroup != null)
            {
                rootGroup.alpha = 0f;
                rootGroup.blocksRaycasts = false;
                rootGroup.interactable = false;
            }

            StyleButton(primaryButton, true);
            StyleButton(secondaryButton, false);
        }

        public void ShowCard(
            string title,
            string subtitle,
            IReadOnlyList<string> tips,
            string primaryLabel,
            Action primaryAction,
            string secondaryLabel = null,
            Action secondaryAction = null)
        {
            titleText.text = title;
            subtitleText.text = subtitle;
            tipsText.text = FormatTips(tips);

            BindButton(primaryButton, primaryLabel, primaryAction);
            BindButton(secondaryButton, secondaryLabel, secondaryAction);
            if (secondaryButton != null)
            {
                secondaryButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(secondaryLabel) && secondaryAction != null);
            }

            rootGroup.alpha = 1f;
            rootGroup.blocksRaycasts = true;
            rootGroup.interactable = true;

            if (cardRoot != null)
            {
                _cardTween?.Kill();
                cardRoot.localScale = Vector3.one * 0.965f;
                _cardTween = cardRoot.DOScale(1f, 0.24f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        public void Hide()
        {
            if (rootGroup == null)
            {
                return;
            }

            rootGroup.alpha = 0f;
            rootGroup.blocksRaycasts = false;
            rootGroup.interactable = false;
        }

        public void HideImmediate()
        {
            _cardTween?.Kill();
            Hide();
        }

        private void OnDestroy()
        {
            _cardTween?.Kill();
        }

        private static string FormatTips(IReadOnlyList<string> tips)
        {
            if (tips == null || tips.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < tips.Count; i++)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(i + 1);
                builder.Append(". ");
                builder.Append(tips[i]);
            }

            return builder.ToString();
        }

        private static void BindButton(Button button, string label, Action callback)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }

            button.onClick.RemoveAllListeners();
            if (callback != null)
            {
                button.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlaySfx(AudioCue.UiClick);
                    callback.Invoke();
                });
            }
        }

        private static void StyleButton(Button button, bool primary)
        {
            if (button == null)
            {
                return;
            }

            var image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = primary
                    ? new Color(0.75f, 0.95f, 1f, 0.98f)
                    : new Color(1f, 1f, 1f, 0.08f);
            }

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.fontStyle = FontStyles.Bold;
                text.color = primary
                    ? new Color(0.03f, 0.08f, 0.12f, 1f)
                    : PrototypeVisualUtility.TextPrimary;
            }
        }
    }
}
