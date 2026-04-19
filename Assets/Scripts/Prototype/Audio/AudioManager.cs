using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CupHeadClone.Prototype
{
    public sealed class AudioManager : MonoBehaviour
    {
        private const string AudioCatalogPath = "Assets/PrototypeGenerated/Config/ProjectAudioCatalog.asset";

        [SerializeField] private TutorialLocalizationAsset localization;
        [SerializeField] private ProjectAudioCatalog catalog;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private List<AudioSource> sfxSources = new();
        [SerializeField] private int sfxVoices = 8;

        private static AudioManager _instance;
        private int _sfxIndex;
        private AudioCue _currentMusicCue = AudioCue.None;
        private AudioClip _currentMusicClip;

        public static AudioManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                _instance.AbsorbSceneReferences(this);
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent != null)
            {
                transform.SetParent(null, true);
            }
            DontDestroyOnLoad(gameObject);
            EnsureDependencies();
            ApplySettings();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        public void ApplySettings()
        {
            if (localization != null)
            {
                if (ProjectSettingsState.HasSavedLocale)
                {
                    localization.ActiveLocale = ProjectSettingsState.Locale;
                }
                else if (localization.Locales.Count > 0)
                {
                    localization.ActiveLocale = localization.Locales[0].LocaleCode;
                }
            }

            if (musicSource != null)
            {
                musicSource.volume = ProjectSettingsState.MasterVolume * ProjectSettingsState.MusicVolume;
            }
        }

        public void PlayMusic(AudioCue cue)
        {
            PlayMusic(cue.ToString());
        }

        public void PlayMusic(string cueId)
        {
            if (catalog == null || musicSource == null)
            {
                return;
            }

            var entry = catalog.Get(cueId);
            if (entry == null)
            {
                return;
            }

            AudioClip targetClip;
            if (_currentMusicCue.ToString() == cueId && _currentMusicClip != null)
            {
                targetClip = _currentMusicClip;
            }
            else
            {
                targetClip = entry.GetRandomClip();
            }

            if (targetClip == null)
            {
                return;
            }

            var alreadyPlayingRequestedCue =
                _currentMusicCue.ToString() == cueId &&
                musicSource.clip == targetClip &&
                musicSource.isPlaying;

            if (alreadyPlayingRequestedCue)
            {
                return;
            }

            if (System.Enum.TryParse<AudioCue>(cueId, out var builtInCue))
            {
                _currentMusicCue = builtInCue;
            }
            else
            {
                _currentMusicCue = AudioCue.None;
            }
            _currentMusicClip = targetClip;
            musicSource.clip = targetClip;
            musicSource.loop = entry.Loop;
            musicSource.pitch = 1f;
            musicSource.volume = ProjectSettingsState.MasterVolume * ProjectSettingsState.MusicVolume;
            musicSource.Play();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureDependencies();
            ApplySettings();

            if (_currentMusicCue != AudioCue.None && (musicSource == null || musicSource.clip == null || !musicSource.isPlaying))
            {
                PlayMusic(_currentMusicCue);
            }
        }

        private void AbsorbSceneReferences(AudioManager other)
        {
            if (other == null)
            {
                return;
            }

            if (catalog == null && other.catalog != null)
            {
                catalog = other.catalog;
            }

            if (localization == null && other.localization != null)
            {
                localization = other.localization;
            }
        }

        public void PlaySfx(AudioCue cue)
        {
            PlaySfx(cue.ToString());
        }

        public void PlaySfx(string cueId)
        {
            if (catalog == null)
            {
                return;
            }

            var entry = catalog.Get(cueId);
            if (entry == null)
            {
                return;
            }

            var selectedClip = entry.GetRandomClip();
            if (selectedClip == null)
            {
                return;
            }

            var source = GetNextSfxSource();
            source.clip = selectedClip;
            source.loop = false;
            source.pitch = Random.Range(entry.MinPitch, entry.MaxPitch);
            source.volume = ProjectSettingsState.MasterVolume * ProjectSettingsState.SfxVolume;
            source.Play();
        }

        private AudioSource GetNextSfxSource()
        {
            EnsureDependencies();
            if (sfxSources.Count == 0)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSources.Add(source);
                return source;
            }

            var sourceIndex = _sfxIndex % sfxSources.Count;
            _sfxIndex += 1;
            return sfxSources[sourceIndex];
        }

        private void EnsureDependencies()
        {
#if UNITY_EDITOR
            if (catalog == null)
            {
                catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<ProjectAudioCatalog>(AudioCatalogPath);
            }
            if (localization == null)
            {
                localization = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>("Assets/PrototypeGenerated/Config/TutorialLocalization.asset");
            }
#endif
            if (musicSource == null)
            {
                var musicObject = transform.Find("MusicSource");
                if (musicObject == null)
                {
                    musicObject = new GameObject("MusicSource").transform;
                    musicObject.SetParent(transform, false);
                }

                musicSource = musicObject.GetComponent<AudioSource>();
                if (musicSource == null)
                {
                    musicSource = musicObject.gameObject.AddComponent<AudioSource>();
                }

                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            while (sfxSources.Count < sfxVoices)
            {
                var sourceObject = new GameObject($"SfxSource_{sfxSources.Count:00}");
                sourceObject.transform.SetParent(transform, false);
                var source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSources.Add(source);
            }
        }
    }
}
