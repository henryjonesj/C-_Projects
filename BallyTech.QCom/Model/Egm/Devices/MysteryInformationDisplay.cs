using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Specifications;
using log4net;
using BallyTech.Gtm.Core;

namespace BallyTech.QCom.Model.Egm.Devices
{
    [GenerateICSerializable]
    public partial class MysteryInformationDisplay : Device, IMysteryInformationDisplay
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(MysteryInformationDisplay));
        private const int MaxMysteryLines = 8;
        #region IMysteryInformationDisplay Members


        private SerializableList<ExternalJackpotDisplayProfile> _Profiles = new SerializableList<ExternalJackpotDisplayProfile>();
        public SerializableList<IExternalJackpotDisplayProfile> Profiles
        {
            get { return _Profiles.Cast<IExternalJackpotDisplayProfile>().ToSerializableList(); }
        }

        private ICollection<IProgressiveLine> _LinkedMysteryLines;
        public ICollection<IProgressiveLine> LinkedMysteryLines
        {
            get { return _LinkedMysteryLines as ICollection<IProgressiveLine>; }
        }

        public void CreateLinkedMysteryLines()
        {
            _LinkedMysteryLines = new SerializableList<IProgressiveLine>();

            for (int i = 0; i < MaxMysteryLines; i++) _LinkedMysteryLines.Add(new LinkedMysteryLine());
        }

        public void ActivateProfile(IExternalJackpotDisplayProfile Profile)
        {
            if (!IsValidProfile(Profile))
            {
                _Log.DebugFormat("Invalid profile received with LevelId = {0}", Profile.LevelId);
                return;
            }

            if (!UpdateLinkedMysteryLine(Profile)) return;

            _Log.Debug("Activating a Mystery Profile");
            UpdateProfiles(Profile);
            Model.EgmRequestHandler.SetExternalJackpotInformation(Profiles);            
        }

        public void DeactivateProfile(int ProgressiveGroupId)
        {
            ExternalJackpotDisplayProfile profile = _Profiles.FirstOrDefault((element) => element.ProgressiveGroupId == ProgressiveGroupId) as ExternalJackpotDisplayProfile;
            if (profile == null)
            {
                _Log.Debug("Received Deactivate Promotion which was not available!! Hence ignoring.");
                return;
            }

            profile.ReturnToPlayer = 0;
            RemoveLinkedMysteryLine(profile);
            Model.EgmRequestHandler.SetExternalJackpotInformation(new SerializableList<IExternalJackpotDisplayProfile>() { profile });
            _Profiles.Remove(profile);
            if (_Profiles.Count > 0) Model.EgmRequestHandler.SetExternalJackpotInformation(Profiles);
        }

        #endregion

        private bool IsValidProfile(IExternalJackpotDisplayProfile Profile)
        {
            return MysteryLevelValiditySpecification.IsSatisfiedBy(Profile.LevelId);
        }

        private void UpdateProfiles(IExternalJackpotDisplayProfile Profile)
        {
            ExternalJackpotDisplayProfile extJpProfile = new ExternalJackpotDisplayProfile(Profile);

            if (_Profiles.Count > 0)
            {
                ExternalJackpotDisplayProfile profile = _Profiles.FirstOrDefault((element) => element.ProgressiveGroupId == extJpProfile.ProgressiveGroupId);
                if (profile != null) _Profiles.Remove(profile);
            }
            _Profiles.Add(extJpProfile);
        }

        private bool RemoveLinkedMysteryLine(IExternalJackpotDisplayProfile Profile)
        {
            _Log.InfoFormat("Removing Mystery Line with Level Id: {0}", Profile.LevelId);

            _LinkedMysteryLines.Remove(_LinkedMysteryLines.FirstOrDefault(ln => ln.OptionalDetails.ProgressiveGroupId == Profile.ProgressiveGroupId));
            _LinkedMysteryLines.Add(new LinkedMysteryLine());
            return true;
        }

        private bool UpdateLinkedMysteryLine(IExternalJackpotDisplayProfile Profile)
        {
            _Log.InfoFormat("Updating Mystery Lines with Level Id: {0}", Profile.LevelId);

            var line = _LinkedMysteryLines.FirstOrDefault(ln => ln.OptionalDetails.ProgressiveGroupId == Profile.ProgressiveGroupId);
            if (line != null)
            {
                _Log.Debug("Updating an existing line"); //This is currently not supported by EBS
                _LinkedMysteryLines.Remove(line);
            }
            else
            {
                if (_LinkedMysteryLines.FirstOrDefault(ln => ln.LineId == 0) == null)
                {
                    _Log.Debug("All the 8 mystery lines are active. Cannot create a new one now. Hence ignoring!!");
                    return false;
                }
                _LinkedMysteryLines.Remove(_LinkedMysteryLines.FirstOrDefault(ln => ln.LineId == 0));
            }

            _LinkedMysteryLines.Add(new LinkedMysteryLine((byte)Profile.LevelId, new OptionalDetails() { ProgressiveGroupId = Profile.ProgressiveGroupId }) 
                                    );

            return true;
        }

        internal void InitiateHandpay(IFundsTransferAuthorization fundsTransferAuthorization)
        {
            if (_Log.IsInfoEnabled) _Log.Info("Mystery win reported as handpay");

            Model.Observers.FundsTransferCompleted(new CanceledFundsTransferCompletion()
            {
                InitiatingAuthorization = fundsTransferAuthorization,
                ResultCode = FundsTransferCompletionResultCode.CallAttendant
            });
        }
    }

    [GenerateICSerializable]
    public partial class ExternalJackpotDisplayProfile : IExternalJackpotDisplayProfile
    {
        public ExternalJackpotDisplayProfile() { }

        public ExternalJackpotDisplayProfile(IExternalJackpotDisplayProfile profile)
        {
            LevelId = profile.LevelId;
            LevelName = profile.LevelName;
            ProgressiveGroupId = profile.ProgressiveGroupId;
            ReturnToPlayer = profile.ReturnToPlayer;
        }

        #region IExternalJackpotDisplayProfile Members

        public int LevelId { get; set; }

        public string LevelName { get; set; }

        public int ProgressiveGroupId { get; set; }

        public decimal ReturnToPlayer { get; set; }

        #endregion
    }
}
