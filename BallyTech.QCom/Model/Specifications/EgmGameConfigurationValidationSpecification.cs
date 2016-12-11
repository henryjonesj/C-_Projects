using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using log4net;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class EgmGameConfigurationValidationSpecification : QComResponseSpecification
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(EgmGameConfigurationValidationSpecification));

        public override bool IsSatisfiedBy(EgmGameConfigurationResponse response)
        {
            _Log.Info("Validating Egm Game Configuration");
            
            if (!response.IsNumberOfVariationAvailableValid)
            {
                _Log.InfoFormat("Ignoring this response as Number of Variations received is Invalid");
                
                return false;
            }

            var TotalSize = response.Size * response.NoOfVariationAvailable;

            var ActualSize = Convert.ToInt32(response.TotalLength) - EgmGameConfigurationResponse.LengthofNonRepeatedEntries;

            if (response.Size < 3 || TotalSize < ActualSize)
            {
                _Log.InfoFormat("Ignoring this response as SIZ received is Invalid");

                return false;
            
            }

            return true;
        }

        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.EgmGameConfigurationResponse; }
        }
    
    
    }
}
