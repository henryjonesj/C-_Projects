using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{    
    public interface IFundsTransferHandler
    {
        void SetState(bool isEnabled);
        void Initiate(IFundsTransferAuthorization fundsTransferRequest);
        void Commit(IFundsTransferAuthorization fundsTransferRequest);
        bool IsSupported
        {
            get;
        }
    }


    [GenerateICSerializable]
    public partial class NullFundsTransferHandler : IFundsTransferHandler
    {

        #region IFundsTransferHandler Members

        public void SetState(bool isEnabled)
        {
            
        }

        public void Initiate(IFundsTransferAuthorization fundsTransferRequest)
        {
           
        }

        public void Commit(IFundsTransferAuthorization fundsTransferRequest)
        {
           
        }

        public bool IsSupported
        {
            get { return false; }
        }

        #endregion
    }
}
