using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Builders;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class ValidatingState : State
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (ValidatingState));

        protected bool _EgmInitializationPending = false;

        private const uint GeneralStatusResponseCountLimit = 2;

        protected QComResponseSpecification _EgmGameConfigurationValidationSpecification;

        protected QComResponseSpecification _ProgressiveConfigurationValidationSpecification;


        public override void Enter()
        {
            _ConfigurationValidator = new ConfigurationValidator(Model);

            _EgmGameConfigurationValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.EgmGameConfigurationResponse);

            _ProgressiveConfigurationValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.ProgressiveConfigurationResponse);

            SetProgressiveConfigurationRequestStatus();

        }


        public override void DataLinkStatusChanged(bool LinkUp)
        {
            if (LinkUp) return;
            Model.State = new DiscoveringState();
        }

      
        protected bool HaveEgmConfigurationProcessed(EgmConfigurationResponse response)
        {
            if (!IsEgmProtocolValid(response)) return false;

            if (!IsValidEgmConfiguration(response))
            {
                OnEgmConfigurationValidationFailed(response);
                return false;
            }

            base.Process(response);

            return true;
        }

        protected void RequestProgressiveConfiguration(ushort version, byte variation,bool status)
        {            
            var generalFlag = status == true ? GeneraFlagStatus.GameEnableFlag : GeneraFlagStatus.None;
            var maintenanceFlagStatus = Model.Egm.CabinetDevice.IsMachineEnabled ? MaintenanceFlagStatus.MachineEnableFlag : MaintenanceFlagStatus.None;
            Model.SendPoll(new EgmGeneralMaintenancePoll()
            {
                GameVersionNumber = version,
                GameVariationNumber = variation,
                MaintenanceFlagStatus = maintenanceFlagStatus,
                GeneralFlag = GeneraFlagStatus.ProgressiveConfigurationResponseRequest | generalFlag,
            });

            _Model.ConfigurationRepository.RequestProgressiveConfiguration = false;
        }


        private static bool IsEgmProtocolValid(EgmConfigurationResponse response)
        {
            if (!response.IsProtocolValid)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Invalid QCom protocol in Egm Configuration");
                return false;
            }

            return true;
        }

        private bool HasGameConfigurationValidationSuccessful(ushort gameVersion)
        {
            var _ConfigurationRepository = Model.ConfigurationRepository;
            var allGameConfigurations = _ConfigurationRepository.GetConfigurationsOfType<QComGameConfiguration>();

            var gameConfiguration = allGameConfigurations.FirstOrDefault((element) => element.ConfigurationData.GameNumber == gameVersion);

            return gameConfiguration == null ? false : gameConfiguration.ValidationStatus == ValidationStatus.Success;
        
        }


        private bool HasEgmConfigurationValidationSuccessful()
        {
            var repository = Model.ConfigurationRepository;
            var egmConfiguration = repository.GetConfigurationOfType<QComEgmConfiguration>();

            return egmConfiguration == null
                       ? false
                       : egmConfiguration.ValidationStatus == ValidationStatus.Success;
        }


        private bool CanValidateGameConfiguration()
        {
            if (!Model.IsRemoteConfigurationEnabled) return true;

            if ((HasEgmConfigurationValidationSuccessful())) return true;

            if (_Log.IsWarnEnabled)
                _Log.Warn("Egm Configuration validation is not successful. Hence igoring game configuration");
            return false;
        }

        private bool CanValidateProgressiveConfiguration(ushort gameVersion)
        {
            if (!Model.IsRemoteConfigurationEnabled) return true;

            if ((HasGameConfigurationValidationSuccessful(gameVersion))) return true;
  

            if (_Log.IsWarnEnabled)
                _Log.WarnFormat("Game Configuration validation is not successful for game {0}. Hence igoring progressive configuration",gameVersion);
            return false;
 
        }

        protected bool HaveAllConfigurationsValidated()
        {
            return Model.ConfigurationRepository.GetAllConfigurations().All((element) => element.ValidationStatus == ValidationStatus.Success);
            
        }


        protected bool HaveGameConfigurationProcessed(EgmGameConfigurationResponse response)
        {
            if (!CanValidateGameConfiguration()) return false;

            if (!_EgmGameConfigurationValidationSpecification.IsSatisfiedBy(response))
            {
                OnGameConfigurationValidationFailed(response);
                return false;
            }

            if (!IsValidGameConfiguration(response))
            {
                OnGameConfigurationValidationFailed(response);
                return false;
            }

            base.Process(response);

            return true;
        }
  

        protected bool HaveProgressiveConfigurationProcessed(ProgressiveConfigurationResponse response)
        {
            if (!CanValidateProgressiveConfiguration(response.GameVersionNumber)) return false;

            if (!_ProgressiveConfigurationValidationSpecification.IsSatisfiedBy(response))
            {
                OnProgressiveConfigurationValidationFailed(response);
                return false;
            
            }
          
            if (!IsValidProgressiveConfiguration(response))
            {
                OnProgressiveConfigurationValidationFailed(response);
                return false;
            }

            base.Process(response);

            return true;
        
        }

        protected virtual void OnEgmConfigurationValidationFailed(EgmConfigurationResponse configurationResponse)
        {
            Model.Egm.ReportMismatchedConfiguration(configurationResponse.Reconcile(_Model));            
        }


        protected virtual void OnGameConfigurationValidationFailed(EgmGameConfigurationResponse configurationResponse)
        {
            Model.Egm.ReportMismatchedConfiguration(configurationResponse);            
        }

        protected virtual void OnProgressiveConfigurationValidationFailed(ProgressiveConfigurationResponse configurationResponse)
        {
            Model.Egm.ReportMismatchedConfiguration(configurationResponse.Reconcile(Model.ConfigurationRepository));            
        }

        protected bool IsGeneralStatusCountLimitReached(ApplicationMessage response)
        {
            _GeneralStatusResponseCounter.Received(response);
            if (!_GeneralStatusResponseCounter.IsCountLimitReached) return false;

            _GeneralStatusResponseCounter.Reset();
            return true;

        }

        protected bool HaveReceivedAllConfigurations()
        {
            if (!HaveReceivedAllGameConfigurationResponses()) return false;

            return !Model.IsRemoteConfigurationEnabled || _ConfigurationValidator.AreAllValidationsCompleted;
        }

        protected bool HasSupportForSharedProgressivesComponenent()
        {
            return Model.ConfigurationRepository.CurrentEgmConfiguration.IsSharedProgressiveComponentSupported ?
                Model.ConfigurationRepository.RequestProgressiveConfiguration: true;

        }

        public override void Process(MeterGroupContributionResponse meterResponse)
        {
            if (!_ConfigurationValidator.AreAllValidationsCompleted)
            {
                _Log.InfoFormat("Ignoring this Meter Group Contribution As Validations are not completed");
                return;
            }
            
            base.Process(meterResponse);

            if(!HaveReceivedAllConfigurations()) return;

            _EgmInitializationPending = true;
        }

        public override void Exit()
        {
            _ConfigurationValidator = null;
        }


        public override void InitializationComplete()
        {
            _Model.ConfigurationRepository.RequestProgressiveConfiguration = true;
            Model.State = new RunningState();
        }

        public override LinkStatus LinkStatus
        {
            get { return LinkStatus.Connecting; }
        }

        protected void UpdateSharedProgressivesValidationStatusIfNecessary()
        {   
            if (!Model.ConfigurationRepository.CurrentEgmConfiguration.IsSharedProgressiveComponentSupported) return;

            if (Model.ConfigurationRepository.GetConfigurationsOfType<QComProgressiveConfiguration>().Any((configuration)
                        => !configuration.HasSupportForCustomSAP))

                Model.ConfigurationRepository.GetConfigurationsOfType<QComProgressiveConfiguration>().ForEach((configuration)
                            => configuration.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success));
        }

        private void SetProgressiveConfigurationRequestStatus()
        {
            if(_Log.IsInfoEnabled)
                _Log.Info("Setting Shared Progressive Configuration Request Status");

            Model.ConfigurationRepository.RequestProgressiveConfiguration = true;
        }

    }
}
