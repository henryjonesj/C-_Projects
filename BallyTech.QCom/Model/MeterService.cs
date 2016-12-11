using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Utility;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class MeterService : IEgmMeters
    {
        #region IEgmMeters Members

        public Meter GetTransferredAmount(Direction? direction, TransferDevice? device, int? deviceIndex, CreditType? creditType)
        {
            return Meter.Zero;
        }

        public Meter GetTransferredCount(Direction? direction, TransferDevice? device, int? deviceIndex, CreditType? creditType)
        {
            return Meter.Zero;
        }

        public Meter GetWageredAmount(int? gameIndex, decimal? denomination, CreditType? creditType, GamePhase? phase)
        {
            return Meter.Zero;
        }

        public Meter GetWonAmount(int? gameIndex, decimal? denomination, GamePhase? phase, WinSource? source, WinPaymentMethod? paymentMethod)
        {
            return Meter.Zero;
        }

        public Meter GetCreditAmount(CreditType? creditType)
        {
            return Meter.Zero;
        }

        public Meter GetGamesPlayed(int? gameIndex, decimal? denomination, GamePhase? phase, GameOutcome? outcome)
        {
            return Meter.Zero;
        }

        public Meter GetGamesPlayedConditional(Condition? condition)
        {
            return Meter.Zero;
        }

        public Meter GetCurrencyCount(decimal? denomination)
        {
            return Meter.Zero;
        }

        public Meter GetCurrencyAmount(decimal? denomination)
        {
            return Meter.Zero;
        }

        #endregion
    }
}
