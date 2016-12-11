using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Model.Meters
{
    public enum MeterValidationResult
    {
        BackMeterValidationFailure,
        CentForCentValidationFailure,
        InconistentMeterValidationFailure,
        Success
    }
    
    public class UnreasonableMeterIncrementTestResult
    {
        public MeterValidationResult _MeterValidationStatus;

        public UnreasonableMeterIncrementTestResult(MeterValidationResult meterValidationResult)
        {
            _MeterValidationStatus = meterValidationResult;
            
        }
    }
}
