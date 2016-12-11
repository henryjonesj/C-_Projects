using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class PlayOutsideLicensedHoursHandler
    {
        internal QComModel Model { get; set; }

        private Meter GetUpdatedBetMeter()
        {
            return Model.MeterTracker.GetMeterFor(MeterCodes.TotalEgmTurnover);
        }

        private bool IsBetMeterChangeDetected(Meter updatedValue)
        {
            return MeterChangedDetector.IsMeterChanged(GetUpdatedBetMeter(), updatedValue);
        
        }


        public void HandlePlayOutsideLicensedHours(SerializableList<MeterInfo> meters)
        {
            if (Model.IsListeningMode) return;

            if (Model.IsSiteEnabled) return;
            
            var MeterInfo = meters.Find(element => element.MeterCode == MeterCodes.TotalEgmTurnover);

            if (MeterInfo == null) return;

            if (IsBetMeterChangeDetected(new Meter((decimal)MeterInfo.RawValue)))
            {
                Model.Egm.ResetExtendedEventData();
                Model.Egm.ReportEvent(EgmEvent.PlayableOutsideLicensedHours);
            }

        
        }
    
    }
}
