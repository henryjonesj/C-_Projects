using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Builders;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class ConfiguringState : ValidatingState
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (ConfiguringState));

        private ConfigurationManager _ConfigurationManager = null;

        public override void Enter()
        {
            base.Enter();
            _ConfigurationManager = new ConfigurationManager(Model);

            PerformConfiguration();
        }

        public override void OnConfigurationTimeOut()
        {
            ResetConfigurationProcedure();
        }

        private void ResetConfigurationProcedure()
        {
            _ConfigurationManager._ConfigurationProcedure = null;
        }

        

        internal override void OnConfigurationReceived(IEgmConfiguration egmConfiguration, ICollection<IGameConfiguration> gameConfigurations)
        {
            bool isUpdated = UpdateConfigurations(egmConfiguration, gameConfigurations);
            if (!isUpdated) return;

            PerformConfiguration();
        }


        public override void ProcessResponse(ApplicationMessage response)
        {
            base.ProcessResponse(response);            

            if (!(IsGeneralStatusCountLimitReached(response))) return;

            var isConfigurationRequestPending = _ConfigurationManager.IsLastAttemptedConfigurationRequestPending;

            _ConfigurationManager.ProcessResponse(response);
            
            if(_ConfigurationManager.IsConfigurationPending) return;

            if (isConfigurationRequestPending)
            {
                RequestLastAttemptedConfiguration();
                return;
            }

            if(Model.ConfigurationRepository.IsAnyConfigurationPending)
            {
                if (_Log.IsInfoEnabled) _Log.Info(" Attempting to perform the remaining configuration");                    
                PerformConfiguration();
                return;
            }

            PostProcess();
        }

        private void PostProcess()
        {
            if (!_ConfigurationManager.AreAllConfigurationsFinished) return;
            if (!_ConfigurationValidator.AreAllValidationsCompleted) return;

            if (_EgmInitializationPending)
                Model.InitializationComplete();
            else
                if (!Model.Egm.IsRomSignatureVerificationEnabled)
                {
                    Model.RequestAllMeters();
                    Model.FetchGameLevelMetersForAllGames();
                }
                else
                {
                    if (_Log.IsInfoEnabled) _Log.Info("Game Rom Signature Verification is enabled, Changing the state to Running");
                    InitializationComplete();
                }

        }


        private void RequestLastAttemptedConfiguration()
        {
            var configurationRequest = _ConfigurationManager.GetRequestForLastAttemptedConfiguration();

            if (configurationRequest != null)
                Model.SendPoll(configurationRequest);
        }

        public override void Process(EgmConfigurationResponse response)
        {
            if (!(HaveEgmConfigurationProcessed(response)))
            {
                _ConfigurationManager.OnConfigurationFailed(response);
                return;
            }

            OnConfigurationSuccessful(response);
        }

        private void PerformConfiguration()
        {
            if (_ConfigurationManager.IsAnyConfigurationInProgress)
            {
                if (_Log.IsInfoEnabled)
                    _Log.Info("Another configuration is progress. Hence not attempting to perform the configuration received now..");
                return;
            }
            
            if (_Log.IsInfoEnabled) _Log.Info("Performing next configuration");
                
            _ConfigurationManager.DoNextConfiguration();
            _GeneralStatusResponseCounter.Reset();
        }

        public override void Process(EgmGameConfigurationResponse applicationMessage)
        {
            if (!_ConfigurationManager.IsExpectedConfiguraionPending(applicationMessage.ConfigurationId))
            {
                _Log.Debug("May be received a new game configuraion in middle of another configuration. Hence ignoring");
                return;
            }

            if (!(HaveGameConfigurationProcessed(applicationMessage)))
            {
                OnGameConfigurationFailed(applicationMessage);
                return;
            }

            OnConfigurationSuccessful(applicationMessage);

            if (applicationMessage.HasSapLevels()) BuildAndReportInitialSapContributionNotification(applicationMessage);
           

            var _shouldRequestProgressiveConfigurationSpecification = new ProgressiveConfigurationRequestSpecification(Model);
            if (_shouldRequestProgressiveConfigurationSpecification.IsSatisfiedBy(applicationMessage))
            {
                RequestProgressiveConfiguration(applicationMessage.GameVersionNumber, applicationMessage.CurrentGameVariationNumber, applicationMessage.IsGameEnabled);
            }
        }

        public override void Process(ProgressiveConfigurationResponse progressiveConfigurationResponse)
        {
            if (!(HaveProgressiveConfigurationProcessed(progressiveConfigurationResponse)))
            {
                _ConfigurationManager.OnConfigurationFailed(progressiveConfigurationResponse);
                return;
            }

            var progressiveConfiguration = _Model.ConfigurationRepository.GetHostProgressiveConfiguration(progressiveConfigurationResponse);

            if (progressiveConfigurationResponse.HasSupportForCustomSAP() && !progressiveConfiguration.CanValidateSAPParamters)
            {
                progressiveConfiguration.HasSupportForCustomSAP = true;
                ResetConfigurationProcedure();
                PerformConfiguration();
                return;
            }

            OnConfigurationSuccessful(progressiveConfigurationResponse);

            UpdateSharedProgressivesValidationStatusIfNecessary();

        }


        private void OnGameConfigurationFailed(ApplicationMessage applicationMessage)
        {
            _ConfigurationManager.OnConfigurationFailed(applicationMessage);

            if(_ConfigurationManager.AreAllConfigurationsFinished) return;

            if (_Log.IsWarnEnabled)
                _Log.Warn(
                    "Proceeding with the next game configuration even though the current game configuration failed");

            PerformConfiguration();
        }

        private void OnConfigurationSuccessful(ApplicationMessage applicationMessage)
        {
            _ConfigurationManager.OnConfigurationSucceeded(applicationMessage);

            if (!(_ConfigurationManager.AreAllConfigurationsFinished))
            {
                PerformConfiguration();
                return;
            }

            if (!_ConfigurationValidator.AreAllValidationsCompleted) return;

            if (!Model.Egm.IsRomSignatureVerificationEnabled)
            {
                Model.RequestAllMeters();
                Model.Egm.GetGameMeters();
                Model.FetchGameLevelMetersForAllGames();
                return;
            }

            if (_Log.IsInfoEnabled) _Log.Info("Game Rom Signature Verification is enabled, Changing the state to Running");
            InitializationComplete();
        }

        private void BuildAndReportInitialSapContributionNotification(EgmGameConfigurationResponse applicationMessage)
        {
            Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
            {
                GameNumber = applicationMessage.GameVersionNumber,
                PaytableId = applicationMessage.CurrentGameVariationNumber.ToString()
            };

            Model.Egm.ReportEvent(EgmEvent.EGMInitialSAPContributionNotification);
        
        }
   }
}
