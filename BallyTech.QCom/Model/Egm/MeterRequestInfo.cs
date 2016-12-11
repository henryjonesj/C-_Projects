using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class MeterRequestInfo
    {
        internal int GameNumber { get; private set; }
        internal byte PayTableId { get; private set; }
        internal MeterType[] Meters { get; private set; }

        /// <summary>
        /// This constructs the object to query the Egm meters.
        /// </summary>
        public MeterRequestInfo()
        {
        }

        /// <summary>
        /// This constructs the object to query the game meters.
        /// </summary>
        /// <param name="gameNumber"></param>
        /// <param name="payTableId"></param>
        public MeterRequestInfo(int gameNumber, byte payTableId)
        {
            GameNumber = gameNumber;
            PayTableId = payTableId;
        }

        internal MeterRequestInfo GetRequestFor(MeterType[] meterTypes)
        {
            Meters = meterTypes;
            return this;
        }

        internal bool IsGameSpecificMeter
        {
            get { return GameNumber > 0 && PayTableId > 0; }
        }

    }
}
