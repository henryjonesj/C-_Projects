using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model
{   
    [GenerateICSerializable]
    public partial class ProgressiveConfigurationRequestSpecification :SpecificationBase<EgmGameConfigurationResponse>
    {
        QComModel _Model = null;

        public ProgressiveConfigurationRequestSpecification()
        {

        }

        public ProgressiveConfigurationRequestSpecification(QComModel model)
        {
            _Model = model;
        }


        public override bool IsSatisfiedBy(EgmGameConfigurationResponse item)
        {
            if (!DoesGameHaveProgressiveLevelsAndSupportsProgressiveConfiguration(item)) return false;

            return ShouldRequestProgressiveConfiguration();
        }


        private bool DoesGameHaveProgressiveLevelsAndSupportsProgressiveConfiguration(EgmGameConfigurationResponse item)
        {
            return item.NoOfProgressiveLevels != 0 && _Model.ProtocolVersion == ProtocolVersion.V16;
        }

        private bool ShouldRequestProgressiveConfiguration()
        {
            return _Model.ConfigurationRepository.CurrentEgmConfiguration.IsSharedProgressiveComponentSupported ?
                _Model.ConfigurationRepository.RequestProgressiveConfiguration : true;
        }

    }
}
