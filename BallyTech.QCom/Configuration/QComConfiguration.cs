using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Builders;
using log4net;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComConfiguration<TConfiguration> : IQComConfiguration where TConfiguration : class
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (QComConfiguration<TConfiguration>));


        public TConfiguration ConfigurationData { get; protected set; }

        internal ProtocolVersion _ProtocolVersion = ProtocolVersion.Unknown;

        #region IQComConfiguration Members

        public QComConfigurationId Id { get; protected set; }

        public ValidationStatus ValidationStatus { get; protected set; }

        public EgmGameConfigurationStatus ConfigurationStatus { get; protected set; }

        public void UpdateConfigurationStatus(EgmGameConfigurationStatus status)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Updating {0} with status {1}", this.Id, status);

            this.ConfigurationStatus = status;

            switch (status)
            {
                case EgmGameConfigurationStatus.Success:
                    this.ValidationStatus = ValidationStatus.Success;
                    break;

                case EgmGameConfigurationStatus.Failure:
                    this.ValidationStatus = ValidationStatus.Failure;
                    break;

                default:
                    break;
            }

        }
        public virtual Request ConfigurationPoll
        {
            get { return null; }
        }


        public virtual Request ConfigurationRequest
        {
            get { return null; }
        }

        public virtual void ResetValidationStatus()
        {
            this.ValidationStatus = ValidationStatus.None;
        }
        

        #endregion

        internal virtual void Update(TConfiguration configuration)
        {
            this.ConfigurationData = configuration;
            this.ConfigurationStatus = EgmGameConfigurationStatus.None;
            this.ValidationStatus = ValidationStatus.None;
        }

        public virtual void Update(QComConfiguration<TConfiguration> configuration)
        {            
            Update(configuration.ConfigurationData);
        }

        public virtual void UpdateProtocolVersion(ProtocolVersion protocolVersion)
        {
            this._ProtocolVersion = protocolVersion;
        }



    }
}
