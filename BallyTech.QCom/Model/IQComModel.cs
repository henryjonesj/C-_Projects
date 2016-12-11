using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Transactions;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model
{
    public interface IQComModel
    {
        void DataLinkStatusChanged(bool linkStatus);
        ApplicationMessage GetNextPoll();
        void ResponseReceived(ApplicationMessage message);

        EgmInfo EgmDetails { [NoSideEffects] get; }

        byte PollAddress { [NoSideEffects] get; }

        bool IsEgmDetailsEntryRequired { [NoSideEffects]get; set; }
        void UpdateCasinoId(string casinoid);
        ConfigurationStatus EgmConfigurationStatus { [NoSideEffects]get; set; }

        bool IsConfigurationRequired { [NoSideEffects]get; }

        bool IsSiteEnabled { [NoSideEffects]get; }
        bool CanSendPoll { [NoSideEffects]get; }

    }
}
