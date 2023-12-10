using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAA.Core.Random
{
    internal class AchivementStep
    {
        public SAM.Game.Stats.AchievementDefinition Achivement { get; set; }
        public TimeSpan RequiredTime { get; set; }
        public bool IsDone { get; set; }
    }
}
