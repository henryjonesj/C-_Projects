using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class Voucher : IVoucher
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (Voucher));

        internal EgmModel Model { get; set; }

        private VoucherProperties _VoucherProperties = new VoucherProperties();

        
        #region IVoucher Members

        public bool IsEnabled { get; private set; }

        public void RedeemTicket(ITicketValidated ticket)
        {
            Model.EgmAdapter.TicketInDevice.HandleTicketRedeemInfo(ticket);
        }

        public void SetValidationId(string machineId, string sequenceNumber)
        {
            ;
        }

        public void SetValidationId(decimal validationNumber, int systemId)
        {
            Model.EgmAdapter.TicketOutDevice.HandleTicketPrintInfo(validationNumber,systemId);               
        }

        public void SetTicketData(ushort hostId, byte expiresInDays, string property, string addressLine, string ticketText, string restrictedTicketTitle, string debitTicketTitle)
        {
            _VoucherProperties.Update(property,addressLine,ticketText);

            SetTicketData();
        }

        public void SetTicketData()
        {            
            if (_VoucherProperties.IsUpdated)
                Model.VoucherHandler.SetProperties(_VoucherProperties.PropertyName, _VoucherProperties.Location,
                                                   _VoucherProperties.TicketText);
            Model.VoucherHandler.SetState(this.IsEnabled);
        }

        public void SetExtendedTicketData(int cashableTicketExpireDays, int nonCashableTicketExpireDays, bool enableRestrictedTickets)
        {
            ;
        }

        public ValidationStyle ValidationStyle
        {
            get { return ValidationStyle.System; }
        }

        public void SetState(bool enableState)
        {
            this.IsEnabled = enableState;
            Model.VoucherHandler.SetState(enableState);
        }

        #endregion


        #region IObservableBy<IVoucherObserver> Members

        public void AddObserver(IVoucherObserver observer)
        {
            
        }

        public void RemoveObserver(IVoucherObserver observer)
        {
            
        }

        #endregion

        #region IVoucher Members


        public void SetMaxTicketLimit(decimal maxTicketLimit)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Setting max ticket print limit as {0}", maxTicketLimit);

            Model.EgmAdapter.TicketPrintLimit = maxTicketLimit;
            SetState(this.IsEnabled);
        }

        #endregion

        #region IVoucher Members

        public void DeclineTicketPrint()
        {
            if (_Log.IsWarnEnabled) _Log.Warn("Host declined the ticket print");
           
            Model.EgmAdapter.TicketOutDevice.HandleTicketPrintInfo(0m, 0);
        }

        #endregion
    }

    [GenerateICSerializable]
    public partial class VoucherProperties
    {
        internal string PropertyName { get; private set; }
        internal string Location { get; private set; }
        internal string TicketText { get; private set; }


        internal bool IsUpdated
        {
            get
            {
                return !(string.IsNullOrEmpty(PropertyName) || string.IsNullOrEmpty(Location) ||
                         string.IsNullOrEmpty(TicketText));
            }
        }


        internal void Update(string property,string location,string ticketText)
        {
            this.PropertyName = property;
            this.Location = location;
            this.TicketText = ticketText;
        }

    }

}
