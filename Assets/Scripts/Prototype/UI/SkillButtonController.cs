using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CupHeadClone.Prototype
{
    public sealed class SkillButtonController : MonoBehaviour
    {
        [SerializeField] private bool passiveMode;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup canvasGroup;

        private GameController _game;
        private Tween _pulseTween;
        private Image _buttonImage;

        public void ForceActiveMode()
        {
            passiveMode = false;
        }

        public void Initialize(GameController game)
        {
            _game = game;
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (passiveMode)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                return;
            }

            _buttonImage = button != null ? button.GetComponent<Image>() : null;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => { _game.SkillController.TryActivateSkill(); });
            _pulseTween = transform.DOScale(1.06f, 0.35f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            if (_buttonImage != null)
            {
                _buttonImage.color = new Color(1f, 0.72f, 0.32f, 0.42f);
            }

            if (label != null)
            {
                label.fontStyle = FontStyles.Bold;
            }

            RefreshState();
        }

        public void RefreshState()
        {
            if (_game == null)
            {
                return;
            }

            if (passiveMode)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                return;
            }

            var visible = _game.State == GameController.RunState.Playing;
            canvasGroup.alpha = visible ? (_game.RageSystem.IsFull ? 0.82f : 0.28f) : 0f;
            canvasGroup.interactable = visible && _game.RageSystem.IsFull;
            canvasGroup.blocksRaycasts = visible;
            label.text = _game.RageSystem.IsFull ? "SKILL\nREADY" : "SKILL";
            if (_buttonImage != null)
            {
                _buttonImage.color = _game.RageSystem.IsFull
                    ? new Color(1f, 0.82f, 0.24f, 0.96f)
                    : new Color(1f, 0.72f, 0.32f, 0.34f);
            }

            if (label != null)
            {
                label.color = _game.RageSystem.IsFull
                    ? new Color(0.08f, 0.12f, 0.16f, 1f)
                    : new Color(0.12f, 0.16f, 0.22f, 0.9f);
            }

            if (_pulseTween != null)
            {
                _pulseTween.timeScale = _game.RageSystem.IsFull ? 1f : 0f;
            }
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
        }
    }
}
