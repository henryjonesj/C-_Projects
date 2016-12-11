using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Specifications
{
    public static class MysteryLevelValiditySpecification
    {
        public static bool IsSatisfiedBy(int LevelId)
        {
            return LevelId >= 1 && LevelId <= 8;
        }
    }
}
