using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Configuration;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class RunningState : State
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(RunningState));

        public override void DataLinkStatusChanged(bool LinkUp)
        {
            if (LinkUp)
            {
                Model.Egm.LinkStatusChanged(this.LinkStatus);
                return;
            }

            Model.State = new DiscoveringState();
        }

        public override void Enter()
        {
            Model.SpamHandler.Clear();
            SetParameters();
        }


        private void SetParameters()
        {
            var parameterConfiguration =
                Model.ConfigurationRepository.GetConfigurationOfType<QComParameterConfiguration>();

            if(parameterConfiguration == null)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Parameter Configuration is not available");
                return;
            }

            Model.SendPoll(QComConfigurationBuilder.Build(parameterConfiguration));

            Model.Egm.NoteAcceptorDevice.IsCreditInputEnabled = true;
        }

        


        public override LinkStatus LinkStatus
        {
            get { return LinkStatus.Connected; }
        }

        public override void Process(ApplicationMessage response)
        {
            Model.MessageProcessorCollection.Dispatch(response);
        }

        internal override void OnConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            bool isUpdated =  UpdateConfigurations(egmConfiguration,gameConfigurations);

            if (isUpdated) AttemptToReconfigure();
        }

        private void AttemptToReconfigure()
        {
            if (Model.ConfigurationRepository.AreAllConfigurationsFinished)
            {
                SetParameters();
                return;
            }

            Model.State = new ReconfiguringState();
        }

    }
}
