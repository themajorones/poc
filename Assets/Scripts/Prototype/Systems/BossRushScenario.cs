using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public enum BossMobilityPreset
    {
        NoMove,
        Slow,
        Normal,
        Fast,
        Fastest
    }

    [CreateAssetMenu(fileName = "BossRushScenario", menuName = "ParryShooter/Boss Rush Scenario")]
    public sealed class BossRushScenario : ScriptableObject
    {
        [Serializable]
        public sealed class BossEncounter
        {
            public string name;
            public GameObject bossPrefab;
            public float hp = 800f;
            public float poise = 120f;
            public BossMobilityPreset mobility = BossMobilityPreset.Normal;
            [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
            public List<BossMoveDefinition> moves = new();

            public int MoveCount => moves?.Count ?? 0;
            public string DisplayName => string.IsNullOrWhiteSpace(name) ? bossPrefab != null ? bossPrefab.name : "Boss" : name;

            public BossMoveDefinition GetMoveDefinition(int index)
            {
                return moves == null || moves.Count == 0 ? null : moves[index % moves.Count];
            }

            public string GetMoveId(int index)
            {
                var move = GetMoveDefinition(index);
                return move != null ? move.MoveId : string.Empty;
            }
        }

        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        [SerializeField] private List<BossEncounter> encounters = new();

        public IReadOnlyList<BossEncounter> Encounters => encounters;
        public int EncounterCount => encounters?.Count ?? 0;

        public BossEncounter GetEncounter(int index)
        {
            return encounters == null || index < 0 || index >= encounters.Count ? null : encounters[index];
        }
    }
}
