using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Messages
{
    partial class ProgressiveConfigurationResponse
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ProgressiveConfigurationResponse));

        internal const int LengthofNonRepeatedEntries = 13;

        internal QComResponseSpecification CustomSapValidationSpecification { get; set; }

        public bool HasLPLevels()
        {
            foreach (var egmProgressiveConfiguration in ProgressiveConfigurationList)
                if ((egmProgressiveConfiguration.ProgressiveLevelFlag & ProgressiveLevelFlag.LevelType) == ProgressiveLevelFlag.LevelType)
                    return true;

            return false;

        }

        public bool HasSupportForCustomSAP()
        {
            return HasLPLevels() || CustomSAPCapabilityFlag;
        
        }

        public bool HasValidCeilingAmount()
        {
            return CustomSapValidationSpecification != null ?
                CustomSapValidationSpecification.IsSatisfiedBy(ProgressiveConfigurationList) : true;
        
        }


        public SerializableList<ProgressiveConfigurationGroup> GetSAPLevelConfigurations()
        {
            var SapLevelConfigurations = new SerializableList<ProgressiveConfigurationGroup>();

            foreach (var egmProgressiveConfiguration in ProgressiveConfigurationList)
                if ((egmProgressiveConfiguration.ProgressiveLevelFlag & ProgressiveLevelFlag.LevelType) != ProgressiveLevelFlag.LevelType)
                    SapLevelConfigurations.Add(egmProgressiveConfiguration);

            return SapLevelConfigurations;
        }
           

        public ProgressiveLevelFlag GetProgressiveLevelNumber(ProgressiveLevelFlag ProgressiveLevelFlag)
        {
            if ((ProgressiveLevelFlag & ProgressiveLevelFlag.LevelType) == ProgressiveLevelFlag.LevelType)
                return ProgressiveLevelFlag ^ ProgressiveLevelFlag.LevelType;
            else
                return ProgressiveLevelFlag;
        }

        public ProgressiveConfigurationGroup GetEgmProgressiveLevelConfiguration(SerializableList<ProgressiveConfigurationGroup> egmProgressiveConfiguration, IProgressiveLevelConfiguration config)
        {           
            return egmProgressiveConfiguration.FirstOrDefault(
                    (element) =>
                    (element.GetLevelType(element.ProgressiveLevelFlag) == config.ProgressiveType) &&
                     ((byte)(GetProgressiveLevelNumber(element.ProgressiveLevelFlag) + 1) == config.ProgressiveLevelNumber));
        }
    
    }
}
