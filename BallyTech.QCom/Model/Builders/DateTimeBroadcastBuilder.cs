using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Time;

namespace BallyTech.QCom.Model.Builders
{
    public static class DateTimeBroadcastBuilder
    {
        private static GlobalFlagStatus _StatusFlag = GlobalFlagStatus.Default;
        public static GlobalFlagStatus StatusFlag
        {
            get { return _StatusFlag; }
            set { _StatusFlag = value; }
        }
        
        public static CurrentDateTimeBroadcast Build()
        {
            return new CurrentDateTimeBroadcast()
                       {
                           SystemDateTime = TimeProvider.UtcNow.ToLocalTime(),
                           GlobalFlag = _StatusFlag | GlobalFlagStatus.ClockDisplayFlag
                       };
        }
        
    }
}
