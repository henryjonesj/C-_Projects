using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm.Devices
{
    [GenerateICSerializable]
    public partial class ResidualCancelCreditJackpot : Jackpot
    {   
        private KeyOffDestination _KeyOffDestination = KeyOffDestination.Handpay;
        internal override KeyOffDestination KeyOffDestination 
        { 
            get { return _KeyOffDestination;  }
        }

        internal override void UpdateMeters(decimal amount)
        {
            _JackpotMeter = GetMeter();
            _JackpotAmount = amount * QComCommon.MeterScaleFactor;
            _HandpayType = HandpayType.ResidualCancelledCredits;
        }

        private Meter GetMeter()
        {
            return Model.GetMeters().GetTransferredAmount(Direction.Out, TransferDevice.HandPay, 1, CreditType.Cashable);
        }

        internal override HandpayType HandpayType
        {
            get { return HandpayType.CancelledCredits; }
        }

        internal override bool IsJackpotMeterUpdated(SerializableList<MeterId> meterIdsReceived)
        {
            if (!meterIdsReceived.Contains(MeterId.CancelCredit)) return false;

            _Log.InfoFormat("old meters = {0}, new meters = {1}, jackpot amount = {2}", _JackpotMeter, GetMeter(), _JackpotAmount);
            return MeterChangedDetector.IsExpectedMeterChange(_JackpotMeter, GetMeter(), _JackpotAmount);
        }

        internal override void UpdateKeyOffDestination()
        {
            _Log.Info("Updated Cancelled Credit meters not received, hence considering the amount to be added to the Credit meter");
            _KeyOffDestination = KeyOffDestination.Credit;
        }
    }
}
