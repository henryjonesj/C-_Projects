using System.Linq;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.QCom.Model.Meters;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using BallyTech.Utility.Configuration;
using BallyTech.QCom.Metadata;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class MeterGroupContributionValidationSpecification : QComResponseSpecification
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(MeterGroupContributionValidationSpecification));

        [AutoWire(Name = "QComModel")]
        public QComModel Model { get; set; }

        internal MeterTracker MeterTracker 
        {
            get { return Model.MeterTracker; }
        }

        public override UnreasonableMeterIncrementTestResult GetMeterValidationStatus(MeterInfo meterInfo)
        {
            var meters = MeterTracker.Meters;

            MeterCodes meterCode = meterInfo.MeterCode;

            decimal incrementThreshold = GetIncrementThreshold(meterCode, "Cabinet");
            Meter newMeter = new Meter(meterInfo.RawValue, 1, uint.MaxValue + 1m);

            if (incrementThreshold <= 0)
            {
                if (!IsExpectedMeterChange(meterInfo)) return MeterTracker.BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.CentForCentValidationFailure);
                    
                return IsBackMeterValidationSatisfied(meters[meterCode].Meter, newMeter);
            }

            if (meterCode == MeterCodes.TotalEgmCentsIn)
                return MeterTracker.EgmCentsMetersValidator.IsEgmCentsInMeterValid(incrementThreshold,
                                                                                    meters[meterCode].Meter,
                                                                                    newMeter);

            if (meterCode == MeterCodes.TotalEgmCentsOut)
                return MeterTracker.EgmCentsMetersValidator.IsEgmCentsOutMeterValid(incrementThreshold,
                                                                                    meters[meterCode].Meter,
                                                                                    newMeter);

            return MeterTracker.CheckAndBuildUnreasonableMeterIncrementTestResult(newMeter, meters[meterCode].Meter, incrementThreshold);
        }

        private bool IsExpectedMeterChange(MeterInfo meterInfo)
        {
            MeterCodes meterCode = meterInfo.MeterCode;
            Meter oldMeter = MeterTracker.Meters[meterCode].Meter;
            Meter newMeter = new Meter(meterInfo.RawValue, 1, uint.MaxValue + 1m);

            decimal meterDelta = (newMeter - oldMeter).DangerousGetSignedValue();
            if (MeterTracker.CentForCentValidators.ContainsKey(meterCode) && 
                        MeterTracker.CentForCentValidators[meterCode] != null)
            {
                _Log.InfoFormat("CentForCent Reconciliation of {0} meter; Old {1}, New {2}",
                                                                       meterCode.ToString(),
                                                                       oldMeter,
                                                                       newMeter);

                return IsTransferInProgress(meterCode) ? true : (meterDelta == 0);
            }

            return true;
        }

        private bool IsTransferInProgress(MeterCodes meterCode)
        {
            return MeterTracker.CentForCentValidators[meterCode].Any((item) => (item.IsAnyTransferInProgress == true));
        }

        private decimal GetIncrementThreshold(MeterCodes meterCode, string meterType)
        {
            decimal incrementThreshold = 0m;
            incrementThreshold = meterType.Equals("Cabinet")
                                    ? (decimal)QComMetadata.Instance.MeterDefinitions.GetIncrementThresholdCabinet(meterCode)
                                    : (decimal)QComMetadata.Instance.MeterDefinitions.GetIncrementThresholdGame(meterCode);

            return incrementThreshold;
        }

        private UnreasonableMeterIncrementTestResult IsBackMeterValidationSatisfied(Meter oldMeter, Meter newMeter)
        {
            if ((newMeter - oldMeter).DangerousGetSignedValue() >= 0) return MeterTracker.BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.Success);

            if (_Log.IsWarnEnabled) _Log.Warn("Back meter validation failed");

            return MeterTracker.BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.BackMeterValidationFailure);
          
        }

        public override bool IsSatisfiedBy(LPContribution item)
        {
            if (!IsLgvnValid(item.LastGameVersionNumber)) return false;

            if (!AreMeterGroupsValid(item.MeterGroups)) return false;

            if (!IsMeterCountValid(item)) return false;

            if( item.LinkedProgressiveAvailable)
            {
                if(Model.Egm.CurrentGame.ProgressiveGroupId != item.LPContributionData.ProgressiveGroupId.ToString())
                {
                    if(_Log.IsInfoEnabled) _Log.Info("Ignoring this meter group contribution as PGID received is invalid");
                    return false;
                }
            }

            return true;
        
        }
    
    
        public override bool IsSatisfiedBy(MeterGroupContributionResponse item)
        {
            return IsLgvnValid(item.LastGameVersionNumber) && AreMeterGroupsValid(item.MeterGroups) && IsMeterCountValid(item);
        }

        private bool IsLgvnValid(ushort gameVersionNumber)
        {

            if (gameVersionNumber == 0) return true;

            var game = Model.Egm.Games.Get(gameVersionNumber);

            if (game == null)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Ignoring this meter group contribution as an invalid version number is received");

                return false;
            }

            return true;
        
        }

        private bool AreMeterGroupsValid(SerializableList<MeterInfo> MeterGroups)
        {
            if (!(MeterGroups.All((meter) => (byte)meter.MeterCode <= 0x24)))
            {
                if (_Log.IsInfoEnabled) _Log.Info("Ignoring this meter group contribution as Meter Codes are not valid");
                return false;
            }
            return true;
        }

        private bool IsMeterCountValid(MeterGroupContributionResponse response)
        {
            if (response.MeterGroups.Count!= response.MeterCount+1)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Ignoring this meter group contribution as Meter Count don't match Flag");
                return false;
            
            }
            return true;
        
        }


        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.MeterGroupContributionResponse; }
        }
    }
}
