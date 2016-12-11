using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;

namespace BallyTech.QCom.Messages
{
    partial class EgmConfigurationResponseV16
    {
        public override bool IsDenominationHotSwitchingEnabled
        {
            get { return (this._FlagS & EgmConfigurationFlagS.DenominationHotSwitching) == EgmConfigurationFlagS.DenominationHotSwitching; }
        }

        public override bool IsSharedProgressivesEnabled
        {
            get { return (this._FlagS & EgmConfigurationFlagS.SharedProgressiveComponentFlag) == EgmConfigurationFlagS.SharedProgressiveComponentFlag; }
        }
        
        public override decimal CreditDenomination
        {
            get { return this.NewCreditDenomination; }
        }

        public override decimal TokenDenomination
        {
            get { return this.NewTokenDenomination; }
        }

        public override decimal MaxDenomination
        {
            get { return this.MaxDenom; }
        }

        public override decimal MinTheoreticalPercent
        {
            get { return this.MinRtp; }
        }

        public override decimal MaxTheoreticalPercent
        {
            get { return this.MaxRtp; }
        }

        public override decimal MaxStandardDeviation
        {
            get { return this.MaxSd; }
        }

        public override int MaxLines
        {
            get { return (int)this.MaximumLines; }
        }

        public override decimal MaxBet
        {
            get { return (decimal)this.MaximumBet; }
        }

        public override decimal MaxNonProgressiveWinAmount
        {
            get { return this.MaxNpWin; }
        }

        public override decimal MaxProgressiveWinAmount
        {
            get { return this.MaxPWin; }
        }

        public override decimal MaxElectronicCreditTransferLimit
        {
            get { return this.MaxEct; }
        }


        internal override void BackFill(IEgmConfiguration configuration)
        {
            UpdateParameters(configuration);
        }


    }
}
