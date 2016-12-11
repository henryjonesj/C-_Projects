using BallyTech.QCom.Model;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;



namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public abstract partial class ConfigurationProcedureBase : ApplicationMessageListener
    {

        protected QComModel _Model = null;
        protected ConfigurationRepository _Repository = null;

        protected IQComConfiguration _CurrentConfiguration = null;


        protected ConfigurationProcedureBase()
        {
        }

        protected ConfigurationProcedureBase(QComModel model)
        {
            _Model = model;
            _Repository = model.ConfigurationRepository;
        }

        internal bool IsAnyConfigurationInProgress
        {
            get { return _CurrentConfiguration != null; }
        }

        internal bool IsSameConfigurationInProgress(QComConfigurationId configuration)
        {
            return _CurrentConfiguration.Id.Equals(configuration);
        }


        internal bool AreAllConfigurationsFinished
        {
            get { return _Repository.AreAllConfigurationsFinished; }
        }

        internal virtual bool IsLastAttemptedConfigurationRequestPending
        {
            get { return _CurrentConfiguration != null && _CurrentConfiguration.ConfigurationStatus == EgmGameConfigurationStatus.InProgress; }
        }

        internal virtual Request GetRequestForLastAttemptedConfiguration()
        {
            return _CurrentConfiguration == null ? null : _CurrentConfiguration.ConfigurationRequest;
        }


        internal virtual bool IsConfigurationPending
        {
            get
            {
                return _CurrentConfiguration == null
                           ? false
                           : _CurrentConfiguration.ConfigurationStatus == EgmGameConfigurationStatus.Pending;
            }
        }


        internal void Dispatch(ApplicationMessage message)
        {
            message.Dispatch(this);
        }


        internal abstract void DoConfiguration(IQComConfiguration configuration);
        internal abstract void OnConfigurationSucceeded(ApplicationMessage applicationMessage);
        internal abstract void OnConfigurationFailed(ApplicationMessage applicationMessage);

    }
}