using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Egm
{    
    public static class CreditMeterCalculator
    {

        internal static Meter Calculate(EgmAdapter egmAdapter)
        {
            var egmMeterData = egmAdapter.MeterRepository;

            var inMeter = egmMeterData.GetMeterValueFor(MeterId.Wins) +
                          egmMeterData.GetMeterValueFor(MeterId.CentsIn) +
                          egmMeterData.GetMeterValueOrZeroFor(MeterId.TicketIn);

            var outMeter = egmMeterData.GetMeterValueFor(MeterId.Bets) +
                           egmMeterData.GetMeterValueFor(MeterId.CancelCredit) +
                           egmMeterData.GetMeterValueFor(MeterId.CentsOut) +
                           egmMeterData.GetMeterValueOrZeroFor(MeterId.TicketOut);

            var creditMeter = inMeter - outMeter;

            creditMeter *= MeterService.MeterScaleFactor;

            return (creditMeter.DangerousGetSignedValue() < 0) ? Meter.Zero : creditMeter;
        }
    }
}
