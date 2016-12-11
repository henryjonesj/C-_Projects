using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.Egm.Devices
{
    [GenerateICSerializable]
    public partial class CancelCreditJackpot : Jackpot
    {    

        internal override void UpdateMeters(decimal amount)
        {
            _JackpotMeter = GetMeter();
            _JackpotAmount = amount * QComCommon.MeterScaleFactor;
            _HandpayType = HandpayType.CancelledCredits;
        }

        internal override KeyOffDestination KeyOffDestination { get { return KeyOffDestination.Handpay; } }

        private Meter GetMeter()
        {
            return Model.GetMeters().GetTransferredAmount(Direction.Out, TransferDevice.HandPay, 1, CreditType.Cashable);
        }

        internal override bool IsJackpotMeterUpdated(SerializableList<MeterId> meterIdsReceived)
        {
            if (!meterIdsReceived.Contains(MeterId.CancelCredit)) return false;

            _Log.InfoFormat("old meters = {0}, new meters = {1}, jackpot amount = {2}", _JackpotMeter, GetMeter(), _JackpotAmount);
            return MeterChangedDetector.IsExpectedMeterChange(_JackpotMeter, GetMeter(), _JackpotAmount);
        }
    }
}
