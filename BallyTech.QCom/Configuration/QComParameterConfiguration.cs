using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Builders;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComParameterConfiguration : QComConfiguration<IEgmConfiguration>, IEquatable<IEgmConfiguration>
    {

        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComParameterConfiguration));


        internal bool IsCreditLimitSupported { get; set; }

        public QComParameterConfiguration()
        {

        }

        public QComParameterConfiguration(IEgmConfiguration configuration): base()
        {
            this.ConfigurationData = configuration;
            this.Id = new QComConfigurationId(FunctionCodes.EgmParameter, 0);
            this.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);
        }

        public override void ResetValidationStatus()
        {
            
        }

        internal override void Update(IEgmConfiguration configuration)
        {
            this.ConfigurationData = configuration;
            this.UpdateConfigurationStatus(EgmGameConfigurationStatus.Success);            
        }


        public override Request ConfigurationPoll
        {
            get
            {
                return _ProtocolVersion == ProtocolVersion.V16
                                  ? QComConfigurationBuilder.Build(this)
                                  : QComV15ConfigurationBuilder.Build(this);
            }
        }


        public override Request ConfigurationRequest
        {
            get { return ConfigurationBasedRequestBuilder.Build(ConfigurationData); }
        }


        

        #region IEquatable<IEgmConfiguration> Members

        bool IEquatable<IEgmConfiguration>.Equals(IEgmConfiguration other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
