using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using log4net;
using BallyTech.Gtm.Core;
using BallyTech.Gtm;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Interaction
{
    [GenerateICSerializable]
    public partial class QComActionProcessor : ActionProcessorBase
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(QComActionProcessor));

        private QComModel _Model;
        public QComModel Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        [ActionMethod]
        public void SetQComDetails(uint assetNumber, decimal manufacturerId, decimal serialNumber)
        {         
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Entered Serial Number: {0}; Manufacture Id: {1}; Asset Number: {2}", serialNumber, manufacturerId, assetNumber);

            Model.SetEgmConfiguration(assetNumber, manufacturerId, serialNumber);
        }

        [ActionMethod]
        public void SetAssetNumber(uint number)
        {
            if (_Log.IsInfoEnabled)
                _Log.InfoFormat("Setting Asset Number: {0}", number);

            Model.SetAssetNumber(number);
        }

    }
}
