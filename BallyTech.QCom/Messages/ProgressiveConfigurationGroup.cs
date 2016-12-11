using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Messages
{
    partial class ProgressiveConfigurationGroup
    {
        internal GameProgressiveType GetLevelType(ProgressiveLevelFlag levelFlag)
        {
            return (levelFlag & ProgressiveLevelFlag.LevelType) == ProgressiveLevelFlag.LevelType ? 
                GameProgressiveType.LinkedProgressive : GameProgressiveType.StandAloneProgressive;
        }

    }
}
