using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Configuration;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Builders
{
    public static class QComV15ConfigurationBuilder
    {
        internal static EgmConfigurationV15Poll Build(QComEgmConfiguration egmConfiguration)
        {
            var configurationData = egmConfiguration.ConfigurationData;

            return new EgmConfigurationV15Poll
                       {
                           SerialNumber = Decimal.Parse(configurationData.SerialNumber),
                           ManufacturerId = configurationData.ManufacturerId.GetByteOrDefault(),
                           OldCreditDenomination = configurationData.CreditDenomination,
                           OldTokenDenomination = configurationData.TokenDenomination,
                           Jurisdiction =
                               (JurisdictionCharacteristics)
                               Enum.Parse(typeof(JurisdictionCharacteristics),
                                          configurationData.Jurisdiction.ToString(), true),
                       };

        }


        internal static EgmParametersV15Poll Build(QComParameterConfiguration egmConfiguration)
        {
            var configurationData = egmConfiguration.ConfigurationData;

            return new EgmParametersV15Poll()
                       {
                           LargeWinLockUpThreshold = configurationData.LargeWinThresholdAmount,
                           CreditInLockOut = configurationData.CreditInLockoutValue,
                           MaxAllowableDoubleUpLimit = (byte)configurationData.MaxDoubleUpAttempts,
                           DoubleUpLimit = configurationData.MaxDoubleUpLimit,
                           OperatorId = configurationData.SystemID
                       };
        }

    }
}
