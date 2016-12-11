using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Management;
using BallyTech.Utility.Serialization;
using log4net;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class RamClearDetector
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(RamClearDetector));

        private EgmAdapter _EgmAdapater = null;
        private bool _IsRamClearDetected = false;

        public RamClearDetector()
        {
        }

        public RamClearDetector(EgmAdapter egmAdapter)
        {
            _EgmAdapater = egmAdapter;
        }

        internal bool HasRamClearAlreadyDetected
        {
            get { return _IsRamClearDetected; }
        }

        internal void EgmResetReceived()
        {
            if (HasRamClearAlreadyDetected)
            {
                if (_Log.IsInfoEnabled) _Log.Info("Ram Clear Already Detected");
                return;
            }

            ProcessRamClear();
        }

        private void ProcessRamClear()
        {
            if (_Log.IsInfoEnabled) _Log.Info("Processing Ram Clear Event");
            

            _EgmAdapater.CabinetDevice.Process(EgmEvent.LifetimeMetersReset);
            _EgmAdapater.ResetCachedMeters();
            _EgmAdapater.ResetAll();
            _IsRamClearDetected = true;
        }

        internal void ValidDataReceived()
        {
            _IsRamClearDetected = false;
        }


    }
}

