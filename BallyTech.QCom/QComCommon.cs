using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility;
using BallyTech.Gtm;
using log4net.Core;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom
{
    public enum ProtocolVersion : byte
    {
        Unknown,
        V15,
        V16
    }

    [Flags]
    public enum HotSwitchType
    {
        None = 0x00,
        GameVariation = 0x01,
        ProgressiveGroupId = 0x02,
        GameStatus = 0x04
    }   


    internal static class ProtocolVersionExtension
    {
        internal static string GetText(this ProtocolVersion protocolVersion)
        {
            switch (protocolVersion)
            {
                case ProtocolVersion.V15:
                    return "QCM1.5";
                case ProtocolVersion.V16:
                    return "QCM1.6";
                default:
                    return "QCM";
            }
        }
    }




    internal static class QComCommon
    {
        internal static readonly decimal MeterScaleFactor = 0.01m;
    }

    internal static class QComConvert
    {
        const int PRESENTCENTURY = 2000;
        const int MINUTES_IN_A_DAY = 1439;

        internal static HandpayType ToHandpayType(this ProgressiveLevelInformation information)
        {
            switch (information)
            {
                case ProgressiveLevelInformation.Level1:
                    return HandpayType.ProgressiveLevel1;
                case ProgressiveLevelInformation.Level2:
                    return HandpayType.ProgressiveLevel2;
                case ProgressiveLevelInformation.Level3:
                    return HandpayType.ProgressiveLevel3;
                case ProgressiveLevelInformation.Level4:
                    return HandpayType.ProgressiveLevel4;
                case ProgressiveLevelInformation.Level5:
                    return HandpayType.ProgressiveLevel5;
                case ProgressiveLevelInformation.Level6:
                    return HandpayType.ProgressiveLevel6;
                default:
                    return HandpayType.UnknownProgressive;
            }
        }


        internal static FCodes ToFCode(this TicketRedeemReasonCode reasonCode)
        {
            switch (reasonCode)
            {
                case TicketRedeemReasonCode.Accepted:
                    return FCodes.TicketAccepted;
                case TicketRedeemReasonCode.AlreadyPaid:
                    return FCodes.TicketAlreadyRedeemed;
                case TicketRedeemReasonCode.NotFound:
                    return FCodes.TicketNotFound;
                case TicketRedeemReasonCode.TooOld:
                    return FCodes.TicketExpired;
                case TicketRedeemReasonCode.ValueTooLarge:
                    return FCodes.TicketAmountTooLarge;
                default:
                    return FCodes.Unknown;
            }
        }

        internal static EgmMainLineCurrentStatus ToEgmMainLineCurrentStatus(this JackpotType jackpotType)
        {
            switch(jackpotType)
            {
                case JackpotType.LargeWin:
                    return EgmMainLineCurrentStatus.LargeWinLockup;                    
                case JackpotType.CancelledCredit:
                    return EgmMainLineCurrentStatus.CancelCreditLockup;
                case JackpotType.LinkedProgressive:
                    return EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup;
                case JackpotType.ResidualCancelledCredit:
                    return EgmMainLineCurrentStatus.ResidualCancelCreditLockup;
                default:
                    return EgmMainLineCurrentStatus.None;
            }
        }

        internal static JackpotType GetJackpotType(this EgmMainLineCurrentStatus state)
        {
            switch(state)
            {
                case EgmMainLineCurrentStatus.CancelCreditLockup:
                    return JackpotType.CancelledCredit;
                case EgmMainLineCurrentStatus.LargeWinLockup:
                    return JackpotType.LargeWin;
                case EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup:
                    return JackpotType.LinkedProgressive;
                case EgmMainLineCurrentStatus.ResidualCancelCreditLockup:
                    return JackpotType.ResidualCancelledCredit;
                default:
                    return JackpotType.None;
            }
        }

        internal static SourceIds ToSourceId(this IFundsTransferRequest transferRequest)
        {
            switch (transferRequest.Origin)
            {
                case TransferOrigin.BonusJackpotWin:
                    return SourceIds.QComLpPrizes;
                default:
                    return transferRequest.IsBonusTransfer() ? SourceIds.ExternalJackpotA : SourceIds.CashlessGaming;
            }
            
        }

        internal static DateTime ConvertQComRawDateTimeToDateTime(QComRawDateTime rawDateTime)
        {
            return new DateTime(rawDateTime.Year+PRESENTCENTURY, rawDateTime.Month, rawDateTime.Day,
                                rawDateTime.Hours, rawDateTime.Minutes, rawDateTime.Seconds);
        }
        
        internal static string ConvertByteArrayToASCIIString(byte[] bytes)
        {
            return System.Text.Encoding.ASCII.GetString(bytes, 0, bytes.Length);
        }

        internal static byte[] PadZerosToLeft(byte[] bytes, int length)
        {
            byte[] result = new byte[length];
            var startAt = length - bytes.Length;
            Buffer.BlockCopy(bytes, 0, result, startAt, bytes.Length);
            return result;
        }


        internal static decimal ConvertDecimalArrayToDecimal(decimal[] decimalArray)
        {
            StringBuilder str = new StringBuilder();
            foreach (var data in decimalArray) str.Append(data);
            return decimal.Parse(str.ToString());
        }

        internal static decimal GetEndOfDayTimeMinutes(decimal timeSeconds)
        {
            decimal timeMinutes = timeSeconds / 60;
            return timeMinutes > MINUTES_IN_A_DAY ? MINUTES_IN_A_DAY : timeMinutes;
        } 

    }
}
