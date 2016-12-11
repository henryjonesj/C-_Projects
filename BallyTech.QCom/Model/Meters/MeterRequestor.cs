using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Meters
{
    public static class MeterRequestor
    {
        private static EgmGeneralMaintenancePoll _MaintenancePoll = null;


        internal static EgmGeneralMaintenancePoll Instance
        {
            get { return _MaintenancePoll = new EgmGeneralMaintenancePoll(); }
        }
    
        internal static EgmGeneralMaintenancePoll RequestAllMeters()
        {
          return _MaintenancePoll.WithGroup(MaintenanceBlockCharacteristics.Group0Meters).WithGroup(
                MaintenanceBlockCharacteristics.Group1Meters).WithGroup(MaintenanceBlockCharacteristics.Group2Meters);
        }

        internal static EgmGeneralMaintenancePoll RequestGroup0Meters()
        {
            return _MaintenancePoll.WithGroup(MaintenanceBlockCharacteristics.Group0Meters);
        }

        internal static EgmGeneralMaintenancePoll RequestGroup1Meters()
        {
            return _MaintenancePoll.WithGroup(MaintenanceBlockCharacteristics.Group1Meters);
        }

        internal static EgmGeneralMaintenancePoll RequestGroup2Meters()
        {
            return _MaintenancePoll.WithGroup(MaintenanceBlockCharacteristics.Group2Meters);
        }

    }
}
