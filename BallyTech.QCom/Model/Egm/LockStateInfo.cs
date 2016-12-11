using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class LockStateInfo
    {
        public bool LockState { get; set; }
        public string DisplayMessage { get; set; }
        public bool IsFanFareRequired { get; set; }
    }
}
