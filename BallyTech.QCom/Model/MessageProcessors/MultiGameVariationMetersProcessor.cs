using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Model.Meters;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class MultiGameVariationMetersProcessor : MessageProcessor
    {
        internal EgmAdapter Egm { get { return Model.Egm; } }
        private static readonly ILog _Log = LogManager.GetLogger(typeof(MultiGameVariationMetersProcessor));
        private QComResponseSpecification _meterMovementSpec = null;

        public override void Process(MultiGameVariationMetersResponse gameMeterResponse)
        {
            ushort version = gameMeterResponse.GameVersionNumber;
            byte variation = gameMeterResponse.GameVariationNumber;

            Model.GameMeterRequestor.MultiGameMeterResponseReceived(gameMeterResponse.GameVersionNumber);

            if (Egm.GetGameMeters(version, variation) == null)
            {
                Egm.UpdateGameMeters(version, variation, gameMeterResponse.GetMeterGroups());
            }
            else if (!AreMetersValid(gameMeterResponse))
            {
                Model.OnMeterValidationSkippedWith(EgmEvent.InconsistentGameMeters);
                return;
            }
            Model.Egm.GameMetersReceived();
        }

        private bool AreMetersValid(MultiGameVariationMetersResponse gameMeterResponse)
        {
            bool _IsMeterValidationPassed = true;
            SerializableDictionary<MeterId, Meter> currentMeterList = Egm.GetGameMeters(gameMeterResponse.GameVersionNumber,
                                                                                        gameMeterResponse.GameVariationNumber);
            SerializableDictionary<MeterId, Meter> newMeterList = gameMeterResponse.GetMeterGroups();

            _meterMovementSpec = Model.SpecificationFactory.GetSpecification(FunctionCodes.MultiGameVariationMetersResponse);

            foreach (var meter in currentMeterList.ToSerializableList())
            {
                Meter currentMeter = currentMeterList.GetMeterValueFor(meter.Key);
                Meter newMeter = newMeterList.GetMeterValueFor(meter.Key);

                if (!_meterMovementSpec.IsGameMeterValid(meter.Key, currentMeter, newMeter))
                    _IsMeterValidationPassed = false;

                Egm.UpdateGameMeter(meter.Key, newMeter, gameMeterResponse.GameVersionNumber,
                                                         gameMeterResponse.GameVariationNumber);
            }

            return _IsMeterValidationPassed;
        }
    }
}
