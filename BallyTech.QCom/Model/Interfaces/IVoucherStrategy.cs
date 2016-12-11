using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Model.Egm
{
    public interface IVoucherStrategy
    {
        void SetState(bool isEnabled);
        void Execute(IVoucherTransaction ticketTransaction);
        void SetProperties(string propertyName, string location,string ticketText);
    }

    [BallyTech.Utility.Serialization.GenerateICSerializable]
    public partial class NullVoucherStrategy : IVoucherStrategy
    {
        #region IVoucherStrategy Members

        public void SetState(bool isEnabled)
        {
          
        }

        public void Execute(IVoucherTransaction ticketTransaction)
        {
            
        }

        public void SetProperties(string propertyName, string location, string ticketText)
        {
            
        }

        #endregion
    }
}
