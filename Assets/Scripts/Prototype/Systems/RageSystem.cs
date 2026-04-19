using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class RageSystem : MonoBehaviour
    {
        private GameController _game;

        public float Current { get; private set; }
        public float Max => _game.CurrentRageMax;
        public bool IsFull => Current >= Max - 0.01f;

        public void Initialize(GameController game)
        {
            _game = game;
        }

        public void ResetState()
        {
            Current = 0f;
        }

        public void Add(float amount)
        {
            Current = Mathf.Clamp(Current + amount, 0f, Max);
            _game.HudController.RefreshState();
            _game.SkillButtonController.RefreshState();
        }

        public void ConsumeAll()
        {
            Current = 0f;
            _game.HudController.RefreshState();
            _game.SkillButtonController.RefreshState();
        }
    }
}
