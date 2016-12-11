using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using BallyTech.QCom.Model.Meters;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class MeterResetSpecification : SpecificationBase<SerializableList<MeterInfo>>
    {
        private SerializableList<MeterCodes> _CoinInMeterCodes = null;

        internal MeterTracker MeterTracker { get; set; }


        public MeterResetSpecification()
        {
            _CoinInMeterCodes = new SerializableList<MeterCodes>()
                                    {
                                        {MeterCodes.TotalEgmWins},
                                        {MeterCodes.TotalEgmTurnover},
                                        {MeterCodes.TotalEgmStroke}
                                    };
        }



        public override bool IsSatisfiedBy(SerializableList<MeterInfo> item)
        {
            if (!(AreCoinInMetersAvailable(item))) return false;

            var coinMeters = GetCoinMeters(item);
            
            if (coinMeters.Count() <= 0) return false;

            return
                coinMeters.All(
                    (element) =>
                    MeterChangedDetector.IsMeterResetToZero(MeterTracker.GetMeterFor(element.MeterCode),element.RawValue));

        }


        private bool AreCoinInMetersAvailable(SerializableList<MeterInfo> item)
        {       
            return _CoinInMeterCodes.All(coinInMeterCode => item.Exists((element) => element.MeterCode == coinInMeterCode));
        }


        private IEnumerable<MeterInfo> GetCoinMeters(IEnumerable<MeterInfo> meterInfos)
        {
            var meters = (from meterInfo in meterInfos
                          where _CoinInMeterCodes.Contains(meterInfo.MeterCode) &&
                                MeterTracker.GetMeterFor(meterInfo.MeterCode).IsNonZero()
                          select meterInfo
                         ).ToSerializableList();

            return meters;
        }

    }
}
