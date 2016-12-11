using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Configuration;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class VoucherHandler : IVoucherStrategy
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (VoucherHandler));

        private const decimal MaxTicketPrintLimit = 42949672.95m;

        private string _TicketText = string.Empty;

        private QComModel _Model = null;
        [AutoWire(Name="QComModel")]
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        #region IVoucherHandler Members

        public void SetState(bool isEnabled)
        {
            decimal ticketOutLimit = 0;

            if (isEnabled)
                ticketOutLimit = GetTicketPrintLimit(Model.Egm.TicketPrintLimit);

            _Model.SendPoll(new HopperTicketPrinterMaintenancePoll() 
                                                                    {
                                                                        TicketOutLimit = ticketOutLimit/QComCommon.MeterScaleFactor,
                                                                        HopperCollectLimit = Model.Egm.HopperCollectLimit / QComCommon.MeterScaleFactor,
                                                                        HopperRefillAmount=Model.Egm.HopperRefillAmount/QComCommon.MeterScaleFactor,
                                                                        
                                                                    });
        }

        private static decimal GetTicketPrintLimit(decimal ticketPrintLimit)
        {
            return ticketPrintLimit > MaxTicketPrintLimit ? MaxTicketPrintLimit : ticketPrintLimit;
        }


        public void Execute(IVoucherTransaction ticketTransaction)
        {
            switch (ticketTransaction.Direction)
            {
                case Direction.Out:
                    PrintTicket(ticketTransaction);
                    break;
                    case Direction.In:
                    RedeemTicket(ticketTransaction);
                    break;
            }

           
        }


        public void SetProperties(string propertyName, string location, string ticketText)
        {
            _TicketText = ticketText;

            var siteDetails = new SiteDetails()
                               {
                                   SText = propertyName,
                                   LText = location
                               };

            Model.SendPoll(siteDetails);
        }

        #endregion


        private void PrintTicket(IVoucherTransaction ticketTransaction)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Ticket Print Authorization Status: {0}", ticketTransaction.IsAuthorized());

            if (!ticketTransaction.IsAuthorized())
            {
                Model.SendPoll(new CashTicketOutRequestAcknowledgementPoll()
                                   {
                                       CashTicketOutFlag = CashTicketOutFlagCharacteristics.OnFailure,                                       
                                   });
                return;
            }


            var cashoutAck = new CashTicketOutRequestAcknowledgementPoll()
                                 {
                                     CashTicketOutFlag = CashTicketOutFlagCharacteristics.Success,
                                     TicketSerialNumber = ticketTransaction.SerialNumber,
                                     CashTicketOutText = _TicketText,
                                     CashTicketOutLength = (byte)_TicketText.Length,
                                     TicketAuthorisationNumber = ticketTransaction.ValidationId,
                                     TicketOutAmount = ticketTransaction.Amount/QComCommon.MeterScaleFactor,
                                     TransactionTime = ticketTransaction.TransactionDateTime,
                                 };

            Model.SendPoll(cashoutAck);

        }


        private void RedeemTicket(IVoucherTransaction ticketTransaction)
        {
            if (!ticketTransaction.IsAuthorized())
            {
                Model.SendPoll(new CashTicketInRequestAcknowledgementPoll()
                {
                    FCode = ticketTransaction.ReasonCode.ToFCode(),
                    TicketAuthorisationNumber = ticketTransaction.ValidationId
                });
                return;
            }


            Model.SendPoll(new CashTicketInRequestAcknowledgementPoll()
            {
                FCode = FCodes.TicketAccepted,
                TicketAuthorisationNumber = ticketTransaction.ValidationId,
                TicketInAmount = ticketTransaction.Amount / QComCommon.MeterScaleFactor
            });

        }

    }
}
