using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class MessageBuilder
    {
        public static EgmGeneralMaintenancePoll BuildEgmEnableStatusChangeMessage(bool status, Game currentGame)
        {
            var generalMaintenancePoll = new EgmGeneralMaintenancePoll() 
                                                {
                                                    GameVersionNumber = (ushort)currentGame.GameNumber,
                                                    GameVariationNumber = currentGame.CurrentGameVariation
                                                };

            if (status) generalMaintenancePoll.MaintenanceFlagStatus = MaintenanceFlagStatus.MachineEnableFlag;

            if (currentGame.Enabled) generalMaintenancePoll.GeneralFlag |= GeneraFlagStatus.GameEnableFlag;
            return generalMaintenancePoll;
        }

        public static EgmGeneralMaintenancePoll BuildNoteAcceptorStatusRequestMessage(bool status, Game currentGame)
        {
            var generalMaintenancePoll = new EgmGeneralMaintenancePoll()
                                                {
                                                    GameVersionNumber = (ushort)currentGame.GameNumber,
                                                    GameVariationNumber = currentGame.CurrentGameVariation
                                                };

            if (status) generalMaintenancePoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.MachineEnableFlag;
            if (currentGame.Enabled) generalMaintenancePoll.GeneralFlag |= GeneraFlagStatus.GameEnableFlag;
            generalMaintenancePoll.MaintenanceFlagStatus |= MaintenanceFlagStatus.NoteAcceptorStatus;

            return generalMaintenancePoll;
        }
    }
}
