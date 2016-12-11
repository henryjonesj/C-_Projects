using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using BallyTech.QCom.Metadata;
using BallyTech.QCom.Model.Meters;
using log4net;
using BallyTech.Utility.Configuration;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class GameMeterValidationSpecification : QComResponseSpecification
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(GameMeterValidationSpecification));

        [AutoWire(Name = "QComModel")]
        public QComModel Model { get; set; }

        public override bool IsGameInfoValid(int gameVersion, byte gameVariation)
        {
            return Model.Egm.Games.Contains(gameVersion) &&
                   Model.Egm.Games[gameVersion].IsGameVariationAvailable(gameVariation);
        }

        public override bool IsGameMeterValid(MeterId meterId, Meter currentMeter, Meter newMeter)
        {
            decimal incrementThreshold = GetIncrementThreshold(MeterCodeIdMapping.GetMeterCode(meterId), "Game");

            _Log.InfoFormat("Validating GameMeter {0}, Old {1}, New {2}, IncThreshold {3}",
                                                      meterId.ToString(),
                                                      currentMeter.DangerousGetUnsignedValue(),
                                                      newMeter.DangerousGetUnsignedValue(),
                                                      incrementThreshold);
            
            if (incrementThreshold <= 0)
                return IsBackMeterValidationSatisfied(currentMeter, newMeter);

            decimal meterDelta = (newMeter - currentMeter).DangerousGetUnsignedValue();
            return (meterDelta <= incrementThreshold);
        }

        private decimal GetIncrementThreshold(MeterCodes meterCode, string meterType)
        {
            decimal incrementThreshold = 0m;
            incrementThreshold = meterType.Equals("Cabinet")
                                    ? (decimal)QComMetadata.Instance.MeterDefinitions.GetIncrementThresholdCabinet(meterCode)
                                    : (decimal)QComMetadata.Instance.MeterDefinitions.GetIncrementThresholdGame(meterCode);

            return incrementThreshold;
        }

        private bool IsBackMeterValidationSatisfied(Meter oldMeter, Meter newMeter)
        {
            if ((newMeter - oldMeter).DangerousGetSignedValue() >= 0) return true;

            if (_Log.IsWarnEnabled) _Log.Warn("Back meter validation failed");
            return false;
        }

        public override BallyTech.QCom.Messages.FunctionCodes FunctionCode
        {
            get { return FunctionCodes.MultiGameVariationMetersResponse; }
        }
    }
}
