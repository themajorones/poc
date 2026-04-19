using UnityEngine;

namespace CupHeadClone.Prototype
{
    public static class LocalizationRuntime
    {
        public static event System.Action LocaleChanged;

        public static void ApplyLocale(TutorialLocalizationAsset localization, string localeCode)
        {
            ProjectSettingsState.Locale = localeCode;
            ProjectSettingsState.Save();

            if (localization != null)
            {
                localization.ActiveLocale = localeCode;
            }

            LocaleChanged?.Invoke();
        }
    }
}
