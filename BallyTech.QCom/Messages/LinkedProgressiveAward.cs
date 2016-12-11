using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Messages
{
    partial class LinkedProgressiveAward
    {
        public int GetLevelNumber()
        {
            return ((int)this.LinkedProgressiveLevel & 0x07);
        }        

        public string GetLevel()
        {
            return string.Concat("Level", GetLevelNumber().ToString());
        }

        public HandpayType GetHandpayType()
        {
            return (GetLevelNumber() < 0 || GetLevelNumber() > 7) ? HandpayType.UnknownProgressive : _HandpayTypes[GetLevelNumber()];
        }

        private HandpayType[] _HandpayTypes = new HandpayType[]
        {
            HandpayType.ProgressiveLevel1,
            HandpayType.ProgressiveLevel2,
            HandpayType.ProgressiveLevel3,
            HandpayType.ProgressiveLevel4,
            HandpayType.ProgressiveLevel5,
            HandpayType.ProgressiveLevel6,
            HandpayType.ProgressiveLevel7,
            HandpayType.ProgressiveLevel8
        };
    }
}
