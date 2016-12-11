using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Configuration
{
    public static class EgmConfigurationExtension
    {
        private static int _maximumDoubleUpAttempts = 16;

        public static int MaximumDoubleUpAttemps
        {
            get { return _maximumDoubleUpAttempts; }
            set { _maximumDoubleUpAttempts = value; }
        }

        
        public static QComConfigurationId CreateId(this IEgmConfiguration egmConfiguration)
        {
            return new QComConfigurationId(FunctionCodes.EgmConfiguration, 0);
        }

        public static QComEgmConfiguration Create(this IEgmConfiguration egmConfiguration)
        {
            return new QComEgmConfiguration(egmConfiguration);
        }

        public static bool IsInAcceptableFormat(this IEgmConfiguration egmconfiguration, out string errorReason)
        {
            errorReason = null;

            if (!egmconfiguration.SerialNumber.IsNumeric())
                errorReason = "SER Non-Numeric";

            if (!egmconfiguration.ManufacturerId.IsNumeric())
                errorReason = "MID Non-Numeric";

            if (egmconfiguration.MaxDoubleUpAttempts > _maximumDoubleUpAttempts)
                errorReason = "DoubleUpAttemptError";

            return errorReason == null ? true : false;

        }

        public static bool IsGamesAvailable(this IEgmConfiguration egmConfiguration)
        {
            return egmConfiguration.GameConfigurations != null ? egmConfiguration.GameConfigurations.Count() != 0 : false;

        }

    }


   
}
