using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Model;
using log4net;
using System.Collections;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class ConfigurationRepository
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (ConfigurationRepository));

        private QComConfigurationCollection _Configurations = new QComConfigurationCollection();

        public CurrentEgmConfiguration CurrentEgmConfiguration { get; private set; }

        private AdditionalConfiguration _AdditionalConfiguration;

        internal bool AwaitingForConfiguration { get; private set; }

        internal bool RequestProgressiveConfiguration { get; set; }


        public ConfigurationRepository()
        {
        }
    
        public ConfigurationRepository(AdditionalConfiguration additionalConfiguration)
        {
            this._AdditionalConfiguration = additionalConfiguration;

            this.CurrentEgmConfiguration = new CurrentEgmConfiguration();

            this.RequestProgressiveConfiguration = true;
        }


        internal void ConfigurationRequested()
        {
            this.AwaitingForConfiguration = true;
        }


        internal void Update(IEgmConfiguration egmConfiguration,ICollection<IGameConfiguration> gameConfigurations)
        {
            if (egmConfiguration != null) Update(egmConfiguration);

            if (gameConfigurations != null) Update(gameConfigurations.ToSerializableList());

            if (AllConfigurationsAvailable) AwaitingForConfiguration = false;
        }

        private void Update(IEgmConfiguration egmConfiguration)
        {
            string errorReason = null;
            
            if (!egmConfiguration.IsInAcceptableFormat(out errorReason)) throw new InvalidEgmConfigurationException(errorReason);

            if (!egmConfiguration.IsGamesAvailable()) throw new InvalidEgmConfigurationException("No Games Available");

            _Configurations.Update(new QComEgmConfiguration(egmConfiguration));
            RemoveOldGameConfigurations(egmConfiguration.GameConfigurations);
            _Configurations.Update(new QComParameterConfiguration(egmConfiguration)
                                       {IsCreditLimitSupported = _AdditionalConfiguration.IsCreditModeSupported});
        }
 
        internal void Update(SerializableList<IGameConfiguration> gameConfigurations)
        {   
            foreach (var gameConfiguration in gameConfigurations)
            {
                if (!gameConfiguration.IsInAccepatableFormat()) throw new InvalidGameConfigurationException("VAR Not Byte BCD");

                if (!CurrentEgmConfiguration.IsValidGame(gameConfiguration.GameNumber))
                    throw new InvalidGameConfigurationException("Invalid GVN");

                if (DoesProgressiveLevelDataExists(gameConfiguration.ProgressiveConfiguration))
                    if (!gameConfiguration.ProgressiveConfiguration.IsProgressiveInformationValid())
                        throw new InvalidProgressiveConfigurationException("Prog Info Invalid");


                _Configurations.Update(new QComGameConfiguration(gameConfiguration));

                RemoveOldProgressiveConfigurationIfNecessary(gameConfiguration);

                if (DoesProgressiveLevelDataExists(gameConfiguration.ProgressiveConfiguration))
                {
                    _Configurations.Update(
                        new QComProgressiveConfiguration(new GameProgressiveConfiguration(gameConfiguration)));
                }
            }
        }

        private void RemoveOldProgressiveConfigurationIfNecessary(IGameConfiguration newConfiguration)
        {
            if (newConfiguration.ProgressiveConfiguration != null) return;

            RemoveProgressiveConfiguration(newConfiguration);
        
        }

        private void RemoveOldProgressiveConfiguration(IGameConfiguration gameConfiguration)
        {
            if (gameConfiguration.ProgressiveConfiguration == null) return;

            RemoveProgressiveConfiguration(gameConfiguration);

        }

        private void RemoveProgressiveConfiguration(IGameConfiguration gameConfiguration)
        {
            var progressiveConfiguration = _Configurations.OfType<QComProgressiveConfiguration>().FirstOrDefault((config) => config.ConfigurationData.GameNumber ==
                                                    gameConfiguration.GameNumber);

            if (progressiveConfiguration == null) return;

            _Log.InfoFormat("Removing Progressive Configuration: Game Number :{0}", progressiveConfiguration.ConfigurationData.GameNumber);

            _Configurations.Remove(progressiveConfiguration);
        
        }
        


        private void RemoveOldGameConfigurations(IEnumerable<IGameConfiguration> newgameConfigurations)
        {
            var oldgameConfigurations = _Configurations.GetConfigurations<QComGameConfiguration>();

            if (oldgameConfigurations == null) return;

            var configToRemove = (from oldConfig in _Configurations.GetConfigurations<QComGameConfiguration>()
                                  where !newgameConfigurations.Any((item) => item.GameNumber == oldConfig.ConfigurationData.GameNumber)
                                  select oldConfig).ToSerializableList();

            if (configToRemove == null || configToRemove.Count==0) return;

            var allGameConfigurations = _Configurations.GetConfigurations<QComGameConfiguration>();
            var gameConfigurationMatch = new Predicate<QComGameConfiguration>((config) => configToRemove.Any((item) =>item.ConfigurationData.GameNumber == config.ConfigurationData.GameNumber));
            
            foreach (var configuration in _Configurations.OfType<QComGameConfiguration>().ToList())
            {
                if (!gameConfigurationMatch(configuration)) continue;

                RemoveOldProgressiveConfiguration(configuration.ConfigurationData);
                
                _Log.InfoFormat("Removing Game Configuration: Game Number :{0}", configuration.ConfigurationData.GameNumber);
                _Configurations.Remove(configuration);
            }                

        }

       

        private static bool DoesProgressiveLevelDataExists(IProgressiveConfiguration progressiveConfiguration)
        {
            if (progressiveConfiguration == null) return false;

            return progressiveConfiguration.ProgressiveLevelConfigurations != null &&
                   progressiveConfiguration.ProgressiveLevelConfigurations.Count() > 0;

        }

        internal void Remove(IQComConfiguration configuration)
        {            
            _Configurations.Remove(configuration);
        }

        internal TConfiguration GetConfigurationOfType<TConfiguration>() where TConfiguration : class
        {
            return _Configurations.GetConfiguration<TConfiguration>();
        }

        internal TConfiguration GetPendingConfigurationOfType<TConfiguration>() where TConfiguration :class
        {
            return _Configurations.GetPendingConfiguration<TConfiguration>();
        }


        internal SerializableList<IQComConfiguration> GetAllConfigurations()
        {           
            return _Configurations.ToSerializableList();
        }

        internal bool AllConfigurationsAvailable
        {
            get
            {
                return GetConfigurationOfType<QComEgmConfiguration>() != null &&
                       GetConfigurationsOfType<QComGameConfiguration>().Count() > 0;
            }
        }

        internal void ResetAllValidationStatus()
        {
            GetAllConfigurations().ForEach((configuration) => configuration.ResetValidationStatus());

            ResetProgressiveConfigurationStatusIfRequired();
        }

        internal void ResetProgressiveConfigurationStatusIfRequired()
        {
            if(!CurrentEgmConfiguration.IsSharedProgressiveComponentSupported) return;

            if (GetConfigurationsOfType<QComProgressiveConfiguration>().Any((element) => element.HasSupportForCustomSAP == true))

                GetConfigurationsOfType<QComProgressiveConfiguration>().ForEach(
                    (configuration) => configuration.ResetConfigurationStatus());
        }

        internal IEnumerable<TConfiguration> GetConfigurationsOfType<TConfiguration>() where TConfiguration : class
        {
            return _Configurations.GetConfigurations<TConfiguration>();
        }

        internal bool AreAllConfigurationsFinished
        {
            get
            {
                return
                    GetAllConfigurations().All(
                        (element) => element.ConfigurationStatus == EgmGameConfigurationStatus.Success);
            }
        }

        internal bool IsAnyConfigurationPending
        {
            get
            {
                return
                    GetAllConfigurations().Any(
                        (element) => element.ConfigurationStatus == EgmGameConfigurationStatus.None);
            }
        }

        internal bool IsBasicConfigurationsCompleted
        {
            get
            {
                var egmConfiguration = GetConfigurationOfType<QComEgmConfiguration>();

                if (egmConfiguration == null) return false;

                return egmConfiguration.ConfigurationStatus != EgmGameConfigurationStatus.None &&
                       GetConfigurationsOfType<QComGameConfiguration>().Any(
                           (element) => element.ConfigurationStatus != EgmGameConfigurationStatus.None);
            }
        }

        internal void OnProtocolVersionReceived(ProtocolVersion protocolVersion)
        {
            if (_Configurations != null)
                _Configurations.ForEach((configuration) => configuration.UpdateProtocolVersion(protocolVersion));

            if(protocolVersion == ProtocolVersion.V16) return;
            if(_Configurations == null) return;

            if (_Configurations.OfType<QComProgressiveConfiguration>().Count() > 0)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Removing the unsupported progressive configuration");
                _Configurations.RemoveAll((configuration) => configuration is QComProgressiveConfiguration);
            }
        }

        internal QComProgressiveConfiguration GetHostProgressiveConfiguration(ProgressiveConfigurationResponse response)
        {         
            var allProgressiveConfigurations = GetConfigurationsOfType<QComProgressiveConfiguration>();

            if (allProgressiveConfigurations == null) return null;

            var progressiveConfiguration = allProgressiveConfigurations.FirstOrDefault((element) => element.Id.Equals(response.ConfigurationId));
            return progressiveConfiguration;
        }


        internal void Clear()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Clearing all the configurations");
            _Configurations.Clear();
        }
    }
}
