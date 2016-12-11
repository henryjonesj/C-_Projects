using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Configuration.Response
{
    [GenerateICSerializable]
    public partial class ProgressiveLevelConfiguration : IProgressiveLevelConfiguration
    {
        public ProgressiveLevelConfiguration()
        {
        }

        public ProgressiveLevelConfiguration(int levelNumber, GameProgressiveType progressiveType )
        {
            this.ProgressiveLevelNumber = (byte) levelNumber;
            this.ProgressiveType = progressiveType;
        }


        internal void Update(ProgressiveConfigurationGroup configurationGroup)
        {
            this.StartupAmount = configurationGroup.StartupAmount;
            this.Increment = configurationGroup.Increment;
            this.AuxPayback = configurationGroup.AuxPayback;
            this.CeilingAmount = configurationGroup.CeilingAmount;            
        }


        #region IProgressiveLevelConfiguration Members

        public byte ProgressiveLevelNumber { get; private set; }

        public GameProgressiveType ProgressiveType { get; private set; }

        public decimal StartupAmount { get; private set; }

        public decimal ContributionAmount { get; private set; }

        public decimal Increment { get; private set; }

        public decimal CeilingAmount { get; private set; }

        public decimal AuxPayback { get; private set; }

        #endregion
    }
}
