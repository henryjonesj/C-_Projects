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

        internal void UpdateMeters(SerializableDictionary<MeterId, Meter> meters)
        {
            meters.ForEach((meterinfo) => UpdateMeter(meterinfo.Key, meterinfo.Value));
        }

        private void UpdateMeter(MeterId meterId, Meter meter)
        {
            _EgmMeters[meterId] = meter;
        }


        internal Meter GetMeterValueFor(MeterId meterId)
        {
            return _EgmMeters.GetMeterValueFor(meterId);
        }




    }
}
