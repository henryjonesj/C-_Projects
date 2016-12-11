using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class DepositSnapshot :IDepositSnapshot
    {

        #region IDepositSnapshot Members

        public bool CanDepositCashable
        {
            get { return true; }
        }

        public bool CanDepositNonCashable
        {
            get { return true; }
        }

        public bool CanDepositPromotional
        {
            get { return true; }
        }

        public bool PartialTransfer
        {
            get { return false; }
        }

        #endregion
    }
}
