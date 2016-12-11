using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;

namespace BallyTech.QCom.Configuration
{
    /// <summary>

    //Use a System Lockup to warn the player with a suitable warning message (e.g. “Game
    //configuration changing, refer Player Information Display “press i button”” or other suitable
    //message). The lockup must be displayed for at least 20 seconds and the VAR change instigated
    //immediately after lockup exit.
    //Summary of the above game variation change procedure (does not include error or interruption
    //handling):

    //1. Disable EGM via MEF (If not already)
    //2. Wait for (Credit == 0) && (State == Idle) mode
    //3. Get Game Meters
    //4. [Queue System Lockup – see above paragraph]
    //5. [Wait 20 seconds then clear System Lockup]
    //6. Change VAR via EGM Game Configuration Change Poll
    //7. [Confirm change via EGM Game Configuration Response]
    //8. Re-enable EGM (if within licensed gaming hours and no other issues)

    /// </summary>

    [GenerateICSerializable]
    public partial class HotSwitchProcedure : QComConfigurationProcedure
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (HotSwitchProcedure));

        private string LockupMessage = null;

        protected Scheduler _Scheduler = null;
        protected TimeSpan ZeroCreditsIdleModeWaitTime = TimeSpan.Zero;
        protected TimeSpan LockupTimer = TimeSpan.Zero;

        protected HotSwitchStatus _ConfigChangeProcedureStatus = HotSwitchStatus.None;

        private QComGameConfiguration _CurrentGameConfiguration = null;
        

        protected enum HotSwitchStatus
        {
            None,
            ZeroCreditsIdleModePending,
            LockupRequestPending,
            LockupPending,
            LockupClearPending,
            HotSwitchRequestPending,
            HotSwitchPending
        }
        

        public HotSwitchProcedure()
        {
          
        }


        public HotSwitchProcedure(QComModel model) : base(model)
        {
            LockupMessage = _Model.AdditionalConfiguration.VariationHotSwitchText;

            ZeroCreditsIdleModeWaitTime = _Model.AdditionalConfiguration.IdleModeWaitTimer;
            LockupTimer = _Model.AdditionalConfiguration.GameVariationLockUpTimer;

            _Scheduler = new Scheduler(_Model.Schedule);
  
        }

        internal override void DoConfiguration(IQComConfiguration configuration)
        {
            _CurrentConfiguration = configuration;
            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Pending);

            _CurrentGameConfiguration = configuration as QComGameConfiguration;

            if (IsGameVariationSwitchInProgress() && !IsVariationHotSwitchingSupported())
            {
                _Log.InfoFormat("Variation Hot Switching not Supported");
                HandleConfigurationFailure("VARSwitchNoSupport");
                return;
            }

            if (IsPgidSwitchInProgress() && !_Model.AdditionalConfiguration.IsPgidHotSwitchingSupported)
            {
                _Log.InfoFormat("PGID Hot Switching not Supported");
                HandleConfigurationFailure("PGIDSwitchNoSupport");
                return;
            }

            DisableEgm();
        }

        private bool CanProcessConfigurationResponse(ApplicationMessage applicationMessage)
        {
            if (_CurrentConfiguration == null) return false;

            return _CurrentConfiguration.IsExpectedSuccessfulResponse(applicationMessage);

        }

        internal override void OnConfigurationSucceeded(ApplicationMessage applicationMessage)
        {
            if (!CanProcessConfigurationResponse(applicationMessage)) return;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Successfully Hot switched");

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);

            _CurrentGameConfiguration.HotSwitch = HotSwitchType.None;

            SetEnabledState(true);
            Reset();            
                
        }

        internal override void OnConfigurationFailed(ApplicationMessage applicationMessage)
        {
            if (!CanProcessConfigurationResponse(applicationMessage)) return;

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Hot Switching failed.");

            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Failure);

            SetEnabledState(true);

            Reset();
        }

        private bool ArePreConditionsSatisfied()
        {
            var cabinet = _Model.Egm.CabinetDevice;
            var meters = _Model.Egm.GetMeters();

            return cabinet.IsIdle && (meters.GetCreditAmount(null).DangerousGetUnsignedValue() == 0);
        }

        private void DisableEgm()
        {
            SetEnabledState(false);

            if (ArePreConditionsSatisfied())
            {
                HandleIdleModeWithZeroCredits();
                return;
            }
            WaitForZeroCreditsWithIdleMode();
        }

        private void SetEnabledState(bool state)
        {
            _Model.EgmLockedByHotSwitchProcedure.Value = !state;
        }

        protected void WaitForZeroCreditsWithIdleMode()
        {
            UpdatePendingStatus(HotSwitchStatus.ZeroCreditsIdleModePending);

            _Scheduler.TimeOutAction = OnZeroCreditsIdleModeWaitTimerElapsed;
            _Scheduler.Start(ZeroCreditsIdleModeWaitTime);            
        }

        private void OnZeroCreditsIdleModeWaitTimerElapsed()
        {
            _Scheduler.Stop();

            if (ArePreConditionsSatisfied())
                HandleIdleModeWithZeroCredits();
            else
            {
                if (_Log.IsWarnEnabled)
                    _Log.Warn("Hot Switching Failed!!");

                SetEnabledState(true);

                HandleConfigurationFailure("HotSwitchPreCondFail");
            }
        }



        private void HandleConfigurationFailure(string errorReason)
        {
            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Failure);

            _CurrentGameConfiguration.HotSwitch = HotSwitchType.None;

            _Model.Egm.ReportMismatchedConfiguration(EgmEvent.InvalidGameConfiguration,errorReason);

            Reset();

            
        }


        private void HandleIdleModeWithZeroCredits()
        {
            RequestCurrentGameMeters();

            UpdatePendingStatus(HotSwitchStatus.LockupRequestPending);
        }

        private void ProcessIdleModeFlag(bool isIdleMode)
        {
            if (!isIdleMode) return;

            var egmCredits = _Model.Egm.GetMeters().GetCreditAmount(null).DangerousGetUnsignedValue();
            if (egmCredits > 0m) return;

            _Scheduler.Stop();
            RequestCurrentGameMeters();
        }

        private void QueueSystemLockup()
        {
            var cabinet = _Model.Egm.CabinetDevice;

            cabinet.LockGameForHotSwitchProcedure(true, GetLockUpMessage());

            UpdatePendingStatus(HotSwitchStatus.LockupPending);           
        }


        private string GetLockUpMessage()
        {
            if ((IsGameVariationSwitchInProgress() && IsPgidSwitchInProgress()) ||
                (IsGameVariationSwitchInProgress() && IsGameStatusSwitchInProgress()) ||
                (IsPgidSwitchInProgress() && IsGameStatusSwitchInProgress()))
                return _Model.AdditionalConfiguration.CombinedHotSwitchText;

            if (IsGameVariationSwitchInProgress()) return _Model.AdditionalConfiguration.VariationHotSwitchText;

            if (IsPgidSwitchInProgress()) return _Model.AdditionalConfiguration.PgidHotSwitchText;

            if (IsGameStatusSwitchInProgress()) return _Model.AdditionalConfiguration.GameStatusHotSwitchText;

            return String.Empty;

        }

        private void UpdatePendingStatus(HotSwitchStatus status)
        {
            _ConfigChangeProcedureStatus = status;
        }


        private void HandleSystemLockup()
        {
            if (_ConfigChangeProcedureStatus == HotSwitchStatus.LockupClearPending) return;

            UpdatePendingStatus(HotSwitchStatus.LockupClearPending);
            _Scheduler.TimeOutAction = OnLockupTimerElapsed;
            _Scheduler.Start(LockupTimer);
        }


        private void OnLockupTimerElapsed()
        {
            _Scheduler.Stop();
            _Model.Egm.CabinetDevice.LockGameForHotSwitchProcedure(false, string.Empty);

            UpdatePendingStatus(HotSwitchStatus.HotSwitchRequestPending);
        }

        private void DoHotSwitch()
        {
            _Model.SendPoll(_CurrentConfiguration.ConfigurationPoll);
            
            UpdatePendingStatus(HotSwitchStatus.HotSwitchPending);
            _CurrentConfiguration.UpdateConfigurationStatus(EgmGameConfigurationStatus.InProgress);
        }


        public bool IsVariationHotSwitchingSupported()
        {
            return _CurrentGameConfiguration != null && _CurrentGameConfiguration.HasSupportForVariationHotSwitching;
        }

        public override void Process(GeneralStatusResponse applicationMessage)
        {
            switch (_ConfigChangeProcedureStatus)
            {
                case HotSwitchStatus.ZeroCreditsIdleModePending:
                    ProcessIdleModeFlag(applicationMessage.IsIdleMode);
                    break;

                case HotSwitchStatus.LockupRequestPending:
                    QueueSystemLockup();
                    break;

                case HotSwitchStatus.LockupPending:
                    if (applicationMessage.IsSystemLockup)
                        HandleSystemLockup();
                    break;

                case HotSwitchStatus.HotSwitchRequestPending:
                    DoHotSwitch();
                    break;
            }
        }

        private void RequestCurrentGameMeters()
        {
            _Model.Egm.FetchCurrentGameMeters();
        }


        private bool IsGameVariationSwitchInProgress()
        {
            return (_CurrentGameConfiguration.HotSwitch & HotSwitchType.GameVariation) == HotSwitchType.GameVariation;
        }

        private bool IsPgidSwitchInProgress()
        {
            return (_CurrentGameConfiguration.HotSwitch & HotSwitchType.ProgressiveGroupId) == HotSwitchType.ProgressiveGroupId;
        }


        private bool IsGameStatusSwitchInProgress()
        {
            return (_CurrentGameConfiguration.HotSwitch & HotSwitchType.GameStatus) == HotSwitchType.GameStatus;
        }
    }
}
