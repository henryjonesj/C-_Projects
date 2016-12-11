using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    internal static class MeterIdDictionaryExtension
    {
        internal static Meter GetMeterValueFor(this SerializableDictionary<MeterId,Meter> items,MeterId meterId)
        {
            return !HasElement(items, meterId) ? Meter.NotAvailable : items[meterId];
        }

        internal static bool HasElement(this SerializableDictionary<MeterId,Meter> items,MeterId meterId)
        {
            return items.ContainsKey(meterId);
        }

    }

    internal static class MeterExtensions
    {
        internal static bool IsNonZero(this Meter meter)
        {
            return (meter.DangerousGetSignedValue() > 0m);
        }

        internal static bool IsZero(this Meter meter)
        {
            return (meter.DangerousGetSignedValue() == 0m);
        }

    }
}
