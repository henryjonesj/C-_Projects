using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Time;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class TicketOutDevice : Device
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(TicketOutDevice));

        private PendingTicketTransaction _PendingTransaction = null;

        private Meter _TicketOutMeter = Meter.Zero;

        internal override bool IsAnyTransferInProgress
        {
            get { return _PendingTransaction != null; }
        }

        private Meter GetUpdatedMeter()
        {
            return Model.GetMeters().GetTransferredAmount(Direction.Out, TransferDevice.Voucher, 0, null);
        }
        
        private bool IsMeterChangedDetected()
        {
            return MeterChangedDetector.IsMeterChanged(GetUpdatedMeter(), _TicketOutMeter);
        }

        
        internal void HandleTicketPrintInfo(decimal validationNumber, int systemId)
        {
            if (!IsAnyTransferInProgress) return;

            string ticketAuthorizationNumber = String.Format("{0:00}{1:0000000000000000}", systemId, validationNumber);

            _PendingTransaction.OnTicketPrintInfoReceived(Decimal.Parse(ticketAuthorizationNumber), TimeProvider.UtcNow.ToLocalTime());

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Received ticket data with Validation Id: {0},transaction time: {1}",
                                _PendingTransaction.ValidationId, _PendingTransaction.TransactionDateTime);

            Model.VoucherHandler.Execute(_PendingTransaction);

            if (!_PendingTransaction.IsAuthorized())
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Committing the transaction as ticket print is not authorized");
                CommitTransaction();
                return;
            }

            this.RequestMeters(MeterType.Ticket);
            
        }

        public void HandleTicketPrintRequest(ushort serialNumber, decimal amount)
        {
            if (IsAnyTransferInProgress) CommitTransaction();

            _PendingTransaction = new PendingTicketTransaction(serialNumber, amount);
            _TicketOutMeter = GetUpdatedMeter();

            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Ticket Print Requested with Serial Number: {0};Amount: {1}", serialNumber, amount);

            Model.Observers.TicketValidationNumberRequired(CreditType.Cashable, amount);
        }

        public void HandleTicketPrinted()
        {
            if (_PendingTransaction == null)
            {
                if (_Log.IsInfoEnabled) _Log.Info("No Ticket Print was Requested");
                return;
            }

            Model.Observers.TicketPrinted(new TicketPrinted(_PendingTransaction));
        }


        
        internal override void OnMetersReceived(SerializableList<MeterId> meterIdsReceived)
        {
            if (!IsAnyTransferInProgress) return;

            if(!meterIdsReceived.Contains(MeterId.TicketOut)) return;

            if (!(IsMeterChangedDetected() || _PendingTransaction.Status == TransactionStatus.Success)) 
                return;

            Model.Observers.TicketPrinted(new TicketPrinted(_PendingTransaction));
            CommitTransaction();
        }

        internal override void MeterRequestSplitIntervalSurpassed()
        {
            if (!IsAnyTransferInProgress) return;

            if (_Log.IsInfoEnabled) _Log.Info("Requesting the ticket meters again");

            Model.RequestMeters(MeterType.Ticket);
        }

        internal override void  MeterRequestTimerExpired()
        {
            if (!IsAnyTransferInProgress) return;

            if(_PendingTransaction.Status != TransactionStatus.Success) return;

            Model.Observers.TicketPrinted(new TicketPrinted(_PendingTransaction));

            CommitTransaction();
        }

        public void TicketPrintResultReceived(bool result)
        {
            if(!IsAnyTransferInProgress) return;

            if (!result)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Ticket Print Failed");
                CommitTransaction();
                return;
            }

            if (_Log.IsInfoEnabled) _Log.Info("Ticket Print Successful");
                
            _PendingTransaction.UpdateTransactionStatus(TransactionStatus.Success);   
            this.RequestMeters(MeterType.Ticket);
        }

        private void CommitTransaction()
        {
            if(!IsAnyTransferInProgress) return;

            if (_Log.IsInfoEnabled) _Log.Info("Committing the pending transaction");

            _PendingTransaction = null;            
            this.RequestedMetersReceived();
            this.AwaitingForGameIdle = false;
        }


        internal override void ForceReset()
        {
            CommitTransaction();
            base.ForceReset();
        }

    }
}
