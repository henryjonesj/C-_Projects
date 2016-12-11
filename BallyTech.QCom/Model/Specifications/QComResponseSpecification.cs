using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using BallyTech.QCom.Model.Meters;

namespace BallyTech.QCom.Model.Specifications
{
    public enum ProgressiveLevelValidationStatus
    {
        Valid,
        InvalidSAPLevel,
        InvalidLPLevel
    }

    [GenerateICSerializable]
    public abstract partial class QComResponseSpecification
    {
        public abstract FunctionCodes FunctionCode { get; }

        public virtual bool IsSatisfiedBy(NoteAcceptorStatusResponse response)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(NoteAcceptorFlagCharacteristics denominations)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(decimal serialNumber, byte manufacturerId)
        {
            return true;
        }

		 public virtual bool IsSatisfiedBy(ProgressiveMetersV15Response response)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(ProgressiveMetersV16Response response)
        {
            return true;
        }

        public virtual ProgressiveLevelValidationStatus GetProgressiveValidationStatus(ProgressiveMetersV15Response response)
        {
            return ProgressiveLevelValidationStatus.Valid;
        }

        public virtual ProgressiveLevelValidationStatus GetProgressiveValidationStatus(ProgressiveMetersV16Response response)
        {
            return ProgressiveLevelValidationStatus.Valid;
        }

        public virtual bool IsSatisfiedBy(Event response)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(QComRawDateTime item)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(EventCodes Event)
        {
            return true;
        }


        public virtual bool IsSatisfiedBy(ProgressiveConfigurationResponse response)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(SerializableList<ProgressiveConfigurationGroup> ProgressiveConfigurationGroup)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(EgmGameConfigurationResponse response)
        {
            return true;
        }
		
        public virtual bool IsSatisfiedBy(LPContribution item)
        {
            return true;
        }

        public virtual bool IsSatisfiedBy(MeterGroupContributionResponse item)
        {
            return true;
        }

        public virtual UnreasonableMeterIncrementTestResult GetMeterValidationStatus(MeterInfo meterInfo)
        {
            return new UnreasonableMeterIncrementTestResult(MeterValidationResult.Success);
        }

        public virtual bool IsGameMeterValid(MeterId meterId, Meter currentMeter, Meter newMeter)
        {
            return true;
        }

        public virtual bool IsGameInfoValid(int gameVersion, byte gameVariation)
        {
            return true;
        }
    }
}
