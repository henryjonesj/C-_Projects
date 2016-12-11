using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Model;

namespace BallyTech.QCom.Configuration
{
    public static class ConfigurationRequestHandlerFactory
    {

        internal static ConfigurationRequestHandlerBase GetRequestHandler(QComModel model )
        {
            if (model.IsRemoteConfigurationEnabled)
                return new RemoteConfigurationRequestHandler() {Model = model};

            return new AttendantConfigurationRequestHandler() {Model = model};
        }

    }
}
