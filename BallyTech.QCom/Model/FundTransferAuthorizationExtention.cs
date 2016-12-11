using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model
{
    internal static class FundTransferAuthorizationExtention
    {
        internal static decimal GetTransferAmount(this IFundsTransferAuthorization authorization, bool isMixedCreditFundTransferAllowed)
        {
            return (isMixedCreditFundTransferAllowed) ? authorization.GetTotalAmount() : authorization.Cashable + authorization.Promotional;
        }
    }
}
