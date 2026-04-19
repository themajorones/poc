using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CupHeadClone.Prototype
{
    public sealed class BossPatternController : MonoBehaviour
    {
        private sealed class PatternScript
        {
            public string name;
            public float timer;
            public float duration;
            public float shotCooldown;
            public int volley;
            public int row;
            public int tick;
            public int gapLane;
            public int gapCenter;
            public int safeLane;
            public int shift;
            public float angle;
            public int shotIndex;
            public Vector2 chargeStart;
            public Vector2 chargeTarget;
            public Vector2 chargeWindup;
            public Vector2 chargeEnd;
            public Vector2 chargeReturn;
            public bool chargePlayerResolved;
            public bool chargeSkillParried;
            public bool spawnedTrackingParryOrb;
        }

        private GameController _game;
        private PatternScript _script;
        private BossMoveDefinition _activeMoveDefinition;
        private readonly float[] _lanes = { 108f, 171f, 225f, 279f, 342f };
        private readonly List<BossShockwaveRing> _activeShockwaves = new();
        private int _preMoveHopsRemaining;

        public void Initialize(GameController game)
        {
            _game = game;
        }

        public void ResetState()
        {
            _script = null;
            _activeMoveDefinition = null;
            _preMoveHopsRemaining = 0;
            ClearShockwaves();
        }

        public void ChooseNextAnchor()
        {
            var boss = _game.Boss;
            var candidates = new List<float>();
            foreach (var lane in _lanes)
            {
                if (Mathf.Abs(lane - boss.AnchorX) > 16f)
                {
                    candidates.Add(lane);
                }
            }

            boss.AnchorX = candidates.Count > 0
                ? candidates[Random.Range(0, candidates.Count)]
                : _lanes[Random.Range(0, _lanes.Length)];
        }

        private void Update()
        {
            if (_game == null || !_game.IsPlaying)
            {
                return;
            }

            TickShockwaves(Time.deltaTime);
            var boss = _game.Boss;
            if (boss == null)
            {
                return;
            }

            if (!boss.Active)
            {
                return;
            }

            if (boss.BreakTimer > 0f)
            {
                return;
            }

            if (boss.Phase == BossController.BossPhase.Rest)
            {
                boss.PhaseTimer -= Time.deltaTime;
                if (boss.PhaseTimer <= 0f)
                {
                    if (ShouldInsertRepositionHop(boss))
                    {
                        DoRepositionHop(boss);
                    }
                    else
                    {
                        StartMove();
                    }
                }

                return;
            }

            if (boss.Phase == BossController.BossPhase.Telegraph)
            {
                boss.PhaseTimer -= Time.deltaTime;
                if (boss.PhaseTimer <= 0f)
                {
                    if (!HasReachedAnchorLane(boss))
                    {
                        boss.PhaseTimer = 0.05f;
                        return;
                    }

                    BeginBossScript(boss.CurrentMove);
                }

                return;
            }

            if (boss.Phase == BossController.BossPhase.Attack && _script != null)
            {
                TickScript(Time.deltaTime);
            }
        }

        private void StartMove()
        {
            var boss = _game.Boss;
            if (boss == null)
            {
                return;
            }

            if (_game.BossRushController.GetMoveCount(boss.BossIndex) <= 0)
            {
                boss.Phase = BossController.BossPhase.Rest;
                boss.PhaseTimer = _game.Config.boss.restTime;
                boss.CurrentMove = null;
                return;
            }

            _activeMoveDefinition = _game.BossRushController.GetMoveDefinition(boss.BossIndex, boss.MoveIndex);
            boss.CurrentMove = _game.BossRushController.GetMoveId(boss.BossIndex, boss.MoveIndex);
            boss.AdvanceMoveIndex();
            boss.Phase = BossController.BossPhase.Telegraph;
            boss.PhaseTimer = _game.Config.boss.telegraphTime;
            if (_game.BossRushController.GetMobilityPreset(boss.BossIndex) == BossMobilityPreset.NoMove)
            {
                boss.AnchorX = _lanes[2];
            }
            else
            {
                ChooseNextAnchor();
            }
            _game.HudController.RefreshState();
        }

        private bool HasReachedAnchorLane(BossController boss)
        {
            if (_game.BossRushController.GetMobilityPreset(boss.BossIndex) == BossMobilityPreset.NoMove)
            {
                return true;
            }

            return Mathf.Abs(boss.LogicalPosition.x - boss.AnchorX) <= 8f;
        }

        private bool ShouldInsertRepositionHop(BossController boss)
        {
            var preset = _game.BossRushController.GetMobilityPreset(boss.BossIndex);
            if (preset == BossMobilityPreset.NoMove)
            {
                boss.AnchorX = _lanes[2];
                return false;
            }

            if (_preMoveHopsRemaining > 0)
            {
                return true;
            }

            _preMoveHopsRemaining = preset switch
            {
                BossMobilityPreset.Slow => 0,
                BossMobilityPreset.Normal => 0,
                BossMobilityPreset.Fast => 0,
                BossMobilityPreset.Fastest => Random.value < 0.7f ? 1 : 0,
                _ => 0
            };

            return _preMoveHopsRemaining > 0;
        }

        private void DoRepositionHop(BossController boss)
        {
            ChooseNextAnchor();
            _preMoveHopsRemaining -= 1;
            boss.Phase = BossController.BossPhase.Rest;
            boss.PhaseTimer = 0.14f;
            boss.CurrentMove = null;
        }

        private void BeginBossScript(string moveName)
        {
            var boss = _game.Boss;
            boss.Phase = BossController.BossPhase.Attack;
            _script = CreateScript(moveName, _activeMoveDefinition);

            if (moveName == "aimedFan")
            {
                EmitAimedFan();
                _script.duration = 0.14f;
            }
            else if (moveName == "parryCharge")
            {
                _script.chargeStart = boss.LogicalPosition;
                _script.chargeTarget = new Vector2(boss.LogicalPosition.x, _game.Player.LogicalPosition.y);
                _script.chargeWindup = _script.chargeStart + new Vector2(0f, -52f);
                var offscreenBottom = _game.Config.logicalHeight + boss.ContactHalfExtents.y * 2f + 72f;
                _script.chargeEnd = new Vector2(_script.chargeStart.x, offscreenBottom);
                _script.chargeReturn = _script.chargeStart;
                _script.chargePlayerResolved = false;
                _script.chargeSkillParried = false;
            }
        }

        private PatternScript CreateScript(string moveName, BossMoveDefinition moveDefinition)
        {
            var script = moveName switch
            {
                "aimedFan" => new PatternScript { name = moveName, duration = 0.14f, shotCooldown = 0.14f },
                "staggerWave" => new PatternScript { name = moveName, duration = 1.15f, shotCooldown = 0.04f },
                "offsetRain" => new PatternScript { name = moveName, duration = 1.4f, shotCooldown = 0.04f },
                "laneBarrage" => new PatternScript { name = moveName, duration = 1.7f, shotCooldown = 0.02f, gapLane = 2 },
                "twinLance" => new PatternScript { name = moveName, duration = 1.25f, shotCooldown = 0.05f },
                "sweepBloom" => new PatternScript { name = moveName, duration = 1.55f, shotCooldown = 0.02f },
                "crossBurst" => new PatternScript { name = moveName, duration = 1.2f, shotCooldown = 0.05f },
                "checkerDrop" => new PatternScript { name = moveName, duration = 1.55f, shotCooldown = 0.02f },
                "pinwheel" => new PatternScript { name = moveName, duration = 1.6f, shotCooldown = 0.02f, angle = -Mathf.PI * 0.5f },
                "sideSnakes" => new PatternScript { name = moveName, duration = 1.55f, shotCooldown = 0.02f },
                "wedgePress" => new PatternScript { name = moveName, duration = 1.4f, shotCooldown = 0.02f },
                "splitCurtain" => new PatternScript { name = moveName, duration = 1.45f, shotCooldown = 0.02f, gapCenter = 3 },
                "cometCurtain" => new PatternScript { name = moveName, duration = 1.5f, shotCooldown = 0.02f },
                "pulseGrid" => new PatternScript { name = moveName, duration = 1.45f, shotCooldown = 0.02f },
                "orbitMinefall" => new PatternScript { name = moveName, duration = 1.5f, shotCooldown = 0.02f },
                "prismFork" => new PatternScript { name = moveName, duration = 1.35f, shotCooldown = 0.02f },
                "crownRain" => new PatternScript { name = moveName, duration = 1.55f, shotCooldown = 0.02f },
                "crushColumns" => new PatternScript { name = moveName, duration = 1.5f, shotCooldown = 0.02f, safeLane = 2 },
                "helixGate" => new PatternScript { name = moveName, duration = 1.7f, shotCooldown = 0.02f, angle = -1.1f },
                "finalConvergence" => new PatternScript { name = moveName, duration = 1.55f, shotCooldown = 0.02f },
                "shockwaveTriple" => new PatternScript { name = moveName, duration = 3.4f, shotCooldown = 0f },
                "parryCharge" => new PatternScript { name = moveName, duration = 2.08f, shotCooldown = 0f },
                "trackingParryOrb" => new PatternScript { name = moveName, duration = 1.15f, shotCooldown = 0f },
                _ => new PatternScript { name = moveName, duration = 1f, shotCooldown = 0.1f }
            };

            if (moveName == "shockwaveTriple" && moveDefinition != null && !moveDefinition.OverrideTiming)
            {
                script.duration = GetShockwaveTripleDuration(moveDefinition);
            }

            if (moveDefinition != null && moveDefinition.OverrideTiming)
            {
                script.duration = moveDefinition.Duration;
                script.shotCooldown = moveDefinition.ShotCooldown;
            }

            return script;
        }

        private void TickScript(float dt)
        {
            var boss = _game.Boss;
            boss.ClearScriptedMotion();
            _script.timer += dt;
            _script.shotCooldown -= dt;

            switch (_script.name)
            {
                case "aimedFan":
                    break;
                case "staggerWave":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.28f;
                        var center = Mathf.Atan2(_game.Player.LogicalPosition.y - boss.LogicalPosition.y, _game.Player.LogicalPosition.x - boss.LogicalPosition.x) + (_script.volley - 1.5f) * 0.09f;
                        EmitFan(boss.LogicalPosition + new Vector2(0f, 18f), 5, center, 0.64f, 285f, _script.volley == 1 || _script.volley == 3 ? new[] { 2 } : null, _script.name);
                        _script.volley += 1;
                    }

                    break;
                case "offsetRain":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.16f;
                        _script.row += 1;
                        var purpleLane = (_script.row + 2) % 6;
                        for (var lane = 0; lane < 6; lane++)
                        {
                            var x = Mathf.Lerp(55f, _game.Config.logicalWidth - 55f, lane / 5f);
                            var drift = (_script.row % 2 == 0 ? -1f : 1f) * 34f;
                            var parry = lane == purpleLane && (_script.row == 2 || _script.row == 5);
                            SpawnBossBullet(new Vector2(x, boss.LogicalPosition.y + 12f), new Vector2(drift, 248f + lane * 4f), parry ? 9f : 7f, parry, _script.name);
                        }
                    }

                    break;
                case "laneBarrage":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.18f;
                        _script.row += 1;
                        _script.gapLane = Mathf.Clamp(_script.gapLane + Random.Range(-1, 2), 0, 4);
                        var purpleLaneIndex = Mathf.Clamp(_script.gapLane + (_script.row % 2 == 0 ? 1 : -1), 0, 4);
                        EmitLaneRow(74f, _game.Config.logicalWidth - 74f, 5, 300f, new[] { _script.gapLane }, (_script.row == 2 || _script.row == 6) ? new[] { purpleLaneIndex } : null, _script.name, boss.LogicalPosition.y + 18f);
                    }

                    break;
                case "twinLance":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.3f;
                        var targetAngle = Mathf.Atan2(_game.Player.LogicalPosition.y - boss.LogicalPosition.y, _game.Player.LogicalPosition.x - boss.LogicalPosition.x);
                        var emitters = new[] { GetLeftMuzzleLogical().x, GetRightMuzzleLogical().x };
                        for (var i = 0; i < emitters.Length; i++)
                        {
                            for (var j = -1; j <= 1; j++)
                            {
                                var angle = targetAngle + j * 0.08f + (i == 0 ? -0.04f : 0.04f);
                                var parry = j == 0 && (_script.volley == 1 || _script.volley == 3) && i == (_script.volley % 2);
                                SpawnBossBullet(new Vector2(emitters[i], boss.LogicalPosition.y + 16f), new Vector2(Mathf.Cos(angle) * 320f, Mathf.Sin(angle) * 320f), parry ? 9f : 7f, parry, _script.name);
                            }
                        }

                        _script.volley += 1;
                    }

                    break;
                case "sweepBloom":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.08f;
                        var t = Mathf.Clamp01(_script.timer / _script.duration);
                        var baseAngle = Mathf.PI * 0.5f + Mathf.Lerp(-0.95f, 0.95f, t);
                        EmitFan(boss.LogicalPosition + new Vector2(0f, 18f), 4, baseAngle, 0.58f, 255f, (_script.shotIndex == 4 || _script.shotIndex == 11) ? new[] { 1 } : null, _script.name);
                        _script.shotIndex += 1;
                    }

                    break;
                case "crossBurst":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.26f;
                        var allowPurple = _script.volley == 1 || _script.volley == 3;
                        var purpleLeft = allowPurple && _script.volley % 2 == 0;
                        var leftMuzzle = GetLeftMuzzleLogical();
                        var rightMuzzle = GetRightMuzzleLogical();
                        SpawnBossBullet(leftMuzzle, new Vector2(140f, 278f), purpleLeft ? 9f : 7f, purpleLeft, _script.name);
                        SpawnBossBullet(leftMuzzle, new Vector2(90f, 320f), 7f, false, _script.name);
                        SpawnBossBullet(rightMuzzle, new Vector2(-140f, 278f), allowPurple && !purpleLeft ? 9f : 7f, allowPurple && !purpleLeft, _script.name);
                        SpawnBossBullet(rightMuzzle, new Vector2(-90f, 320f), 7f, false, _script.name);
                        _script.volley += 1;
                    }

                    break;
                case "checkerDrop":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.18f;
                        _script.row += 1;
                        var activeLanes = new List<int>();
                        for (var lane = 0; lane < 7; lane++)
                        {
                            if ((lane + _script.row) % 2 == 0)
                            {
                                activeLanes.Add(lane);
                            }
                        }

                        var purpleIndex = activeLanes[(_script.row + 1) % activeLanes.Count];
                        EmitLaneRow(48f, _game.Config.logicalWidth - 48f, 7, 288f, null, (_script.row == 2 || _script.row == 5) ? new[] { purpleIndex } : null, _script.name, boss.LogicalPosition.y + 16f);
                    }

                    break;
                case "pinwheel":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.28f;
                        EmitRing(boss.LogicalPosition + new Vector2(0f, 10f), 5, _script.angle, 255f, (_script.volley == 1 || _script.volley == 4) ? _script.volley % 5 : -1, _script.name);
                        _script.angle += 0.5f;
                        _script.volley += 1;
                    }

                    break;
                case "sideSnakes":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.12f;
                        _script.tick += 1;
                        SpawnBossBullet(new Vector2(8f, boss.LogicalPosition.y + 22f), new Vector2(126f, 236f), _script.tick == 3 ? 9f : 7f, _script.tick == 3, _script.name, 1.35f);
                        SpawnBossBullet(new Vector2(_game.Config.logicalWidth - 8f, boss.LogicalPosition.y + 22f), new Vector2(-126f, 236f), _script.tick == 9 ? 9f : 7f, _script.tick == 9, _script.name, -1.35f);
                    }

                    break;
                case "wedgePress":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.2f;
                        _script.row += 1;
                        var purpleSide = _script.row == 2 ? -1 : _script.row == 5 ? 1 : 0;
                        for (var step = 0; step < 4; step++)
                        {
                            var spreadX = 22f + step * 28f;
                            SpawnBossBullet(new Vector2(boss.LogicalPosition.x - spreadX, boss.LogicalPosition.y + 18f), new Vector2(60f + step * 10f, 260f), purpleSide < 0 && step == 0 ? 9f : 7f, purpleSide < 0 && step == 0, _script.name);
                            SpawnBossBullet(new Vector2(boss.LogicalPosition.x + spreadX, boss.LogicalPosition.y + 18f), new Vector2(-60f - step * 10f, 260f), purpleSide > 0 && step == 0 ? 9f : 7f, purpleSide > 0 && step == 0, _script.name);
                        }
                    }

                    break;
                case "splitCurtain":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.14f;
                        _script.row += 1;
                        _script.gapCenter = Mathf.Clamp(_script.gapCenter + Random.Range(-1, 2), 1, 6);
                        EmitLaneRow(38f, _game.Config.logicalWidth - 38f, 8, 276f, new[] { _script.gapCenter, _script.gapCenter + 1 }, (_script.row == 2 || _script.row == 6) ? new[] { Mathf.Max(0, _script.gapCenter - 1) } : null, _script.name, boss.LogicalPosition.y + 14f);
                    }

                    break;
                case "cometCurtain":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.12f;
                        _script.tick += 1;
                        var fromLeft = _script.tick % 2 == 0;
                        var x = fromLeft ? 30f : _game.Config.logicalWidth - 30f;
                        var vx = fromLeft ? 150f : -150f;
                        var parry = _script.tick % 5 == 0;
                        SpawnBossBullet(new Vector2(x, 92f), new Vector2(vx, 330f), parry ? 9f : 7f, parry, _script.name);
                        SpawnBossBullet(new Vector2(x + (fromLeft ? 24f : -24f), 112f), new Vector2(vx * 0.85f, 300f), 7f, false, _script.name);
                    }

                    break;
                case "pulseGrid":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.22f;
                        _script.row += 1;
                        _script.shift = Mathf.Clamp(_script.shift + Random.Range(-1, 2), 0, 2);
                        var gapA = (_script.shift + _script.row) % 6;
                        var gapB = (gapA + 3) % 6;
                        EmitLaneRow(56f, _game.Config.logicalWidth - 56f, 6, 270f, new[] { gapA, gapB }, (_script.row == 2 || _script.row == 5) ? new[] { (gapA + 1) % 6 } : null, _script.name, boss.LogicalPosition.y + 12f + (_script.row % 2) * 10f, _script.row % 2 == 0 ? 18f : -18f);
                    }

                    break;
                case "orbitMinefall":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.22f;
                        _script.tick += 1;
                        _script.angle += 0.6f;
                        var parry = _script.tick == 3 || _script.tick == 6;
                        var ox = Mathf.Cos(_script.angle) * 46f;
                        var oy = Mathf.Sin(_script.angle) * 14f;
                        SpawnBossBullet(new Vector2(boss.LogicalPosition.x + ox, boss.LogicalPosition.y + oy), new Vector2(0f, 180f), parry ? 10f : 8f, parry, _script.name, _script.tick % 2 == 0 ? 1.8f : -1.8f);
                        SpawnBossBullet(new Vector2(boss.LogicalPosition.x - ox * 0.7f, boss.LogicalPosition.y - oy), new Vector2(0f, 205f), 7f, false, _script.name, _script.tick % 2 == 0 ? -1.2f : 1.2f);
                    }

                    break;
                case "prismFork":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.28f;
                        _script.volley += 1;
                        var baseAngle = Mathf.Atan2(_game.Player.LogicalPosition.y - boss.LogicalPosition.y, _game.Player.LogicalPosition.x - boss.LogicalPosition.x);
                        var angles = new[] { -0.55f, -0.22f, 0f, 0.22f, 0.55f };
                        for (var i = 0; i < angles.Length; i++)
                        {
                            var parry = (_script.volley == 1 || _script.volley == 3) && i == (_script.volley % 2 == 0 ? 2 : 1);
                            var speed = Mathf.Approximately(angles[i], 0f) ? 320f : 286f;
                            SpawnBossBullet(boss.LogicalPosition + new Vector2(0f, 18f), new Vector2(Mathf.Cos(baseAngle + angles[i]) * speed, Mathf.Sin(baseAngle + angles[i]) * speed), parry ? 9f : 7f, parry, _script.name);
                        }
                    }

                    break;
                case "crownRain":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.18f;
                        _script.volley += 1;
                        EmitFan(boss.LogicalPosition + new Vector2(0f, 8f), 7, Mathf.PI * 0.5f, 1.24f, 248f, _script.volley == 2 ? new[] { 3 } : _script.volley == 5 ? new[] { 1 } : null, _script.name);
                    }

                    break;
                case "crushColumns":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.2f;
                        _script.row += 1;
                        _script.safeLane = Mathf.Clamp(_script.safeLane + Random.Range(-1, 2), 1, 4);
                        EmitLaneRow(62f, _game.Config.logicalWidth - 62f, 6, 320f, new[] { _script.safeLane }, (_script.row == 2 || _script.row == 5) ? new[] { Mathf.Max(0, _script.safeLane - 1) } : null, _script.name, boss.LogicalPosition.y + 14f);
                    }

                    break;
                case "helixGate":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.09f;
                        _script.tick += 1;
                        _script.angle += 0.38f;
                        var leftAngle = Mathf.PI * 0.5f + Mathf.Sin(_script.angle) * 0.52f - 0.22f;
                        var rightAngle = Mathf.PI * 0.5f - Mathf.Sin(_script.angle) * 0.52f + 0.22f;
                        SpawnBossBullet(GetLeftMuzzleLogical(), new Vector2(Mathf.Cos(leftAngle) * 250f, Mathf.Sin(leftAngle) * 250f), _script.tick == 4 ? 9f : 7f, _script.tick == 4, _script.name);
                        SpawnBossBullet(GetRightMuzzleLogical(), new Vector2(Mathf.Cos(rightAngle) * 250f, Mathf.Sin(rightAngle) * 250f), _script.tick == 11 ? 9f : 7f, _script.tick == 11, _script.name);
                    }

                    break;
                case "finalConvergence":
                    if (_script.shotCooldown <= 0f)
                    {
                        _script.shotCooldown += 0.24f;
                        _script.volley += 1;
                        var targetX = _game.Player.LogicalPosition.x;
                        SpawnBossBullet(new Vector2(22f, 116f), new Vector2((targetX - 22f) * 0.9f, 300f), 7f, false, _script.name);
                        SpawnBossBullet(new Vector2(_game.Config.logicalWidth - 22f, 116f), new Vector2((targetX - (_game.Config.logicalWidth - 22f)) * 0.9f, 300f), 7f, false, _script.name);
                        var parry = _script.volley % 2 == 1;
                        SpawnBossBullet(boss.LogicalPosition + new Vector2(0f, 18f), new Vector2((targetX - boss.LogicalPosition.x) * 0.8f, 328f), parry ? 10f : 8f, parry, _script.name);
                        EmitFan(boss.LogicalPosition + new Vector2(0f, 18f), 2, Mathf.PI * 0.5f, 0.48f, 268f, null, _script.name);
                    }

                    break;
                case "shockwaveTriple":
                    var shockwaveRingCount = GetShockwaveTripleRingCount();
                    var shockwaveRingInterval = GetShockwaveTripleRingInterval();
                    if (_script.shotIndex < shockwaveRingCount && _script.timer >= _script.shotIndex * shockwaveRingInterval)
                    {
                        SpawnShockwaveRing(boss.LogicalPosition, GetShockwaveTripleRingSpeed());
                        _script.shotIndex += 1;
                    }

                    break;
                case "parryCharge":
                    TickParryCharge(boss);
                    break;
                case "trackingParryOrb":
                    TickTrackingParryOrb(boss);
                    break;
            }

            if (_script == null)
            {
                _game.HudController.RefreshState();
                return;
            }

            if (_script.timer >= _script.duration)
            {
                boss.ClearScriptedMotion();
                boss.Phase = BossController.BossPhase.Rest;
                boss.PhaseTimer = _game.Config.boss.restTime;
                boss.CurrentMove = null;
                _script = null;
                _game.HudController.RefreshState();
            }
        }

        private void EmitAimedFan()
        {
            var boss = _game.Boss;
            var angleToPlayer = Mathf.Atan2(_game.Player.LogicalPosition.y - boss.LogicalPosition.y, _game.Player.LogicalPosition.x - boss.LogicalPosition.x);
            var purpleCount = Random.value < 0.55f ? 1 : 2;
            var purpleIndexes = new List<int>();
            while (purpleIndexes.Count < purpleCount)
            {
                var idx = Random.Range(1, _game.Config.patterns.aimedFanCount - 1);
                if (!purpleIndexes.Contains(idx))
                {
                    purpleIndexes.Add(idx);
                }
            }

            EmitFan(GetCenterMuzzleLogical(new Vector2(0f, 4f)), _game.Config.patterns.aimedFanCount, angleToPlayer, _game.Config.patterns.aimedFanSpread, _game.Config.patterns.aimedFanSpeed, purpleIndexes.ToArray(), "aimedFan");
        }

        private Vector2 GetCenterMuzzleLogical(Vector2 logicalOffset = default)
        {
            var rig = _game.Boss.FirePointRig;
            if (rig != null && rig.CenterMuzzle != null)
            {
                return _game.References.WorldToLogical(rig.CenterMuzzle.position) + logicalOffset;
            }

            return _game.Boss.LogicalPosition + new Vector2(0f, 18f) + logicalOffset;
        }

        private Vector2 GetLeftMuzzleLogical()
        {
            var rig = _game.Boss.FirePointRig;
            if (rig != null && rig.LeftMuzzle != null)
            {
                return _game.References.WorldToLogical(rig.LeftMuzzle.position);
            }

            return _game.Boss.LogicalPosition + new Vector2(-50f, 16f);
        }

        private Vector2 GetRightMuzzleLogical()
        {
            var rig = _game.Boss.FirePointRig;
            if (rig != null && rig.RightMuzzle != null)
            {
                return _game.References.WorldToLogical(rig.RightMuzzle.position);
            }

            return _game.Boss.LogicalPosition + new Vector2(50f, 16f);
        }

        private void EmitFan(Vector2 origin, int count, float centerAngle, float spread, float speed, int[] purpleIndices, string sourceMove)
        {
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0.5f : i / (float)(count - 1);
                var angle = centerAngle - spread * 0.5f + spread * t;
                var parry = purpleIndices != null && System.Array.IndexOf(purpleIndices, i) >= 0;
                var shotSpeed = speed + Mathf.Abs(i - (count - 1) * 0.5f) * 4f;
                SpawnBossBullet(origin, new Vector2(Mathf.Cos(angle) * shotSpeed, Mathf.Sin(angle) * shotSpeed), parry ? 9f : 7f, parry, sourceMove);
            }
        }

        private void EmitLaneRow(float startX, float endX, int laneCount, float speed, int[] gapLanes, int[] purpleLanes, string sourceMove, float y, float drift = 0f)
        {
            for (var lane = 0; lane < laneCount; lane++)
            {
                if (gapLanes != null && System.Array.IndexOf(gapLanes, lane) >= 0)
                {
                    continue;
                }

                var x = laneCount <= 1 ? startX : Mathf.Lerp(startX, endX, lane / (float)(laneCount - 1));
                var parry = purpleLanes != null && System.Array.IndexOf(purpleLanes, lane) >= 0;
                SpawnBossBullet(new Vector2(x, y), new Vector2(drift, speed), parry ? 10f : 8f, parry, sourceMove);
            }
        }

        private void EmitRing(Vector2 origin, int count, float angleBase, float speed, int purpleIndex, string sourceMove)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = angleBase + Mathf.PI * 2f * i / count;
                var parry = i == purpleIndex;
                SpawnBossBullet(origin, new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed), parry ? 9f : 7f, parry, sourceMove);
            }
        }

        private void SpawnBossBullet(Vector2 position, Vector2 velocity, float radius, bool parry, string sourceMove, float spin = 0f)
        {
            _game.SpawnBossProjectile(position, velocity, radius, parry ? BossProjectile.ProjectileKind.Parry : BossProjectile.ProjectileKind.Normal, sourceMove, spin);
        }

        private void TickTrackingParryOrb(BossController boss)
        {
            var definition = _activeMoveDefinition;
            var windup = definition != null ? definition.TrackingParryOrbWindup : 0.5f;
            if (_script.timer < windup)
            {
                return;
            }

            if (_script.spawnedTrackingParryOrb)
            {
                return;
            }

            _script.spawnedTrackingParryOrb = true;
            var origin = GetCenterMuzzleLogical(new Vector2(0f, 4f));
            var towardPlayer = (_game.Player.LogicalPosition - origin);
            if (towardPlayer.sqrMagnitude <= 0.001f)
            {
                towardPlayer = Vector2.down;
            }

            towardPlayer.Normalize();
            var speed = definition != null ? definition.TrackingParryOrbSpeed : 110f;
            var radius = definition != null ? definition.TrackingParryOrbRadius : 30f;
            var lifetime = definition != null ? definition.TrackingParryOrbLifetime : 6f;
            var turnRate = definition != null ? definition.TrackingParryOrbTurnRate : 190f;
            _game.SpawnBossProjectile(
                origin,
                towardPlayer * speed,
                radius,
                BossProjectile.ProjectileKind.Parry,
                _script.name,
                0f,
                lifetime,
                true,
                turnRate,
                true);
        }

        private void TickParryCharge(BossController boss)
        {
            const float windupDuration = 0.5f;
            const float chargeDuration = 0.62f;
            const float repositionDuration = 0.8f;
            const float respawnGapDuration = 0.08f;
            var offscreenTop = -boss.ContactHalfExtents.y * 2f - 72f;
            var respawnTop = new Vector2(_script.chargeStart.x, offscreenTop);

            if (_script.timer <= windupDuration)
            {
                var windupT = Mathf.Clamp01(_script.timer / Mathf.Max(0.001f, windupDuration));
                var easedWindup = DOVirtual.EasedValue(0f, 1f, windupT, Ease.OutSine);
                var currentPosition = Vector2.Lerp(_script.chargeStart, _script.chargeWindup, easedWindup);
                boss.SetScriptedMotion(currentPosition);
                return;
            }

            if (_script.timer <= windupDuration + chargeDuration)
            {
                var chargeT = Mathf.Clamp01((_script.timer - windupDuration) / Mathf.Max(0.001f, chargeDuration));
                var easedCharge = DOVirtual.EasedValue(0f, 1f, chargeT, Ease.InSine);
                var currentPosition = Vector2.Lerp(_script.chargeWindup, _script.chargeEnd, easedCharge);
                boss.SetScriptedMotion(currentPosition);

                var player = _game.Player;
                var chargeCenter = boss.ContactCenterLogical;
                var extents = boss.ContactHalfExtents;
                var hit = boss.IntersectsPlayerSweep(boss.PreviousContactCenterLogical, chargeCenter, extents, player.HitboxRadius);
                if (!_script.chargeSkillParried &&
                    _game.PlayerFieldController != null &&
                    _game.PlayerFieldController.TryResolveParryCharge(chargeCenter, extents, chargeCenter))
                {
                    _script.chargeSkillParried = true;
                    boss.ApplyDamage(_game.Config.parry.counterDamage, _game.Config.parry.counterPoiseDamage, boss.LogicalPosition);
                    if (boss.BreakTimer > 0f || boss.Phase == BossController.BossPhase.Break)
                    {
                        boss.BeginForcedRecover(_script.chargeStart, 0.72f);
                        boss.ClearScriptedMotion();
                        _script = null;
                    }
                    return;
                }

                if (_script.chargePlayerResolved)
                {
                    return;
                }

                if (!hit)
                {
                    return;
                }

                _script.chargePlayerResolved = true;
                if (player.IsParrying)
                {
                    player.OnSuccessfulWaveParry(player.LogicalPosition);
                    boss.ApplyDamage(_game.Config.parry.counterDamage, _game.Config.parry.counterPoiseDamage, boss.LogicalPosition);
                    if (boss.BreakTimer > 0f || boss.Phase == BossController.BossPhase.Break)
                    {
                        boss.BeginForcedRecover(_script.chargeStart, 0.72f);
                        boss.ClearScriptedMotion();
                        _script = null;
                    }
                    return;
                }

                player.TakeDamage(player.LogicalPosition);
                return;
            }

            if (_script.timer <= windupDuration + chargeDuration + respawnGapDuration)
            {
                boss.SetScriptedMotion(_script.chargeEnd);
                return;
            }

            var descendT = Mathf.Clamp01((_script.timer - windupDuration - chargeDuration - respawnGapDuration) / Mathf.Max(0.001f, repositionDuration));
            var easedDescend = DOVirtual.EasedValue(0f, 1f, descendT, Ease.OutSine);
            var returnPosition = Vector2.Lerp(respawnTop, _script.chargeReturn, easedDescend);
            boss.SetScriptedMotion(returnPosition);
        }

        private float GetShockwaveTripleDuration(BossMoveDefinition moveDefinition)
        {
            var spawnTail = 1.4f;
            return Mathf.Max(0.1f, (moveDefinition.ShockwaveTripleRingCount - 1) * moveDefinition.ShockwaveTripleRingInterval + spawnTail);
        }

        private int GetShockwaveTripleRingCount()
        {
            return _activeMoveDefinition != null ? _activeMoveDefinition.ShockwaveTripleRingCount : 3;
        }

        private float GetShockwaveTripleRingInterval()
        {
            return _activeMoveDefinition != null ? _activeMoveDefinition.ShockwaveTripleRingInterval : 1f;
        }

        private float GetShockwaveTripleRingSpeed()
        {
            return _activeMoveDefinition != null ? _activeMoveDefinition.ShockwaveTripleRingSpeed : 265f;
        }

        private void SpawnShockwaveRing(Vector2 centerLogical, float expandSpeed)
        {
            var ringObject = new GameObject("ShockwaveRing");
            var parent = _game.References.VfxRoot != null ? _game.References.VfxRoot : transform;
            ringObject.transform.SetParent(parent, false);
            var ring = ringObject.AddComponent<BossShockwaveRing>();
            ring.Initialize(_game, centerLogical, 24f, expandSpeed, 26f, _game.Config.logicalHeight);
            _activeShockwaves.Add(ring);
        }

        private void TickShockwaves(float dt)
        {
            for (var i = _activeShockwaves.Count - 1; i >= 0; i--)
            {
                var ring = _activeShockwaves[i];
                if (ring == null)
                {
                    _activeShockwaves.RemoveAt(i);
                    continue;
                }

                ring.ManagedTick(dt);
                if (!ring.Finished)
                {
                    continue;
                }

                Destroy(ring.gameObject);
                _activeShockwaves.RemoveAt(i);
            }
        }

        private void ClearShockwaves()
        {
            for (var i = _activeShockwaves.Count - 1; i >= 0; i--)
            {
                if (_activeShockwaves[i] != null)
                {
                    Destroy(_activeShockwaves[i].gameObject);
                }
            }

            _activeShockwaves.Clear();
        }
    }
}
