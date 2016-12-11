using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class PlayerInformationDisplay : IPlayerInformationDisplay
    {
        public EgmModel EgmModel { get; set; }

        #region IPlayerInformationDisplay Members

        public bool DisplayEnabled { get; set; }

        public TimeSpan InformationMenuTimeout { get; set; }

        public TimeSpan SessionStartMessageTimeout { get; set; }

        public TimeSpan ViewSessionScreenTimeout { get; set; }

        public TimeSpan ViewGameInformationTimeout { get; set; }

        public TimeSpan ViewGameRulesTimeout { get; set; }

        public TimeSpan ViewPayTableTimeout { get; set; }

        public TimeSpan SessionTimeoutInterval { get; set; }

        public bool AllowTimeoutWithCreditMeterNotZero { get; set; }

        public SessionPerformanceFormula SessionPerformanceFormula { get; set; }

        public byte LinkEnrollmentCount { get; set; }

        public decimal TotalLinkContributionRate { get; set; }

        public bool HideLinkEnrollmentCount { get; set; }

        public IMysteryInformationDisplay MysteryInformationDisplay
        {
            get  { return EgmModel.EgmAdapter.MysteryInformationDisplay;  }
        }

        #endregion
    }
}
