using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model;
using log4net;

namespace BallyTech.QCom.Messages
{
    partial class GeneralStatusResponse
    {   
        public bool IsMainLineCodeStateSet(EgmMainLineCurrentStatus state)
        {
            return this.State == state;
        }

        public bool IsIdleMode
        {
            get { return IsMainLineCodeStateSet(EgmMainLineCurrentStatus.IdleMode); }
        }

        public bool IsInNonFaultyMode
        {
            get { return !(IsAnyDoorOpened || IsFaultConditionActive || IsInLockupMode); }

        }

        private bool IsGeneralStatusFlagASet(GeneralStatusFlagA generalStatusFlagA)
        {
            return ((this.FlagA & generalStatusFlagA) == generalStatusFlagA);
        }

        private bool IsGeneralStatusFlagBSet(GeneralStatusFlagB generalStatusFlagB)
        {
            return ((this.FlagB & generalStatusFlagB) == generalStatusFlagB);
        }


        private bool IsAnyDoorOpened
        {
            get
            {
                var flagAs = default(GeneralStatusFlagA).GetAllValues();
                return flagAs.Where(flagA => flagA != GeneralStatusFlagA.None).Any(IsGeneralStatusFlagASet);
            }
        }

        private bool IsFaultConditionActive
        {
            get 
            { 
                var flagBs = default(GeneralStatusFlagB).GetAllValues();
                return flagBs.Where((flagB) => IsNotFaultConditionFlag(flagB)).Any(IsGeneralStatusFlagBSet);
            }
        }

        private bool IsNotFaultConditionFlag(GeneralStatusFlagB flagB)
        {
            return ((flagB != GeneralStatusFlagB.CashlessModeActive) && (flagB != GeneralStatusFlagB.None));
        }

        private bool IsInLockupMode
        {
            get
            {
                var lineCurrentStatuses = default(EgmMainLineCurrentStatus).GetAllValues();
                return lineCurrentStatuses.Where((status) => (IsEGMInLockupState(status))).Any(IsMainLineCodeStateSet);
            }
        }

        private bool IsEGMInLockupState(EgmMainLineCurrentStatus status)
        {
            return ((status != EgmMainLineCurrentStatus.None)
                    && (status != EgmMainLineCurrentStatus.IdleMode)
                    && (status != EgmMainLineCurrentStatus.PlayInProgress));
        }

        internal bool EgmInJackpotLockup
        {
            get { return (IsLargeWinLockup || IsResidualCancelCreditLockup || IsCancelCreditLockup); }
        }

        internal bool IsSystemLockup
        {
            get { return IsMainLineCodeStateSet(EgmMainLineCurrentStatus.SystemLockup); }
        }


        internal bool IsLargeWinLockup
        {
            get { return IsMainLineCodeStateSet(EgmMainLineCurrentStatus.LargeWinLockup); }
        }

        internal bool IsResidualCancelCreditLockup
        {
            get { return IsMainLineCodeStateSet(EgmMainLineCurrentStatus.ResidualCancelCreditLockup); }
        }

        internal bool IsCancelCreditLockup
        {
            get { return IsMainLineCodeStateSet(EgmMainLineCurrentStatus.CancelCreditLockup); }
        }

        internal bool IsLinkedProgressiveAwardLockup
        {
            get { return IsMainLineCodeStateSet(EgmMainLineCurrentStatus.LinkedProgressiveAwardLockup); }
        }


    }
}
