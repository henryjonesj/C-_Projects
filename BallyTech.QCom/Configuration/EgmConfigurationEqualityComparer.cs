using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Configuration
{
    public static class EgmConfigurationEqualityComparer
    {

        private static readonly ILog _Log = LogManager.GetLogger(typeof(EgmConfigurationEqualityComparer));

        public static bool AreEqual(this IEgmConfiguration egmConfiguration, IEgmConfiguration newConfiguration)
        {
            return AreEgmConfigurationsEqual(egmConfiguration, newConfiguration)
                       && egmConfiguration.GameConfigurations.Count() == newConfiguration.GameConfigurations.Count();
        }

        public static bool AreEqual(this IEgmConfiguration egmConfiguration, IQComEgmConfiguration newConfiguration)
        {
            return AreEgmConfigurationsEqual(egmConfiguration, newConfiguration)
                       && AreNumberOfGamesAvailableEqual(egmConfiguration, newConfiguration);
        }

        public static bool AreEgmConfigurationsEqual(this IEgmConfiguration egmConfiguration, IEgmConfiguration newConfiguration)
        {
            return (newConfiguration.MaxDenomination == decimal.MinusOne)
                       ? IsBasicConfigurationValidationSuccessful(egmConfiguration, newConfiguration)
                       : AreConfigurationEqual(egmConfiguration, newConfiguration);
        
        }


        private static bool IsBasicConfigurationValidationSuccessful(IEgmConfiguration configurationData, IEgmConfiguration other)
        {
            return configurationData.CreditDenomination == other.CreditDenomination &&
                   decimal.Parse(configurationData.SerialNumber) == decimal.Parse(other.SerialNumber) &&
                   configurationData.TokenDenomination == other.TokenDenomination &&
                   byte.Parse(configurationData.ManufacturerId) == byte.Parse(other.ManufacturerId);
        }

        private static bool AreNumberOfGamesAvailableEqual(IEgmConfiguration configurationData, IQComEgmConfiguration other)
        {
            return configurationData.GameConfigurations.Count() == other.TotalNumberOfGames;
        
        }

        private static bool AreConfigurationEqual(IEgmConfiguration configurationData, IEgmConfiguration other)
        {
            var areEqual = IsBasicConfigurationValidationSuccessful(configurationData, other) &&
                           configurationData.MaxBet == other.MaxBet &&
                           configurationData.MaxDenomination == other.MaxDenomination &&
                           configurationData.MaxElectronicCreditTransferLimit == other.MaxElectronicCreditTransferLimit &&
                           configurationData.MaxLines == other.MaxLines &&
                           configurationData.MinTheoreticalPercent == other.MinTheoreticalPercent &&
                           configurationData.MaxTheoreticalPercent == other.MaxTheoreticalPercent &&
                           configurationData.MaxStandardDeviation == other.MaxStandardDeviation &&
                           configurationData.MaxNonProgressiveWinAmount == other.MaxNonProgressiveWinAmount &&
                           configurationData.MaxProgressiveWinAmount == other.MaxProgressiveWinAmount;


            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Egm Configuration V1.6 Validation Status: {0}", areEqual);

            return areEqual;
        }

    }
}
