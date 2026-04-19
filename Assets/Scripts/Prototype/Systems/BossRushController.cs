using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossRushController : MonoBehaviour
    {
        [SerializeField] private BossRushScenario scenario;

        private GameController _game;
        private bool _waitingForHeal;
        private int _nextBossIndex;
        private float _spawnTimer;
        private BossController _spawnedBossInstance;
        private GameObject _spawnedBossPrefab;
        private BossController _sceneBoss;

        public int CurrentBossIndex => _game != null && _game.Boss != null && _game.Boss.Active ? _game.Boss.BossIndex : Mathf.Clamp(_nextBossIndex - 1, 0, Mathf.Max(0, BossCount - 1));
        public int BossCount => scenario != null && scenario.EncounterCount > 0 ? scenario.EncounterCount : _game != null ? _game.Config.bosses.Count : 0;

        public void Initialize(GameController game)
        {
            _game = game;
            _sceneBoss = game.Boss;
        }

        public void ResetState()
        {
            _waitingForHeal = false;
            _nextBossIndex = 0;
            _spawnTimer = 0f;
            ReleaseSpawnedBoss();
        }

        public void PreloadFirstBoss()
        {
            SpawnBoss(0, true);
        }

        public void OnBossDefeated()
        {
            var defeatedIndex = _game.Boss.BossIndex;
            var defeatedName = _game.Boss.BossName;
            var dropPosition = _game.Boss.LogicalPosition + new Vector2(0f, 10f);
            _game.Boss.Deactivate();
            _game.ClearProjectiles();

            if (defeatedIndex >= BossCount - 1)
            {
                _game.TriggerWin();
                return;
            }

            _game.RecoveryCoreController.Spawn(dropPosition);
            _waitingForHeal = true;
            _nextBossIndex = defeatedIndex + 1;
            _spawnTimer = 0f;
            _game.OverlayController.ShowBanner($"{defeatedName} down", "Recovery core dropped");
        }

        public void OnRecoveryCoreCollected()
        {
            if (!_waitingForHeal)
            {
                return;
            }

            _waitingForHeal = false;
            _spawnTimer = 1.4f;
            _game.OverlayController.ShowBanner($"NEXT BOSS: {GetEncounterDisplayName(_nextBossIndex)}", "Recovery core secured");
        }

        private void Update()
        {
            if (_game == null || !_game.IsPlaying)
            {
                return;
            }

            if (_spawnTimer <= 0f)
            {
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                SpawnBoss(_nextBossIndex, false);
            }
        }

        private void SpawnBoss(int index, bool immediate)
        {
            EnsureBossInstance(index);
            if (_game.Boss == null)
            {
                Debug.LogError($"Unable to spawn boss at index {index}. Assign a boss prefab in BossRushScenario or PrototypeCombatConfig.");
                return;
            }

            var bossName = GetEncounterDisplayName(index);
            _game.Boss.ApplyDefinition(index, bossName, GetEncounterHp(index), GetEncounterPoise(index), immediate);
            _game.BossPatternController.ResetState();
            _game.HudController.RefreshState();
            _game.OverlayController.ShowBanner($"BOSS {index + 1}: {_game.Boss.BossName}", immediate ? "Boss rush begins" : "Incoming");
        }

        public BossMoveDefinition GetMoveDefinition(int bossIndex, int moveIndex)
        {
            var encounter = scenario != null ? scenario.GetEncounter(bossIndex) : null;
            if (encounter != null)
            {
                return encounter.GetMoveDefinition(moveIndex);
            }

            return _game.Config.bosses[bossIndex].GetMoveDefinition(moveIndex);
        }

        public string GetMoveId(int bossIndex, int moveIndex)
        {
            var encounter = scenario != null ? scenario.GetEncounter(bossIndex) : null;
            if (encounter != null)
            {
                return encounter.GetMoveId(moveIndex);
            }

            return _game.Config.bosses[bossIndex].GetMoveId(moveIndex);
        }

        public int GetMoveCount(int bossIndex)
        {
            var encounter = scenario != null ? scenario.GetEncounter(bossIndex) : null;
            if (encounter != null)
            {
                return encounter.MoveCount;
            }

            return _game.Config.bosses[bossIndex].MoveCount;
        }

        public BossMobilityPreset GetMobilityPreset(int bossIndex)
        {
            var encounter = scenario != null ? scenario.GetEncounter(bossIndex) : null;
            return encounter != null ? encounter.mobility : BossMobilityPreset.Normal;
        }

        private void EnsureBossInstance(int bossIndex)
        {
            var prefab = GetEncounterPrefab(bossIndex);
            if (prefab == null)
            {
                if (_sceneBoss != null)
                {
                    _sceneBoss.gameObject.SetActive(true);
                    _game.SetBoss(_sceneBoss);
                }

                return;
            }

            if (_spawnedBossInstance != null && _spawnedBossPrefab == prefab)
            {
                if (_sceneBoss != null)
                {
                    _sceneBoss.gameObject.SetActive(false);
                }

                _game.SetBoss(_spawnedBossInstance);
                _spawnedBossInstance.gameObject.SetActive(true);
                return;
            }

            ReleaseSpawnedBoss();

            var spawnParent = ResolveBossSpawnParent();
            var instance = Instantiate(prefab, spawnParent);
            instance.name = prefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            _spawnedBossInstance = instance.GetComponent<BossController>();
            _spawnedBossPrefab = prefab;
            if (_spawnedBossInstance == null)
            {
                Debug.LogError($"Boss prefab '{prefab.name}' is missing BossController.");
                Destroy(instance);
                return;
            }

            if (_game.Boss != null && _game.Boss != _spawnedBossInstance)
            {
                _game.Boss.gameObject.SetActive(false);
            }

            if (_sceneBoss != null && _sceneBoss != _spawnedBossInstance)
            {
                _sceneBoss.gameObject.SetActive(false);
            }

            _game.SetBoss(_spawnedBossInstance);
        }

        private Transform ResolveBossSpawnParent()
        {
            var anchor = _game.References.BossRoot;
            if (anchor == null)
            {
                return null;
            }

            return anchor.GetComponent<BossController>() != null ? anchor.parent : anchor;
        }

        private void ReleaseSpawnedBoss()
        {
            if (_spawnedBossInstance != null)
            {
                Destroy(_spawnedBossInstance.gameObject);
            }

            _spawnedBossInstance = null;
            _spawnedBossPrefab = null;
            if (_game != null)
            {
                _game.SetBoss(_sceneBoss);
                if (_sceneBoss != null)
                {
                    _sceneBoss.gameObject.SetActive(false);
                }
            }
        }

        private GameObject GetEncounterPrefab(int index)
        {
            var encounter = scenario != null ? scenario.GetEncounter(index) : null;
            if (encounter != null)
            {
                return encounter.bossPrefab;
            }

            return _game.Config.bosses[index].bossPrefab;
        }

        private string GetEncounterDisplayName(int index)
        {
            var encounter = scenario != null ? scenario.GetEncounter(index) : null;
            if (encounter != null)
            {
                return encounter.DisplayName;
            }

            return _game.Config.bosses[index].name;
        }

        private float GetEncounterHp(int index)
        {
            var encounter = scenario != null ? scenario.GetEncounter(index) : null;
            return encounter != null ? encounter.hp : _game.Config.bosses[index].hp;
        }

        private float GetEncounterPoise(int index)
        {
            var encounter = scenario != null ? scenario.GetEncounter(index) : null;
            return encounter != null ? encounter.poise : _game.Config.bosses[index].poise;
        }
    }
}
