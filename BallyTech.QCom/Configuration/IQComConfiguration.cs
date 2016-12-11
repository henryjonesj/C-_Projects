using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Configuration
{
    public interface IQComConfiguration
    {
        QComConfigurationId Id { get; }
        ValidationStatus ValidationStatus { get; }
        EgmGameConfigurationStatus ConfigurationStatus { get; }     


        Request ConfigurationPoll { get; }
        Request ConfigurationRequest { get; }

        void UpdateConfigurationStatus(EgmGameConfigurationStatus status);
        void ResetValidationStatus();

        void UpdateProtocolVersion(ProtocolVersion protocolVersion);
    }

    public enum ValidationStatus
    {
        None,
        Success,
        Failure
    }

    public enum EgmGameConfigurationStatus
    {
        None,
        Success,
        Failure,
        InProgress,
        Pending
    }
    
}
    