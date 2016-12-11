using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.QCom.Messages;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Configuration
{
    [GenerateICSerializable]
    public partial class QComConfigurationCollection : SerializableKeyedCollection<QComConfigurationId,IQComConfiguration>
    {
        protected override QComConfigurationId GetKeyForItem(IQComConfiguration item)
        {
            return item.Id;
        }


        public void Update<TConfiguration>(QComConfiguration<TConfiguration> configuration) where TConfiguration : class//, IEgmConfiguration, IGameConfiguration 
        {
            var qComConfiguration = IsConfigurationAvailable(configuration);

            if (qComConfiguration != null)
                (qComConfiguration as QComConfiguration<TConfiguration>).Update(configuration);
            else
                Add(configuration);
        }

        public IQComConfiguration IsConfigurationAvailable<TConfiguration>(QComConfiguration<TConfiguration> configuration) where TConfiguration: class 
        {
            return this.Items.FirstOrDefault((element) => element.Id.Equals(configuration.Id));
        }

        public IQComConfiguration GetNextConfigurationInfo()
        {
            return this.Items.FirstOrDefault((element) => element.ConfigurationStatus == EgmGameConfigurationStatus.None);
        }

        public IEnumerable<TConfiguration> GetConfigurations<TConfiguration>() where TConfiguration : class
        {
            return this.Items.OfType<TConfiguration>();
        }

        public TConfiguration GetConfiguration<TConfiguration>() where TConfiguration : class
        {
            return this.Items.OfType<TConfiguration>().FirstOrDefault();
        }

        public TConfiguration GetPendingConfiguration<TConfiguration>() where TConfiguration : class
        {
            return
                this.Items.OfType<TConfiguration>().FirstOrDefault(
                    (item) => (item as IQComConfiguration).ConfigurationStatus == EgmGameConfigurationStatus.None);
        }

    }
    
}
