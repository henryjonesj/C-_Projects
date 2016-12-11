using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Configuration;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Model.Meters;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class InitializingState : ValidatingState
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (InitializingState));

        public override void Enter()
        {
            base.Enter();
            
            SendEgmConfigurationRequest();
        }

        private void SendEgmConfigurationRequest()
        {
            StatusRequestFlag StatusRequestFlag= StatusRequestFlag.Reserved;

            if (_Model.RequestGameConfigurationViaEgmConfigurationRequestPoll || !_Model.IsRemoteConfigurationEnabled)
                StatusRequestFlag = StatusRequestFlag| StatusRequestFlag.GameConfigurationRequestFlag;

            Model.SendPoll(new EgmConfigurationRequestPoll()
            {
                StatusRequestFlag = StatusRequestFlag
            });
        }


        private void RequestGameConfigurationViaGeneralMaintenancePoll()
        {
            var allGameConfigurations = _Model.ConfigurationRepository.GetConfigurationsOfType<QComGameConfiguration>();
            allGameConfigurations.ForEach((element) => _Model.SendPoll(ConfigurationBasedRequestBuilder.Build(element.ConfigurationData)));    
        
        }

        private void OnReceivingNonConfiguredEgmConfiguration()
        {
            if(Model.RamclearAlreadyProcessed)
                Model.State = new ConfiguringState();
            else
                Model.RamCleared();
        }


        public override void Process(EgmConfigurationResponse response)
        {
            if (!response.IsConfigured)
            {
                OnReceivingNonConfiguredEgmConfiguration();
                return;
            }

            if (!HaveEgmConfigurationProcessed(response))
            {
                if (_Log.IsWarnEnabled) _Log.Warn("Egm Configuration Failed");
                return;
            }

            if (_Model.RequestGameConfigurationViaEgmConfigurationRequestPoll) return;

            RequestGameConfigurationViaGeneralMaintenancePoll();
        }

        public override void Process(EgmGameConfigurationResponse applicationMessage)
        {
            if (!applicationMessage.IsConfigured)
            {
                Model.State = new ConfiguringState();
                return;
            }

            if (!(HaveGameConfigurationProcessed(applicationMessage))) return;

            var _shouldRequestProgressiveConfigurationSpecification = new ProgressiveConfigurationRequestSpecification(Model);
            if (_shouldRequestProgressiveConfigurationSpecification.IsSatisfiedBy(applicationMessage))
            {
                RequestProgressiveConfiguration(applicationMessage.GameVersionNumber,applicationMessage.CurrentGameVariationNumber,applicationMessage.IsGameEnabled);
                return;
            }

            if (!HaveReceivedAllConfigurations()) return;

            if (!_ConfigurationValidator.AreAllValidationsCompleted) return;

        

            if (!Model.Egm.IsRomSignatureVerificationEnabled)
            {
                Model.RequestAllMeters();
                Model.FetchGameLevelMetersForAllGames();
                return;
            }

            if (_Log.IsInfoEnabled) _Log.Info("Game Rom Signature Verification is enabled, Changing the state to Running");

            InitializationComplete();
           
        }

        public override void Process(ProgressiveConfigurationResponse response)
        {
           var progressiveConfiguration= Model.ConfigurationRepository.GetHostProgressiveConfiguration(response);

           if (progressiveConfiguration != null)
               progressiveConfiguration.HasSupportForCustomSAP = response.HasSupportForCustomSAP();
            
            if (!response.IsConfigured || response.HasSupportForCustomSAP())
            {
                Model.State = new ConfiguringState();
                return;
            }

            if (progressiveConfiguration != null)
                progressiveConfiguration.CanValidateSAPParamters = response.HasSupportForCustomSAP();

            if (!(HaveProgressiveConfigurationProcessed(response))) return;

            UpdateSharedProgressivesValidationStatusIfNecessary();

            if (!HaveReceivedAllConfigurations()) return;

            if (!_ConfigurationValidator.AreAllValidationsCompleted) return;

            if (!Model.Egm.IsRomSignatureVerificationEnabled)
            {
                Model.RequestAllMeters();
                Model.FetchGameLevelMetersForAllGames();
                return;
            }            

            if (_Log.IsInfoEnabled) _Log.Info("Game Rom Signature Verification is enabled, Changing the state to Running");
            InitializationComplete();
        }

        public override void ProcessResponse(ApplicationMessage response)
        {
            base.ProcessResponse(response);

            if (!_EgmInitializationPending) return;

            PostProcess(response);
        }

        protected virtual void PostProcess(ApplicationMessage response)
        {
            if (IsGeneralStatusCountLimitReached(response))
                _Model.InitializationComplete();
        }

        internal override void OnConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            base.OnConfigurationReceived(egmConfiguration, gameConfigurations);

            _Model.State = new ConfiguringState();
        }
    }
}
