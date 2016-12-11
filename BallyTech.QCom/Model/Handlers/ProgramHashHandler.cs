using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class ProgramHashHandler
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ProgramHashHandler));       
        internal QComModel Model { get; set; }

       internal bool CanProcessProgramHashResponse(ApplicationMessage response,Request request)
       {
           if (!(response is ProgramHashResponse)) return true;

           if (response.CanAcceptResponse(request) && Model.Egm.SoftwareAuthenticationDevice.IsRomSignatureVerificationInProgress)
               return true;

           var ProgramHashResponse= response as ProgramHashResponse;

           ReportUnexpectedProgramHash(ProgramHashResponse.ProgramHash);

           return false;

       }

       private void ReportUnexpectedProgramHash(byte[] signature)
       {
           if (_Log.IsInfoEnabled) _Log.Info("Unexpected Program Hash Detected");

           Model.Egm.ExtendedEventData = new ExtendedEgmEventData()
           {
               Seed= Enumerable.Repeat(0,20).Select(x=> (byte)x).ToArray(),
               Signature = signature
           };

           Model.Egm.ReportEvent(EgmEvent.UnsolicitedRomSignatureResponse);

       }


    
    
    }
}
