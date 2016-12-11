using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm.Core;
using BallyTech.Gtm;
using BallyTech.QCom.Model.Egm;
using BallyTech.Utility;
using log4net;

namespace BallyTech.QCom.Interaction
{
    public partial class EgmTiltScreen : Screen<ICoreGtmModel>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EgmTiltScreen));
        public override IDictionary<string, string> GetDisplay(ICoreGtmModel context)
        {

            var model = (EgmModel)context.Egm;
            var egmTiltHandler = model.EgmTiltHandler;

            if (egmTiltHandler.IsEgmInTiltCondition)
            {
                IDictionary<string, string> result = base.GetDisplay(context);
                result["IsEgmInTiltCondition"] = egmTiltHandler.IsEgmInTiltCondition.ToString();
                result["IsEgmConfigurationValid"] = (!model.GameLockedForInvalidConfiguration.LockValue).ToString();
                result["IsProcessorDoorAccessed"] = model.EgmAdapter.GameLockedOnUnauthorizedAccess.LockValue.ToString();
                result["TiltMessage"] = egmTiltHandler.CurrentTiltMessage;
                return result;
            }

            return null;
        }
    }
}
