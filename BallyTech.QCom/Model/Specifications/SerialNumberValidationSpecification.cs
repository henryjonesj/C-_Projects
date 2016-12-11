using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Configuration;
using BallyTech.Utility.Configuration;
using log4net;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class SerialNumberValidationSpecification : QComResponseSpecification
    {
        [AutoWire (Name="QComModel")]
        public QComModel Model { get; set; }
        private static readonly ILog _Log = LogManager.GetLogger(typeof(SerialNumberValidationSpecification));

        public override bool IsSatisfiedBy(decimal serialNumber, byte manufacturerId)
        {
            _Log.Info("Validating Serial Number and Manufacturer Id");
            
            var hostConfiguration = Model.ConfigurationRepository.GetConfigurationOfType<QComEgmConfiguration>();

            if (hostConfiguration == null) return true;

            IEgmConfiguration egmConfig = hostConfiguration.ConfigurationData;

            return egmConfig != null ?  (decimal.Parse(egmConfig.SerialNumber) == serialNumber &&
                                        byte.Parse(egmConfig.ManufacturerId) == manufacturerId) : true;
        }

        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.EgmConfigurationResponse; }
        }
    }
}
