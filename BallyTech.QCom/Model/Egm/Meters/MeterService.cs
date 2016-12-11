using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class MeterService : IEgmMeters
    {
        public const decimal MeterScaleFactor = 0.01m;

        private SerializableDictionary<MeterId, Meter> _EgmMeters = null;

        private SerializableDictionary<int, Meter> _GameLinkedProgressiveMeters = null;

        private EgmAdapter _EgmAdapter = null;

        public MeterService() { }

        public MeterService(EgmAdapter egmAdapter)
        {
            _EgmAdapter = egmAdapter;
            _EgmMeters = new SerializableDictionary<MeterId, Meter>(egmAdapter.MeterRepository.EgmMeters);
            _GameLinkedProgressiveMeters = new SerializableDictionary<int, Meter>();
            _EgmAdapter.Games.ForEach((game) => _GameLinkedProgressiveMeters.Add(game.GameNumber, game.LinkedProgressiveContributionAmount));
        }
		
		private Meter ConvertToDollars(Meter meter)
        {            
            return meter != Meter.Zero ? meter * MeterScaleFactor : Meter.Zero;
        }

        #region IEgmMeters Members

        private Meter GetMeter(int? gameIndex, MeterId meterId, bool isCountMeter)
        {
            Meter meter = Meter.NotAvailable;

            if (gameIndex.HasValue)
            {
                if (_EgmAdapter.Games.Contains((int)gameIndex))
                    meter = _EgmAdapter.Games[(int)gameIndex].GetMeter(meterId);
            }
            else
                meter = _EgmMeters.GetMeterValueFor(meterId);
            return isCountMeter ? meter: ConvertToDollars(meter);
        }

        private Meter GetAccountTransferAmount(Direction? direction)
        {
            switch (direction)
            {
                case Direction.In:
                    return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.CashlessIn));
                case Direction.Out:
                    return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.CashlessOut));
            }
            return Meter.Zero;
        }

        private Meter GetHandpayAmount(Direction? direction, CreditType? creditType, int? deviceIndex)
        {
            if (direction.Equals(Direction.Out) && creditType == CreditType.Cashable)
            {
                if (!deviceIndex.HasValue) return Meter.Zero;

                switch (deviceIndex.Value)
                {
                    case 1:
                        return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.CancelCredit));
                    default:
                        return Meter.Zero;
                }
            }
            return Meter.Zero;
        }

        private Meter GetVoucherAmount(Direction? direction)
        {
            switch (direction)
            {
                case Direction.In:
                    return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.TicketIn));
                case Direction.Out:
                    return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.TicketOut));
            }
            return Meter.Zero;
        }

        public Meter GetTransferredAmount(Direction? direction, TransferDevice? device, int? deviceIndex, CreditType? creditType)
        {
            switch (device)
            {
                case TransferDevice.AccountTransfer:
                    if (!IsCreditTypeSupported(creditType)) return Meter.Zero;

                    return GetAccountTransferAmount(direction);

                case TransferDevice.NoteAcceptor:
                    return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.NoteAcceptorCentsIn));

                case TransferDevice.CoinAcceptor:
                    if (direction.Equals(Direction.In))
                    {
                        if(deviceIndex.HasValue && deviceIndex.Value == 2)
                            return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.CoinsDropped));

                        return GetMeter(null, MeterId.CoinsIn, false);
                    }
                    break;

                case TransferDevice.Hopper:
                    if (direction.Equals(Direction.Out))
                        return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.CoinsOut));

                    else if (direction.Equals(Direction.In))
                        return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.HopperRefill));
                    break;

                case TransferDevice.HandPay:
                    return GetHandpayAmount(direction, creditType, deviceIndex);

                case TransferDevice.Voucher:
                    if (!IsCreditTypeSupported(creditType)) return Meter.Zero;
                    return GetVoucherAmount(direction);
                case TransferDevice.Other:
                    if (direction.Equals(Direction.In))
                        return GetMeter(null, MeterId.CentsIn, false);
                    else if (direction.Equals(Direction.Out))
                        return GetMeter(null, MeterId.CentsOut, false);
                    break;
            }
            return Meter.Zero;
        }

        public Meter GetWageredAmount(int? gameIndex, decimal? denomination, CreditType? creditType, GamePhase? phase)
        {
            switch(phase)
            {
                case null:
                case GamePhase.Primary:
                    if (creditType == null) return GetMeter(gameIndex, MeterId.Bets, false);
                    switch (creditType)
                    {
                        case CreditType.Cashable:
                            return GetMeter(gameIndex, MeterId.Bets, false);
                        default:
                            return Meter.Zero;
                    }
                case GamePhase.Secondary:
                    return GetMeter(null, MeterId.GambleTurnover, false);
                case GamePhase.Residual:
                    return GetMeter(null, MeterId.ResidualTurnover, false); 
                   
                case GamePhase.Progressive:
                    return GetLinkedProgressiveWageredAmount(gameIndex);

            }
            return Meter.Zero;
        }

        public Meter GetWonAmount(int? gameIndex, decimal? denomination, GamePhase? phase, WinSource? source, WinPaymentMethod? paymentMethod)
        {
            switch(phase)
            {
                case null:
                case GamePhase.Primary:                            
                    switch (source)
                    {
                        case null:
                            switch (paymentMethod)
                            {
                                case WinPaymentMethod.HandPaid:
                                    return Meter.Zero; // as there is no handpaid meter in QCOM
                                default:
                                    return GetMeter(gameIndex, MeterId.Wins, false);
                            }
                        case WinSource.Progressive:
                            switch (paymentMethod)
                            {
                                case WinPaymentMethod.EgmPaid:
                                    return ConvertToDollars(_EgmMeters.GetMeterValueFor(MeterId.SapWins));
                                case WinPaymentMethod.HandPaid:
                                    return GetMeter(gameIndex, MeterId.LpWins, false);
                                case null:
                                    return Meter.Zero;
                            }
                            break;
                    }
                    break;
                case GamePhase.Secondary:
                    return GetMeter(null, MeterId.GambleWin, false);
                case GamePhase.Residual:
                    return GetMeter(null, MeterId.ResidualWin, false);
            }

            return Meter.Zero;
        }

        public Meter GetCreditAmount(CreditType? creditType)
        {
            if (creditType.HasValue && creditType != CreditType.Cashable) return Meter.Zero;

            return CreditMeterCalculator.Calculate(_EgmAdapter);
        }

        public Meter GetGamesPlayed(int? gameIndex, decimal? denomination, GamePhase? phase, GameOutcome? outcome)
        {
            if (outcome.HasValue && outcome.Value == GameOutcome.Won)            
                return GetMeter(gameIndex, MeterId.GamesWon, true);            

            return GetMeter(gameIndex, MeterId.Plays, true);
        }

        public Meter GetGamesPlayedConditional(Condition? condition)
        {
            return Meter.Zero;
        }

        public Meter GetCurrencyCount(decimal? denomination)
        {
            if (denomination.HasValue)
            {
                switch ((int)denomination.Value)
                {
                    case 5:
                        return GetMeter(null, MeterId.Dollar5InCount, true);
                    case 10:
                        return GetMeter(null, MeterId.Dollar10InCount, true);
                    case 20:
                        return GetMeter(null, MeterId.Dollar20InCount, true);
                    case 50:
                        return GetMeter(null, MeterId.Dollar50InCount, true);
                    case 100:
                        return GetMeter(null, MeterId.Dollar100InCount, true);
                    default:
                        return Meter.Zero;
                }
            }

            return GetMeter(null, MeterId.NotesAccepted, true);
        }

        public Meter GetRejectedItemCount(TransferDevice? transferDevice)
        {
            if (transferDevice.Value == TransferDevice.NoteAcceptor)
                return GetMeter(null, MeterId.NotesRejected, true);

            return Meter.NotAvailable;
        }


        public Meter GetCurrencyAmount(decimal? denomination)
        {
            return Meter.Zero;
        }

        public Meter GetTransferredCount(Direction? direction, TransferDevice? device, int? deviceIndex, CreditType? creditType)
        {
            if (!device.HasValue)
            {
                return Meter.Zero;
            }

            switch (device)
            {
                case TransferDevice.CoinAcceptor:
                    {
                        if (!direction.Equals(Direction.In)) break;

                        var coinsPurchased = _EgmMeters.GetMeterValueFor(MeterId.CoinsIn);
                        if (coinsPurchased.IsNotAvailable) return Meter.NotAvailable;

                        decimal coinsIn =
                            decimal.Floor((coinsPurchased/_EgmAdapter.TokenDenomination).DangerousGetUnsignedValue());
                        return new Meter(coinsIn, 1, uint.MaxValue + 1m);
                    }
                case TransferDevice.Hopper:
                    {
                        if (!direction.Equals(Direction.Out)) break;

                        var coinsCollected = _EgmMeters.GetMeterValueFor(MeterId.CoinsOut);
                        if (coinsCollected.IsNotAvailable) return Meter.NotAvailable;

                        decimal coinsOut =
                            decimal.Floor((coinsCollected/_EgmAdapter.TokenDenomination).DangerousGetUnsignedValue());
                        return new Meter(coinsOut, 1, uint.MaxValue + 1m);
                    }
                default:
                    break;
            }
            return Meter.Zero;
        }

        public MeterFormat GetMeterType()
        {
            
            return MeterFormat.Binary;
        }

        #endregion

        private static bool IsCreditTypeSupported(CreditType? creditType)
        {
            return !creditType.HasValue || creditType.Value == CreditType.Cashable;
        }

        #region IEgmMeters Members

        public Meter GetRemovedCurrencyAmount(TransferDevice? transferDevice)
        {
            if (!transferDevice.HasValue) return Meter.NotAvailable;

            switch (transferDevice.Value)
            {
                case TransferDevice.CoinAcceptor:
                    return GetMeter(null, MeterId.CoinsCleared, false);
                case TransferDevice.NoteAcceptor:
                    return GetMeter(null, MeterId.NotesCleared, false);
            }
            return Meter.NotAvailable;
        }

        #endregion

        public Meter GetTotalPIDAccessed()
        {
            return GetMeter(null, MeterId.TotalEGMPIDAccessed, true);
        }

        public Meter GetLinkedProgressiveWageredAmount(int? gameVersion)
        {
            if (!gameVersion.HasValue || !IsValidGameVersion(gameVersion.Value) || !_EgmAdapter.Games[gameVersion.Value].IsLinkedProgressiveGame()) 
                return Meter.NotAvailable;

            return ConvertToDollars(_GameLinkedProgressiveMeters[gameVersion.Value]);
        }

        private bool IsValidGameVersion(int gameVersion)
        {
            return (_GameLinkedProgressiveMeters != null && _GameLinkedProgressiveMeters.ContainsKey(gameVersion));
        }
    }
}
