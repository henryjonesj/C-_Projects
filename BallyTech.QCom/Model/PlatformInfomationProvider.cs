using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class PlatformInfomationProvider : IPlatformInfoObserver
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof (PlatformInfomationProvider));

        private IPlatformInfo _PlatformInfo;

        private QComModel _Model = null;
        private Scheduler _Scheduler = null;

        public PlatformInfomationProvider()
        {
        }

        public PlatformInfomationProvider(QComModel model)
        {
            _Model = model;
            _Scheduler = new Scheduler(model.Schedule);
            _Scheduler.TimeOutAction += CheckRepositoryForEgmDetails;
        }

        private void CheckRepositoryForEgmDetails()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Have not received Save data modified event");

            _Scheduler.Stop();

            RestoreAndUpdateEgmDetails();
            _Model.IsConfigurationRequired = _Model.EgmDetails.AssetNumber == 0;
        }


        public IPlatformInfo PlatformInfo
        {
            get { return _PlatformInfo; }
            set
            {
                if (_PlatformInfo != null)
                    _PlatformInfo.RemoveObserver(this);

                _PlatformInfo = value;

                if (_PlatformInfo == null) return;

                _PlatformInfo.AddObserver(this);
                _Scheduler.Start(TimeSpan.FromSeconds(10));
            }
        }


        public void RestoreAssetNumberFromRepository()
        {
            try
            {
                string assetNumber = string.Empty;
                if (PlatformInfo.SavedData.TryGetValue("AssetNumber", out assetNumber))
                {
                    var egmDetails = _Model.EgmDetails;
                    egmDetails.AssetNumber = uint.Parse(assetNumber);
                    SetEgmInfo(egmDetails);
                }
            }
            catch (Exception ex)
            {
                if (_Log.IsDebugEnabled) _Log.DebugFormat("Failed to get asset number : {0}", ex.Message);
            }
        }

        private bool TryRestoreData(string key,out string value)
        {
            return PlatformInfo.SavedData.TryGetValue(key, out value);
        }



        public bool RestoreEgmDetailsFromRepository(out string serialNumber, out string manufacturerId)
        {
            string assetNumber = string.Empty;

            try
            {
                if (TryRestoreData("AssetNumber", out assetNumber))
                    _Model.EgmDetails.AssetNumber = uint.Parse(assetNumber);

                if (TryRestoreData("SerialNumber", out serialNumber) && TryRestoreData("ManufacturerId", out manufacturerId))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (_Log.IsDebugEnabled) _Log.DebugFormat("Failed to get details : {0}", ex.Message);
            }

            serialNumber = string.Empty;
            manufacturerId = string.Empty;

            return false;
        }


        internal void SaveDetailsInRepository(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            if (_Log.IsInfoEnabled) _Log.Info("Saving the Egm information");

            var egmDetails = UpdateEgmDetails(assetNumber, manufactureId, serialNumber);

            var saveData = new SerializableDictionary<string, string>();

            saveData["AssetNumber"] = Convert.ToString(assetNumber);
            saveData["ManufacturerId"] = Convert.ToString(manufactureId);
            saveData["SerialNumber"] = Convert.ToString(serialNumber);

            try
            {
                SetEgmInfo(egmDetails);
                PlatformInfo.SaveData(saveData);

                if (_Log.IsInfoEnabled)
                    _Log.InfoFormat("Saved details are, Asset Number:{0}, ManufacturerId:{1}, SerialNumber:{2}",
                                    assetNumber, manufactureId, serialNumber);
            }
            catch (Exception ex)
            {
                if (_Log.IsDebugEnabled) _Log.DebugFormat("Failed to set details : {0}", ex.Message);
            }
        }

        private void SetEgmInfo(EgmInfo egmDetails)
        {
            _Model.Egm.SetEgmInfo(egmDetails);
            _Model.IsConfigurationRequired = _Model.EgmDetails.AssetNumber == 0;

            _Model.State.RequestConfigurationIfNecessary();

        }

        private EgmInfo UpdateEgmDetails(uint assetNumber, decimal manufactureId, decimal serialNumber)
        {
            var egmDetails = _Model.EgmDetails;

            egmDetails.AssetNumber = (assetNumber == 0) ? egmDetails.AssetNumber : assetNumber;
            egmDetails.ManufacturerId = (byte)manufactureId;
            egmDetails.SerialNumber = (serialNumber == 0) ? egmDetails.SerialNumber : serialNumber;
            return egmDetails;
        }

        public void RestoreAndUpdateEgmDetails()
        {
            var serialNumber = string.Empty;
            var manufactuerId = string.Empty;

            if(!RestoreEgmDetailsFromRepository(out serialNumber, out manufactuerId)) return;

            UpdateEgmDetails(_Model.EgmDetails.AssetNumber, decimal.Parse(manufactuerId), decimal.Parse(serialNumber));

            RestoreAssetNumberFromRepository();
        }



        #region IPlatformInfoObserver Members

        public void IPAddressChanged(string newip)
        {
            
        }

        public void VendorStringChanged(string newVendorString)
        {
           
        }

        public void SaveDataChanged(bool isDirty)
        {
            _Scheduler.Stop();

            var egmDetails = _Model.EgmDetails;
            if(egmDetails.AssetNumber > 0) return;

            if (_Log.IsInfoEnabled)
                _Log.Info("Attempting to restore the asset number from the repository");

            RestoreAndUpdateEgmDetails();

            _Model.IsConfigurationRequired = egmDetails.AssetNumber == 0;
        }

        public void MacAddressChanged(string newMacAddress)
        {
           
        }

        #endregion
    }


    
}
