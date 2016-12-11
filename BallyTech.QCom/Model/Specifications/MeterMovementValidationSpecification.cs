using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.QCom.Metadata;
using log4net;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Model.Meters;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class MeterMovementValidationSpecification : SpecificationBase<MeterInfo>
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (MeterMovementValidationSpecification));

        internal MeterTracker MeterTracker { get; set; }

        public override bool IsSatisfiedBy(MeterInfo meterInfo)
        {
            var meters = MeterTracker.Meters;

            MeterCodes meterCode = meterInfo.MeterCode;

            decimal incrementThreshold = GetIncrementThreshold(meterCode,"Cabinet");
            Meter newMeter = new Meter(meterInfo.RawValue, 1, uint.MaxValue + 1m);

            if (incrementThreshold <= 0)
                return IsBackMeterValidationSatisfied(meters[meterCode].Meter, newMeter);
            
            return ((newMeter - meters[meterCode].Meter).DangerousGetUnsignedValue() <= incrementThreshold);
        }

        public bool AreMetersValid(MeterId meterId, Meter currentMeter,Meter newMeter ,string meterType)
        {
            decimal incrementThreshold = GetIncrementThreshold(MeterCodeIdMapping.GetMeterCode(meterId), meterType);
            if (incrementThreshold <= 0)
                return IsBackMeterValidationSatisfied(currentMeter, newMeter);

            decimal meterDelta = (newMeter - currentMeter).DangerousGetUnsignedValue();
            return ( meterDelta <= incrementThreshold );
        }

        private decimal GetIncrementThreshold(MeterCodes meterCode, string meterType)
        {
            decimal incrementThreshold = 0m;
            incrementThreshold = meterType.Equals("Cabinet") 
                                    ? (decimal)QComMetadata.Instance.MeterDefinitions.GetIncrementThresholdCabinet(meterCode)
                                    : (decimal)QComMetadata.Instance.MeterDefinitions.GetIncrementThresholdGame(meterCode);
            string unit = QComMetadata.Instance.MeterDefinitions.GetUnit(meterCode);

            if (meterCode == MeterCodes.TotalEgmCentsIn && MeterTracker.Model.Egm.TransferInDevice.IsAnyTransferInProgress)
                return decimal.MaxValue;

            if (meterCode == MeterCodes.TotalEgmCentsOut && MeterTracker.Model.Egm.TransferOutDevice.IsAnyTransferInProgress)
                return decimal.MaxValue;

            return unit == "Count" ? incrementThreshold : incrementThreshold / QComCommon.MeterScaleFactor;
        }

        private bool IsBackMeterValidationSatisfied(Meter oldMeter,Meter newMeter)
        {
            if ((newMeter - oldMeter).DangerousGetSignedValue() >= 0) return true;

            if (_Log.IsWarnEnabled) _Log.Warn("Back meter validation failed");
            return false;
        }
    }
}
