using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.Meters
{
    public static class MeterCodeIdMapping
    {
        static readonly Dictionary<MeterCodes,MeterId>  MeterCodeIpMap = new Dictionary<MeterCodes, MeterId>();

        static MeterCodeIdMapping()
        {
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCashlessCreditIn,MeterId.CashlessIn);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCashlessCreditOut,MeterId.CashlessOut);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmTurnover,MeterId.Bets);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmWins,MeterId.Wins);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmStroke,MeterId.Plays);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmNoteAcceptorCentsIn,MeterId.NoteAcceptorCentsIn);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmFiveNotesIn, MeterId.Dollar5InCount);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmTenNotesIn, MeterId.Dollar10InCount);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmTwentyNotesIn, MeterId.Dollar20InCount);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmFiftyNotesIn, MeterId.Dollar50InCount);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmHundredNotesIn, MeterId.Dollar100InCount);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCoinsTokensIn,MeterId.CoinsIn);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCoinsTokenOut,MeterId.CoinsOut);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCentsIn, MeterId.CentsIn);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCentsOut, MeterId.CentsOut);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCashTicketIn, MeterId.TicketIn);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCashTicketOut, MeterId.TicketOut);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCancelCredit, MeterId.CancelCredit);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmSAPWins,MeterId.SapWins);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmLinkedProgressiveWins, MeterId.LpWins);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmGamesWon, MeterId.GamesWon);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCoinsTokensToCashbox, MeterId.CoinsDropped);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmHopperRefills, MeterId.HopperRefill);
            MeterCodeIpMap.Add(MeterCodes.TotalRejectedEnabledNotes, MeterId.NotesRejected);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmNotesInCount, MeterId.NotesAccepted);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmCoinsTokensCleared, MeterId.CoinsCleared);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmNotesCleared, MeterId.NotesCleared);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmGambleTurnover, MeterId.GambleTurnover);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmGambleWins, MeterId.GambleWin);
            MeterCodeIpMap.Add(MeterCodes.TotalResidualCreditRemovalTurnover, MeterId.ResidualTurnover);
            MeterCodeIpMap.Add(MeterCodes.TotalResidualCreditRemovalWins, MeterId.ResidualWin);
            MeterCodeIpMap.Add(MeterCodes.TotalEgmPIDAccessed, MeterId.TotalEGMPIDAccessed);
        }

        internal static MeterId? ConverToMeterId(this MeterCodes meterCodes)
        {
            if (MeterCodeIpMap.ContainsKey(meterCodes)) return MeterCodeIpMap[meterCodes];

            return null;
        }

        internal static MeterCodes GetMeterCode(MeterId meterId)
        {
            return MeterCodeIpMap.FirstOrDefault(entry => (entry.Value == meterId)).Key;
        }

    }
}
