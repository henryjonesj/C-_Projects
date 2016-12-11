using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class GeneralResponseListener : MessageProcessor
    {
     
        public override void Process(GeneralStatusResponse response)
        {
            CheckGamePlayStatus(response);

            CheckECTFromEgmStatus(response);

            CheckForLPLockup(response);

            CheckForSystemLockup(response);

            CheckIfJackpotLockupsAreCleared(response);
            Model.Egm.GameFaultStatusChanged(!(response.IsInNonFaultyMode));

            UpdateIdleModeStatus(response);

            Model.Egm.EgmCurrentStatus = response.State;

            Model.Egm.EctFromEgmMonitor.MonitorEctFromEgm(response.State);
        }

        private void UpdateIdleModeStatus(GeneralStatusResponse response)
        {
            var cabinet = Devices.GetDevice<Cabinet>();
            cabinet.ProcessIdleModeStatus(response.IsIdleMode);
        }

        private void CheckForLPLockup(GeneralStatusResponse response)
        {
            if (!response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup))
                return;             
            
            if (Devices.GetDevice<LinkedProgressiveDevice>().LinkedProgressiveStatus != LinkedProgressiveStatus.AutoResetPending)
                return;
            
            Devices.GetDevice<LinkedProgressiveDevice>().HandleLinkedProgressiveLockup();
        }

        private void CheckForSystemLockup(GeneralStatusResponse response)
        {
            if (Model.SystemLockUpHandler == null) return;
            
            Model.SystemLockUpHandler.IsSystemInLockUp = response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.SystemLockup);
        
        }

        private void CheckGamePlayStatus(GeneralStatusResponse response)
        {
            var playInProgress = response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.PlayInProgress) ||
                response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.PlayInProgressDoubleUp) || 
                response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.PlayInProgressFeature);

            EdgeDetector<bool> game = new EdgeDetector<bool>(Model.IsGameInPlay, playInProgress);

            if (game.Rising())
            {
                Model.IsGameInPlay = true;
                Devices.GetDevice<Cabinet>().GamePlayStatusChanged(true);
                return;
            }
            if (game.Falling())
            {
                Model.IsGameInPlay = false;
                Devices.GetDevice<Cabinet>().GamePlayStatusChanged(false);
            }                
        }

        private void CheckECTFromEgmStatus(GeneralStatusResponse response)
        {
            var isEctfromEgmInProgress = response.IsMainLineCodeStateSet(EgmMainLineCurrentStatus.EctFromEGMLock);

            var ectfromEgmClearedDetector = new EdgeDetector<bool>(Model.EcTFromEgmInProgress, isEctfromEgmInProgress); 

            Model.EcTFromEgmInProgress = isEctfromEgmInProgress;

            if (Model.EcTFromEgmInProgress)
                Model.Egm.RequestFullDeposit(-1);

            if (!ectfromEgmClearedDetector.Falling()) return;

            Model.Egm.OnLockupCleared();
            Model.Egm.SetCashlessModeIfNecessary();
        }


        private void CheckIfJackpotLockupsAreCleared(GeneralStatusResponse response)
        {
            if (!(response.IsLinkedProgressiveAwardLockup))
            {
                var linkedProgressiveDevice = Model.Egm.LinkedProgressiveDevice;
                if(linkedProgressiveDevice != null) linkedProgressiveDevice.HandleHandpayLockupCleared();
            }

            if(response.EgmInJackpotLockup) return;

            var jackpotDevice = Devices.GetDevice<JackpotDevice>();

            if (!(response.IsCancelCreditLockup))
                jackpotDevice.HandleHandpayLockupClearedFor(HandpayType.CancelledCredits);

            if (!(response.IsResidualCancelCreditLockup))
                jackpotDevice.HandleHandpayLockupClearedFor(HandpayType.ResidualCancelledCredits);

            if (!(response.IsLargeWinLockup))
                jackpotDevice.HandleHandpayLockupClearedFor(HandpayType.LargeWin);

        }
    }
}
