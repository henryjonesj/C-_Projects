using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    partial class EgmGeneralMaintenancePoll
    {

        internal EgmGeneralMaintenancePoll WithGroup(MaintenanceBlockCharacteristics group)
        {
            this.MaintenanceBlock |= group;
            return this;
        }

        internal EgmGeneralMaintenancePoll ForGameVariationNumber(byte variationNumber)
        {
            this.GameVariationNumber = variationNumber;
            return this;
        }


        internal EgmGeneralMaintenancePoll ForGameVersionNumber(ushort gameVersionNumber)
        {
            this.GameVersionNumber = gameVersionNumber;
            return this;
        }

        internal EgmGeneralMaintenancePoll EnableGame()
        {
            this.MaintenanceFlagStatus = MaintenanceFlagStatus.MachineEnableFlag;
            return this;
        }



    }
}
