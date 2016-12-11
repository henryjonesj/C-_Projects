using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Utility;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm.Devices
{
    [GenerateICSerializable]
    public partial class LargeWin : Jackpot
    {

        internal override void UpdateMeters(decimal amount) 
        {
            _JackpotMeter = GetMeter();
            _JackpotAmount = amount * QComCommon.MeterScaleFactor;
            _HandpayType = HandpayType.LargeWin;
        }

        internal override KeyOffDestination KeyOffDestination { get { return KeyOffDestination.Credit;}}

        private Meter GetMeter()
        {
            return Model.GetMeters().GetWonAmount(null, null, null, null, WinPaymentMethod.EgmPaid);
        }

        internal override bool IsJackpotMeterUpdated(SerializableList<MeterId> meterIdsReceived) 
        {
            if (!meterIdsReceived.Contains(MeterId.Wins)) return false;

            _Log.InfoFormat("old meters = {0}, new meters = {1}, jackpot amount = {2}", _JackpotMeter, GetMeter(), _JackpotAmount);
            return MeterChangedDetector.IsExpectedMeterChange(_JackpotMeter, GetMeter(), _JackpotAmount);
        }
    }
}
