using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using log4net;
using BallyTech.Gtm;
using BallyTech.Utility.Diagnostics;
using BallyTech.Utility;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Model.Meters;

namespace BallyTech.QCom.Model.Egm
{
    public enum LinkedProgressiveStatus
    {
        None,
        LPPendingAcknowledgement,
        LPAcknowledged,
        ManualResetPending,
        AutoResetPending
    }

    [GenerateICSerializable]
    public partial class LinkedProgressiveDevice : Device
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(LinkedProgressiveDevice));

        private SerializableDictionary<string, string> _AutoPayLimit = new SerializableDictionary<string, string>();
        public SerializableDictionary<string, string> AutoPayLimit
        {
            get { return _AutoPayLimit; }
            set { _AutoPayLimit = value; }
        }

        private LinkedProgressiveStatus _LinkedProgressiveStatus = LinkedProgressiveStatus.None;
        public LinkedProgressiveStatus LinkedProgressiveStatus
        {
            get { return _LinkedProgressiveStatus; }
        }

        private KeyOffDestination _KeyOffDestination;
        private HandpayType _HandpayType;
        private bool _ShouldAmountBeAutopaid = false;

        private uint _JackpotAmount = 0;

        private byte _NumberOfProgressiveLevels;
        public byte NumberOfProgressiveLevels
        {
            get { return _NumberOfProgressiveLevels; }
            set { _NumberOfProgressiveLevels = value; }
        }

        private Meter _CashlessCreditIn = Meter.Zero;
        private Meter _LpWin = Meter.Zero;

        private IFundsTransferAuthorization _PendingTransfer = null;
        public bool IsAutopayInProgress
        {
            get { return _PendingTransfer != null; }
        }

        internal override bool IsAnyTransferInProgress
        {
            get { return IsAutopayInProgress || (LinkedProgressiveStatus != LinkedProgressiveStatus.None); }
        }

        private bool _HasJackpotOccured = false;
        private bool _IsJackpotCleared = false;
        private bool _HaveReceivedMeters = false;

        public void UpdateProgressiveDevice(SerializableList<ProgressiveLevelInfo> progressiveLevelInfo, Game game)
        {
            this.NumberOfProgressiveLevels = (byte)progressiveLevelInfo.Count;
            Model.LinkedProgressiveHandler.UpdateProgressiveLevels(game, progressiveLevelInfo);
        }

        public void SetProgressiveLineCount(int lineCount)
        {
            this.NumberOfProgressiveLevels = (byte)lineCount;
        }


        public void HandleProgressiveJackpot(uint JackpotAmount, string LevelId, HandpayType handpayType, LinkedProgressiveLine progressiveLine)
        {
            _Log.InfoFormat("Linked progressive jackpot hit. Amount = {0}, level id = {1}, line number = {2}", JackpotAmount, LevelId, progressiveLine.LineId);

            if (_LinkedProgressiveStatus != LinkedProgressiveStatus.None)
            {
                HandleHandpayReset();
                RequestedMetersReceived();
            }

            _HandpayType = handpayType;
            _KeyOffDestination = KeyOffDestination.Handpay;
            _LinkedProgressiveStatus = LinkedProgressiveStatus.LPPendingAcknowledgement;

            Model.Observers.ProgressiveLineHit(progressiveLine);
            _JackpotAmount = JackpotAmount;
            _HasJackpotOccured = true;
            _LpWin = Model.GetMeters().GetWonAmount(null, null, null, WinSource.Progressive, WinPaymentMethod.HandPaid);

            _ShouldAmountBeAutopaid = ShouldAmountBeAutoPaid(LevelId);

            _Log.InfoFormat("Should amount be auto paid = {0}", _ShouldAmountBeAutopaid);
        }

        private bool ShouldAmountBeAutoPaid(string LevelId)
        {
            if (!IsAmountWithinAutoPayLimit(LevelId))
            {
                _Log.Info("Amount not within AutoPay Limit");
                return false;
            }

            return IsAmountWithTransferLimit();
        }

        private bool IsAmountWithTransferLimit()
        {
            if (!IsAmountWithinMaxCreditLimit())
            {
                _Log.Info("Amount not within Max Credit Limit");
                return false;
            }

            if (!IsAmountWithinMaxTransferLimit())
            {
                _Log.Info("Amount not within Max Transfer Limit");
                return false;
            }

            return true;
        }

        private bool IsAmountWithinMaxCreditLimit()
        {
            decimal currentCredits = Model.GetMeters().GetCreditAmount(null).DangerousGetUnsignedValue();
            return (currentCredits + (_JackpotAmount * MeterService.MeterScaleFactor)) < Model.EgmAdapter.MaxCreditLimit;
        }
        

       private bool IsAmountWithinMaxTransferLimit()
       {
           return Model.EgmAdapter.IsAmountWithinMaxTransferLimit(_JackpotAmount * MeterService.MeterScaleFactor);
       }

        private bool IsAmountWithinAutoPayLimit(string levelId)
        {
            if (AutoPayLimit.Count <= 0)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Auto pay limit is not present");

                return false;
            }

            if (!AutoPayLimit.ContainsKey(levelId))
            {
                if (_Log.IsInfoEnabled)
                    _Log.InfoFormat("Auto pay limit for the progressive level is {0} is not configured", levelId);

                return false;
            }

            var autoPayLimit = AutoPayLimit.FirstOrDefault((levelid) => levelid.Key == levelId).Value;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Auto Pay Limit: {0};Jackpot Amount: {1}", autoPayLimit, _JackpotAmount);

            return (decimal.Parse(autoPayLimit) >= _JackpotAmount);
        }

        private void SendHandpayPending()
        {
            _LinkedProgressiveStatus = LinkedProgressiveStatus.ManualResetPending;
            _KeyOffDestination = KeyOffDestination.Handpay;
            
            _IsJackpotCleared = false;
            var handpayPending = new PendingJackpot
                                                {
                                                    HandpayType = _HandpayType,
                                                    HandpayAmount = _JackpotAmount*QComCommon.MeterScaleFactor
                                                };
            Model.HandpayType = _HandpayType;
            Model.IsHandpayPending.Value = true;
            Model.EgmAdapter.FetchProgressiveMeters();
            Model.Observers.HandpayPending(handpayPending);
        }

        private void SendAutoPayCompleted()
        {
            var gameProgressiveId= Model.EgmAdapter.CurrentGame.ProgressiveGroupId;

            var ProgressiveId = String.IsNullOrEmpty(gameProgressiveId) ? 0 : Convert.ToInt32(gameProgressiveId);

            var autoPaidJackpot = new PaymentCompletedProgressiveJackpot()
            {
                Amount = _JackpotAmount * QComCommon.MeterScaleFactor,
                LevelType = _HandpayType,
                PaymentType = JackpotPaymentType.CreditMeter,
                ProgressiveGroupId = ProgressiveId
            };


            Model.Observers.ProgressiveHitPaymentCompleted(autoPaidJackpot);
            Model.LinkedProgressiveHandler.OnSuccessfulAutopay();
            Model.EgmRequestHandler.ResetPSN(ResetStatus.Success);

            _HaveReceivedMeters = false;
       
        }

        public void HandleHandpayLockupCleared()
        {
            if(_Log.IsDebugEnabled)
                _Log.DebugFormat("LinkedProgressiveStatus = {0}", _LinkedProgressiveStatus);

            if (_LinkedProgressiveStatus == LinkedProgressiveStatus.None || _IsJackpotCleared == true) return;

            if (IsAutopayInProgress && _LinkedProgressiveStatus == LinkedProgressiveStatus.LPAcknowledged)
            {                
                _PendingTransfer = null;
                _KeyOffDestination = KeyOffDestination.Handpay;
            }

            _IsJackpotCleared = true;
         
            _HaveReceivedMeters = false;
            AwaitingForUnsolicitedMeters();
        }

        private bool ShouldSendJackpotReset()
        {
            return (_HaveReceivedMeters && _IsJackpotCleared);
        }

        private void HandleHandpayReset()
        {
            if (_KeyOffDestination != KeyOffDestination.NotSpecified)
            {
                _Log.Info("Handpay reset sent to observers");
                Model.Observers.HandpayReset(_KeyOffDestination, (_JackpotAmount * MeterService.MeterScaleFactor));
                _KeyOffDestination = KeyOffDestination.NotSpecified;
            }
            ResetJackpotVariables();
        }


        internal override void MeterRequestSplitIntervalSurpassed()
        {
            if (!IsAutopayInProgress)
            {
                Model.RequestMeters(MeterType.Jackpot);
                return;
            }

            _Log.Info("Have not received the updated meters. Hence requesting the meters...");
            Model.RequestMeters(MeterType.Cashless);
        }

        internal override void  MeterRequestTimerExpired()
        {
            if (!IsAutopayInProgress)
            {
                if (_IsJackpotCleared)
                {
                    HandleHandpayReset();
                    RequestedMetersReceived();
                    return;
                }
                RequestMeters(MeterType.Jackpot);
                return;
            }

            _Log.Info("received MeterRequestTimerExpired");

            Model.EgmRequestHandler.ResetPSN(ResetStatus.Attempt);
            ResetAutopayTransfer();
        }

        private void ResetAutopayTransfer()
        {            
            Model.EgmAdapter.IsEctToEgmInProgress = false;
            _PendingTransfer = null;
            _LinkedProgressiveStatus = LinkedProgressiveStatus.ManualResetPending;
            _KeyOffDestination = KeyOffDestination.Handpay;
            SendHandpayPending();
        }

        private void ProcessInvalidMeterChange()
        {
            _Log.Info("Invalid Cashless Meter change detected");
            var newValue = Model.GetMeters().GetTransferredAmount(Direction.In, TransferDevice.AccountTransfer,
                                                           null, null);
            Model.EgmAdapter.ExtendedEventData = Model.EgmAdapter.ConstructUnreasonableMeterData(MeterCodeIdMapping.GetMeterCode(MeterId.CashlessIn), _CashlessCreditIn, newValue);
            Model.EgmAdapter.ReportErrorEvent(EgmErrorCodes.CentForCentMeterReconcilationFailure);

        }

        internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            if (IsAutopayInProgress)
            {
                if (meterIdsReceived.Contains(MeterId.CashlessIn))
                {
                    if (!IsMeterChangeDetected()) return;

                    if (!IsValidCashlessInMeterChangeDetected())
                        ProcessInvalidMeterChange();

                    _HaveReceivedMeters = true;
                    RequestedMetersReceived();

                    SendAutoPayCompleted();

                    _KeyOffDestination = KeyOffDestination.NotSpecified;

                    Model.EgmAdapter.IsEctFromEgmInProgress = false;

                    _PendingTransfer = null;
                    _LinkedProgressiveStatus = LinkedProgressiveStatus.AutoResetPending;
                 
                }
                return;                    
            }

            if (!(meterIdsReceived.Contains(MeterId.LpWins, MeterId.Wins))) return;

            if (_HasJackpotOccured)
            {
                if (!IsLpWinMeterChangeDetected() && _LinkedProgressiveStatus != LinkedProgressiveStatus.LPPendingAcknowledgement)
                {
                    _Log.Info("LPWin Meter change not detected");
                    var newValue = Model.GetMeters().GetWonAmount(null, null, null, WinSource.Progressive, WinPaymentMethod.HandPaid).DangerousGetUnsignedValue();
                    Model.EgmAdapter.ExtendedEventData = Model.EgmAdapter.ConstructUnreasonableMeterData(MeterCodeIdMapping.GetMeterCode(MeterId.LpWins), _LpWin, newValue);
                    Model.EgmAdapter.ReportErrorEvent(EgmErrorCodes.CentForCentMeterReconcilationFailure);
                }
                _HaveReceivedMeters = true;
                RequestedMetersReceived();

                if (ShouldSendJackpotReset()) HandleHandpayReset();                
            }
        }

        private bool IsMeterChangeDetected()
        {
            var newCashlessIn = Model.GetMeters().GetTransferredAmount(Direction.In, TransferDevice.AccountTransfer,
                                                                      null, null);
            
            return MeterChangedDetector.IsMeterChanged(_CashlessCreditIn, newCashlessIn);
        }

        private bool IsLpWinMeterChangeDetected()
        {
            var newLpWin = Model.GetMeters().GetWonAmount(null, null, null, WinSource.Progressive, WinPaymentMethod.HandPaid);

            _Log.InfoFormat("OldLpWin = {0}, newLPwin = {1}, jackpot amount = {2}", _LpWin, newLpWin, _JackpotAmount * QComCommon.MeterScaleFactor);
            return MeterChangedDetector.IsExpectedMeterChange(_LpWin, newLpWin, _JackpotAmount * QComCommon.MeterScaleFactor);
        }

        private bool IsValidCashlessInMeterChangeDetected()
        {
            var newCashlessIn = Model.GetMeters().GetTransferredAmount(Direction.In, TransferDevice.AccountTransfer,
                                                                       null, null);

            return MeterChangedDetector.IsExpectedMeterChange(_CashlessCreditIn, newCashlessIn, _PendingTransfer.GetTotalAmount());
        }

        private void ProcessLpAwardPayment(bool toBeAutoPaid)
        {
            if (toBeAutoPaid)
                InitiateLpAutopay();
            else
                SendHandpayPending();
        
        }

    
        private void InitiateLpAutopay()
        {
            if (Model.SoftwareAuthentication.IsMeterExclusionRequired)
            {
                _Log.Warn("Rom Signature Verification not Complete");
                SendHandpayPending();
                return;
            }
         
            _CashlessCreditIn = Model.GetMeters().GetTransferredAmount(Direction.In, TransferDevice.AccountTransfer,
                                                                       null, null);

            CreatePendingRequest();
            Model.EgmAdapter.IsEctToEgmInProgress = true;
            Model.FundsTransferHandler.Initiate(_PendingTransfer);
            AwaitingForUnsolicitedMeters();
            _ShouldAmountBeAutopaid = false;
        
        }

        private void CreatePendingRequest()
        {
            var fundsTransferRequest = new FundsTransferRequest() { Cashable = _JackpotAmount * QComCommon.MeterScaleFactor, Destination = TransferDestination.WithdrawToCreditMeter, Origin = TransferOrigin.BonusJackpotWin};

            _PendingTransfer = new DeclinedFundsTransferAuthorization() { InitiatingRequest = fundsTransferRequest, ResultCode = FundsTransferAuthorizationResultCode.Success};
        }

        public void HandleLinkedProgressiveLockup()
        {
            Model.LinkedProgressiveHandler.HandleLinkedProgressiveLockup();
        }

        public void LPAcknowledged(JackpotPaymentType paymentType)
        {
            _LinkedProgressiveStatus = LinkedProgressiveStatus.LPAcknowledged;

            _Log.InfoFormat("Payment Type received from EBS:{0}", paymentType);

            switch (paymentType)
            {
                case JackpotPaymentType.EgmDecides: ProcessLpAwardPayment(_ShouldAmountBeAutopaid);
                                                    break;

                case JackpotPaymentType.CreditMeter: ProcessLpAwardPayment(IsAmountWithTransferLimit());
                                                    break;

                case JackpotPaymentType.HandPay: SendHandpayPending();
                                                 break;
            }

        }

        
        internal override void ForceReset()
        {
            HandleHandpayReset();
            base.ForceReset();
        }

        private void ResetJackpotVariables()
        {
            _LinkedProgressiveStatus = LinkedProgressiveStatus.None;
            _IsJackpotCleared = false;
            _HasJackpotOccured = false;
            _HaveReceivedMeters = false;
            _JackpotAmount = 0;
            Model.IsHandpayPending.Value = false;
        }

        public void RemoteReset()
        {
            if (_Log.IsInfoEnabled)
                _Log.Info("Received remote reset from the host");
            

            Model.LinkedProgressiveHandler.ResetLockup();
        }

        internal void HandleOutOfSequencePsn()
        {
            if (!IsAutopayInProgress) return;
            ResetAutopayTransfer();
        }
    }
}