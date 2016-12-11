using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model;
using BallyTech.Gtm;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class DenominationSwitchProcedure : QComConfigurationProcedure
    {
        protected enum HotSwitchStatus
        {
            None,
            ZeroCreditsIdleModePending,
            HotSwitchRequestPending
        }
        
        
        private static readonly ILog _Log = LogManager.GetLogger(typeof(DenominationSwitchProcedure));

        protected Scheduler _Scheduler = null;

        protected TimeSpan ZeroCreditsIdleModeWaitTime = TimeSpan.Zero;
        protected TimeSpan LockupTimer = TimeSpan.Zero;

        private TimeSpan DenominationSuccessDisplayTimer = TimeSpan.Zero;
        private TimeSpan DenominationConfiguringDisplayTimer = TimeSpan.Zero;

        private string SpamText = null;
        private string ConfiguringText = null;

        protected HotSwitchStatus _ConfigChangeProcedureStatus = HotSwitchStatus.None;

        private QComEgmConfiguration egmConfiguration
        {
            get { return _CurrentConfiguration as QComEgmConfiguration; }
        }

        private bool AwaitingForDenominationHotSwitch
        {
            get
            {
                return egmConfiguration.AwaitingForDenominationHotSwitch;
            }

            set
            {
                egmConfiguration.AwaitingForDenominationHotSwitch = value;
            
            }
        }

        public DenominationSwitchProcedure() { }


        public DenominationSwitchProcedure(QComModel model)
            : base(model)
        {
            ZeroCreditsIdleModeWaitTime = _Model.AdditionalConfiguration.IdleModeWaitTimer;
            LockupTimer = _Model.AdditionalConfiguration.GameVariationLockUpTimer;

            SpamText = _Model.AdditionalConfiguration.DenominationHotSwitchSuccessText;
            ConfiguringText = _Model.AdditionalConfiguration.DenominationHotSwitchConfiguringText;

            _Scheduler = new Scheduler(_Model.Schedule);

        }
        
        internal override void DoConfiguration(IQComConfiguration configuration)
        {
            _CurrentConfiguration = configuration;
            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Pending);

            if (IsDenominationNotChanged())
            {
                _Log.Debug("No change in denomination. Hence resetting denomination hot switch status");
                _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);
                ResetStatus();
                return;
            }

            if (!IsDenominationHotSwitchingSupported())
            {
                _Log.InfoFormat("Denomination Hot Switching not Supported");
                HandleConfigurationFailure("DenomSwitchNoSupport");
                return;
            }

            AttemptDenominationHotSwitch();
        }

        private bool IsDenominationNotChanged()
        {
            return egmConfiguration.ConfigurationData.CreditDenomination == _Model.Egm.CreditDenomination;
        }

        public bool IsDenominationHotSwitchingSupported()
        {   
            return _Model.ConfigurationRepository.CurrentEgmConfiguration != null &&
                _Model.ConfigurationRepository.CurrentEgmConfiguration.HasSupportForDenominationSwitching;
        }

        private void AttemptDenominationHotSwitch()
        {
            _Log.InfoFormat("Attempting Denomination Hot Switching");
            
            SetEnabledState(false);

            if (ArePreConditionsSatisfied())
            {
                UpdatePendingStatus(HotSwitchStatus.ZeroCreditsIdleModePending);
                return;
            }

            _Log.InfoFormat("Conditions not met! Cannot perform Denomination Hot Switch!");

            UpdatePendingStatus(HotSwitchStatus.None);
            SetEnabledState(true);
            HandleConfigurationFailure("DenomHotSwitchFailed");
        }
        
        private void ResetDenomninationHotSwitchStatus()
        {
            _ConfigChangeProcedureStatus = HotSwitchStatus.None;

            if (egmConfiguration == null) return;
            
            AwaitingForDenominationHotSwitch = false;
        
        }

        internal override void OnConfigurationSucceeded(ApplicationMessage applicationMessage)
        {
            if (!CanProcessConfigurationResponse(applicationMessage)) return;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Denomination Hot Switching Success!!");

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);

             if (_CurrentConfiguration.Id.ConfigurationType == FunctionCodes.EgmConfiguration) SendSpamPoll();

             ResetStatus();
        }

        private void ResetStatus()
        {
            ResetDenomninationHotSwitchStatus();

            SetEnabledState(true);

            Reset();
        }

        private void SendSpamPoll()
        {
            if (!AwaitingForDenominationHotSwitch) return; 
            
            decimal newDenom = egmConfiguration.ConfigurationData.CreditDenomination;
            _Model.SpamHandler.Send(SpamText + newDenom.ToString(), DenominationSuccessDisplayTimer);
              
        }

        internal override void OnConfigurationFailed(ApplicationMessage applicationMessage)
        {
            if (!CanProcessConfigurationResponse(applicationMessage)) return;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Denomination Hot Switching Failed!!");

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Failure);

            ResetDenomninationHotSwitchStatus();

            Reset();
        }


        private void SetEnabledState(bool state)
        {
            _Model.EgmLockedByHotSwitchProcedure.Value = !state;
        }

        protected bool ArePreConditionsSatisfied()
        {
            var cabinet = _Model.Egm.CabinetDevice;
            var meters = _Model.Egm.GetMeters();

            return cabinet.IsIdle && (meters.GetCreditAmount(null).DangerousGetUnsignedValue() == 0);
        }

        private bool CanProcessConfigurationResponse(ApplicationMessage applicationMessage)
        {
            if (_CurrentConfiguration == null) return false;

            return _CurrentConfiguration.IsExpectedSuccessfulResponse(applicationMessage);

        }

        protected void HandleConfigurationFailure(string errorReason)
        {
            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Failure);

            _Model.Egm.ReportMismatchedConfiguration(EgmEvent.InvalidEGMConfiguration, errorReason);

            Reset();
        }
       
        private void DoDenominationChange()
        {
             UpdatePendingStatus(HotSwitchStatus.HotSwitchRequestPending);
            
            _Model.SpamHandler.Send(ConfiguringText,DenominationConfiguringDisplayTimer);

            _Scheduler.TimeOutAction= OnSpamDisplayTimeOut;
            _Scheduler.Start(ZeroCreditsIdleModeWaitTime);
           
        }

        private void OnSpamDisplayTimeOut()
        {
            _Scheduler.Stop();
            
            _Model.SendPoll(_CurrentConfiguration.ConfigurationPoll);

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.InProgress);
        }

        public override void Process(GeneralStatusResponse applicationMessage)
        {
            switch (_ConfigChangeProcedureStatus)
            {
                case HotSwitchStatus.ZeroCreditsIdleModePending:
                    if (applicationMessage.IsIdleMode)
                        DoDenominationChange();
                    break;

                default: break;

            }
        
        }

        protected void UpdatePendingStatus(HotSwitchStatus status)
        {
            _ConfigChangeProcedureStatus = status;
        }

    }
}
