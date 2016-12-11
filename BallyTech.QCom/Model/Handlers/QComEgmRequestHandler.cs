using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Configuration;
using log4net;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Configuration;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm.Devices.FundsTransfer;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class QComEgmRequestHandler : IEgmRequestHandler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (QComEgmRequestHandler));

        [AutoWire(Name = "QComModel")]
        public QComModel Model { get; set; }

        private bool _WasPsnResetAttempted = false;


        #region IEgmRequestHandler Members

        public void SetEnabledState(bool enabledState)
        {
            Model.SendPoll(MessageBuilder.BuildEgmEnableStatusChangeMessage(enabledState, Model.Egm.CurrentGame));
        }

        public void SetLockState(LockStateInfo lockStateInfo)
        {
            if (string.IsNullOrEmpty(lockStateInfo.DisplayMessage)) lockStateInfo.DisplayMessage = Model.SystemLockupText;
            Request systemLockupRequestResetPoll;

            string displayMessage = StringExtensions.AdjustLength(lockStateInfo.DisplayMessage, 80);

            if (_Log.IsInfoEnabled)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine(string.Format("System Lockup {0} {1} ", lockStateInfo.LockState ? "Created -" : "Cleared", lockStateInfo.DisplayMessage));
                str.AppendLine(string.Format("DispalyMessage {1} ", lockStateInfo.DisplayMessage, displayMessage));
                _Log.Info(str);
            }

            if (lockStateInfo.LockState)
            {
                systemLockupRequestResetPoll = new SystemLockupRequestPoll()
                                                   {
                                                       LockUpFlag = (Model.ShouldEnableFanfareForExternalJackpot && lockStateInfo.IsFanFareRequired) ?
                                                                    LockUpFlagCharacteristics.ResetKeyDisable | LockUpFlagCharacteristics.Fanfare :
                                                                    LockUpFlagCharacteristics.ResetKeyDisable,
                                                       LockUpTextLength = (byte)displayMessage.Length,
                                                       LockUpText = displayMessage
                                                   };

                Model.SystemLockUpHandler.SystemLockUpPoll = systemLockupRequestResetPoll;
            }
            else
            {
                systemLockupRequestResetPoll = EgmGeneralResetPollBuilder.BuildLockupResetPoll(Model.ProtocolVersion,
                                                                                               EgmMainLineCurrentStatus.
                                                                                                   SystemLockup);
            }

            Model.SendPoll(systemLockupRequestResetPoll);

            if (Model.ProtocolVersion == ProtocolVersion.V15 && lockStateInfo.LockState)
                Model.SystemLockUpHandler.HandleSystemLockup();
            
          
        }

      

        public void RequestMeters(MeterRequestInfo meterRequestInfo)
        {
            var cabinet = Model.Egm.CabinetDevice;

            if (!meterRequestInfo.IsGameSpecificMeter)
            {
                var meterRequestPoll = MeterRequestBuilder.RequestFor(meterRequestInfo.Meters,
                                                                        Model.Egm.CurrentGame);

                if (cabinet.IsEnabled)
                    meterRequestPoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;

                Model.SendPoll(meterRequestPoll);
                return;
            }

            Model.RequestAllGameMeters(meterRequestInfo);
        }

        public void ResetJackpot(JackpotType jackpotType)
        {
            var currentLineStatus = GetState(jackpotType);
            if (currentLineStatus == EgmMainLineCurrentStatus.None)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("No Jackpot Condition Present");
                return;
            }
            Model.SendPoll(EgmGeneralResetPollBuilder.BuildLockupResetPoll(Model.ProtocolVersion, currentLineStatus));
        }

        public void ResetPSN(ResetStatus status)
        {
            if (status == ResetStatus.Success)
            {
                _WasPsnResetAttempted = false;
                return;
            }
            
            if (_WasPsnResetAttempted)
            {
                Model.Egm.ResetExtendedEventData();
                Model.Egm.ReportErrorEvent(EgmErrorCodes.FundTransferPollSequenceNumberFailure);
                _Log.Info("Disabling EGM due to Poll Sequence Number Failure");

                _WasPsnResetAttempted = false;

                return;
            }  

             Model.QueuePSNResetPoll();

             _WasPsnResetAttempted = true;
        }

        #endregion

        private static EgmMainLineCurrentStatus GetState(JackpotType jackpotType)
        {
            return jackpotType.ToEgmMainLineCurrentStatus();
        }

        #region IEgmRequestHandler Members


        public void Configure(IDenominationConfiguration configuration)
        {
            Model.ConfigureNoteAcceptor(configuration);
        }

        public void SetExternalJackpotInformation(ICollection<IExternalJackpotDisplayProfile> Profiles)
        {
            Model.SendPoll(ExternalJackpotInformationPollBuilder.Build(Profiles));

            if (Model.MysteryBroadcastScheduler == null)            
                Model.MysteryBroadcastScheduler = new MysteryBroadcastScheduler(Model);            
        }

        #endregion

        #region IEgmRequestHandler Members


        public void RequestAllMeters()
        {
           var meterPoll= MeterRequestBuilder.RequestAllMeters(Model.Egm.CabinetDevice.IsEnabled, Model.Egm.CurrentGame);

           meterPoll.Sender = Model.Egm.MeterRequestHandler;

           Model.SendPoll(meterPoll);
        }

        public void RequestGameLevelMetersForAllGames()
        {
            Model.FetchGameLevelMetersForAllGames();
        }

        public void ClearEgmFaults()
        {
            Model.SendPoll(EgmGeneralResetPollBuilder.BuildFaultsResetPoll(Model.ProtocolVersion));
        }

        public void SetNoteAcceptorState(bool state)
        {
            var parameterConfiguration = Model.ConfigurationRepository.GetConfigurationOfType<QComParameterConfiguration>();

            if (parameterConfiguration == null) return;

            var egmParametersPoll = QComConfigurationBuilder.Build(parameterConfiguration);

            egmParametersPoll.CreditInLockOut = state == true ? egmParametersPoll.CreditInLockOut : 0;

            Model.SendPoll(egmParametersPoll);
        
        }

        #endregion
    }
}

