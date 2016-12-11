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
    public partial class MeterGroup
    {
        private MeterCodes _MeterCode;
        public MeterCodes MeterCode
        {
            get { return _MeterCode; }
            set { _MeterCode = value; }
        }

        private Meter _Meter;
        public Meter Meter
        {
            get { return _Meter; }
            set { _Meter = value; }
        }

        private bool _IsSynced = false;
        public bool IsSynced
        {
            get { return _IsSynced; }
            set { _IsSynced = value; }
        }
    }
}
