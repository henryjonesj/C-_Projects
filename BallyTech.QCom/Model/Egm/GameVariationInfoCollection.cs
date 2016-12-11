using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class GameVariationInfoCollection : SerializableKeyedCollection<byte, GameVariationInfo>
    {
        protected override byte GetKeyForItem(GameVariationInfo item)
        {
            return item.VariationNumber;
        }
    }


    [GenerateICSerializable]
    public partial class GameVariationInfo
    {
        public byte VariationNumber { get; private set; }
        public decimal TheorticalPercentage { get; private set; }

        private SerializableDictionary<MeterId, Meter> _Meters = new SerializableDictionary<MeterId, Meter>();
        public SerializableDictionary<MeterId, Meter> Meters
        {
            get { return _Meters; }
        }

        public GameVariationInfo()
        {

        }

        public bool AreMetersAvailable
        {
            get { return _Meters.Count > 0; }
        }

        public GameVariationInfo(byte variation, decimal percentageReturn)
        {
            this.VariationNumber = variation;
            this.TheorticalPercentage = percentageReturn;
        }

        public void UpdateMeters(SerializableDictionary<MeterId, Meter> meters)
        {
            _Meters = meters;
        }

        public void UpdateMeter(MeterId meterId, Meter meter)
        {
            _Meters[meterId] = meter;
        }

        public void ResetMeters()
        {
            foreach (var meter in _Meters.ToSerializableList())
            {
                var meterId = meter.Key;
                var meterValue = meter.Value;

                var updatedMeter = new Meter(0, meterValue.Unit, meterValue.Modulus);
                UpdateMeter(meterId, updatedMeter);
            }
        }

    }



}
