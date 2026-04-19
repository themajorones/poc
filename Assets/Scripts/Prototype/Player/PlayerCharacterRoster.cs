using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    [CreateAssetMenu(fileName = "PlayerCharacterRoster", menuName = "ParryShooter/Player/Character Roster")]
    public sealed class PlayerCharacterRoster : ScriptableObject
    {
        [BoxGroup("Roster")]
        [SerializeField] private int defaultIndex;

        [BoxGroup("Roster")]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        [SerializeField] private List<PlayerCharacterDefinition> characters = new();

        public IReadOnlyList<PlayerCharacterDefinition> Characters => characters;
        public int DefaultIndex => Mathf.Clamp(defaultIndex, 0, Mathf.Max(0, characters.Count - 1));
        public int Count => characters != null ? characters.Count : 0;

        public PlayerCharacterDefinition GetCharacter(int index)
        {
            if (characters == null || characters.Count == 0)
            {
                return null;
            }

            return characters[Mathf.Clamp(index, 0, characters.Count - 1)];
        }

        public int WrapIndex(int index)
        {
            if (characters == null || characters.Count == 0)
            {
                return -1;
            }

            if (index < 0)
            {
                index = characters.Count - 1;
            }
            else if (index >= characters.Count)
            {
                index = 0;
            }

            return index;
        }
    }
}
