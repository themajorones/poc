using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    [CreateAssetMenu(menuName = "ParryShooter/Tutorial Localization", fileName = "TutorialLocalization")]
    public sealed class TutorialLocalizationAsset : ScriptableObject
    {
        [SerializeField] private string activeLocale = "vi";
        [SerializeField] private List<TutorialLocaleTable> locales = new();

        public string ActiveLocale
        {
            get => activeLocale;
            set => activeLocale = string.IsNullOrWhiteSpace(value) ? "vi" : value.Trim();
        }

        public IReadOnlyList<TutorialLocaleTable> Locales => locales;

        public string Get(string key, string fallback = "")
        {
            var localeCode = ProjectSettingsState.HasSavedLocale ? ProjectSettingsState.Locale : activeLocale;
            return Get(localeCode, key, fallback);
        }

        public string Get(string localeCode, string key, string fallback = "")
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return fallback;
            }

            for (var i = 0; i < locales.Count; i++)
            {
                var locale = locales[i];
                if (!string.Equals(locale.LocaleCode, localeCode, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = locale.Get(key);
                return string.IsNullOrEmpty(value) ? fallback : value;
            }

            return fallback;
        }

        public void SetLocales(List<TutorialLocaleTable> values)
        {
            locales = values ?? new List<TutorialLocaleTable>();
        }
    }

    [Serializable]
    public sealed class TutorialLocaleTable
    {
        [SerializeField] private string localeCode = "vi";
        [SerializeField] private string displayName = "Vietnamese";
        [SerializeField] private List<TutorialLocalizedEntry> entries = new();

        public string LocaleCode
        {
            get => localeCode;
            set => localeCode = string.IsNullOrWhiteSpace(value) ? localeCode : value.Trim();
        }
        public string DisplayName
        {
            get => displayName;
            set => displayName = string.IsNullOrWhiteSpace(value) ? localeCode : value.Trim();
        }
        public List<TutorialLocalizedEntry> Entries => entries;

        public TutorialLocaleTable(string localeCode, string displayName)
        {
            this.localeCode = localeCode;
            this.displayName = displayName;
        }

        public string Get(string key)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].Key, key, StringComparison.Ordinal))
                {
                    return entries[i].Value;
                }
            }

            return string.Empty;
        }

        public void Set(string key, string value)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (string.Equals(entries[i].Key, key, StringComparison.Ordinal))
                {
                    entries[i].Value = value;
                    return;
                }
            }

            entries.Add(new TutorialLocalizedEntry(key, value));
        }
    }

    [Serializable]
    public sealed class TutorialLocalizedEntry
    {
        [SerializeField] private string key;
        [SerializeField] private string value;

        public string Key => key;
        public string Value
        {
            get => value;
            set => this.value = value;
        }

        public TutorialLocalizedEntry(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
