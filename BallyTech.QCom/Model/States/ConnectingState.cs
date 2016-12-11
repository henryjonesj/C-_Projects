using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Handlers;

namespace BallyTech.QCom.Model.States
{
    [GenerateICSerializable]
    public partial class ConnectingState : State
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(ConnectingState));

        public override void DataLinkStatusChanged(bool LinkUp)
        {
            if (!LinkUp)
                Model.State = new DiscoveringState();
        }

        public override void Enter()
        {
            _Model.Egm.SetMachineState(true);
            ResetPsnOnRamReset();
            _Model.RaleHandler.HandleGameConnecting();
        }

        private void ResetPsnOnRamReset()
        {
            if (!Model.IsRamReset) return;

            Model.SendPoll(new EgmConfigurationRequestPoll()
            {
                StatusRequestFlag = StatusRequestFlag.Reserved | StatusRequestFlag.ResetPSN
            });
            Model.ResetPollSequenceNumbers();
        }

        public override void ProcessResponse(ApplicationMessage response)
        {
            base.ProcessResponse(response);
            if (CanStartInitialization())
            {
                Model.State = new InitializingState();
                return;
            }

            RequestConfigurationIfNecessary();
        }

        public override void Process(EgmConfigurationResponse applicationMessage)
        {
            if (_Log.IsInfoEnabled)
                _Log.Info("Ignoring this Egm Configuration response to have uniform flow for detecting Egm Ram clear");
        }

        private bool CanStartInitialization()
        {
            return ((!Model.IsRemoteConfigurationEnabled ||
                    Model.ConfigurationRepository.AllConfigurationsAvailable) &&
                    (Model.RaleHandler.State == RaleProgressState.Complete) &&
                    !Model.IsPollQueued<RequestAllLoggedEventsPoll>());
        }


        internal override void OnConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            bool isUpdated =  UpdateConfigurations(egmConfiguration, gameConfigurations);

            if(!isUpdated) return;
            if(!CanStartInitialization()) return;

            Model.State = new InitializingState();
        }

        public override LinkStatus LinkStatus
        {
            get { return LinkStatus.Connecting; }
        }

    }
}
