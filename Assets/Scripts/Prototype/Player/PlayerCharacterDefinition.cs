using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    [CreateAssetMenu(fileName = "PlayerCharacter", menuName = "ParryShooter/Player/Character Definition")]
    public sealed class PlayerCharacterDefinition : ScriptableObject
    {
        [BoxGroup("Identity")]
        [SerializeField] private string characterId = "blue";
        [BoxGroup("Identity")]
        [SerializeField] private string displayName = "Blue";
        [BoxGroup("Identity")]
        [PreviewField(70, ObjectFieldAlignment.Left)]
        [SerializeField] private Sprite previewSprite;

        [BoxGroup("Prefab")]
        [AssetsOnly]
        [SerializeField] private GameObject playerPrefab;

        [BoxGroup("Stats")]
        [SerializeField] private int maxHp = 3;
        [BoxGroup("Stats")]
        [SerializeField] private float rageMax = 100f;

        public string CharacterId => string.IsNullOrWhiteSpace(characterId) ? name : characterId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public Sprite PreviewSprite => previewSprite;
        public GameObject PlayerPrefab => playerPrefab;
        public int MaxHp => Mathf.Max(1, maxHp);
        public float RageMax => Mathf.Max(1f, rageMax);
    }
}
