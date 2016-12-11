using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Utility.Time;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Model.States;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class DiscoveringState : State
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(DiscoveringState));

        public override void Enter()
        {
            Model.ResetPollQueue();
            Model.ResetValidationStatus();
            Model.Egm.SoftwareAuthenticationDevice.ResetRomSignatureCompletionStatus();
            AttemptToConnect();            
        } 
  
        public override void Process(SeekEgmBroadcastResponse response)
        {
            base.Process(response);
            Model.SendPoll(PollAddressConfigurationBuilder.Build(_Model.EgmDetails.SerialNumber, _Model.EgmDetails.ManufacturerId).
                WithAddress(_Model.PollAddress));

            if (Model.IsAssetNumberPresent)
                Model.State = new ConnectingState();
        }


        public override void Process(ApplicationMessage response)
        {        
            base.Process(response);

            if (!(Model.IsAssetNumberPresent)) return;

            Model.State = new ConnectingState();
        }

        private bool IsValidConfiguration(IEgmConfiguration egmConfiguration)
        {
            if (egmConfiguration == null) return false;

            if (string.IsNullOrEmpty(egmConfiguration.ManufacturerId)) return false;
            return !string.IsNullOrEmpty(egmConfiguration.SerialNumber) && egmConfiguration.SerialNumber.IsNonZero();
        }

        internal override void OnConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            base.OnConfigurationReceived(egmConfiguration,gameConfigurations);

            if(!IsValidConfiguration(egmConfiguration)) return;

            Model.EgmDetails.SerialNumber = Decimal.Parse(egmConfiguration.SerialNumber);
            Model.EgmDetails.ManufacturerId = egmConfiguration.ManufacturerId.GetByteOrDefault();

            CheckEgmDetails();
            AttemptToConnect();
        }


        public override void DataLinkStatusChanged(bool LinkUp)
        {
            if (!LinkUp) return;
            if (!(Model.IsAssetNumberPresent)) return;

            Model.State = new ConnectingState();
        }


        public override void NoResponseReceived()
        {
            AttemptToConnect();
         
            if (!Model.IsAssetNumberPresent) return;

            RequestConfigurationIfNecessary();
        }     

    }
}
