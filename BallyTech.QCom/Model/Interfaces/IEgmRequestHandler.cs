using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    public enum ResetStatus
    {
        Attempt,
        Success,
        Failure
    }
    
    public interface IEgmRequestHandler
    {
        void SetEnabledState(bool enabledState);
        void SetLockState(LockStateInfo lockStateInfo);
        void RequestMeters(MeterRequestInfo meterRequestInfo);
        void RequestAllMeters();
        void RequestGameLevelMetersForAllGames();
        void ResetJackpot(JackpotType jackpotType);
        void ClearEgmFaults();
        void ResetPSN(ResetStatus status);
        void Configure(IDenominationConfiguration configuration);
        void SetExternalJackpotInformation(ICollection<IExternalJackpotDisplayProfile> Profiles);
        void SetNoteAcceptorState(bool state);
    }


    [GenerateICSerializable]
    public partial class NullEgmRequestHandler : IEgmRequestHandler
    {

        #region IEgmRequestHandler Members

        public void SetEnabledState(bool enabledState)
        {
           
        }

        public void SetLockState(LockStateInfo lockStateInfo)
        {
            
        }

        public void RequestMeters(MeterRequestInfo meterRequestInfo)
        {
            
        }

        public void ResetJackpot(JackpotType jackpotType)
        {
           
        }

        public void ResetPSN(ResetStatus status)
        {
        
        }

        public void RequestAllMeters()
        {

        }

        public void RequestGameLevelMetersForAllGames()
        {

        }


        public void SetExternalJackpotInformation(ICollection<IExternalJackpotDisplayProfile> Profiles)
        {

        }

        public void Configure(IDenominationConfiguration configuration)
        {

        }

        public void ClearEgmFaults()
        {

        }

        public void SetNoteAcceptorState(bool state)
        {

        }

        #endregion

     
    }
   


}
