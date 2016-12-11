using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.QCom.Model.Egm.Devices.FundsTransfer;
using log4net;

namespace BallyTech.QCom.Model
{ 
    public class EctFromEgmMonitor
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(EctFromEgmMonitor));
        
        internal decimal PendingTransactionAmount { get; private set; }

        internal EgmAdapter EgmAdapter { get; set; }

        public void MonitorEctFromEgm(EgmMainLineCurrentStatus state)
        {
            var fundTransferOutDevice = EgmAdapter.Devices.OfType<FundsTransferOut>().FirstOrDefault();

            if (state == EgmMainLineCurrentStatus.EctFromEGMLock) fundTransferOutDevice.OnEctfromEgmLockUp();

            if (fundTransferOutDevice.IsAnyTransferInProgress) return;

            PendingTransactionAmount = 0;
        }

        internal void UpdateTransactionAmount(decimal amount)
        {   
            PendingTransactionAmount = amount;
        }
    
    }
}
