using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Configuration
{
    public static class QComConfigurationExtension
    {
        /// <summary>
        /// This method is used to check whether the message received is the expected response for the configuration for the successful configuration
        ///     Configuration               Response
        /// EgmConfiguration            EgmConfigurationResponse
        /// EgmGameConfiguration        EgmGameConfigurationResponse (GVN & VAR should match)
        /// EgmParameterPoll            EgmConfigurationResponse
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="applicationMessage"></param>
        /// <returns></returns>
        public static bool IsExpectedSuccessfulResponse(this IQComConfiguration configuration, ApplicationMessage applicationMessage)
        {
            return configuration.Id.ConfigurationType == FunctionCodes.EgmParameter
                       ? applicationMessage is EgmConfigurationResponse
                       : configuration.Id.Equals(applicationMessage.ConfigurationId);
        }



        /// <summary>
        /// This method is used to check whether the message received is the expected response for the configuration for the failed configuration
        ///     Configuration               Response
        /// EgmConfiguration            EgmConfigurationResponse
        /// EgmGameConfiguration        EgmGameConfigurationResponse (GVN & VAR need not match, since variation hot switching would have failed)
        /// EgmParameterPoll            EgmConfigurationResponse
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="applicationMessage"></param>
        /// <returns></returns>

        public static bool IsExpectedFailureResponse(this IQComConfiguration configuration, ApplicationMessage applicationMessage)
        {
            return configuration.Id.ConfigurationType == FunctionCodes.EgmGameConfiguration
                       ? applicationMessage is EgmGameConfigurationResponse
                       : configuration.Id.ConfigurationType == FunctionCodes.EgmParameter
                             ? applicationMessage is EgmConfigurationResponse
                             : configuration.Id.Equals(applicationMessage.ConfigurationId);
        }
    }
}
