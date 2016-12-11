using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class LPBroadcastScheduler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(LPBroadcastScheduler));
        private QComModel _Model;
        private Scheduler _LPBroadcastScheduler;

        public LPBroadcastScheduler() { }

        public LPBroadcastScheduler(QComModel model)
        {
            if(_Log.IsInfoEnabled) _Log.Info("LPBroadcastScheduler started");

            _Model = model;
            _LPBroadcastScheduler = new Scheduler(model.Schedule);
            _LPBroadcastScheduler.TimeOutAction += SendLPBroadcast;
            SendLPBroadcast();
        }

        private void SendLPBroadcast()
        {
            LinkedProgressiveJackpotCurrentAmounts LPBroadcast = null;
            if (LPBroadCastCounter.IsValidCount)
                LPBroadcast = LinkedProgressiveBroadcastBuilder.Build(_Model.Egm);
            if (LPBroadcast != null)
            {
                _Model.SendPoll(LPBroadcast);
                LPBroadCastCounter.CountDecrement();
            }

            _LPBroadcastScheduler.Start(_Model.LinkedProgressiveBroadcastTimeout);
        }
    }
}
