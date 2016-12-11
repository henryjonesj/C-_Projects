using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.QCom.Configuration;

namespace BallyTech.QCom.Model.Builders
{
    public static class ConfigurationBasedRequestBuilder
    {
        internal static EgmConfigurationRequestPoll Build(IEgmConfiguration configuration)
        {
            return new EgmConfigurationRequestPoll()
                       {
                           StatusRequestFlag = StatusRequestFlag.Reserved
                       };

        }


        private static EgmGeneralMaintenancePoll CreateEgmGeneralMaintenancePoll(int gameNumber,string payTableId,bool gameStatus)
        {
            return new EgmGeneralMaintenancePoll()
                       {
                           GameVersionNumber = Convert.ToUInt16(gameNumber),
                           GameVariationNumber = Byte.Parse(payTableId),
                           GeneralFlag = gameStatus ? GeneraFlagStatus.GameEnableFlag : GeneraFlagStatus.None
                       };
        }


        internal static EgmGeneralMaintenancePoll Build(IGameConfiguration configuration)
        {
            var generalMaintenancePoll = CreateEgmGeneralMaintenancePoll(configuration.GameNumber,
                                                                         configuration.PayTableId,
                                                                         configuration.GameStatus);

            generalMaintenancePoll.GeneralFlag |= GeneraFlagStatus.EGMGameConfigurationResponseRequestBit;

            return generalMaintenancePoll;

        }


        internal static Request Build(IGameProgressiveConfiguration configurationData)
        {
            var generalMaintenancePoll = CreateEgmGeneralMaintenancePoll(configurationData.GameNumber,
                                                                          configurationData.PayTableId,
                                                                          configurationData.GameStatus);

            generalMaintenancePoll.GeneralFlag |= GeneraFlagStatus.ProgressiveConfigurationResponseRequest;
         
            return generalMaintenancePoll;         
        }
    }
}
