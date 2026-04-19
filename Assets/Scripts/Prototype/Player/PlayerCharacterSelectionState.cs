using UnityEngine;

namespace CupHeadClone.Prototype
{
    public static class PlayerCharacterSelectionState
    {
        private const string SelectedIndexKey = "parryshooter.player.selected_index";

        public static int LoadIndex(PlayerCharacterRoster roster)
        {
            if (roster == null || roster.Count <= 0)
            {
                return -1;
            }

            var fallback = roster.DefaultIndex;
            var saved = PlayerPrefs.GetInt(SelectedIndexKey, fallback);
            return Mathf.Clamp(saved, 0, roster.Count - 1);
        }

        public static void SaveIndex(PlayerCharacterRoster roster, int index)
        {
            if (roster == null || roster.Count <= 0)
            {
                return;
            }

            PlayerPrefs.SetInt(SelectedIndexKey, Mathf.Clamp(index, 0, roster.Count - 1));
            PlayerPrefs.Save();
        }
    }
}
