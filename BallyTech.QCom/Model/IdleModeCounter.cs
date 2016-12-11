using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class IdleModeCounter : MessageReceivedCounter<GeneralStatusResponse>
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (IdleModeCounter));

        public override void Received(ApplicationMessage message)
        {
            var generalStatusResponse = message.As<GeneralStatusResponse>();
            if (generalStatusResponse != null && IsIdleMode(generalStatusResponse))
            {
                if (_Log.IsDebugEnabled)
                    _Log.Debug("Idle Mode Received");
                base.Received(message);
                return;
            }

            _Log.Info("Reset Counter");
            Reset();
        }

        private bool IsIdleMode(GeneralStatusResponse response)
        {
            return response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.IdleMode)
                   && response.IsInNonFaultyMode;
        }
    }
}
