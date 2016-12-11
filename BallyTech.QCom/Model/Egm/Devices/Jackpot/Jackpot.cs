using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Egm.Devices
{
    [GenerateICSerializable]
    public partial class Jackpot
    {
        protected static readonly ILog _Log = LogManager.GetLogger(typeof(Jackpot));
        
        protected HandpayType _HandpayType = HandpayType.UnknownProgressive;
        protected Meter _JackpotMeter = Meter.Zero;
        protected decimal _JackpotAmount = 0m;

        protected EgmModel _Model;
        public EgmModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        internal virtual void UpdateMeters(decimal meters) { }

        internal virtual KeyOffDestination KeyOffDestination
        {
            get { return KeyOffDestination.NotSpecified; }
        }

        internal virtual HandpayType HandpayType
        {
            get { return _HandpayType; }
        }

        internal virtual bool IsJackpotMeterUpdated(SerializableList<MeterId> meterIdsReceived) { return false; }

        internal virtual void UpdateKeyOffDestination() { }

        internal bool IsSameHandpayType(HandpayType handpayType)
        {
            return (handpayType == this._HandpayType);
        }
    }
}
