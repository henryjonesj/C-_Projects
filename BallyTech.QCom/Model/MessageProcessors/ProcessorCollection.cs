using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class ProcessorCollection
    {
        internal SerializableList<MessageProcessor> _ProcessorCollection = new SerializableList<MessageProcessor>();

        public ProcessorCollection() { }

        public ProcessorCollection(QComModel model)
        {
            _ProcessorCollection.Add(new GeneralResponseListener() { Model = model });
            _ProcessorCollection.Add(new ElectronicCreditTransferResponseProcessor() {Model = model});
            _ProcessorCollection.Add(new EgmConfigurationProcessor(model));
            _ProcessorCollection.Add(new MultiGameVariationMetersProcessor() { Model = model });
            _ProcessorCollection.Add(new ProgressiveMetersResponseProcessor() { Model = model });
            _ProcessorCollection.Add(new NoteAcceptorStatusResponseProcessor(model));
            _ProcessorCollection.Add(new PurgeEventsAckResponseProcessor() { Model = model });
        }

        public void Dispatch(ApplicationMessage response)
        {
            foreach (MessageProcessor ResponseListener in _ProcessorCollection)
            {
                response.Dispatch(ResponseListener);
            }
        }
    }
}
