using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Builders;
using log4net;

namespace BallyTech.QCom.Model.EventsListeners
{
    [GenerateICSerializable]
    public partial class JackpotEventListener : EventListenerBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(JackpotEventListener));        
        private JackpotDevice JackpotDevice
        {
            get { return EgmAdapter.JackpotDevice; }
        }

        private LinkedProgressiveDevice LinkedProgressiveDevice
        {
            get { return EgmAdapter.LinkedProgressiveDevice; }
        }

      
        public override void Process(LargeWin response)
        {
            JackpotDevice.HandlePendingHandpay(HandpayType.LargeWin, response.WinAmount);
        }

        public override void Process(TopNpPrizeHitEvent response)
        {
            Model.Egm.JackpotDevice.RaiseEvent(EgmEvent.TopNpPrizeHit);
        }

        public override void Process(CancelCreditCancelled response)
        {
            JackpotDevice.HandleCancelCreditCancelled();
        }


        public override void Process(CancelCredit response)
        {
            JackpotDevice.HandlePendingHandpay(HandpayType.CancelledCredits, response.CancelCreditAmount);
        }

        public override void Process(LockUpCleared response)
        {
            JackpotDevice.HandleHandpayLockupCleared();
            LinkedProgressiveDevice.HandleHandpayLockupCleared();
        }

        public override void Process(StandAloneProgressiveAwardV16 @event)
        {
            EgmAdapter.SapHandler.HandleProgressiveHit(@event.ProgressiveLevelInfo.ToHandpayType(),
                                                       @event.JackpotAmount * QComCommon.MeterScaleFactor);

        }

        public override void Process(StandAloneProgressiveAwardV15 @event)
        {
            EgmAdapter.SapHandler.HandleProgressiveHit(@event.ProgressiveLevelInfo.ToHandpayType(),
                                                       @event.JackpotAmount * QComCommon.MeterScaleFactor);

        }

        private void SendJackpotReset()
        {
            JackpotDevice.HandleHandpayReset();
        }

        public override void Process(LinkedProgressiveAward response)
        {      
            JackpotDevice.RaiseEvent(EgmEvent.EGMLinkedProgressiveAwardHit);
            
            LinkedProgressiveLine progressiveLine = new LinkedProgressiveLine()
                                                {
                                                    LineAmount = response.LastJackpotAmount * QComCommon.MeterScaleFactor,
                                                    LineId = response.GetLevelNumber() + 1,
                                                    OptionalDetails = new OptionalDetails()
                                                    {
                                                        GameNumber = response.GameVersionNumber,
                                                        PaytableId = response.GameVariationNumber.ToString(),
                                                        ProgressiveGroupId = response.ProgressiveGroupId,
                                                        HitTime = response.GetHitDateTime()
                                                    }                                                    
                                                };
            if (_Log.IsDebugEnabled)
                _Log.DebugFormat("Sending Optional Hit details, Game number = {0}, Paytable Id = {1}, ProgressiveGroupId = {2}",
                    progressiveLine.OptionalDetails.GameNumber, progressiveLine.OptionalDetails.PaytableId, progressiveLine.OptionalDetails.ProgressiveGroupId);

            LinkedProgressiveDevice.HandleProgressiveJackpot(response.LastJackpotAmount, response.GetLevel(), response.GetHandpayType(), progressiveLine);
        }

        public override void Process(OldResidualCancelCreditLockUp @event)
        {
            JackpotDevice.HandlePendingHandpay(HandpayType.ResidualCancelledCredits, @event.CancelCreditAmount);      
        }

        public override void Process(ResidualCancelCreditLockUp @event)
        {
            JackpotDevice.HandlePendingHandpay(HandpayType.ResidualCancelledCredits, @event.CancelCreditAmount);      
        }

        

    }
}
