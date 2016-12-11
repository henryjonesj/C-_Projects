using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Model.Meters;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Builders
{
    internal static class MeterRequestBuilder
    {

        internal static EgmGeneralMaintenancePoll BuildMeterGroupRequest(bool machineEnabled, Game currentGame)
        {
            var gameVersionNumber = currentGame != null ? currentGame.GameNumber : 0;
            var gameVariationNumber = currentGame != null ? currentGame.CurrentGameVariation : 0;
            
            var generalMaintenancePoll = new EgmGeneralMaintenancePoll()
                       {
                           MaintenanceBlock =
                               MaintenanceBlockCharacteristics.Group0Meters |
                               MaintenanceBlockCharacteristics.Group1Meters |
                               MaintenanceBlockCharacteristics.Group2Meters |
                               MaintenanceBlockCharacteristics.Reserved,
                           GeneralFlag = (currentGame.Enabled == true ? GeneraFlagStatus.GameEnableFlag : GeneraFlagStatus.None)
                                            | GeneraFlagStatus.MultiGameVariationMetersResponseRequest,
                           GameVersionNumber = (ushort)gameVersionNumber,
                           GameVariationNumber =(byte)gameVariationNumber,
                       };

            if (machineEnabled)
                generalMaintenancePoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;

            return generalMaintenancePoll;

        }


        internal static EgmGeneralMaintenancePoll RequestFor(MeterType[] meterTypes, Game currentGame)
        {
            var meterPoll = new EgmGeneralMaintenancePoll()
                                {
                                    GameVersionNumber = (ushort)currentGame.GameNumber,
                                    GameVariationNumber = currentGame.CurrentGameVariation
                                };


            meterTypes.ForEach((meterType) => meterPoll.MaintenanceBlock |= meterType.GetMeterGroup());
            meterPoll.MaintenanceBlock |= MaintenanceBlockCharacteristics.Reserved;
            meterPoll.GeneralFlag = currentGame.Enabled == true ? GeneraFlagStatus.GameEnableFlag : GeneraFlagStatus.None;

            return meterPoll;
        }

        internal static EgmGeneralMaintenancePoll RequestForAllGameMeters(int gameVersion, byte gameVariation)
        {
            var meterPoll = new EgmGeneralMaintenancePoll()
                                {
                                    GameVersionNumber = (ushort)gameVersion,
                                    GameVariationNumber = gameVariation,
                                    GeneralFlag = GeneraFlagStatus.MultiGameVariationMetersResponseRequest |
                                                  GeneraFlagStatus.ProgressiveMetersResponseRequest1 |
                                                  GeneraFlagStatus.ProgressiveMetersResponseRequest2
                                };

            return meterPoll;
        }

        internal static EgmGeneralMaintenancePoll RequestForProgressiveMeters(bool machineEnabled, Game game)
        {
            var meterPoll = new EgmGeneralMaintenancePoll()
            {
                GameVersionNumber = (ushort)game.VersionNumber,
                GameVariationNumber = game.CurrentGameVariation,
                GeneralFlag = (game.Enabled ? GeneraFlagStatus.GameEnableFlag : GeneraFlagStatus.None) 
                            | GeneraFlagStatus.ProgressiveMetersResponseRequest2 | GeneraFlagStatus.ProgressiveMetersResponseRequest1
            };

            if (machineEnabled)
                meterPoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;

            return meterPoll;
        }

        internal static EgmGeneralMaintenancePoll RequestForMultiGameMeters(bool machineEnabled, Game game)
        {
            var meterPoll = new EgmGeneralMaintenancePoll()
            {
                GameVersionNumber = (ushort)game.VersionNumber,
                GameVariationNumber = game.CurrentGameVariation,
                GeneralFlag = (game.Enabled ? GeneraFlagStatus.GameEnableFlag : GeneraFlagStatus.None)
                            | GeneraFlagStatus.MultiGameVariationMetersResponseRequest
            };

            if (machineEnabled)
                meterPoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;

            return meterPoll;
        }

        internal static EgmGeneralMaintenancePoll RequestAllMeters(bool machineEnabled,Game currentGame)
        {
            var meterPoll = BuildMeterGroupRequest(machineEnabled, currentGame);
            return meterPoll;
        }
    }
}
