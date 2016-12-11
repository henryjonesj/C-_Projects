using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm.Core;
using BallyTech.QCom.Model;
using BallyTech.Gtm;

namespace BallyTech.QCom.Interaction
{
    public interface IQComScreen : ICoreGtmModel
    {
        IQComModel QComModel { get; }
    }

    public class QComScreen : Screen<IQComScreen>
    {
        public override IDictionary<string, string> GetDisplay(IQComScreen context)
        {            
            IDictionary<string, string> result = base.GetDisplay(context);

            IQComModel Model = context.QComModel;

            var egmDetails = Model.EgmDetails;

            result["AssetNumber"] = egmDetails.AssetNumber.ToString();
            if (Model.IsConfigurationRequired)
            {
                result["ManufacturerId"] = egmDetails.ManufacturerId.ToString();
                result["SerialNumber"] = egmDetails.SerialNumber.ToString();

                var assetNumberNotAvailable = egmDetails.AssetNumber == 0;

                if (Model.IsEgmDetailsEntryRequired)
                    result["EgmDetailsEntryRequired"] = assetNumberNotAvailable.ToString();
                else
                    result["PromptAssetNumber"] = assetNumberNotAvailable.ToString();

               
            }

            result["ConfigurationStatus"] = Model.EgmConfigurationStatus.ToString();
            
            return result;
        }


        

    }
}
