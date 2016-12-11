using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class ElectronicCreditTransferResponseProcessor : MessageProcessor
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ElectronicCreditTransferResponseProcessor));
       
        public override void Process(EctToEgmAcknowledgementResponse applicationMessage)
        {
            var expectedECTPollSequenceNumber = PollSequenceNumberSupplier.SupplyNext(Model.ECTPollSequenceNumber);
            Model.ECTPollSequenceNumber = applicationMessage.EctPollSequenceNumber;

            if (expectedECTPollSequenceNumber != applicationMessage.EctPollSequenceNumber)
            {
                BuildAndRaiseUnexpectedFundTransferPollSequenceNumberEvent(expectedECTPollSequenceNumber);

                Model.Egm.ReceivedInValidFundTransferPsn();
                Model.Egm.TransferInDevice.HandleOutOfSequencePsn();
                Model.Egm.LinkedProgressiveDevice.HandleOutOfSequencePsn();
            }
            else
                Model.Egm.ReceivedValidFundTransferPSN();

            Model.EctToEgmTimeoutDetector.AcknowledgementResponseReceived();
            Model.EctToEgmPollDispatcher.OnReceivingAck();
        }


        private void BuildAndRaiseUnexpectedFundTransferPollSequenceNumberEvent(byte expectedSequenceNumber)
        {
            Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
            {
                EctPollSequenceNumber = Model.ECTPollSequenceNumber,
                ExpectedSequenceNumber = expectedSequenceNumber,
            };

            Model.Egm.ReportEvent(EgmEvent.UnexpectedFundTransferPollSequenceNumber);

            _Log.InfoFormat("Unexp ECT Poll PSN , Received: {0}, Expected {0}", Model.ECTPollSequenceNumber, expectedSequenceNumber);        
        }
    }
}
