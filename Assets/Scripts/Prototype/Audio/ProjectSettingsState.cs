using UnityEngine;

namespace CupHeadClone.Prototype
{
    public static class ProjectSettingsState
    {
        private const string MasterVolumeKey = "ParryShooter.Settings.MasterVolume";
        private const string MusicVolumeKey = "ParryShooter.Settings.MusicVolume";
        private const string SfxVolumeKey = "ParryShooter.Settings.SfxVolume";
        private const string LocaleKey = "ParryShooter.Settings.Locale";

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MasterVolumeKey, 0.5f);
            set => PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(value));
        }

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
            set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxVolumeKey, 0.5f);
            set => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
        }

        public static string Locale
        {
            get => PlayerPrefs.GetString(LocaleKey, string.Empty);
            set => PlayerPrefs.SetString(LocaleKey, string.IsNullOrWhiteSpace(value) ? string.Empty : value);
        }

        public static bool HasSavedLocale => PlayerPrefs.HasKey(LocaleKey) && !string.IsNullOrWhiteSpace(Locale);

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
