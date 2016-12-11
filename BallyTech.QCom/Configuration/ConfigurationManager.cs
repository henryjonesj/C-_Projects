using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class ConfigurationManager
    {
        protected QComModel _Model = null;
        protected ConfigurationRepository _Repository = null;

        internal ConfigurationProcedureBase _ConfigurationProcedure = null;

        public ConfigurationManager()
        {
        }

        public ConfigurationManager(QComModel model)
        {
            _Model = model;
            _Repository = model.ConfigurationRepository;
        }


        internal void DoNextConfiguration()
        {
            var configuration = ConfigurationSequence.GetNextConfigurationElement(_Repository);
            if (configuration == null) return; 

            _ConfigurationProcedure =
                new ConfigurationProcedureFactory() {Model = _Model}.GetConfigurationProcedure(configuration);

            if (_ConfigurationProcedure == null) return;
            _ConfigurationProcedure.DoConfiguration(configuration);
        }


        internal void OnConfigurationSucceeded(ApplicationMessage applicationMessage)
        {
            if(_ConfigurationProcedure == null) return;

            _ConfigurationProcedure.OnConfigurationSucceeded(applicationMessage);
        }


        internal void OnConfigurationFailed(ApplicationMessage applicationMessage)
        {
            if (_ConfigurationProcedure == null) return;

            _ConfigurationProcedure.OnConfigurationFailed(applicationMessage);
        }

        internal bool IsExpectedConfiguraionPending(QComConfigurationId configuration)
        {
            return IsAnyConfigurationInProgress ? _ConfigurationProcedure.IsSameConfigurationInProgress(configuration) : false;
        }


        internal bool IsAnyConfigurationInProgress
        {
            get { return _ConfigurationProcedure != null ? _ConfigurationProcedure.IsAnyConfigurationInProgress : false; }
        }


        internal bool AreAllConfigurationsFinished
        {
            get { return _Repository.AreAllConfigurationsFinished; }
        }

        internal virtual bool IsLastAttemptedConfigurationRequestPending
        {
            get
            {
                return _ConfigurationProcedure != null
                           ? _ConfigurationProcedure.IsLastAttemptedConfigurationRequestPending
                           : false;
            }
        }

        internal virtual Request GetRequestForLastAttemptedConfiguration()
        {
            return _ConfigurationProcedure != null
                       ? _ConfigurationProcedure.GetRequestForLastAttemptedConfiguration()
                       : null;
        }

        internal virtual bool IsConfigurationPending
        {
            get { return _ConfigurationProcedure != null ? _ConfigurationProcedure.IsConfigurationPending : false; }
        }

        public void ProcessResponse(ApplicationMessage message)
        {
            if (_ConfigurationProcedure == null) return;

            _ConfigurationProcedure.Dispatch(message);
        
        }
            

    }
}
