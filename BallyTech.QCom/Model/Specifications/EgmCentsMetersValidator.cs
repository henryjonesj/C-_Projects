using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;
using BallyTech.QCom.Model.Meters;
using log4net;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class EgmCentsMetersValidator
    {
        internal MeterTracker MeterTracker { get; set; }

        internal UnreasonableMeterIncrementTestResult IsEgmCentsInMeterValid(decimal incrementThreshold, Meter oldMeter, Meter newMeter)
        {
            if (MeterTracker.Egm.IsEctToEgmInProgress) return MeterTracker.BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.Success);

            return MeterTracker.CheckAndBuildUnreasonableMeterIncrementTestResult(newMeter, oldMeter, incrementThreshold);
        }

        internal UnreasonableMeterIncrementTestResult IsEgmCentsOutMeterValid(decimal incrementThreshold, Meter oldMeter, Meter newMeter)
        {
            if (MeterTracker.Egm.IsEctFromEgmInProgress)
            {
                MeterTracker.Egm.IsEctFromEgmInProgress = false;
                return MeterTracker.BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.Success);
            }
            return MeterTracker.CheckAndBuildUnreasonableMeterIncrementTestResult(newMeter, oldMeter, incrementThreshold);
        }
    }
}
