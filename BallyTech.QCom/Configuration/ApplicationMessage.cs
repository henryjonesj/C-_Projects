using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Configuration;
using BallyTech.QCom.Model;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Messages
{
    public partial class ApplicationMessage
    {
        public virtual  QComConfigurationId ConfigurationId
        {
            get { return new QComConfigurationId(); }
        }

        internal virtual bool IsConfigured
        {
            get { return false; }
        }

        

    }


    public partial class EgmConfigurationResponse
    {
        public override QComConfigurationId ConfigurationId
        {
            get { return new QComConfigurationId(FunctionCodes.EgmConfiguration, 0); }
        }

        internal override bool IsConfigured
        {
            get { return ConfigurationCompletionSpecification.IsSatisfiedBy(this); }
        }

        
    }

    public partial class EgmGameConfigurationResponse
    {
        public override QComConfigurationId ConfigurationId
        {
            get { return new QComConfigurationId(FunctionCodes.EgmGameConfiguration, GameVersionNumber); }
        }

        internal override bool IsConfigured
        {
            get { return ConfigurationCompletionSpecification.IsSatisfiedBy(this); }
        }
    }


    public partial class ProgressiveConfigurationResponse
    {
        public override QComConfigurationId ConfigurationId
        {
            get { return new QComConfigurationId(FunctionCodes.ProgressiveConfiguration, this.GameVersionNumber); }
        }

        internal override bool IsConfigured
        {
            get { return ConfigurationCompletionSpecification.IsSatisfiedBy(this); }
        }
    }






}
