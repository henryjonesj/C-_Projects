using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BallyTech.QCom.Messages
{
    public static class EventDataSizeMapping
    {
        private static Dictionary<EventCodes, int> _EventSizeMap = new Dictionary<EventCodes, int>();

        static EventDataSizeMapping()
        {
            _EventSizeMap.Add(EventCodes.CancelCredit, 4);
            _EventSizeMap.Add(EventCodes.CashTicketIn, 16);
            _EventSizeMap.Add(EventCodes.CashTicketOutPrintFailure, 6);
            _EventSizeMap.Add(EventCodes.CashTicketOutPrintSuccessful, 6);
            _EventSizeMap.Add(EventCodes.CashTicketOutRequest, 6);
            _EventSizeMap.Add(EventCodes.CashTicketPrinted, 12);
            _EventSizeMap.Add(EventCodes.DenominationChanged, 4);
            _EventSizeMap.Add(EventCodes.ECTFromEGMNew, 4);
            _EventSizeMap.Add(EventCodes.ECTfromEGMOld, 4);
            _EventSizeMap.Add(EventCodes.EGMTicketInTimeOut, 16);
            _EventSizeMap.Add(EventCodes.EGMTicketInAborted, 16);
            _EventSizeMap.Add(EventCodes.EXTJIPIconDisplayEnabled, 16);
            _EventSizeMap.Add(EventCodes.GameVariationEnabled, 3);
            _EventSizeMap.Add(EventCodes.HopperCalibrated, 4);
            _EventSizeMap.Add(EventCodes.HopperLevelMismatch, 4);
            _EventSizeMap.Add(EventCodes.HopperOverpayAmount, 4);
            _EventSizeMap.Add(EventCodes.HopperPayout, 4);
            _EventSizeMap.Add(EventCodes.HopperRefillRecorded, 4);  
            _EventSizeMap.Add(EventCodes.InvalidProgressiveConfiguration, 3);
            _EventSizeMap.Add(EventCodes.LargeWin, 7);
            _EventSizeMap.Add(EventCodes.LicenseKeyDetected, 8);
            _EventSizeMap.Add(EventCodes.LicenseKeyMissing, 8);
            _EventSizeMap.Add(EventCodes.LinkedProgressiveAward, 10);
            _EventSizeMap.Add(EventCodes.ManufacturerSpecificEvent, 16);
            _EventSizeMap.Add(EventCodes.NewGameSelected, 3);
            _EventSizeMap.Add(EventCodes.NonProductionLicenseKeyDetected, 8);
            _EventSizeMap.Add(EventCodes.NVRAMCleared, 4);
            _EventSizeMap.Add(EventCodes.OldResidualCancelCreditLockUp, 2);
            _EventSizeMap.Add(EventCodes.ProgressiveConfigurationChanged, 7);
            _EventSizeMap.Add(EventCodes.ResidualCancelCreditLockUp, 4);
            _EventSizeMap.Add(EventCodes.StandAloneProgressiveAwardV15, 8);
            _EventSizeMap.Add(EventCodes.StandAloneProgressiveAwardV16, 8);
            _EventSizeMap.Add(EventCodes.SystemLockupUserResponse, 1);
            _EventSizeMap.Add(EventCodes.TopNpPrizeHit, 3);
            _EventSizeMap.Add(EventCodes.CRanEHit, 5);
            
        
        }

        public static int GetEventDataSize(EventCodes eventCode)
        {
            return _EventSizeMap.ContainsKey(eventCode) ? _EventSizeMap[eventCode] : 0;
        }
    
    
    
    }
}
