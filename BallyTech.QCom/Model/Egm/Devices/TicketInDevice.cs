using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Gtm;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class TicketInDevice : Device
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (TicketInDevice));

        private PendingTicketTransaction _PendingTransaction = null;

        internal override bool IsAnyTransferInProgress
        {
            get { return _PendingTransaction != null; }
        }

        private Meter _TicketInMeter = Meter.Zero;

        private Meter GetUpdatedMeter()
        {
            return Model.GetMeters().GetTransferredAmount(Direction.In, TransferDevice.Voucher, null,
                                                          CreditType.Cashable);
        }


        internal void HandleTicketRedeemInfo(ITicketValidated ticketValidated)
        {
            if (!IsAnyTransferInProgress) return;

            _TicketInMeter = GetUpdatedMeter();

            _PendingTransaction.OnTicketRedeemInfoReceived(ticketValidated);

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Ticket Redeem Info: {0}", _PendingTransaction.ToString());

            Model.VoucherHandler.Execute(_PendingTransaction);

            if (_PendingTransaction.IsAuthorized())
                this.AwaitingForUnsolicitedMeters();
            else
                CompleteTransaction(TransactionStatus.Failure);
        }


        public void HandleTicketInserted(decimal validationId)
        {
            if (IsAnyTransferInProgress)
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Committing the previous transaction");                    
                CompleteTransaction(TransactionStatus.Failure);
            }

            _PendingTransaction = new PendingTicketTransaction(validationId);
            Model.Observers.TicketInserted(new TicketInserted(validationId));            
        }


        internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            if (!IsAnyTransferInProgress) return;

            if (!(meterIdsReceived.Contains(MeterId.TicketIn))) return;

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Ticket In Meter:: Old: {0}, New: {1}", _TicketInMeter, GetUpdatedMeter());

            CompleteTransaction(MeterChangedDetector.IsMeterChanged(_TicketInMeter, GetUpdatedMeter())
                                    ? TransactionStatus.Success
                                    : TransactionStatus.Failure);

        }

        internal override void MeterRequestSplitIntervalSurpassed()
        {
            if(!IsAnyTransferInProgress) return;

            if (_Log.IsInfoEnabled) _Log.Info("Requesting the ticket meters again");

            Model.RequestMeters(MeterType.Ticket);
        }

        internal override void MeterRequestTimerExpired()
        {
            if (!IsAnyTransferInProgress) return;
            CompleteTransaction(TransactionStatus.Failure);
        }

        private void CompleteTransaction(TransactionStatus transactionStatus)
        {
            if (!IsAnyTransferInProgress) return;
            
            _PendingTransaction.UpdateTransactionStatus(transactionStatus);
            Model.Observers.TicketRedeemed(new TicketRedeemed(_PendingTransaction));

            ResetTransaction();   
        }

        
        private void ResetTransaction()
        {
            this.RequestedMetersReceived();
            _PendingTransaction = null;
            
        }


        internal override void ForceReset()
        {
            CompleteTransaction(TransactionStatus.Failure);
            base.ForceReset();
        }

    }
}
