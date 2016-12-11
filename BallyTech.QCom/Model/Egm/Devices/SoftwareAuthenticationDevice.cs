using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Gtm;
using BallyTech.Gtm.Core;
using BallyTech.Utility.Collections;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class SoftwareAuthenticationDevice : Device, ISoftwareAuthentication 
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(SoftwareAuthenticationDevice));
        private ISoftwareAuthenticationInfo _SoftwareAuthenticationInfo;

        private bool _IsRomSignatureVerificationComplete = false;

        internal bool IsRomSignatureVerificationComplete
        {
            get { return Model.EgmAdapter.IsRomSignatureVerificationEnabled ? _IsRomSignatureVerificationComplete : true; }

        }

        internal bool IsRomSignatureVerificationInProgress { get { return _SoftwareAuthenticationInfo != null; } }


        private void InitiateSoftwareAuthentication(ISoftwareAuthenticationInfo softwareAuthenticationInfo)
        {
            if (_Log.IsInfoEnabled) _Log.Info("Initiated Software Authentication");

            if (Model.EgmAdapter.SoftwareAuthenticationDevice.IsMeterExclusionRequired)
                Model.EgmAdapter.Devices.NotifyRomSignatureInitiated();

            if(Model.LinkStatus == LinkStatus.Disconnected)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Game link is down");
                Model.Observers.SoftwareVerificationCompleted(new EgmSoftwareAuthentication(softwareAuthenticationInfo.Signature, SoftwareVerificationCompletionStatus.GameOffline));
                NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus.GameOffline);
                return;
            }
            _IsRomSignatureVerificationComplete = false;

            _SoftwareAuthenticationInfo = softwareAuthenticationInfo;

            Model.EgmAdapter.OnInitiateRomSignatureVerification(softwareAuthenticationInfo);
        }

        public void Process(byte[] signature)
        {
            if (_SoftwareAuthenticationInfo == null)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Skipping duplicate signature");
                return;
            }

            signature = signature.Reverse().ToArray();
            byte[] finalEgmSignature = signature;
            if(_SoftwareAuthenticationInfo.Signature.Length < signature.Length)
                finalEgmSignature = signature.Skip(signature.Length - _SoftwareAuthenticationInfo.Signature.Length).ToArray();

            if (!CompareSignature(finalEgmSignature, _SoftwareAuthenticationInfo.Signature))
            {
                if (_Log.IsInfoEnabled) _Log.Info("Signature validation failed");
                Model.Observers.SoftwareVerificationCompleted(new EgmSoftwareAuthentication(finalEgmSignature, SoftwareVerificationCompletionStatus.SignatureVerificationFailed));
                NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus.SignatureVerificationFailed);
                _SoftwareAuthenticationInfo = null;
                return;
            } 
            
            if (_Log.IsInfoEnabled) _Log.Info("Signature validation Success");

            Model.Observers.SoftwareVerificationCompleted(new EgmSoftwareAuthentication(finalEgmSignature, SoftwareVerificationCompletionStatus.Success));

            NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus.Success);

            _IsRomSignatureVerificationComplete = true;
            Model.RequestAllMeters();

            _SoftwareAuthenticationInfo = null;

            if (Model.ShouldNotifyMeterInitialization)
            {
                Model.EgmRequestHandler.RequestGameLevelMetersForAllGames();
                return;
            }

            Model.EgmAdapter.FetchCurrentGameMeters();
        }

        private bool CompareSignature(byte[] egmSignature, byte[] hostSignature)
        {

            if (_Log.IsInfoEnabled) _Log.InfoFormat("Host Signature {0} - Egm Signature {1}", ArrayUtil.HexDump(hostSignature), ArrayUtil.HexDump(egmSignature));
            return egmSignature.AsEnumerable().SequenceEqual(hostSignature.AsEnumerable());
        }

        private void NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus status)
        {
            Model.EgmAdapter.Devices.NotifyRomSignatureStatus(status);

        }

        public void OnGameLinkStatusChanged()
        {
            if (_SoftwareAuthenticationInfo == null) return;

            if (Model.LinkStatus == LinkStatus.Disconnected)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Game got disconnected");
                Model.Observers.SoftwareVerificationCompleted(new EgmSoftwareAuthentication(_SoftwareAuthenticationInfo.Signature, SoftwareVerificationCompletionStatus.GameOffline));
                NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus.GameOffline);
            }
        }

        internal void ResetRomSignatureCompletionStatus()
        {
            _IsRomSignatureVerificationComplete = false;
        }



        #region ISoftwareAuthentication Members

        public void Initiate(ISoftwareAuthenticationInfo softwareAuthenticationInfo)
        {   
            InitiateSoftwareAuthentication(softwareAuthenticationInfo);
        }

        public void Cancel()
        {
            if (_Log.IsWarnEnabled)
                _Log.Warn("Cancelling the current software verification process");

            _SoftwareAuthenticationInfo = null;
             NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus.GameOffline);
            
            ResetRomSignatureCompletionStatus();
        }

        public bool IsMeterExclusionRequired { get; set; }
      

        #endregion



      

      
    }
}
