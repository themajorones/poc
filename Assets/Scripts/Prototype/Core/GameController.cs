using System;
using System.Collections.Generic;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class GameController : MonoBehaviour
    {
        public enum GameplayMode
        {
            BossRush,
            Tutorial
        }

        public enum RunState
        {
            Start,
            Playing,
            Win,
            Lose
        }

        [SerializeField] private GameplayMode gameplayMode = GameplayMode.BossRush;
        [SerializeField] private bool autoStartOnLoad;
        [SerializeField] private PrototypeCombatConfig config;
        [SerializeField] private GameReferences references;
        [SerializeField] private PlayerInputController playerInput;
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private BossController boss;
        [SerializeField] private BossPatternController bossPatternController;
        [SerializeField] private RageSystem rageSystem;
        [SerializeField] private SkillController skillController;
        [SerializeField] private BossRushController bossRushController;
        [SerializeField] private RecoveryCoreController recoveryCoreController;
        [SerializeField] private PrototypePoolController poolController;
        [SerializeField] private ScreenShakeController screenShakeController;
        [SerializeField] private VFXPoolController vfxPoolController;
        [SerializeField] private ParryFeedbackController parryFeedbackController;
        [SerializeField] private PlayerFieldController playerFieldController;
        [SerializeField] private PrototypePresentationController presentationController;
        [SerializeField] private HUDController hudController;
        [SerializeField] private OverlayController overlayController;
        [SerializeField] private SkillButtonController skillButtonController;
        [SerializeField] private TutorialGameController tutorialController;

        private readonly List<ProjectileBase> _activeProjectiles = new();
        private readonly List<ProjectileBase> _projectileTickBuffer = new();

        private RunState _state = RunState.Start;
        private bool _bootstrapped;
        private PlayerController _scenePlayer;
        private PlayerCombat _scenePlayerCombat;
        private GameObject _spawnedPlayerPrefab;
        private int _selectedCharacterIndex = -1;

        public event Action<RunState> StateChanged;
        public event Action<BossProjectile> SuccessfulParry;
        public event Action SkillActivated;
        public event Action PlayerCharacterChanged;

        public GameplayMode Mode => gameplayMode;
        public PrototypeCombatConfig Config => config;
        public GameReferences References => references;
        public PlayerInputController PlayerInput => playerInput;
        public PlayerController Player => player;
        public PlayerCombat PlayerCombat => playerCombat;
        public PlayerLoadoutDefinition PlayerLoadout
        {
            get
            {
                var authoring = player != null ? player.GetComponent<PlayerAuthoring>() : null;
                if (authoring != null && authoring.PlayerLoadout != null)
                {
                    return authoring.PlayerLoadout;
                }

                return config != null ? config.playerLoadout : null;
            }
        }
        public BossController Boss => boss;
        public BossPatternController BossPatternController => bossPatternController;
        public RageSystem RageSystem => rageSystem;
        public SkillController SkillController => skillController;
        public BossRushController BossRushController => bossRushController;
        public RecoveryCoreController RecoveryCoreController => recoveryCoreController;
        public PrototypePoolController PoolController => poolController;
        public ScreenShakeController ScreenShakeController => screenShakeController;
        public VFXPoolController VfxPoolController => vfxPoolController;
        public ParryFeedbackController ParryFeedbackController => parryFeedbackController;
        public PlayerFieldController PlayerFieldController => playerFieldController;
        public PrototypePresentationController PresentationController => presentationController;
        public HUDController HudController => hudController;
        public OverlayController OverlayController => overlayController;
        public SkillButtonController SkillButtonController => skillButtonController;
        public TutorialGameController TutorialController => tutorialController;
        public RunState State => _state;
        public PlayerCharacterDefinition SelectedCharacter => ResolveSelectedCharacter();
        public int CurrentPlayerMaxHp => SelectedCharacter != null ? SelectedCharacter.MaxHp : config.player.maxHp;
        public float CurrentRageMax => SelectedCharacter != null ? SelectedCharacter.RageMax : config.skill.max;

        public bool IsTutorialMode => gameplayMode == GameplayMode.Tutorial;
        public bool IsPlaying => _state == RunState.Playing;
        public bool AllowPlayerAutoShot { get; private set; } = true;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            ResolveReferences();
            LoadCharacterSelection();
            EnsurePlayerInstance();

            if (config == null || references == null || playerInput == null || player == null || playerCombat == null ||
                bossPatternController == null || rageSystem == null || skillController == null ||
                bossRushController == null || recoveryCoreController == null || poolController == null || screenShakeController == null ||
                vfxPoolController == null || parryFeedbackController == null || playerFieldController == null || presentationController == null || hudController == null ||
                overlayController == null || skillButtonController == null ||
                (IsTutorialMode && tutorialController == null))
            {
                Debug.LogError("Prototype bootstrap is missing required references in the current scene. Assign the missing objects in Inspector or let GameController auto-create manager components.");
                enabled = false;
                return;
            }

            references.ApplyCameraFraming();
            rageSystem.Initialize(this);
            poolController.Initialize(this);
            screenShakeController.Initialize(this);
            vfxPoolController.Initialize(this);
            parryFeedbackController.Initialize(this);
            playerFieldController.Initialize(this);
            presentationController.Initialize(this);
            playerInput.Initialize(this);
            player.Initialize(this);
            playerCombat.Initialize(this);
            tutorialController?.Initialize(this);
            if (boss != null)
            {
                boss.Initialize(this);
            }
            bossPatternController.Initialize(this);
            skillController.Initialize(this);
            recoveryCoreController.Initialize(this);
            bossRushController.Initialize(this);
            hudController.Initialize(this);
            overlayController.Initialize(this);
            skillButtonController.Initialize(this);
            PrepareRun(RunState.Start);
            _bootstrapped = true;
            AudioManager.Instance?.ApplySettings();
            AudioManager.Instance?.PlayMusic(IsTutorialMode ? AudioCue.MenuMusic : AudioCue.MenuMusic);

            if (IsTutorialMode)
            {
                tutorialController.BeginTutorial();
            }
            else if (autoStartOnLoad || SceneFlowController.ConsumePendingBossRushAutoStart())
            {
                StartRun();
            }
        }

        private void ResolveReferences()
        {
            references ??= GetComponent<GameReferences>();
            config ??= references != null ? references.Config : null;
            playerInput ??= FindFirstObjectByType<PlayerInputController>();
            player ??= FindFirstObjectByType<PlayerController>();
            playerCombat ??= FindFirstObjectByType<PlayerCombat>();
            _scenePlayer ??= player;
            _scenePlayerCombat ??= playerCombat;
            boss ??= FindFirstObjectByType<BossController>();
            bossPatternController ??= FindOrCreateManager<BossPatternController>("BossPatternController");
            rageSystem ??= FindOrCreateManager<RageSystem>("RageSystem");
            skillController ??= FindOrCreateManager<SkillController>("SkillController");
            bossRushController ??= FindOrCreateManager<BossRushController>("BossRushController");
            recoveryCoreController ??= FindFirstObjectByType<RecoveryCoreController>();
            screenShakeController ??= FindOrCreateManager<ScreenShakeController>("ScreenShakeController");
            poolController ??= FindOrCreateManager<PrototypePoolController>("PrototypePoolController");
            vfxPoolController ??= FindOrCreateManager<VFXPoolController>("VFXPoolController");
            parryFeedbackController ??= FindOrCreateManager<ParryFeedbackController>("ParryFeedbackController");
            playerFieldController ??= FindOrCreateManager<PlayerFieldController>("PlayerFieldController");
            presentationController ??= FindOrCreateManager<PrototypePresentationController>("PrototypePresentationController");
            hudController ??= FindFirstObjectByType<HUDController>();
            overlayController ??= FindFirstObjectByType<OverlayController>();
            skillButtonController ??= FindFirstObjectByType<SkillButtonController>();
            tutorialController ??= FindFirstObjectByType<TutorialGameController>();
        }

        private T FindOrCreateManager<T>(string objectName) where T : Component
        {
            var existing = FindFirstObjectByType<T>();
            if (existing != null)
            {
                return existing;
            }

            var managersRoot = GameObject.Find("Managers");
            if (managersRoot == null)
            {
                managersRoot = new GameObject("Managers");
            }

            var managerObject = new GameObject(objectName);
            managerObject.transform.SetParent(managersRoot.transform, false);
            return managerObject.AddComponent<T>();
        }

        private static bool ShouldResolveBeforeDamage(ProjectileBase projectile)
        {
            return projectile is BossProjectile bossProjectile &&
                   bossProjectile.Kind == BossProjectile.ProjectileKind.Parry;
        }


        private void Update()
        {
            if (!_bootstrapped)
            {
                return;
            }

            if (_state == RunState.Playing)
            {
                playerFieldController.Tick(Time.deltaTime);
                _projectileTickBuffer.Clear();
                _projectileTickBuffer.AddRange(_activeProjectiles);
                for (var pass = 0; pass < 2; pass++)
                {
                    for (var i = _projectileTickBuffer.Count - 1; i >= 0; i--)
                    {
                        var projectile = _projectileTickBuffer[i];
                        if (projectile == null || !projectile.IsGameplayActive)
                        {
                            continue;
                        }

                        var shouldTickThisPass = pass == 0
                            ? ShouldResolveBeforeDamage(projectile)
                            : !ShouldResolveBeforeDamage(projectile);
                        if (!shouldTickThisPass)
                        {
                            continue;
                        }

                        projectile.ManagedTick(Time.deltaTime);
                    }
                }

                boss?.CheckContactDamage();
                recoveryCoreController.Tick(Time.deltaTime);
            }

            hudController.Tick(Time.deltaTime);
        }

        private void Start()
        {
            if (!_bootstrapped)
            {
                return;
            }

            if (_state == RunState.Playing)
            {
                AudioManager.Instance?.PlayMusic(IsTutorialMode ? AudioCue.MenuMusic : AudioCue.BossRushMusic);
            }
            else
            {
                AudioManager.Instance?.PlayMusic(AudioCue.MenuMusic);
            }
        }

        public void PrepareRun(RunState initialState)
        {
            ClearProjectiles();
            playerInput.ResetState();
            rageSystem.ResetState();
            skillController.ResetState();
            vfxPoolController.ResetState();
            playerFieldController.ResetState();
            recoveryCoreController.ResetState();
            player.ResetState();
            playerCombat.ResetState();
            bossPatternController.ResetState();
            bossRushController.ResetState();
            if (boss != null && IsTutorialMode)
            {
                boss.Deactivate();
            }

            if (!IsTutorialMode)
            {
                bossRushController.PreloadFirstBoss();
            }

            AllowPlayerAutoShot = !IsTutorialMode;
            _state = initialState;
            overlayController.RefreshState(_state);
            skillButtonController.RefreshState();
            hudController.RefreshState();
            presentationController.RefreshState(_state);
            StateChanged?.Invoke(_state);
        }

        public void StartRun()
        {
            PrepareRun(RunState.Playing);
            AudioManager.Instance?.PlayMusic(IsTutorialMode ? AudioCue.MenuMusic : AudioCue.BossRushMusic);
        }

        public void RestartRun()
        {
            if (IsTutorialMode)
            {
                tutorialController?.BeginTutorial();
                AudioManager.Instance?.PlayMusic(AudioCue.MenuMusic);
                return;
            }

            PrepareRun(RunState.Start);
            AudioManager.Instance?.PlayMusic(AudioCue.MenuMusic);
        }

        public bool CanSelectCharacters => !IsTutorialMode && config != null && config.characterRoster != null && config.characterRoster.Count > 0;

        public void SelectNextCharacter()
        {
            if (!CanSelectCharacters)
            {
                return;
            }

            SetSelectedCharacterIndex(config.characterRoster.WrapIndex(_selectedCharacterIndex + 1));
        }

        public void SelectPreviousCharacter()
        {
            if (!CanSelectCharacters)
            {
                return;
            }

            SetSelectedCharacterIndex(config.characterRoster.WrapIndex(_selectedCharacterIndex - 1));
        }

        public void SetState(RunState nextState)
        {
            _state = nextState;
            overlayController.RefreshState(_state);
            skillButtonController.RefreshState();
            presentationController.RefreshState(_state);
            StateChanged?.Invoke(_state);
        }

        public void TriggerLose()
        {
            if (_state != RunState.Playing)
            {
                return;
            }

            if (IsTutorialMode && tutorialController != null)
            {
                tutorialController.HandleLessonFailed();
                return;
            }

            _state = RunState.Lose;
            AudioManager.Instance?.PlaySfx(AudioCue.PlayerDeath);
            overlayController.RefreshState(_state);
            skillButtonController.RefreshState();
            presentationController.RefreshState(_state);
            StateChanged?.Invoke(_state);
        }

        public void TriggerWin()
        {
            _state = RunState.Win;
            AudioManager.Instance?.PlaySfx(AudioCue.LessonComplete);
            overlayController.RefreshState(_state);
            skillButtonController.RefreshState();
            presentationController.RefreshState(_state);
            StateChanged?.Invoke(_state);
        }

        public PlayerProjectile SpawnPlayerProjectile(Vector2 logicalPosition, Vector2? velocityOverride = null, PlayerShotDefinition shotOverride = null)
        {
            var shotDefinition = shotOverride != null ? shotOverride : PlayerLoadout != null ? PlayerLoadout.PrimaryShot : null;
            if (shotDefinition == null)
            {
                return null;
            }

            var projectile = poolController.SpawnPlayerProjectile(references.LogicalToWorld(logicalPosition));
            if (projectile == null)
            {
                return null;
            }

            projectile.ActivateForGameplay(this);
            projectile.Spawn(logicalPosition, shotDefinition, velocityOverride);
            _activeProjectiles.Add(projectile);
            return projectile;
        }

        public CounterProjectile SpawnCounterProjectile(Vector2 logicalPosition)
        {
            var projectile = poolController.SpawnCounterProjectile(references.LogicalToWorld(logicalPosition));
            if (projectile == null)
            {
                return null;
            }

            projectile.ActivateForGameplay(this);
            projectile.Spawn(logicalPosition, PlayerLoadout != null ? PlayerLoadout.CounterShot : null);
            _activeProjectiles.Add(projectile);
            return projectile;
        }

        public PlayerProjectile SpawnPlayerSkillProjectile(Vector2 logicalPosition, PlayerSkillDefinition skillDefinition, Vector2? velocityOverride = null)
        {
            if (skillDefinition == null)
            {
                return null;
            }

            var projectile = poolController.SpawnPlayerProjectile(references.LogicalToWorld(logicalPosition));
            if (projectile == null)
            {
                return null;
            }

            projectile.ActivateForGameplay(this);
            projectile.SpawnSkillProjectile(logicalPosition, skillDefinition, velocityOverride);
            _activeProjectiles.Add(projectile);
            return projectile;
        }

        public MolotovFireZone SpawnMolotovFireZone(Vector2 logicalPosition, PlayerCounterShotDefinition counterDefinition)
        {
            if (counterDefinition == null)
            {
                return null;
            }

            var fireZone = poolController.SpawnMolotovFireZone(references.LogicalToWorld(logicalPosition));
            if (fireZone == null)
            {
                return null;
            }

            fireZone.ActivateForGameplay(this);
            fireZone.Spawn(logicalPosition, counterDefinition);
            _activeProjectiles.Add(fireZone);
            return fireZone;
        }

        public BossProjectile SpawnBossProjectile(
            Vector2 logicalPosition,
            Vector2 velocity,
            float radius,
            BossProjectile.ProjectileKind kind,
            string sourceMove,
            float spin = 0f,
            float lifetime = 7f,
            bool homesTowardPlayer = false,
            float homingTurnRate = 0f,
            bool persistsOnParry = false)
        {
            var projectile = poolController.SpawnBossProjectile(references.LogicalToWorld(logicalPosition));
            if (projectile == null)
            {
                return null;
            }

            projectile.ActivateForGameplay(this);
            projectile.Spawn(logicalPosition, velocity, radius, kind, sourceMove, spin, lifetime, homesTowardPlayer, homingTurnRate, persistsOnParry);
            _activeProjectiles.Add(projectile);
            return projectile;
        }

        public void DespawnProjectile(ProjectileBase projectile)
        {
            if (!_activeProjectiles.Remove(projectile))
            {
                return;
            }

            projectile.DeactivateForGameplay();
            poolController.Despawn(projectile.transform);
        }

        public void ClearProjectiles(bool preserveMarkedProjectiles = false)
        {
            var preservedProjectiles = preserveMarkedProjectiles ? new List<ProjectileBase>() : null;
            for (var i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = _activeProjectiles[i];
                if (preserveMarkedProjectiles && projectile != null && projectile.PreserveOnProjectileClear)
                {
                    preservedProjectiles?.Add(projectile);
                    continue;
                }

                projectile.DeactivateForGameplay();
                poolController.Despawn(projectile.transform);
            }

            _activeProjectiles.Clear();
            if (preservedProjectiles != null)
            {
                for (var i = preservedProjectiles.Count - 1; i >= 0; i--)
                {
                    _activeProjectiles.Add(preservedProjectiles[i]);
                }
            }
        }

        public void SetPlayerAutoShotEnabled(bool enabled)
        {
            AllowPlayerAutoShot = enabled;
        }

        public void NotifyParrySucceeded(BossProjectile projectile)
        {
            SuccessfulParry?.Invoke(projectile);
        }

        public void TriggerParrySpecial(Vector2 logicalPosition)
        {
            var parrySpecial = PlayerLoadout != null ? PlayerLoadout.CounterShot : null;
            if (parrySpecial == null)
            {
                return;
            }

            switch (parrySpecial.ShotKind)
            {
                case PlayerCounterShotKind.StraightCounter:
                case PlayerCounterShotKind.Molotov:
                    SpawnCounterProjectile(logicalPosition + parrySpecial.SpawnOffset);
                    break;
                case PlayerCounterShotKind.DefensiveRing:
                    playerFieldController?.SpawnDefensiveRing(logicalPosition, parrySpecial);
                    break;
            }
        }

        public void NotifySkillActivated()
        {
            SkillActivated?.Invoke();
        }

        public bool TryHandleCounterProjectile(CounterProjectile projectile)
        {
            return tutorialController != null && tutorialController.TryHandleCounterProjectile(projectile);
        }

        public void SetBoss(BossController nextBoss)
        {
            if (ReferenceEquals(boss, nextBoss))
            {
                return;
            }

            boss = nextBoss;
            if (boss != null)
            {
                boss.Initialize(this);
            }
        }

        private void EnsurePlayerInstance()
        {
            if (config == null || references == null)
            {
                return;
            }

            var targetPrefab = ResolveCurrentPlayerPrefab();
            if (targetPrefab == null)
            {
                if (_scenePlayer != null)
                {
                    player = _scenePlayer;
                    playerCombat = _scenePlayerCombat != null ? _scenePlayerCombat : _scenePlayer.GetComponent<PlayerCombat>();
                    _scenePlayer.gameObject.SetActive(true);
                }

                return;
            }

            if (player != null && _spawnedPlayerPrefab == targetPrefab)
            {
                return;
            }

            if (player != null && player != _scenePlayer)
            {
                Destroy(player.gameObject);
            }

            if (_scenePlayer != null)
            {
                _scenePlayer.gameObject.SetActive(false);
            }

            var parent = ResolvePlayerSpawnParent();
            var instance = Instantiate(targetPrefab, parent);
            instance.name = targetPrefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            player = instance.GetComponent<PlayerController>();
            playerCombat = instance.GetComponent<PlayerCombat>();
            _spawnedPlayerPrefab = targetPrefab;
            if (_bootstrapped)
            {
                player?.Initialize(this);
                playerCombat?.Initialize(this);
            }
        }

        private Transform ResolvePlayerSpawnParent()
        {
            var anchor = references.PlayerRoot;
            if (anchor == null)
            {
                return null;
            }

            return anchor.GetComponent<PlayerController>() != null ? anchor.parent : anchor;
        }

        private void LoadCharacterSelection()
        {
            if (config == null || config.characterRoster == null || config.characterRoster.Count <= 0)
            {
                _selectedCharacterIndex = -1;
                return;
            }

            _selectedCharacterIndex = PlayerCharacterSelectionState.LoadIndex(config.characterRoster);
        }

        private void SetSelectedCharacterIndex(int index)
        {
            if (!CanSelectCharacters)
            {
                return;
            }

            var wrapped = config.characterRoster.WrapIndex(index);
            if (wrapped < 0 || wrapped == _selectedCharacterIndex)
            {
                return;
            }

            _selectedCharacterIndex = wrapped;
            PlayerCharacterSelectionState.SaveIndex(config.characterRoster, _selectedCharacterIndex);
            EnsurePlayerInstance();
            player?.ResetState();
            playerCombat?.ResetState();
            rageSystem?.ResetState();
            hudController?.RefreshState();
            skillButtonController?.RefreshState();
            PlayerCharacterChanged?.Invoke();
        }

        private PlayerCharacterDefinition ResolveSelectedCharacter()
        {
            if (IsTutorialMode || config == null || config.characterRoster == null || config.characterRoster.Count <= 0)
            {
                return null;
            }

            var index = _selectedCharacterIndex >= 0 ? _selectedCharacterIndex : config.characterRoster.DefaultIndex;
            return config.characterRoster.GetCharacter(index);
        }

        private GameObject ResolveCurrentPlayerPrefab()
        {
            if (IsTutorialMode && config != null && config.tutorialBluePlayerPrefab != null)
            {
                return config.tutorialBluePlayerPrefab.gameObject;
            }

            var selectedCharacter = ResolveSelectedCharacter();
            if (selectedCharacter != null && selectedCharacter.PlayerPrefab != null)
            {
                return selectedCharacter.PlayerPrefab;
            }

            return config != null ? config.playerPrefab : null;
        }
    }
}
