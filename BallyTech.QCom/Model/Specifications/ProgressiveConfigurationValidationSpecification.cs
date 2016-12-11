using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.Utility.Configuration;
using BallyTech.QCom.Configuration;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class ProgressiveConfigurationValidationSpecification: QComResponseSpecification
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(ProgressiveConfigurationValidationSpecification));

        [AutoWire(Name = "QComModel")]
        public QComModel Model { get; set; }
        
        public override bool IsSatisfiedBy(ProgressiveConfigurationResponse response)
        {
            _Log.InfoFormat("Validating Progressive Configuration Response");
            
            var TotalSize = response.ProgressiveConfigurationSize * response.ProgressiveConfigurationNumber;

            var ActualSize = Convert.ToInt32(response.TotalLength) - ProgressiveConfigurationResponse.LengthofNonRepeatedEntries;
            
            if (response.ProgressiveConfigurationSize < 17 || TotalSize < ActualSize )
            {
                _Log.InfoFormat("Ignoring this response as SIZ received is Invalid");

                return false;

            }

            return true;
        }

        public override bool IsSatisfiedBy(SerializableList<ProgressiveConfigurationGroup> ProgressiveConfiguration)
        {
            _Log.InfoFormat("Validating Ceiling Amount");

             var hostConfiguration = Model.ConfigurationRepository.GetConfigurationOfType<QComEgmConfiguration>();

            if (hostConfiguration == null) return true;

            var SapConfiguration = ProgressiveConfiguration.FindAll(element => (element.ProgressiveLevelFlag & ProgressiveLevelFlag.LevelType)                                                                                                != ProgressiveLevelFlag.LevelType);

            if (SapConfiguration == null || SapConfiguration.Count == 0) return true;
            
            IEgmConfiguration egmConfig = hostConfiguration.ConfigurationData;

            return egmConfig != null ?
                SapConfiguration.All((element) => element.CeilingAmount <= egmConfig.MaxProgressiveWinAmount) : true;
        }

        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.ProgressiveConfigurationResponse; }
        }
    
    }
}
