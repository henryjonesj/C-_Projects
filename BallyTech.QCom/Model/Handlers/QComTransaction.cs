using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public abstract partial class QComTransaction
    {
        internal ElectronicCreditTransferHandler Parent { get; set; }
        internal QComModel Model { get; set; }

        internal virtual void Initiate(IFundsTransferAuthorization authorization) { }
        internal abstract void Execute(IFundsTransferAuthorization authorization);
        internal virtual void Cancel(bool isForced) { }

    }
}
