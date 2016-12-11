using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class MeterRepository
    {
        private SerializableDictionary<MeterId, Meter> _EgmMeters = new SerializableDictionary<MeterId, Meter>();


        internal SerializableDictionary<MeterId, Meter> EgmMeters
        {
            get { return _EgmMeters; }
        }

        internal Meter GetMeterValueFor(MeterId meterId)
        {
            return _EgmMeters.GetMeterValueFor(meterId);
        }

        internal Meter GetMeterValueOrZeroFor(MeterId meterId)
        {
            var meterValue = _EgmMeters.GetMeterValueFor(meterId);
            return meterValue == Meter.NotAvailable ? Meter.Zero : meterValue;
        }

        internal void UpdateMeters(SerializableDictionary<MeterId, Meter> meters)
        {
            meters.ForEach((meterinfo) => UpdateMeter(meterinfo.Key, meterinfo.Value));
        }

        private void UpdateMeter(MeterId meterId, Meter meter)
        {
            _EgmMeters[meterId] = meter;
        }

        internal void Reset()
        {
            foreach (var egmMeter in EgmMeters.ToSerializableList())
            {
                var meterId = egmMeter.Key;
                var meterValue = egmMeter.Value;

                var updatedMeter = new Meter(0, meterValue.Unit, meterValue.Modulus);
                UpdateMeter(meterId,updatedMeter);
            }
        }

        internal bool ValidMetersAvailable
        {
            get { return _EgmMeters.Any((meter) => meter.Value.IsNonZero()); }
        }

    }


}
