using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace BallyTech.QCom.Model
{
    internal class LPBroadCastCounter
    {
        private static byte countNumber = 0;
        private static byte limiter = 2;
        private static readonly ILog _Log = LogManager.GetLogger(typeof(LPBroadCastCounter));

        static LPBroadCastCounter()
        {
            countNumber = limiter;
        }
       
        public static void CountDecrement()
        {
            if (countNumber == 0)
                return;
            countNumber -= 1;
            if (_Log.IsDebugEnabled) _Log.DebugFormat("LPBroadCastCount - {0} ", countNumber);
        }

        public static void Reset()
        {
            countNumber = limiter;
        }

        public static bool IsValidCount { get { return countNumber > 0; } }

    }
}
