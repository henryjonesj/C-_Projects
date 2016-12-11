using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.Meters
{
    public static class MeterTypeGroupMetersMapping
    {
        private static Dictionary<MeterType, MaintenanceBlockCharacteristics> _MeterTypeMap = 
            new Dictionary<MeterType, MaintenanceBlockCharacteristics>();

        static MeterTypeGroupMetersMapping()
        {
            _MeterTypeMap.Add(MeterType.Game, MaintenanceBlockCharacteristics.Group0Meters);
            _MeterTypeMap.Add(MeterType.NoteAcceptor, MaintenanceBlockCharacteristics.Group2Meters);
            _MeterTypeMap.Add(MeterType.Coins, MaintenanceBlockCharacteristics.Group1Meters);
            _MeterTypeMap.Add(MeterType.Cashless, MaintenanceBlockCharacteristics.Group1Meters);
            _MeterTypeMap.Add(MeterType.Ticket, MaintenanceBlockCharacteristics.Group0Meters);
            _MeterTypeMap.Add(MeterType.Jackpot, MaintenanceBlockCharacteristics.Group0Meters);
        }


        public static MaintenanceBlockCharacteristics GetMeterGroup(this MeterType meterType)
        {
            MaintenanceBlockCharacteristics groupMeter = MaintenanceBlockCharacteristics.Reserved;
            _MeterTypeMap.TryGetValue(meterType, out groupMeter);
            return groupMeter;
        }
    }
}
