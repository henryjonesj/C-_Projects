using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model
{
    public static class ConfigurationCompletionSpecification
    {
        internal  static bool IsSatisfiedBy(EgmConfigurationResponse response)
        {
            return (response.CreditDenomination > 0 && response.TokenDenomination > 0);
        }

        internal static bool IsSatisfiedBy(EgmGameConfigurationResponse response)
        {
            return (response.GameVersionNumber > 0 && response.CurrentGameVariationNumber > 0);
        }

        internal static bool IsSatisfiedBy(ProgressiveConfigurationResponse response)
        {
            return (response.NumberOfProgressiveLevels > 0);
            
        }

    }
}
