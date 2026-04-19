using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class TutorialGameController : MonoBehaviour
    {
        private const string LocalizationAssetPath = "Assets/PrototypeGenerated/Config/TutorialLocalization.asset";
        private const string LocalizationResourcePath = "PrototypeGenerated/Config/TutorialLocalization";
        private enum LessonEnemyPattern
        {
            AlternatePair,
            LaneMoveShootPause,
            SkillTurret
        }

        private enum LessonEnemyActionState
        {
            Idle,
            ChooseLane,
            Moving,
            Shoot,
            Pause
        }

        private sealed class LessonEnemy
        {
            public GameObject root;
            public SpriteRenderer bodyRenderer;
            public SpriteRenderer coreRenderer;
            public float x;
            public float y;
            public int hp;
            public bool alive = true;
            public float fireCooldown;
            public float fireInterval;
            public BossProjectile.ProjectileKind nextShotKind;
            public LessonEnemyPattern pattern;
            public LessonEnemyActionState actionState;
            public float stateTimer;
            public float pauseDuration;
            public float moveStartX;
            public float targetLaneX;
            public float moveDuration;
            public float moveProgress;
            public float shootDelay;
            public int currentLane = -1;
            public int repeatedLaneCount;
            public float hitFlash;
        }

        private sealed class BarrierTarget
        {
            public GameObject root;
            public SpriteRenderer bodyRenderer;
            public SpriteRenderer coreRenderer;
            public float x;
            public float y;
            public float width;
            public float height;
            public float hp;
            public float maxHp;
            public float pulse;
        }

        [SerializeField] private GameController gameController;
        [SerializeField] private TutorialLocalizationAsset localization;
        [SerializeField] private TutorialOverlayController overlayController;
        [SerializeField] private TutorialHUDController hudController;
        [SerializeField] private TutorialBreakLessonController breakLessonController;
        [SerializeField] private Transform runtimeRoot;
        [SerializeField] private Transform enemyRoot;
        [SerializeField] private Transform propRoot;
        [SerializeField] private Transform receiverRoot;
        [SerializeField] private SpriteRenderer receiverFillRenderer;
        [SerializeField] private SpriteRenderer receiverOutlineRenderer;

        private readonly List<LessonEnemy> _enemies = new();

        private GameController _game;
        private BarrierTarget _barrier;
        private int _stepIndex = -1;
        private TutorialLessonId _currentLesson;
        private int _lesson1ReceiverCount;
        private bool _advanceQueued;
        private float _advanceTimer;
        private bool _breakEnteredZone;
        private bool _breakRageFilled;
        private bool _breakSkillUsed;
        private bool _breakWaitingForSkillResolve;
        private bool _breakPostSkillDelayStarted;
        private float _breakPostSkillDelayTimer;

        public TutorialLessonId CurrentLesson => _currentLesson;

        public void Initialize(GameController game)
        {
            _game = gameController != null ? gameController : game;
            gameController = _game;
            EnsureLocalization();
            LocalizationRuntime.LocaleChanged -= HandleLocaleChanged;
            LocalizationRuntime.LocaleChanged += HandleLocaleChanged;
            overlayController.Initialize();
            hudController.Initialize(_game);
            breakLessonController.Initialize(_game);
            if (_game.SkillButtonController != null)
            {
                _game.SkillButtonController.ForceActiveMode();
                _game.SkillButtonController.Initialize(_game);
            }

            EnsureRuntimeRoots();
            EnsureReceiverVisual();
            HideReceiver();

            _game.SuccessfulParry += HandleSuccessfulParry;
            _game.SkillActivated += HandleSkillActivated;
        }

        public void BeginTutorial()
        {
            GoToStep(0);
        }

        public void HandleLessonFailed()
        {
            RestartCurrentLesson("Trúng đạn", "Trúng 1 hit nên reset đúng bài đang học.");
        }

        public void HandleRestartShortcut()
        {
            if (_currentLesson != TutorialLessonId.None)
            {
                RestartCurrentLesson("Reset bài hiện tại", "R restart đúng bài đang học.");
                return;
            }

            BeginTutorial();
        }

        [Button]
        private void DebugNextStep()
        {
            if (_stepIndex < TutorialLessonDefinitions.Steps.Length - 1)
            {
                GoToStep(_stepIndex + 1);
            }
        }

        [Button]
        private void DebugRestartLesson()
        {
            if (_currentLesson != TutorialLessonId.None)
            {
                RestartCurrentLesson("Debug restart", "Reset đúng lesson hiện tại.");
            }
        }

        public bool TryHandleCounterProjectile(CounterProjectile projectile)
        {
            if (_currentLesson == TutorialLessonId.None)
            {
                return false;
            }

            var position = projectile.Position;
            if (_currentLesson == TutorialLessonId.Lesson1 && IsInsideReceiver(position))
            {
                _lesson1ReceiverCount += 1;
                _game.VfxPoolController.SpawnRing(new Vector2(position.x, TutorialLessonDefinitions.LessonOneReceiverY + 4f), PrototypeVisualUtility.HealMint.WithAlpha(0.95f), 0.3f, 0.18f, 0.42f);
                _game.VfxPoolController.SpawnBurst(new Vector2(position.x, TutorialLessonDefinitions.LessonOneReceiverY + 4f), PrototypeVisualUtility.HealMint, 12, 0.35f);
                hudController.SetProgress($"Tiến trình: {_lesson1ReceiverCount} / 3");
                if (_lesson1ReceiverCount >= 3)
                {
                    hudController.ShowBanner("Bài 1 hoàn thành", "Đạn phản phải chạm receiver phía trên mới được tính.", 1.6f);
                    QueueAdvance(0.15f);
                }

                return true;
            }

            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (!enemy.alive)
                {
                    continue;
                }

                var hitRadius = enemy.pattern == LessonEnemyPattern.LaneMoveShootPause ? 30f : 26f;
                if (Mathf.Abs(position.x - enemy.x) > hitRadius || Mathf.Abs(position.y - enemy.y) > hitRadius)
                {
                    continue;
                }

                DamageEnemy(enemy);
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            if (_advanceQueued)
            {
                _advanceTimer -= Time.deltaTime;
                if (_advanceTimer <= 0f)
                {
                    _advanceQueued = false;
                    GoToStep(_stepIndex + 1);
                }
            }

            if (_currentLesson == TutorialLessonId.None || _game.State != GameController.RunState.Playing)
            {
                return;
            }

            var dt = Time.deltaTime;
            UpdateEnemies(dt);
            UpdateBarrier(dt);
            UpdateBreak(dt);
            UpdateProgressText();
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            LocalizationRuntime.LocaleChanged -= HandleLocaleChanged;
            if (_game == null)
            {
                return;
            }

            _game.SuccessfulParry -= HandleSuccessfulParry;
            _game.SkillActivated -= HandleSkillActivated;
        }

        private void GoToStep(int nextStepIndex)
        {
            if (nextStepIndex < 0 || nextStepIndex >= TutorialLessonDefinitions.Steps.Length)
            {
                return;
            }

            _advanceQueued = false;
            _advanceTimer = 0f;
            _stepIndex = nextStepIndex;
            var step = TutorialLessonDefinitions.Steps[_stepIndex];

            ClearLessonEntities();

            if (step.Kind == TutorialStepKind.Card)
            {
                PrepareOverlayState();
                overlayController.ShowCard(
                    GetCardTitle(_stepIndex, step.Title),
                    GetCardSubtitle(_stepIndex, step.Subtitle),
                    GetCardTips(_stepIndex, step.Tips),
                    GetCardPrimaryLabel(_stepIndex, step.PrimaryButtonLabel),
                    () => GoToStep(_stepIndex + 1),
                    GetCardSecondaryLabel(_stepIndex, step.SecondaryButtonLabel),
                    ResolveSecondaryAction());
                return;
            }

            if (step.Kind == TutorialStepKind.End)
            {
                PrepareOverlayState();
                overlayController.ShowCard(
                    GetCardTitle(_stepIndex, step.Title),
                    GetCardSubtitle(_stepIndex, step.Subtitle),
                    GetCardTips(_stepIndex, step.Tips),
                    GetCardPrimaryLabel(_stepIndex, step.PrimaryButtonLabel),
                    () => SceneFlowController.LoadBossRush(false),
                    GetCardSecondaryLabel(_stepIndex, step.SecondaryButtonLabel),
                    BeginTutorial);
                return;
            }

            PrepareLessonState();
            SetupLesson(step.LessonId);
            overlayController.Hide();
            hudController.ShowGameplay(true);
            _game.SetState(GameController.RunState.Playing);
        }

        private void PrepareOverlayState()
        {
            _currentLesson = TutorialLessonId.None;
            _game.ClearProjectiles();
            _game.RageSystem.ResetState();
            _game.SkillController.ResetState();
            _game.PlayerInput.ResetState();
            _game.Player.ResetState();
            _game.PlayerCombat.ResetState();
            _game.SetPlayerAutoShotEnabled(false);
            _game.SetState(GameController.RunState.Start);
            hudController.ShowGameplay(false);
        }

        private System.Action ResolveSecondaryAction()
        {
            if (_stepIndex == 0)
            {
                return () => SceneFlowController.LoadBossRush(false);
            }

            return null;
        }

        private string GetCardTitle(int stepIndex, string fallback)
        {
            return T(GetCardKey(stepIndex, "title"), fallback);
        }

        private string GetCardSubtitle(int stepIndex, string fallback)
        {
            return T(GetCardKey(stepIndex, "subtitle"), fallback);
        }

        private string GetCardPrimaryLabel(int stepIndex, string fallback)
        {
            return T(GetCardKey(stepIndex, "primary"), fallback);
        }

        private string GetCardSecondaryLabel(int stepIndex, string fallback)
        {
            return T(GetCardKey(stepIndex, "secondary"), fallback);
        }

        private IReadOnlyList<string> GetCardTips(int stepIndex, IReadOnlyList<string> fallback)
        {
            var localizedTips = new List<string>();
            var cardKey = GetCardKey(stepIndex, string.Empty);
            if (!string.IsNullOrWhiteSpace(cardKey))
            {
                for (var i = 1; i <= 8; i++)
                {
                    var key = $"{cardKey}tip{i}";
                    var value = T(key, string.Empty);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        break;
                    }

                    localizedTips.Add(value);
                }
            }

            return localizedTips.Count > 0 ? localizedTips : fallback;
        }

        private string GetCardKey(int stepIndex, string suffix)
        {
            return stepIndex switch
            {
                0 => $"card.intro.{suffix}",
                1 => $"card.parry.{suffix}",
                5 => $"card.skill.{suffix}",
                7 => $"card.break.{suffix}",
                9 => $"card.end.{suffix}",
                _ => string.Empty
            };
        }

        private string T(string key, string fallback)
        {
            return localization != null && !string.IsNullOrWhiteSpace(key)
                ? localization.Get(key, fallback)
                : fallback;
        }

        private void HandleLocaleChanged()
        {
            if (_stepIndex >= 0 && _stepIndex < TutorialLessonDefinitions.Steps.Length)
            {
                GoToStep(_stepIndex);
            }
        }

        private void EnsureLocalization()
        {
#if UNITY_EDITOR
            if (localization == null)
            {
                localization = UnityEditor.AssetDatabase.LoadAssetAtPath<TutorialLocalizationAsset>(LocalizationAssetPath);
            }
#endif
            localization ??= Resources.Load<TutorialLocalizationAsset>(LocalizationResourcePath);
            if (localization != null && ProjectSettingsState.HasSavedLocale)
            {
                localization.ActiveLocale = ProjectSettingsState.Locale;
            }
        }

        private void PrepareLessonState()
        {
            _game.ClearProjectiles();
            _game.RageSystem.ResetState();
            _game.SkillController.ResetState();
            _game.PlayerInput.ResetState();
            _game.Player.ResetState();
            _game.PlayerCombat.ResetState();
            _game.SetPlayerAutoShotEnabled(false);
            _lesson1ReceiverCount = 0;
            _breakEnteredZone = false;
            _breakRageFilled = false;
            _breakSkillUsed = false;
            _breakWaitingForSkillResolve = false;
            _breakPostSkillDelayStarted = false;
            _breakPostSkillDelayTimer = 0f;
            breakLessonController.Hide();
            HideReceiver();
        }

        private void RestartCurrentLesson(string title, string subtitle)
        {
            if (_currentLesson == TutorialLessonId.None)
            {
                return;
            }

            hudController.ShowBanner(title, subtitle, 1.45f);
            ClearLessonEntities();
            PrepareLessonState();
            SetupLesson(_currentLesson);
            _game.SetState(GameController.RunState.Playing);
        }

        private void SetupLesson(TutorialLessonId lessonId)
        {
            _currentLesson = lessonId;

            switch (lessonId)
            {
                case TutorialLessonId.Lesson1:
                    hudController.SetHeader(
                        "BÀI 1 · PARRY CƠ BẢN",
                        "Có 3 viên đạn tím xếp ngang sẵn trên màn. Parry đủ 3 viên, và chỉ tính khi đạn phản chạm receiver phía trên.",
                        "Parry xong chưa tính ngay. Mỗi viên phải phản lên và chạm receiver mới được cộng tiến trình.");
                    hudController.SetProgress("Tiến trình: 0 / 3");
                    ShowReceiver();
                    SpawnStaticLessonOrb(450f * 0.28f);
                    SpawnStaticLessonOrb(450f * 0.5f);
                    SpawnStaticLessonOrb(450f * 0.72f);
                    hudController.ShowBanner("Bài 1", "Parry 3 viên tím và để đạn phản chạm receiver phía trên.", 2.1f);
                    break;
                case TutorialLessonId.Lesson2:
                    hudController.SetHeader(
                        "BÀI 2 · NHỊP XANH / TÍM",
                        "Cứ mỗi 1 giây sẽ có 1 đạn xanh và 1 đạn tím. Né đạn xanh, parry đạn tím để hạ cả 2 enemy.",
                        "Hai enemy bắn cùng nhịp và sẽ đổi vai giữa đạn xanh và đạn tím.");
                    CreateEnemy(LessonEnemyPattern.AlternatePair, 145f, 190f, 1, 0.85f, 1f, BossProjectile.ProjectileKind.Normal);
                    CreateEnemy(LessonEnemyPattern.AlternatePair, 305f, 190f, 1, 0.85f, 1f, BossProjectile.ProjectileKind.Parry);
                    hudController.ShowBanner("Bài 2", "Mỗi nhịp: một bên xanh, một bên tím, rồi đổi vai.", 1.9f);
                    break;
                case TutorialLessonId.Lesson3:
                    hudController.SetHeader(
                        "BÀI 3 · PARRY NÂNG CAO",
                        "Enemy sẽ sang 1 lane, bắn 1 phát rồi dừng lại. Hãy dùng nhịp đó để phản công bằng parry.",
                        "Sau mỗi phát bắn sẽ có khoảng dừng rõ ràng trước khi enemy đổi lane tiếp.");
                    CreateLaneEnemy();
                    hudController.ShowBanner("Bài 3", "Enemy đổi 4 lane và bắn từng nhịp ngắn để bạn đọc pattern.", 1.9f);
                    break;
                case TutorialLessonId.Skill:
                    hudController.SetHeader(
                        "SKILL",
                        "Parry để nạp đầy Rage, rồi dùng skill để phá barrier.",
                        "Parry nạp Rage. Khi nút SKILL sáng, dùng ngay để phá barrier.");
                    CreateBarrier(225f, 162f, 170f, 28f, 100f);
                    CreateEnemy(LessonEnemyPattern.SkillTurret, 225f, 212f, 999, 0.55f, 0.95f, BossProjectile.ProjectileKind.Parry);
                    hudController.ShowBanner("Skill", "Parry sẽ nạp Rage. Dùng skill khi thanh đầy.", 1.9f);
                    break;
                case TutorialLessonId.Break:
                    ApplyBreakTexts();
                    breakLessonController.Show(new Vector2(225f, 150f), new Vector2(225f, 235f), TutorialLessonDefinitions.BreakWeakZoneRadius);
                    hudController.ShowBanner("Boss Break", "Vào weak zone, nạp đầy Rage rồi dùng skill để hoàn tất tutorial.", 2f);
                    break;
            }
        }

        private void HandleSuccessfulParry(BossProjectile projectile)
        {
            if (_currentLesson != TutorialLessonId.Skill || projectile.Kind != BossProjectile.ProjectileKind.Parry)
            {
                return;
            }

            _game.RageSystem.Add(TutorialLessonDefinitions.SkillParryRageGain);
        }

        private void HandleSkillActivated()
        {
            if (_currentLesson != TutorialLessonId.Break || !_breakRageFilled || _breakSkillUsed)
            {
                return;
            }

            _breakSkillUsed = true;
            _breakWaitingForSkillResolve = true;
            _breakPostSkillDelayStarted = false;
            _breakPostSkillDelayTimer = 0f;
            hudController.ShowBanner("Tutorial hoàn tất", "Bạn đã vào weak zone, nạp Rage đầy và dùng skill đúng flow.", 1.6f);
        }

        private void UpdateEnemies(float dt)
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                enemy.hitFlash = Mathf.Max(0f, enemy.hitFlash - dt);
                if (!enemy.alive)
                {
                    continue;
                }

                enemy.fireCooldown -= dt;
                switch (enemy.pattern)
                {
                    case LessonEnemyPattern.AlternatePair:
                        if (enemy.fireCooldown <= 0f)
                        {
                            enemy.fireCooldown += enemy.fireInterval;
                            SpawnEnemyBullet(enemy.x, enemy.y + 20f, enemy.nextShotKind, enemy.pattern.ToString());
                            enemy.nextShotKind = enemy.nextShotKind == BossProjectile.ProjectileKind.Normal
                                ? BossProjectile.ProjectileKind.Parry
                                : BossProjectile.ProjectileKind.Normal;
                        }

                        break;
                    case LessonEnemyPattern.SkillTurret:
                        if (enemy.fireCooldown <= 0f)
                        {
                            enemy.fireCooldown += enemy.fireInterval;
                            SpawnEnemyBullet(enemy.x, enemy.y + 16f, BossProjectile.ProjectileKind.Parry, enemy.pattern.ToString());
                        }

                        break;
                    case LessonEnemyPattern.LaneMoveShootPause:
                        UpdateLaneEnemy(enemy, dt);
                        break;
                }
            }
        }

        private void UpdateLaneEnemy(LessonEnemy enemy, float dt)
        {
            var lanes = new[] { 90f, 180f, 270f, 360f };
            enemy.stateTimer -= dt;

            switch (enemy.actionState)
            {
                case LessonEnemyActionState.ChooseLane:
                    var nextLaneIndex = Random.Range(0, lanes.Length);
                    if (enemy.repeatedLaneCount >= 2)
                    {
                        while (nextLaneIndex == enemy.currentLane)
                        {
                            nextLaneIndex = Random.Range(0, lanes.Length);
                        }
                    }

                    enemy.moveStartX = enemy.x;
                    enemy.moveProgress = 0f;
                    enemy.actionState = LessonEnemyActionState.Moving;

                    if (nextLaneIndex == enemy.currentLane)
                    {
                        enemy.repeatedLaneCount += 1;
                    }
                    else
                    {
                        enemy.currentLane = nextLaneIndex;
                        enemy.repeatedLaneCount = 1;
                    }

                    enemy.targetLaneX = lanes[nextLaneIndex];
                    break;
                case LessonEnemyActionState.Moving:
                    enemy.moveProgress = Mathf.Min(1f, enemy.moveProgress + dt / enemy.moveDuration);
                    enemy.x = Mathf.Lerp(enemy.moveStartX, enemy.targetLaneX, EaseInOutSine(enemy.moveProgress));
                    if (enemy.moveProgress >= 1f)
                    {
                        enemy.x = enemy.targetLaneX;
                        enemy.actionState = LessonEnemyActionState.Shoot;
                        enemy.stateTimer = enemy.shootDelay;
                    }

                    break;
                case LessonEnemyActionState.Shoot:
                    if (enemy.stateTimer <= 0f)
                    {
                        var shotKind = Random.value < 0.5f
                            ? BossProjectile.ProjectileKind.Parry
                            : BossProjectile.ProjectileKind.Normal;
                        SpawnEnemyBullet(enemy.x, enemy.y + 20f, shotKind, enemy.pattern.ToString());
                        enemy.actionState = LessonEnemyActionState.Pause;
                        enemy.stateTimer = enemy.pauseDuration;
                    }

                    break;
                case LessonEnemyActionState.Pause:
                    if (enemy.stateTimer <= 0f)
                    {
                        enemy.actionState = LessonEnemyActionState.ChooseLane;
                    }

                    break;
            }
        }

        private void UpdateBarrier(float dt)
        {
            if (_barrier == null)
            {
                return;
            }

            _barrier.pulse += dt * 3.2f;
            if (_barrier.bodyRenderer != null)
            {
                var alpha = 0.72f + Mathf.Sin(_barrier.pulse) * 0.08f;
                _barrier.bodyRenderer.color = new Color(1f, 0.83f, 0.43f, alpha);
            }

            if (!_game.SkillController.IsCasting)
            {
                return;
            }

            if (Mathf.Abs(_game.Player.LogicalPosition.x - _barrier.x) > _barrier.width * 0.58f)
            {
                return;
            }

            _barrier.hp = Mathf.Max(0f, _barrier.hp - _game.Config.skill.laserDps * dt);
            if (_barrier.coreRenderer != null)
            {
                var widthPercent = Mathf.Max(0.12f, _barrier.hp / Mathf.Max(1f, _barrier.maxHp));
                _barrier.coreRenderer.transform.localScale = new Vector3(_barrier.width / _game.Config.pixelsPerUnit * widthPercent, 0.05f, 1f);
            }

            if (Random.value < 0.55f)
            {
                _game.VfxPoolController.SpawnBurst(new Vector2(_barrier.x, _barrier.y), PrototypeVisualUtility.CounterGold, 2, 0.18f);
            }
            if (_barrier.hp > 0f)
            {
                return;
            }

            _barrier = null;
            hudController.ShowBanner("Skill thành công", "Bạn đã nạp Rage và dùng skill đúng flow.", 1.6f);
            QueueAdvance(0.18f);
        }

        private void UpdateBreak(float dt)
        {
            if (_currentLesson != TutorialLessonId.Break)
            {
                return;
            }

            var playerInside = breakLessonController.ContainsPlayer(_game.Player.LogicalPosition, _game.Player.HitboxRadius);
            breakLessonController.Tick(playerInside, _breakRageFilled);

            if (playerInside)
            {
                _breakEnteredZone = true;
                _game.RageSystem.Add(TutorialLessonDefinitions.BreakWeakZoneRagePerSecond * dt);
                if (Random.value < 0.28f)
                {
                    _game.VfxPoolController.SpawnBurst(new Vector2(225f, 235f), PrototypeVisualUtility.WeakGold, 1, 0.12f);
                }
            }

            if (!_breakRageFilled && _game.RageSystem.IsFull)
            {
                _breakRageFilled = true;
                hudController.ShowBanner("Rage đầy", "Đầy Rage thôi chưa đủ. Dùng skill để pass bài cuối.", 1.5f);
            }

            if (_breakWaitingForSkillResolve)
            {
                if (!_breakPostSkillDelayStarted)
                {
                    if (_game.SkillController.IsCasting)
                    {
                        _breakPostSkillDelayTimer = 0f;
                    }
                    else
                    {
                        _breakPostSkillDelayStarted = true;
                        _breakPostSkillDelayTimer = 0f;
                    }
                }
                else
                {
                    _breakPostSkillDelayTimer += dt;
                    if (_breakPostSkillDelayTimer >= 1f)
                    {
                        _breakWaitingForSkillResolve = false;
                        QueueAdvance(0.18f);
                    }
                }
            }

            ApplyBreakTexts();
        }

        private void UpdateProgressText()
        {
            switch (_currentLesson)
            {
                case TutorialLessonId.Lesson1:
                    hudController.SetProgress($"Tiến trình: {_lesson1ReceiverCount} / 3");
                    break;
                case TutorialLessonId.Lesson2:
                    hudController.SetProgress($"Enemy còn lại: {CountAliveEnemies()}");
                    break;
                case TutorialLessonId.Lesson3:
                    var hp = 0;
                    for (var i = 0; i < _enemies.Count; i++)
                    {
                        if (_enemies[i].alive)
                        {
                            hp = _enemies[i].hp;
                            break;
                        }
                    }

                    hudController.SetProgress($"Enemy giữa HP: {hp} / 3");
                    break;
                case TutorialLessonId.Skill:
                    hudController.SetProgress("Tiến trình: nạp Rage đầy rồi dùng skill để phá barrier");
                    break;
                case TutorialLessonId.Break:
                    if (!_breakEnteredZone)
                    {
                        hudController.SetProgress("Tiến trình: 1/3 vào weak zone");
                    }
                    else if (!_breakRageFilled)
                    {
                        hudController.SetProgress("Tiến trình: 2/3 giữ trong weak zone để nạp đầy Rage");
                    }
                    else if (!_breakSkillUsed)
                    {
                        hudController.SetProgress("Tiến trình: 3/3 dùng skill để hoàn tất tutorial");
                    }

                    break;
            }
        }

        private void UpdateVisuals()
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy.root == null)
                {
                    continue;
                }

                enemy.root.transform.position = _game.References.LogicalToWorld(new Vector2(enemy.x, enemy.y));
                var baseColor = enemy.pattern == LessonEnemyPattern.LaneMoveShootPause
                    ? PrototypeVisualUtility.BossRose
                    : new Color(1f, 0.49f, 0.62f, 1f);
                if (enemy.bodyRenderer != null)
                {
                    enemy.bodyRenderer.color = enemy.hitFlash > 0f ? Color.white : baseColor;
                }

                if (enemy.coreRenderer != null)
                {
                    enemy.coreRenderer.color = enemy.hitFlash > 0f ? PrototypeVisualUtility.CounterGold : Color.white;
                }
            }

            if (_barrier?.root != null)
            {
                _barrier.root.transform.position = _game.References.LogicalToWorld(new Vector2(_barrier.x, _barrier.y));
            }
        }

        private void ApplyBreakTexts()
        {
            if (!_breakEnteredZone)
            {
                hudController.SetHeader(
                    "BOSS BREAK",
                    "Di chuyển vào weak zone trước.",
                    "Boss đã break sẵn. Bước 1 là chạm đúng vùng vàng.");
                return;
            }

            if (!_breakRageFilled)
            {
                hudController.SetHeader(
                    "BOSS BREAK",
                    "Đứng trong weak zone để nạp đầy Rage.",
                    "Giữ ship trong vùng vàng đến khi nút SKILL sáng.");
                return;
            }

            if (!_breakSkillUsed)
            {
                hudController.SetHeader(
                    "BOSS BREAK",
                    "Rage đã đầy. Dùng skill để kết thúc tutorial.",
                    "Đầy Rage thôi chưa đủ. Bấm SKILL hoặc Space / E để pass bài cuối.");
            }
        }

        private void QueueAdvance(float delay)
        {
            if (_advanceQueued)
            {
                return;
            }

            _game.ClearProjectiles();
            _advanceQueued = true;
            _advanceTimer = delay;
        }

        private void DamageEnemy(LessonEnemy enemy)
        {
            enemy.hp = Mathf.Max(0, enemy.hp - 1);
            enemy.hitFlash = 0.18f;
            _game.VfxPoolController.SpawnBurst(new Vector2(enemy.x, enemy.y), PrototypeVisualUtility.LaserCore, 8, 0.25f);
            if (enemy.hp > 0)
            {
                return;
            }

            enemy.alive = false;
            if (enemy.root != null)
            {
                enemy.root.SetActive(false);
            }

            _game.VfxPoolController.SpawnRing(new Vector2(enemy.x, enemy.y), PrototypeVisualUtility.CounterGold.WithAlpha(0.9f), 0.28f, 0.2f, 0.42f);
            _game.VfxPoolController.SpawnBurst(new Vector2(enemy.x, enemy.y), new Color(1f, 0.73f, 0.48f, 1f), 16, 0.42f);

            if (_currentLesson == TutorialLessonId.Lesson2 && CountAliveEnemies() == 0)
            {
                hudController.ShowBanner("Bài 2 hoàn thành", "Bạn đã né đạn xanh và parry đúng nhịp đạn tím.", 1.5f);
                QueueAdvance(0.15f);
            }
            else if (_currentLesson == TutorialLessonId.Lesson3)
            {
                hudController.ShowBanner("Bài 3 hoàn thành", "Bạn đã parry ổn trong pattern lane đơn giản hơn.", 1.5f);
                QueueAdvance(0.15f);
            }
        }

        private int CountAliveEnemies()
        {
            var count = 0;
            for (var i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].alive)
                {
                    count += 1;
                }
            }

            return count;
        }

        private bool IsInsideReceiver(Vector2 logicalPosition)
        {
            var halfWidth = (_game.Config.logicalWidth - TutorialLessonDefinitions.LessonOneReceiverWidthPadding) * 0.5f;
            var halfHeight = TutorialLessonDefinitions.LessonOneReceiverHeight * 0.5f;
            return logicalPosition.y <= TutorialLessonDefinitions.LessonOneReceiverY + halfHeight &&
                   logicalPosition.x >= _game.Config.logicalWidth * 0.5f - halfWidth &&
                   logicalPosition.x <= _game.Config.logicalWidth * 0.5f + halfWidth;
        }

        private void SpawnStaticLessonOrb(float x)
        {
            _game.SpawnBossProjectile(
                new Vector2(x, 430f),
                Vector2.zero,
                14f,
                BossProjectile.ProjectileKind.Parry,
                "TutorialLesson1Orb",
                0f,
                999f);
        }

        private void SpawnEnemyBullet(float x, float y, BossProjectile.ProjectileKind kind, string sourceMove)
        {
            _game.SpawnBossProjectile(
                new Vector2(x, y),
                new Vector2(0f, TutorialLessonDefinitions.EnemyBulletSpeed),
                kind == BossProjectile.ProjectileKind.Parry ? 11f : 8f,
                kind,
                sourceMove);
        }

        private void CreateEnemy(
            LessonEnemyPattern pattern,
            float x,
            float y,
            int hp,
            float fireCooldown,
            float fireInterval,
            BossProjectile.ProjectileKind nextShotKind)
        {
            var root = new GameObject(pattern.ToString());
            root.transform.SetParent(enemyRoot, false);
            var body = PrototypeVisualUtility.EnsureSpriteChild(root.transform, "Body", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.BossRose, 18);
            body.transform.localScale = pattern == LessonEnemyPattern.LaneMoveShootPause
                ? new Vector3(0.52f, 0.52f, 1f)
                : new Vector3(0.4f, 0.4f, 1f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var core = PrototypeVisualUtility.EnsureSpriteChild(root.transform, "Core", PrototypeVisualUtility.CircleSprite, Color.white, 19);
            core.transform.localScale = pattern == LessonEnemyPattern.LaneMoveShootPause
                ? Vector3.one * 0.12f
                : Vector3.one * 0.1f;
            root.transform.position = _game.References.LogicalToWorld(new Vector2(x, y));

            _enemies.Add(new LessonEnemy
            {
                root = root,
                bodyRenderer = body,
                coreRenderer = core,
                x = x,
                y = y,
                hp = hp,
                fireCooldown = fireCooldown,
                fireInterval = fireInterval,
                nextShotKind = nextShotKind,
                pattern = pattern
            });
        }

        private void CreateLaneEnemy()
        {
            CreateEnemy(LessonEnemyPattern.LaneMoveShootPause, 225f, 166f, 3, 0f, 1.35f, BossProjectile.ProjectileKind.Parry);
            var enemy = _enemies[_enemies.Count - 1];
            enemy.actionState = LessonEnemyActionState.ChooseLane;
            enemy.stateTimer = 0.2f;
            enemy.pauseDuration = 0.9f;
            enemy.moveStartX = 225f;
            enemy.moveDuration = 0.58f;
            enemy.shootDelay = 0.08f;
            enemy.repeatedLaneCount = 0;
        }

        private void CreateBarrier(float x, float y, float width, float height, float hp)
        {
            var root = new GameObject("BarrierTarget");
            root.transform.SetParent(propRoot, false);
            var body = PrototypeVisualUtility.EnsureSpriteChild(root.transform, "Body", PrototypeVisualUtility.SquareSprite, new Color(1f, 0.83f, 0.43f, 0.72f), 16);
            body.transform.localScale = new Vector3(width / _game.Config.pixelsPerUnit, height / _game.Config.pixelsPerUnit, 1f);
            var core = PrototypeVisualUtility.EnsureSpriteChild(root.transform, "Core", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.LaserCore.WithAlpha(0.92f), 17);
            core.transform.localScale = new Vector3(width / _game.Config.pixelsPerUnit, 0.05f, 1f);
            root.transform.position = _game.References.LogicalToWorld(new Vector2(x, y));

            _barrier = new BarrierTarget
            {
                root = root,
                bodyRenderer = body,
                coreRenderer = core,
                x = x,
                y = y,
                width = width,
                height = height,
                hp = hp,
                maxHp = hp
            };
        }

        private void EnsureRuntimeRoots()
        {
            if (runtimeRoot == null)
            {
                runtimeRoot = CreateRuntimeRoot();
            }

            if (enemyRoot == null)
            {
                enemyRoot = CreateChildRoot(runtimeRoot, "Enemies");
            }

            if (propRoot == null)
            {
                propRoot = CreateChildRoot(runtimeRoot, "Props");
            }

            if (receiverRoot == null)
            {
                receiverRoot = CreateChildRoot(runtimeRoot, "Receiver");
            }
        }

        private void EnsureReceiverVisual()
        {
            if (receiverRoot == null)
            {
                receiverRoot = CreateChildRoot(runtimeRoot, "Receiver");
            }

            if (receiverFillRenderer == null)
            {
                receiverFillRenderer = PrototypeVisualUtility.EnsureSpriteChild(receiverRoot, "Fill", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.Panel.WithAlpha(0.72f), 12);
            }

            if (receiverOutlineRenderer == null)
            {
                receiverOutlineRenderer = PrototypeVisualUtility.EnsureSpriteChild(receiverRoot, "Outline", PrototypeVisualUtility.SquareSprite, PrototypeVisualUtility.HealMint.WithAlpha(0.42f), 13);
            }

            receiverFillRenderer.transform.localScale = new Vector3((_game.Config.logicalWidth - TutorialLessonDefinitions.LessonOneReceiverWidthPadding) / _game.Config.pixelsPerUnit, TutorialLessonDefinitions.LessonOneReceiverHeight / _game.Config.pixelsPerUnit, 1f);
            receiverOutlineRenderer.transform.localScale = new Vector3((_game.Config.logicalWidth - TutorialLessonDefinitions.LessonOneReceiverWidthPadding + 6f) / _game.Config.pixelsPerUnit, (TutorialLessonDefinitions.LessonOneReceiverHeight + 6f) / _game.Config.pixelsPerUnit, 1f);
        }

        private void ShowReceiver()
        {
            receiverRoot.gameObject.SetActive(true);
            receiverRoot.position = _game.References.LogicalToWorld(new Vector2(_game.Config.logicalWidth * 0.5f, TutorialLessonDefinitions.LessonOneReceiverY));
        }

        private void HideReceiver()
        {
            if (receiverRoot != null)
            {
                receiverRoot.gameObject.SetActive(false);
            }
        }

        private void ClearLessonEntities()
        {
            _enemies.Clear();
            _barrier = null;
            breakLessonController.Hide();
            HideReceiver();
            RebuildRuntimeRoots();
        }

        private void RebuildRuntimeRoots()
        {
            if (runtimeRoot != null)
            {
                runtimeRoot.gameObject.SetActive(false);
                Object.Destroy(runtimeRoot.gameObject);
            }

            runtimeRoot = CreateRuntimeRoot();
            enemyRoot = CreateChildRoot(runtimeRoot, "Enemies");
            propRoot = CreateChildRoot(runtimeRoot, "Props");
            receiverRoot = CreateChildRoot(runtimeRoot, "Receiver");
            receiverFillRenderer = null;
            receiverOutlineRenderer = null;
            EnsureReceiverVisual();
            HideReceiver();
        }

        private Transform CreateRuntimeRoot()
        {
            var root = new GameObject("TutorialRuntime").transform;
            root.SetParent(_game.References.GameplayRoot, false);
            return root;
        }

        private static Transform CreateChildRoot(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                return existing;
            }

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
        }
    }
}
