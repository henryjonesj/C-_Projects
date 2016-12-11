using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Egm;
using BallyTech.Gtm;
using System.Diagnostics.CodeAnalysis;

namespace BallyTech.QCom.Model.EventsListeners
{
    public static class CabinetEventMapping
    {
        private static Dictionary<EventCodes, EgmEvent?> _CabinetEventMap = new Dictionary<EventCodes, EgmEvent?>();

        static CabinetEventMapping()
        {
            _CabinetEventMap.Add(EventCodes.DenominationChanged, EgmEvent.DenominationChanged);
            _CabinetEventMap.Add(EventCodes.ProgressiveConfigurationChanged, EgmEvent.ProgressiveConfigurationChanged);          
            _CabinetEventMap.Add(EventCodes.RecoverableRAMCorruption, EgmEvent.RecoverableRamCorruption);
            _CabinetEventMap.Add(EventCodes.AuxiliaryDisplayDeviceFailure, EgmEvent.DisplayError);
            _CabinetEventMap.Add(EventCodes.PrimaryDisplayDeviceFailure, EgmEvent.DisplayError);
            _CabinetEventMap.Add(EventCodes.TertiaryDisplayDeviceFailure, EgmEvent.DisplayError);
            _CabinetEventMap.Add(EventCodes.TouchScreenFault, EgmEvent.EgmTouchScreenError);
            _CabinetEventMap.Add(EventCodes.LowNVRAMBattery, EgmEvent.LowNVRAMBattery);
            _CabinetEventMap.Add(EventCodes.LowPFDoorDetectionBattery, EgmEvent.LowBattery);
            _CabinetEventMap.Add(EventCodes.EEPROMFault, EgmEvent.EepromBadDevice);
            _CabinetEventMap.Add(EventCodes.LicenseKeyMissing, EgmEvent.LicenseKeyMissing);
            _CabinetEventMap.Add(EventCodes.NonProductionLicenseKeyDetected, EgmEvent.NonProductionLicenseKeyDetected);
            _CabinetEventMap.Add(EventCodes.EgmPowerUp, EgmEvent.EgmPowerUp);
            _CabinetEventMap.Add(EventCodes.MechanicalMetersDisconnected, EgmEvent.MechanicalMeterDisconnected);
            _CabinetEventMap.Add(EventCodes.EgmPowerDown, EgmEvent.EgmPowerDown);
            _CabinetEventMap.Add(EventCodes.StepperReelFault, EgmEvent.Reel1Tilt);
            _CabinetEventMap.Add(EventCodes.PowerOffProcessorDoorAccess, EgmEvent.PowerOffCardCageAccess);
            _CabinetEventMap.Add(EventCodes.PowerOffMainDoorAccess, EgmEvent.PowerOffSlotDoorAccess);
            _CabinetEventMap.Add(EventCodes.PowerOffNoteAcceptorDoorAccess, EgmEvent.PowerOffCashboxDoorAccess);
            _CabinetEventMap.Add(EventCodes.CallServiceTechnician, EgmEvent.ChangeLampOn);
            _CabinetEventMap.Add(EventCodes.NVRAMCleared, EgmEvent.NVRAMCleared);
            _CabinetEventMap.Add(EventCodes.LowMemory,EgmEvent.LowMemory);
            _CabinetEventMap.Add(EventCodes.ProgressiveControllerFault, EgmEvent.ProgressiveControllerFault);
            _CabinetEventMap.Add(EventCodes.IOControllerFault, EgmEvent.IOControllerFault);
            _CabinetEventMap.Add(EventCodes.BonusDeviceFault, EgmEvent.BonusDeviceFault);
            _CabinetEventMap.Add(EventCodes.ManufacturerSpecificEvent, EgmEvent.ManufacturerSpecificFault);
            _CabinetEventMap.Add(EventCodes.ManufacturerSpecificFaultA, EgmEvent.ManufacturerSpecificFault);
            _CabinetEventMap.Add(EventCodes.ManufacturerSpecificFaultB, EgmEvent.ManufacturerSpecificFault);
            _CabinetEventMap.Add(EventCodes.AllFaultsCleared,EgmEvent.AllFaultsCleared);
            _CabinetEventMap.Add(EventCodes.PrimaryEventQueueFull, EgmEvent.PrimaryEventQueueFull);
            _CabinetEventMap.Add(EventCodes.SecondaryEventQueueFull, EgmEvent.SecondaryEventQueueFull);
            _CabinetEventMap.Add(EventCodes.PowerOffCashDoorAccess, EgmEvent.PowerOffCashDoorAccess);
            _CabinetEventMap.Add(EventCodes.RtcRefreshed,EgmEvent.RtcRefreshed);
            _CabinetEventMap.Add(EventCodes.PowerOffMechanicalMeterDoorAccess, EgmEvent.PowerOffMechanicalMeterDoorAccess);
            _CabinetEventMap.Add(EventCodes.InvalidProgressiveConfiguration, EgmEvent.InvalidProgressiveConfiguration);
            _CabinetEventMap.Add(EventCodes.InvalidEGMConfiguration, EgmEvent.InvalidEGMConfiguration);
            _CabinetEventMap.Add(EventCodes.InvalidGameConfiguration, EgmEvent.InvalidGameConfiguration);
            _CabinetEventMap.Add(EventCodes.InvalidDenomination, EgmEvent.InvalidDenomination);
            _CabinetEventMap.Add(EventCodes.CRanEHit, EgmEvent.CraneHit);
            _CabinetEventMap.Add(EventCodes.PowerDownIncomplete, EgmEvent.PowerDownIncomplete);
            _CabinetEventMap.Add(EventCodes.LockUpCleared, EgmEvent.LockUpCleared);
            _CabinetEventMap.Add(EventCodes.QsimProtocolSimulator, EgmEvent.EGMSimulatorInUse);
            _CabinetEventMap.Add(EventCodes.EXTJIPIconDisplayEnabled, EgmEvent.ExternalJackpotDisplayEnabled);
            _CabinetEventMap.Add(EventCodes.ProcessorOverTemperature, EgmEvent.ProcessorOverTemperature);
            _CabinetEventMap.Add(EventCodes.CommunicationsTimeOut, EgmEvent.CommunicationsTimeOut);
            _CabinetEventMap.Add(EventCodes.CoolingFanLowRPM, EgmEvent.CoolingFanLowRPM);
            _CabinetEventMap.Add(EventCodes.NewPIDSessionStarted, EgmEvent.NewPIDSessionStarted);
            _CabinetEventMap.Add(EventCodes.InvalidTicketAcknowledgement, EgmEvent.InvalidTicketAcknowledgement);
            _CabinetEventMap.Add(EventCodes.LicenseKeyDetected, EgmEvent.LicenseKeyDetected);
            _CabinetEventMap.Add(EventCodes.SystemLockupUserResponse, EgmEvent.SystemLockUpResponse);
            _CabinetEventMap.Add(EventCodes.EGMTransactionDeniedCreditLimitReached, EgmEvent.TransactionDeniedCreditLimitReached);
            _CabinetEventMap.Add(EventCodes.CashBoxCleared, EgmEvent.CashBoxCleared);
            _CabinetEventMap.Add(EventCodes.ReservedNewPIDSessionStarted, EgmEvent.ReservedNewPIDSessionStarted);
            _CabinetEventMap.Add(EventCodes.PIDSessionStopped, EgmEvent.PIDSessionStopped);
            _CabinetEventMap.Add(EventCodes.PeriodMetersReset, EgmEvent.PeriodMetersReset);
            
        }

        [SuppressMessage("BallyTech.FxCop.Repeatability", "BR0002", Justification = "Instance creation not required. Hence making it static")]
        public static EgmEvent? GetFault(EventCodes eventCode)
        {
            return _CabinetEventMap.ContainsKey(eventCode) ? _CabinetEventMap[eventCode] : null;
        }
    }

    public static class CoinFaultEventMapping
    {
        private static Dictionary<EventCodes, EgmEvent?> _CoinFaultEventMap = new Dictionary<EventCodes, EgmEvent?>();

        static CoinFaultEventMapping()
        {
            _CoinFaultEventMap.Add(EventCodes.CoinInYoYo, EgmEvent.ReverseCoinInDetected);
            _CoinFaultEventMap.Add(EventCodes.CoinInFault, EgmEvent.CoinInFault);            
            _CoinFaultEventMap.Add(EventCodes.ExcessiveCoinRejectsFault, EgmEvent.CoinInLockoutMalfunction);
        }

        [SuppressMessage("BallyTech.FxCop.Repeatability", "BR0002", Justification = "Instance creation not required. Hence making it static")]
        public static EgmEvent? GetFault(EventCodes eventCode)
        {
            return _CoinFaultEventMap.ContainsKey(eventCode) ? _CoinFaultEventMap[eventCode] : null;
        }
    }

    public static class HopperEventFaultMapping
    {
        private static Dictionary<EventCodes, EgmEvent?> _HopperFaultEventMap = new Dictionary<EventCodes, EgmEvent?>();

        static HopperEventFaultMapping()
        {
            _HopperFaultEventMap.Add(EventCodes.HopperEmpty, EgmEvent.HopperEmpty);
            _HopperFaultEventMap.Add(EventCodes.HopperJammed, EgmEvent.HopperJammed);
            _HopperFaultEventMap.Add(EventCodes.HopperRunaway, EgmEvent.HopperRunaway);
            _HopperFaultEventMap.Add(EventCodes.HopperRefillRecorded, EgmEvent.HopperRefillRecorded);
            _HopperFaultEventMap.Add(EventCodes.HopperDisconnected, EgmEvent.HopperDisconnected);
            _HopperFaultEventMap.Add(EventCodes.HopperLevelMismatch, EgmEvent.HopperLevelMismatch);
            _HopperFaultEventMap.Add(EventCodes.HopperPayout, EgmEvent.HopperPayout);
            _HopperFaultEventMap.Add(EventCodes.HopperCalibrated, EgmEvent.HopperCalibrated);
            _HopperFaultEventMap.Add(EventCodes.HopperOverpayAmount, EgmEvent.HopperOverPayAmount);
        }

        [SuppressMessage("BallyTech.FxCop.Repeatability", "BR0002", Justification = "Instance creation not required. Hence making it static")]
        public static EgmEvent? GetFault(EventCodes eventCode)
        {
            return _HopperFaultEventMap.ContainsKey(eventCode) ? _HopperFaultEventMap[eventCode] : null;
        }
    }

    public static class NoteAcceptorEventFaultMapping
    {
        private static Dictionary<EventCodes, EgmEvent?> _NoteAcceptorFaultEventMap = new Dictionary<EventCodes, EgmEvent?>();

        static NoteAcceptorEventFaultMapping()
        {
            _NoteAcceptorFaultEventMap.Add(EventCodes.CashBoxOpticDiverterFault, EgmEvent.DiverterMalfunction);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteStackerFull, EgmEvent.NoteStackerFull);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteAcceptorFault, EgmEvent.NoteAcceptorFault);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteAcceptorDisconnected, EgmEvent.NoteAcceptorDisconnected);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteStackerFullFault, EgmEvent.CashboxFull);            
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteAcceptorYoYo, EgmEvent.ReverseBillDetected);
            _NoteAcceptorFaultEventMap.Add(EventCodes.EgmNoteAcceptorStackerRemoved, EgmEvent.CashboxRemoved);
            _NoteAcceptorFaultEventMap.Add(EventCodes.EgmNoteAcceptorStackerReturned, EgmEvent.CashboxInstalled);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteStackerCleared, EgmEvent.NoteStackerCleared);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteAcceptorJammed, EgmEvent.BillJam);
            _NoteAcceptorFaultEventMap.Add(EventCodes.NoteStackerHighLevelWarning, EgmEvent.NoteStackerHighLevelWarning);
            _NoteAcceptorFaultEventMap.Add(EventCodes.ExcessiveNoteTicketAcceptorRejectsFault, EgmEvent.TooManyBillsRejected);  
        }

        [SuppressMessage("BallyTech.FxCop.Repeatability", "BR0002", Justification = "Instance creation not required. Hence making it static")]
        public static EgmEvent? GetFault(EventCodes eventCode)
        {
            return _NoteAcceptorFaultEventMap.ContainsKey(eventCode) ? _NoteAcceptorFaultEventMap[eventCode] : null;
        }
    }

    public static class PrinterFaultEventsMapping
    {
        private static Dictionary<EventCodes,EgmEvent?> _PrinterFaultEventMap = new Dictionary<EventCodes, EgmEvent?>();

        static PrinterFaultEventsMapping()
        {
            _PrinterFaultEventMap.Add(EventCodes.TicketPrinterInkLow,EgmEvent.TicketPrinterInkLow);
            _PrinterFaultEventMap.Add(EventCodes.TicketPrinterPaperJam,EgmEvent.PrinterCarriageJam);
            _PrinterFaultEventMap.Add(EventCodes.TicketPrinterpaperLow,EgmEvent.PrinterPaperLow);
            _PrinterFaultEventMap.Add(EventCodes.TicketPrinterPaperOut,EgmEvent.PrinterPaperOut);
            _PrinterFaultEventMap.Add(EventCodes.TicketPrinterGeneralFault,EgmEvent.PrinterCommunicationError);
            _PrinterFaultEventMap.Add(EventCodes.EGMTicketInAborted, EgmEvent.TicketInAborted);
            _PrinterFaultEventMap.Add(EventCodes.EGMTicketInTimeOut, EgmEvent.TicketInTimeOut);
        }

        [SuppressMessage("BallyTech.FxCop.Repeatability", "BR0002", Justification = "Instance creation not required. Hence making it static")]
        public static EgmEvent? GetFault(EventCodes eventCodes)
        {
            return _PrinterFaultEventMap.ContainsKey(eventCodes) ? _PrinterFaultEventMap[eventCodes] : null;
        }
    }
}
