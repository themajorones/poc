using System.Collections.Generic;

namespace CupHeadClone.Prototype
{
    public static class BossPatternDefinitions
    {
        private static readonly Dictionary<string, string> Labels = new()
        {
            { "aimedFan", "AIMED FAN" },
            { "staggerWave", "STAGGER WAVE" },
            { "offsetRain", "OFFSET RAIN" },
            { "laneBarrage", "LANE BARRAGE" },
            { "twinLance", "TWIN LANCE" },
            { "sweepBloom", "SWEEP BLOOM" },
            { "crossBurst", "CROSS BURST" },
            { "checkerDrop", "CHECKER DROP" },
            { "pinwheel", "PINWHEEL BURST" },
            { "sideSnakes", "SIDE SNAKES" },
            { "wedgePress", "WEDGE PRESS" },
            { "splitCurtain", "SPLIT CURTAIN" },
            { "cometCurtain", "COMET CURTAIN" },
            { "pulseGrid", "PULSE GRID" },
            { "orbitMinefall", "ORBIT MINEFALL" },
            { "prismFork", "PRISM FORK" },
            { "crownRain", "CROWN RAIN" },
            { "crushColumns", "CRUSH COLUMNS" },
            { "helixGate", "HELIX GATE" },
            { "finalConvergence", "FINAL CONVERGENCE" },
            { "shockwaveTriple", "SHOCKWAVE TRIPLE" },
            { "parryCharge", "PARRY CHARGE" },
            { "trackingParryOrb", "TRACKING PARRY ORB" }
        };

        public static string GetLabel(string moveName)
        {
            return string.IsNullOrEmpty(moveName) ? "REST" : Labels.TryGetValue(moveName, out var label) ? label : moveName;
        }

        public static string GetLabel(BossMoveKind moveKind)
        {
            return GetLabel(GetMoveId(moveKind));
        }

        public static string GetMoveId(BossMoveKind moveKind)
        {
            return moveKind switch
            {
                BossMoveKind.AimedFan => "aimedFan",
                BossMoveKind.StaggerWave => "staggerWave",
                BossMoveKind.OffsetRain => "offsetRain",
                BossMoveKind.LaneBarrage => "laneBarrage",
                BossMoveKind.TwinLance => "twinLance",
                BossMoveKind.SweepBloom => "sweepBloom",
                BossMoveKind.CrossBurst => "crossBurst",
                BossMoveKind.CheckerDrop => "checkerDrop",
                BossMoveKind.Pinwheel => "pinwheel",
                BossMoveKind.SideSnakes => "sideSnakes",
                BossMoveKind.WedgePress => "wedgePress",
                BossMoveKind.SplitCurtain => "splitCurtain",
                BossMoveKind.CometCurtain => "cometCurtain",
                BossMoveKind.PulseGrid => "pulseGrid",
                BossMoveKind.OrbitMinefall => "orbitMinefall",
                BossMoveKind.PrismFork => "prismFork",
                BossMoveKind.CrownRain => "crownRain",
                BossMoveKind.CrushColumns => "crushColumns",
                BossMoveKind.HelixGate => "helixGate",
                BossMoveKind.FinalConvergence => "finalConvergence",
                BossMoveKind.ShockwaveTriple => "shockwaveTriple",
                BossMoveKind.ParryCharge => "parryCharge",
                BossMoveKind.TrackingParryOrb => "trackingParryOrb",
                _ => string.Empty
            };
        }
    }
}
