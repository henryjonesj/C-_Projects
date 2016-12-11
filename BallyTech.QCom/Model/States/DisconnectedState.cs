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
    public partial class DisconnectedState : State
    {
        private EgmDetailsChecker _EgmDetailsChecker;
        private static readonly ILog _Log = LogManager.GetLogger(typeof(DisconnectedState));
        
        public override LinkStatus LinkStatus
        {
            get { return LinkStatus.Disconnected; }
        }

        public override void DataLinkStatusChanged(bool LinkUp)
        {
            if (!LinkUp) return;
            _EgmDetailsChecker.CheckForEgmDetails(DetailsEntryStatus.OnDataLinkStatusChanged);
        }

        protected override void OnResponseReceived()
        {
            _EgmDetailsChecker.CheckForEgmDetails(DetailsEntryStatus.OnResponseReceived);
        }

        public override void Process(ApplicationMessage response)
        {
            Model.MessageProcessorCollection.Dispatch(response);
        }

        public override void Enter()
        {
            Model.MeterTracker.InvalidateMeters();
            _EgmDetailsChecker = new EgmDetailsChecker(Model);
            _EgmDetailsChecker.CheckForEgmDetails(DetailsEntryStatus.OnEnter);
        }

        public override void SetEgmDetails(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            Model.EgmDetails.AssetNumber = assetNumber;
            if (!EgmDetailsReceived())
            {
                Model.EgmDetails.ManufacturerId = (byte)manufactureId;
                Model.EgmDetails.SerialNumber = serialNumber;
            }
            Model.SaveDetailsForRepository(Model.EgmDetails.AssetNumber, Model.EgmDetails.ManufacturerId, Model.EgmDetails.SerialNumber);
            _EgmDetailsChecker.CheckForEgmDetails(DetailsEntryStatus.OnDetailsEntry);
        }

        public override void InitializationComplete()
        {
            Model.State = new ConnectedState();
        }

        protected override void AttemptToConnect()
        {
            
        }

        public override void RequestConfigurationIfNecessary()
        {
            
        }
    }
}
