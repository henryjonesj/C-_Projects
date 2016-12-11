using System.Linq;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using log4net;
using BallyTech.Gtm;
using System;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Configuration;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class ProgressiveMetersValidationSpecification: QComResponseSpecification
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(ProgressiveMetersValidationSpecification));

        [AutoWire(Name = "QComModel")]
        public QComModel Model { get; set; }

        public override bool IsSatisfiedBy(ProgressiveMetersV16Response response)
        {
            _Log.Info("Validating Progressive Meters Response");
            
            if (!IsGameInformationValid(response)) return false;

            if (!IsProgressiveV16InformationValid(response)) return false;

            return true;
        }
        
        public override bool IsSatisfiedBy(ProgressiveMetersV15Response response)
        {
            _Log.Info("Validating Progressive Meters Response");
            
            if (!IsGameInformationValid(response)) return false;

            if (!IsProgressiveV15InformationValid(response)) return false;

            return true;

        }

        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.ProgressiveMetersResponse; }
        }

        private bool IsProgressiveV16InformationValid(ProgressiveMetersV16Response response)
        {
            var game = Model.Egm.Games.Get(response.GameDetails.GameVersionNumber);

            if (game.ProgressiveLevelInfoCollection.Count != response.ProgressiveLevelNumber || response.ProgressiveLevelNumber > 8)
            {
                _Log.Info("Ignoring this response as Number of Progressive Levels is Invalid");
                return false;
            }

            var TotalSize = response.Size * response.ProgressiveLevelNumber;

            var ActualSize= Convert.ToInt32(response.TotalLength) -ProgressiveMetersV16Response.LengthofNonRepeatedEntries;

            if (response.Size < 15 || TotalSize < ActualSize)
            {
                _Log.Info("Ignoring this response as the size is Invalid");
                return false;
            
            }

            if (!(response.ProgressiveDataList.All(item => item.ProgressiveLevelData.ProgressiveLevelNumber < 8)))
            {
                _Log.Info("Ignoring this repsonse as the Level Id of Progressive Levels is Invalid");
                return false;
            }

            return true;
        }

        private bool IsProgressiveV15InformationValid(ProgressiveMetersV15Response response)
        {
            var game = Model.Egm.Games.Get(response.GameDetails.GameVersionNumber);

            if (game.ProgressiveLevelInfoCollection.Count != response.ProgressiveLevelNumber || response.ProgressiveLevelNumber > 8)
            {
                _Log.Info("Ignoring this response as Number of Progressive Levels is Invalid");
                return false;
            }

            if (!(response.ProgressiveDataList.All(item => item.ProgressiveLevelData.ProgressiveLevelNumber < 8)))
            {
                _Log.Info("Ignoring this repsonse as the Level Id of Progressive Levels is Invalid");
                return false;
            }

            return true;
        }

        public override ProgressiveLevelValidationStatus GetProgressiveValidationStatus(ProgressiveMetersV16Response response)
        {
            var game = Model.Egm.Games.Get(response.GameDetails.GameVersionNumber);

            foreach (var progressiveInfo in game.ProgressiveLevelInfoCollection.Values)
            {
                var progressiveInformation = response.ProgressiveDataList.FirstOrDefault(
                     (element) =>
                     (element.ProgressiveLevelData.ProgressiveLevelNumber + 1 == progressiveInfo.LineId) &&
                      (response.GetProgressiveType(element.ProgressiveLevelData.QComProgressiveType)
                      == progressiveInfo.ProgressiveType));

                if (progressiveInformation == null)
                {
                    _Log.Info("Ignoring this response as the Level does not match with that in the Egm Configuration");
                    return progressiveInfo.ProgressiveType == GameProgressiveType.LinkedProgressive ?
                        ProgressiveLevelValidationStatus.InvalidLPLevel :
                        ProgressiveLevelValidationStatus.InvalidSAPLevel;
                }
            }
            return ProgressiveLevelValidationStatus.Valid;
        }

        public override ProgressiveLevelValidationStatus GetProgressiveValidationStatus(ProgressiveMetersV15Response response)
        {
            var game = Model.Egm.Games.Get(response.GameDetails.GameVersionNumber);

            foreach (var progressiveInfo in game.ProgressiveLevelInfoCollection.Values)
            {
                var progressiveInformation = response.ProgressiveDataList.FirstOrDefault(
                     (element) =>
                     (element.ProgressiveLevelData.ProgressiveLevelNumber + 1 == progressiveInfo.LineId) &&
                      (response.GetProgressiveType(element.ProgressiveLevelData.QComProgressiveType)
                      == progressiveInfo.ProgressiveType));

                if (progressiveInformation == null)
                {
                    _Log.Info("Ignoring this response as the Level does not match with that in the Egm Configuration");
                    return progressiveInfo.ProgressiveType == GameProgressiveType.LinkedProgressive ?
                        ProgressiveLevelValidationStatus.InvalidLPLevel :
                        ProgressiveLevelValidationStatus.InvalidSAPLevel;
                }
            }
            return ProgressiveLevelValidationStatus.Valid;
        }


        private bool IsGameInformationValid(ProgressiveMetersResponse response)
        {
            var game = Model.Egm.Games.Get(response.GameDetails.GameVersionNumber);

            if (game == null)
            {
                _Log.Info("Ignoring this response as GVN is invalid");
                return false;
            }

            if (Convert.ToUInt16(game.ProgressiveGroupId) != response.GameDetails.ProgressiveGroupId)
            {
                _Log.Info("Ignoring this response as PGID is invalid");
                return false;

            }


            return true;
        }


    }
   

}