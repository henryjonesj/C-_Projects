using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.QCom.Messages;
using BallyTech.Gtm;
using log4net;
using BallyTech.QCom.Model.Specifications;

namespace BallyTech.QCom.Model.MessageProcessors
{
    [GenerateICSerializable]
    public partial class NoteAcceptorStatusResponseProcessor : MessageProcessor
    {
        private static readonly ILog _Log = LogManager.GetLogger(typeof(NoteAcceptorStatusResponseProcessor));
        private QComResponseSpecification _Specification = null;
        private static string NADS = string.Empty;

        public NoteAcceptorStatusResponseProcessor()
        {
        }

        public NoteAcceptorStatusResponseProcessor(QComModel model)
        {
            Model = model;
        }

        public override void Process(NoteAcceptorStatusResponse response)
        {
            _Specification = Model.SpecificationFactory.GetSpecification(FunctionCodes.NoteAcceptorStatusResponse);
            if (_Specification.IsSatisfiedBy(response))
            {
                NADS = QComConvert.ConvertByteArrayToASCIIString(response.NoteAcceptorDescriptorString.ToArray());
                _Log.InfoFormat("NADS Null Validation successful: {0}", NADS);
            }

            var DenominationConfiguraiton = Model.Egm.NoteAcceptorDevice.DenominationConfiguration;

            if (DenominationConfiguraiton == null) return;

            if (!_Specification.IsSatisfiedBy(response.NoteAcceptorMsbFlag))
            {
                Model.ConfigureNoteAcceptor(DenominationConfiguraiton);
                return;
            }

            var BillDenominations = new SerializableList<BillDenomination>();

            if ((response.NoteAcceptorMsbFlag & NoteAcceptorFlagCharacteristics.Five) == NoteAcceptorFlagCharacteristics.Five)
                BillDenominations.Add(BillDenomination.Bill5);

            if ((response.NoteAcceptorMsbFlag & NoteAcceptorFlagCharacteristics.Ten) == NoteAcceptorFlagCharacteristics.Ten)
                BillDenominations.Add(BillDenomination.Bill10);

            if ((response.NoteAcceptorMsbFlag & NoteAcceptorFlagCharacteristics.Hundred) == NoteAcceptorFlagCharacteristics.Hundred)
                BillDenominations.Add(BillDenomination.Bill100);

            if ((response.NoteAcceptorMsbFlag & NoteAcceptorFlagCharacteristics.Twenty) == NoteAcceptorFlagCharacteristics.Twenty)
                BillDenominations.Add(BillDenomination.Bill20);

            if ((response.NoteAcceptorMsbFlag & NoteAcceptorFlagCharacteristics.Fifty) == NoteAcceptorFlagCharacteristics.Fifty)
                BillDenominations.Add(BillDenomination.Bill50);

            Model.Egm.ReportNoteAcceptorStatus(NADS, BillDenominations);
        }

    }
}
