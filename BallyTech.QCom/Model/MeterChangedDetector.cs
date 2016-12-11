using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model
{
    public static class MeterChangedDetector
    {
        public static bool IsMeterChanged(Meter oldMeter, Meter newMeter)
        {
            if (oldMeter.IsNotAvailable || newMeter.IsNotAvailable) return false;

            return ((newMeter - oldMeter).DangerousGetUnsignedValue() > 0);

        }

        public static bool IsExpectedMeterChange(Meter oldMeter, Meter newMeter, decimal expectedDifference)
        {
            if (oldMeter.IsNotAvailable || newMeter.IsNotAvailable) return false;

            return ((newMeter - oldMeter).DangerousGetSignedValue() == expectedDifference);
        }


        public static bool IsMeterResetToZero(Meter oldMeter, Meter newMeter)
        {
            return (!newMeter.IsNonZero()) && oldMeter.IsNonZero();
        }
    }
}
