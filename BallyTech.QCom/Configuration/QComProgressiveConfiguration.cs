using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model;
using BallyTech.QCom.Model.Builders;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComProgressiveConfiguration : QComConfiguration<IGameProgressiveConfiguration>, IEquatable<ProgressiveConfigurationResponse>
    {        
        public bool HasSupportForCustomSAP = false;

        public bool CanValidateSAPParamters = false;

        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComProgressiveConfiguration));

        private ProgressiveConfigurationEqualityComparer ProgressiveConfigurationEqualityComparer = null;

        public QComProgressiveConfiguration()
        {
            
        }

        public QComProgressiveConfiguration(IGameProgressiveConfiguration gameProgressiveConfiguration)
        {
            ConfigurationData = gameProgressiveConfiguration;
            this.Id = new QComConfigurationId(FunctionCodes.ProgressiveConfiguration, gameProgressiveConfiguration.GameNumber);
        }

        internal override void Update(IGameProgressiveConfiguration configuration)
        {
            if (this.ConfigurationData.AreEqual(configuration))
            {
                if (_Log.IsInfoEnabled) _Log.Info("No change in configuration");
                return;
            }

            base.Update(configuration);
        }


        public override Request ConfigurationPoll
        {
            get
            {
                if (_ProtocolVersion == ProtocolVersion.V16 && HasSupportForCustomSAP)
                {
                    this.CanValidateSAPParamters = true;
                    return QComConfigurationBuilder.Build(this);
                }
                return null;
            }
        }


        public override Request ConfigurationRequest
        {
            get { return ConfigurationBasedRequestBuilder.Build(ConfigurationData); }
        }


        private void UpdateValidationStatus(ProgressiveConfigurationResponse response,bool isProgressiveConfigurationEqual)
        {
            if (!response.HasSupportForCustomSAP())
                this.ValidationStatus = isProgressiveConfigurationEqual ? ValidationStatus.Success : ValidationStatus.Failure;
            else
            {
                if (CanValidateSAPParamters)
                    this.ValidationStatus = isProgressiveConfigurationEqual ? ValidationStatus.Success : ValidationStatus.Failure;
                else
                    this.ValidationStatus = ValidationStatus.None;
            }

            if (this.ValidationStatus == ValidationStatus.None) return;

            if (!isProgressiveConfigurationEqual)
                _Log.WarnFormat("Host Configuration {0} mismatches with the progressive configuration {1}", this.ConfigurationData, response);

            this.UpdateConfigurationStatus(isProgressiveConfigurationEqual ? EgmGameConfigurationStatus.Success : EgmGameConfigurationStatus.Failure);

        
        }

        public override void ResetValidationStatus()
        {
            base.ResetValidationStatus();
            if (this.HasSupportForCustomSAP) ResetConfigurationStatus();

        }

        public void ResetConfigurationStatus()
        {
            this.ConfigurationStatus = EgmGameConfigurationStatus.None;
            this.CanValidateSAPParamters = false;
        }


        #region IEquatable<ProgressiveConfigurationResponse> Members
   

        public bool Equals(ProgressiveConfigurationResponse response)
        {
            ProgressiveConfigurationEqualityComparer= new ProgressiveConfigurationEqualityComparer(ConfigurationData,CanValidateSAPParamters);

            var areEqual = ProgressiveConfigurationEqualityComparer.Compare(response) && response.HasValidCeilingAmount();

            UpdateValidationStatus(response, areEqual);

            return areEqual;

        }

        #endregion
    }
}
