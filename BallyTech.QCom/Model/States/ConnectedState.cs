using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class ConnectedState : State
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ConnectedState));

        public override LinkStatus LinkStatus
        {
            get { return LinkStatus.Connected; }
		}

        public override void DataLinkStatusChanged(bool LinkUp)
        {
            if (!LinkUp) Model.State = new DisconnectedState();            
        }

        public override void Process(ApplicationMessage response)
        {
            Model.MessageProcessorCollection.Dispatch(response);
        }

        public override void Process(PurgeEventsPollAcknowledgementResponse applicationMessage)
        {

        }

        public override void Process(PurgeEventsPollAcknowledgementResponseV16 applicationMessage)
        {
            
        }

        public override void SetEgmDetails(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            Model.SaveDetailsForRepository(assetNumber, manufactureId, serialNumber);
        }

        public override void RequestConfigurationIfNecessary() { }



    }
}
