using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Specifications;
using BallyTech.Utility.Configuration;
using BallyTech.Utility;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class ProgressiveMetersResponseProcessor :MessageProcessor
    {
        private QComResponseSpecification _ProgressiveMetersValidationSpecification = null;

        public override void Process(ProgressiveMetersV16Response applicationMessage)
        {
            Meter oldMeter = Meter.Zero;
            bool isMessageValid = true;

            Model.GameMeterRequestor.ProgressiveMetersResponseReceived(applicationMessage.GameDetails.GameVersionNumber);
            isMessageValid = Model.UpdateLinkedProgressiveContributionAmount(applicationMessage.GameDetails, out oldMeter);

            _ProgressiveMetersValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.ProgressiveMetersResponse);
            if (!_ProgressiveMetersValidationSpecification.IsSatisfiedBy(applicationMessage)) isMessageValid = false;

            if (!isMessageValid)
            {
                if (applicationMessage.HasLpLevels())
                    BuildAndReportLpContributionIgnoredEvent(applicationMessage, oldMeter);
                else
                    Model.BuildAndReportInvalidProgressiveConfigEvent(applicationMessage.GameDetails.GameVersionNumber,
                                                                      applicationMessage.GameDetails.ProgressiveGroupId);
                return;            
            }

            ProgressiveLevelValidationStatus status = _ProgressiveMetersValidationSpecification.GetProgressiveValidationStatus(applicationMessage);

            if (status == ProgressiveLevelValidationStatus.InvalidLPLevel)
            {
                BuildAndReportLpContributionIgnoredEvent(applicationMessage, oldMeter);
                return;
            }

            if (status == ProgressiveLevelValidationStatus.InvalidSAPLevel)
            {
                Model.BuildAndReportInvalidProgressiveConfigEvent(applicationMessage.GameDetails.GameVersionNumber,
                                                                  applicationMessage.GameDetails.ProgressiveGroupId);
                return;
            }

            var game = Model.Egm.Games.Get(applicationMessage.GameDetails.GameVersionNumber);
           
            applicationMessage.UpdateProgressiveLevelInfo(game);
            if (IsSharedProgressiveGame())
                applicationMessage.SetProgressiveLevelAmount(Model.Egm.Games);                

            Model.Egm.ProgressiveMetersReceived();
        }

        private bool IsSharedProgressiveGame()
        {            
            var currentEgmConfiguration = Model.ConfigurationRepository.CurrentEgmConfiguration;
            return (currentEgmConfiguration.IsSharedProgressiveComponentSupported);
        }


        public override void Process(ProgressiveMetersV15Response applicationMessage)
        {            
            Meter oldMeter = Meter.Zero;
            bool isMessageValid = true;

            Model.GameMeterRequestor.ProgressiveMetersResponseReceived(applicationMessage.GameDetails.GameVersionNumber);
            isMessageValid = Model.UpdateLinkedProgressiveContributionAmount(applicationMessage.GameDetails, out oldMeter);

            _ProgressiveMetersValidationSpecification = Model.SpecificationFactory.GetSpecification(FunctionCodes.ProgressiveMetersResponse);
            if (!_ProgressiveMetersValidationSpecification.IsSatisfiedBy(applicationMessage)) isMessageValid = false;

            if (!isMessageValid)
            {
                if(applicationMessage.HasLpLevels())   
                    BuildAndReportLpContributionIgnoredEvent(applicationMessage, oldMeter);
                else
                    Model.BuildAndReportInvalidProgressiveConfigEvent(applicationMessage.GameDetails.GameVersionNumber,
                                                                      applicationMessage.GameDetails.ProgressiveGroupId);
                return;
            }

            ProgressiveLevelValidationStatus status = _ProgressiveMetersValidationSpecification.GetProgressiveValidationStatus(applicationMessage);

            if (status == ProgressiveLevelValidationStatus.InvalidLPLevel)
            {
                BuildAndReportLpContributionIgnoredEvent(applicationMessage, oldMeter);
                return;
            }

            if (status == ProgressiveLevelValidationStatus.InvalidSAPLevel)
            {
                Model.BuildAndReportInvalidProgressiveConfigEvent(applicationMessage.GameDetails.GameVersionNumber,
                                                                  applicationMessage.GameDetails.ProgressiveGroupId);
                return;
            }

            var game = Model.Egm.Games.Get(applicationMessage.GameDetails.GameVersionNumber);

            applicationMessage.UpdateProgressiveLevelInfo(game);
            Model.Egm.ProgressiveMetersReceived();
        }


        public void BuildAndReportLpContributionIgnoredEvent(ProgressiveMetersResponse response, Meter oldMeter)
        {
            Model.BuildAndReportLpContributionIgnoredEvent(response.GameDetails, oldMeter);
        }        
    }
}
