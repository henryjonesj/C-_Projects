using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Meters
{
    [GenerateICSerializable]
    public partial class MeterGroupKeyedCollection : SerializableKeyedCollection<MeterCodes,MeterGroup>
    {
        protected override MeterCodes GetKeyForItem(MeterGroup item)
        {
            return item.MeterCode;
        }

        public Meter GetMeter(MeterCodes meterCode)
        {
            
            if (Contains(meterCode)) return this[meterCode].Meter;
            return Meter.Zero;
        }

        public bool IsAvailable(MeterCodes meterCode)
        {
            return this.Contains(meterCode);
        }
    }
}
