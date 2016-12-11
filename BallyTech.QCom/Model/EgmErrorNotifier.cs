using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.Utility.Collections;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class EgmErrorObserverCollection : ObserverCollection<IEgmErrorObserver>
    {
    }

    [GenerateICSerializable]
    public partial class EgmErrorNotifier : IExtendedEgm
    {
        private EgmErrorObserverCollection _EgmErrorObservers = new EgmErrorObserverCollection();
        public EgmErrorObserverCollection EgmErrorObservers
        {
            get { return _EgmErrorObservers; }
        }

        public void Notify(EgmErrorCodes errorCode)
        {
            _EgmErrorObservers.HandleEgmError(errorCode);
        }

        #region IObservableBy<IEgmErrorObserver> Members

        public void AddObserver(IEgmErrorObserver observer)
        {
            _EgmErrorObservers.AddObserver(observer);
        }

        public void RemoveObserver(IEgmErrorObserver observer)
        {
            _EgmErrorObservers.RemoveObserver(observer);
        }

        #endregion

        #region IExtendedEgm Members

        public void Configure(IEgmConfiguration egmConfiguration)
        {

        }

        public void ConfigureMaxCreditLimit(ICreditLimitConfiguration creditLimitConfiguration)
        {
            
        }

        #endregion

        #region IObservableBy<IExtendedEgmObserver> Members

        public void AddObserver(IExtendedEgmObserver observer)
        {
            
        }

        public void RemoveObserver(IExtendedEgmObserver observer)
        {
            
        }

        #endregion

        #region IExtendedEgm Members


        public IExtendedEgmEventData GetExtendedEventData()
        {
            return null;
        }

        public void ResetExtendedEventData()
        {
            
        }

        public void ForceRequestMeters()
        {
        
        }

        #endregion

        #region IExtendedEgm Members


        public Meter GetTotalPlayerInformationDisplayAccessedCount()
        {
            return Meter.NotAvailable;
        }

        #endregion

        #region IExtendedEgm Members


        public Meter GetLinkedProgressiveWageredAmount(int? gameVersion)
        {
            return Meter.NotAvailable;
        }

        #endregion
    }
}
