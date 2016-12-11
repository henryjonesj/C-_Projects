using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using BallyTech.Utility;
using BallyTech.QCom.Model.Egm;
using log4net;
using BallyTech.QCom.Model.Specifications;
using BallyTech.Utility.Configuration;

namespace BallyTech.QCom.Model.Meters
{
    [GenerateICSerializable]
    public partial class MeterTracker
    {
        private readonly static ILog _Log = LogManager.GetLogger(typeof(MeterTracker));

        private QComResponseSpecification meterMovementValidation = null;
        private MeterResetSpecification _MeterResetSpecification = null;

        private EgmCentsMetersValidator _EgmCentsMetersValidator = null;
        public EgmCentsMetersValidator EgmCentsMetersValidator 
        { 
            get { return _EgmCentsMetersValidator; } 
        }
        
        public Action<EgmEvent> MeterValidationSkipped = delegate { };

        private MeterGroupKeyedCollection _Meters = new MeterGroupKeyedCollection();
        public MeterGroupKeyedCollection Meters
        {
            get { return _Meters; }
            set { _Meters = value; }
        }

        public QComModel Model { get; set; }

        internal EgmAdapter Egm 
        {
            get { return Model.Egm; }
        }

        private SerializableDictionary<MeterCodes, SerializableList<Device>> _CentForCentValidators
                                            = new SerializableDictionary<MeterCodes, SerializableList<Device>>();
        public SerializableDictionary<MeterCodes, SerializableList<Device>> CentForCentValidators
        {
            get { return _CentForCentValidators; }
        }

        public void InitializeCentForCentValidators()
        {
            _CentForCentValidators[MeterCodes.TotalEgmCashlessCreditIn] = new SerializableList<Device> { Egm.TransferInDevice, Egm.LinkedProgressiveDevice };
            _CentForCentValidators[MeterCodes.TotalEgmCashlessCreditOut] = new SerializableList<Device> { Egm.TransferOutDevice };
            _CentForCentValidators[MeterCodes.TotalEgmCashTicketIn] = new SerializableList<Device> { Egm.TicketInDevice };
            _CentForCentValidators[MeterCodes.TotalEgmCashTicketOut] = new SerializableList<Device> { Egm.TicketOutDevice };
            _CentForCentValidators[MeterCodes.TotalEgmLinkedProgressiveWins] = new SerializableList<Device> { Egm.LinkedProgressiveDevice };
        }

        public MeterTracker()
        {
            _MeterResetSpecification = new MeterResetSpecification() { MeterTracker = this };
            _EgmCentsMetersValidator = new EgmCentsMetersValidator() { MeterTracker = this };
            
        }

        internal void InvalidateMeters()
        {
            foreach (MeterGroup meterGroup in _Meters)            
                meterGroup.IsSynced = false;            
        }


        internal Meter GetMeterFor(MeterCodes meterCodes)
        {
            return !_Meters.Contains(meterCodes) ? (Meter) Meter.Zero : _Meters[meterCodes].Meter;
        }


        internal void ResetMeters()
        {
            _Meters.Clear();
        }


        private bool HaveMetersResetToZeroAndProcessed(SerializableList<MeterInfo> meters)
        {
            if (!_MeterResetSpecification.IsSatisfiedBy(meters)) return false;
                
            if (_Log.IsInfoEnabled) _Log.Info("Coin Meters reset to zero.Possible Egm Ram Clear");
            MeterValidationSkipped(EgmEvent.LifetimeMetersReset);
            ResetMeters();
            return true;
        }


        public void UpdateMeters(SerializableList<MeterInfo> meters)
        {
            if(HaveMetersResetToZeroAndProcessed(meters)) return;

            IExtendedEgmEventData ExtendedEventData=null;

            foreach (MeterInfo meterInfo in meters)
            {
                var validationResult = IsValid(meterInfo)._MeterValidationStatus;

                if (validationResult != MeterValidationResult.Success)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.InfoFormat("Meter movement validation failed for Metercode = {0}, Meter Value = {1} with result={2}",
                            meterInfo.MeterCode, meterInfo.RawValue, validationResult.ToString());

                    var newMeter = new Meter(meterInfo.RawValue, 1, uint.MaxValue + 1m);
                    var oldMeter = Meters.GetMeter(meterInfo.MeterCode);

                    ExtendedEventData = Model.Egm.ConstructUnreasonableMeterData(meterInfo.MeterCode, oldMeter, newMeter);
                }
                MeterCodes meterCode = meterInfo.MeterCode;

                _Meters.Remove(meterCode);

                _Meters.Add(new MeterGroup()
                {
                    MeterCode = meterCode,
                    Meter = new Meter(meterInfo.RawValue, 1, uint.MaxValue + 1m),
                    IsSynced = true
                });


                if (validationResult == MeterValidationResult.Success) continue;
                
                Model.OnMetersUpdated(meters);
                Model.Egm.ExtendedEventData = ExtendedEventData;

                if (validationResult == MeterValidationResult.CentForCentValidationFailure)
                    Egm.ReportErrorEvent(EgmErrorCodes.CentForCentMeterReconcilationFailure);
                else
                    Egm.ReportEvent(EgmEvent.InconsistentGameMeters);

            }
        }


        private UnreasonableMeterIncrementTestResult IsValid(MeterInfo meterInfo)
        {
            if (!_Meters.IsAvailable(meterInfo.MeterCode)) return BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.Success);

            if (!_Meters[meterInfo.MeterCode].IsSynced) return BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.Success);

            meterMovementValidation = Model.SpecificationFactory.GetSpecification(FunctionCodes.MeterGroupContributionResponse);
            return (meterMovementValidation.GetMeterValidationStatus(meterInfo));
        }

        internal UnreasonableMeterIncrementTestResult BuildUnreasonableMeterIncrementTestResult(MeterValidationResult meterValidationResult)
        {
            return new UnreasonableMeterIncrementTestResult(meterValidationResult);
        }

        internal UnreasonableMeterIncrementTestResult CheckAndBuildUnreasonableMeterIncrementTestResult(Meter newMeter,Meter oldMeter,decimal incrementThreshold)
        {
            return (newMeter - oldMeter).DangerousGetUnsignedValue() <= incrementThreshold
                 ? BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.Success)
                 : BuildUnreasonableMeterIncrementTestResult(MeterValidationResult.InconistentMeterValidationFailure);
        
        }

        
    }
}
