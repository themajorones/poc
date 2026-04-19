using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public enum AudioCue
    {
        None,
        MenuMusic,
        BossRushMusic,
        UiClick,
        UiOpen,
        UiClose,
        PlayerShoot,
        ParrySuccess,
        CounterHit,
        PlayerHit,
        PlayerDeath,
        SkillCast,
        BossBreak,
        LessonComplete,
        TutorialComplete,
        PlayerShootStraight,
        PlayerShootSpreadshot,
        PlayerShootChaser,
        PlayerParrySpecialCounter,
        PlayerParrySpecialDefensiveRing,
        PlayerParrySpecialMolotov,
        MolotovFireZone,
        PlayerSkillLaserCast,
        PlayerSkillGlobalRingCast,
        PlayerSkillStickyProjectileCast
    }

    [CreateAssetMenu(menuName = "ParryShooter/Audio Catalog", fileName = "ProjectAudioCatalog")]
    public sealed class ProjectAudioCatalog : ScriptableObject
    {
        [SerializeField] private List<AudioCueEntry> entries = new();

        public AudioCueEntry Get(AudioCue cue)
        {
            return Get(cue.ToString());
        }

        public AudioCueEntry Get(string cueId)
        {
            if (string.IsNullOrWhiteSpace(cueId))
            {
                return null;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                entries[i]?.MigrateLegacyClip();
                if (string.Equals(entries[i].CueId, cueId, StringComparison.Ordinal))
                {
                    return entries[i];
                }
            }

            return null;
        }

        public void SetEntries(List<AudioCueEntry> values)
        {
            entries = values ?? new List<AudioCueEntry>();
        }

        public void EnsureEntries(List<AudioCueEntry> defaults)
        {
            entries ??= new List<AudioCueEntry>();
            defaults ??= new List<AudioCueEntry>();

            for (var i = 0; i < defaults.Count; i++)
            {
                defaults[i]?.MigrateLegacyClip();
                var existing = Get(defaults[i].CueId);
                if (existing == null)
                {
                    entries.Add(defaults[i]);
                    continue;
                }

                existing.ApplyDefaultsFrom(defaults[i]);
            }
        }

        private void OnValidate()
        {
            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                entries[i]?.MigrateLegacyClip();
            }
        }
    }

    [Serializable]
    public sealed class AudioCueEntry
    {
        [FormerlySerializedAs("cue")]
        [SerializeField, HideInInspector] private AudioCue legacyCue = AudioCue.None;
        [SerializeField] private string cueId;
        [FormerlySerializedAs("clip")]
        [SerializeField, HideInInspector] private AudioClip legacyClip;
        [SerializeField] private List<AudioClip> clips = new();
        [SerializeField] private float volume = 1f;
        [SerializeField] private float minPitch = 1f;
        [SerializeField] private float maxPitch = 1f;
        [SerializeField] private bool loop;

        public string CueId => cueId;
        public IReadOnlyList<AudioClip> Clips => clips;
        public float Volume => volume;
        public float MinPitch => minPitch;
        public float MaxPitch => maxPitch;
        public bool Loop => loop;

        public AudioCueEntry(AudioCue cue, float volume, float minPitch, float maxPitch, bool loop = false)
            : this(cue.ToString(), volume, minPitch, maxPitch, loop)
        {
            legacyCue = cue;
        }

        public AudioCueEntry(string cueId, float volume, float minPitch, float maxPitch, bool loop = false)
        {
            this.cueId = cueId;
            this.volume = volume;
            this.minPitch = minPitch;
            this.maxPitch = maxPitch;
            this.loop = loop;
        }

        public AudioClip GetRandomClip()
        {
            MigrateLegacyClip();

            if (clips == null || clips.Count == 0)
            {
                return null;
            }

            var validClips = 0;
            for (var i = 0; i < clips.Count; i++)
            {
                if (clips[i] != null)
                {
                    validClips++;
                }
            }

            if (validClips == 0)
            {
                return null;
            }

            var candidates = new List<AudioClip>();
            for (var i = 0; i < clips.Count; i++)
            {
                var clip = clips[i];
                if (clip == null)
                {
                    continue;
                }

                candidates.Add(clip);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        public void ApplyDefaultsFrom(AudioCueEntry defaults)
        {
            if (defaults == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(cueId))
            {
                cueId = defaults.cueId;
            }

            volume = defaults.volume;
            minPitch = defaults.minPitch;
            maxPitch = defaults.maxPitch;
            loop = defaults.loop;
        }

        public void MigrateLegacyClip()
        {
            clips ??= new List<AudioClip>();
            if (legacyClip != null && !clips.Contains(legacyClip))
            {
                clips.Insert(0, legacyClip);
            }

            legacyClip = null;

            if (string.IsNullOrWhiteSpace(cueId) && legacyCue != AudioCue.None)
            {
                cueId = legacyCue.ToString();
            }
        }
    }
}
