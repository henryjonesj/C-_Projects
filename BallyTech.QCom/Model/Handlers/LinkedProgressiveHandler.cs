using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Configuration;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class LinkedProgressiveHandler : ILinkedProgressiveHandler
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(LinkedProgressiveHandler));

        private bool _RetryJackpotReset = false;
        private Scheduler _ResetTimer;

        private TimeSpan _MaximumResetRetryTime = TimeSpan.FromSeconds(5);
        public TimeSpan MaximumResetRetryTime
        {
            get { return _MaximumResetRetryTime; }
            set { _MaximumResetRetryTime = value; }
        }

        private QComModel _Model = null;
        [AutoWire(Name = "QComModel")]
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        #region ILinkedProgressiveHandler Members

        public void UpdateProgressiveLevels(Game game, SerializableList<ProgressiveLevelInfo> progressiveLevelInfo)
        {
            Model.LPBroadcastScheduler = new LPBroadcastScheduler(Model);
            game.InitializeLinkedProgressiveLevels(progressiveLevelInfo, SendLPAck);
        }

        public void OnSuccessfulAutopay()
        {
            _Log.InfoFormat("Received meters. Linked progressive jackpot Reset sent to the EGM");

            StartJackpotResetTimer();
            _RetryJackpotReset = true;

            RemoteJackpotReset();
        }

        public void HandleLinkedProgressiveLockup()
        {
            if (!_RetryJackpotReset)
            {
                _Log.Info("LP Jackpot Reset not sent to game and not retried as time elapsed");
                return;
            }

            _Log.Info("LP Jackpot Reset not sent to game. Retried!!");

            RemoteJackpotReset();
        }

        private void RemoteJackpotReset()
        {
            if (Model.Egm.EgmCurrentStatus != EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup) return;
            
            var mainLineCurrentStatus = JackpotType.LinkedProgressive.ToEgmMainLineCurrentStatus();
            Model.SendPoll(EgmGeneralResetPollBuilder.BuildLockupResetPoll(Model.ProtocolVersion, mainLineCurrentStatus));
        }

        #endregion

        private void SendLPAck(JackpotPaymentType paymentType)
        {
            _Log.Info("Sending LP Acknowledgement to EGM");
            Model.SendPoll(new LinkedProgressiveAwardAcknowledgedPoll());
            Model.Egm.LinkedProgressiveDevice.LPAcknowledged(paymentType);
        }

        private void LPResetTimeExpired()
        {
            _RetryJackpotReset = false;
            _ResetTimer = null;
        }

        private void StartJackpotResetTimer()
        {
            _ResetTimer = new Scheduler(Model.Schedule);
            _ResetTimer.TimeOutAction = LPResetTimeExpired;
            _ResetTimer.Start(MaximumResetRetryTime);
        }

        #region ILinkedProgressiveHandler Members


        public void ResetLockup()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Queuing Linkedprogressive Ack poll...");

            Model.SendPoll(new LinkedProgressiveAwardAcknowledgedPoll());

            if (_Log.IsInfoEnabled) _Log.Info("Queuing LP lockup reset poll");

            Model.SendPoll(EgmGeneralResetPollBuilder.BuildLockupResetPoll(Model.ProtocolVersion, EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup));

        }

        #endregion

        #region ILinkedProgressiveHandler Members


        public void AcknowledgeLinkedProgressiveAward(JackpotPaymentType paymentType)
        {
            SendLPAck(paymentType);
        }

        #endregion
    }
}
