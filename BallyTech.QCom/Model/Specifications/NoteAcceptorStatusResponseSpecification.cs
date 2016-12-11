using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;
using BallyTech.QCom.Messages;
using BallyTech.QCom.Model.Builders;
using BallyTech.Utility.Configuration;

namespace BallyTech.QCom.Model.Specifications
{
    [GenerateICSerializable]
    public partial class NoteAcceptorStatusResponseSpecification : QComResponseSpecification
    {
        [AutoWire(Name="QComModel")]
        public QComModel Model { get; set; }

        public override bool IsSatisfiedBy(NoteAcceptorStatusResponse response)
        {
            return response.NoteAcceptorDescriptorString[39] == 0;
        }

        public override bool IsSatisfiedBy(NoteAcceptorFlagCharacteristics denominations)
        {
            NoteAcceptorFlagCharacteristics hostDenoms = QComConfigurationBuilder.UpdateNoteAcceptorFlagCharacteristics(Model.Egm.NoteAcceptorDevice.DenominationConfiguration.SupportedDenominations);
            return denominations == hostDenoms;
        }

        public override FunctionCodes FunctionCode
        {
            get { return FunctionCodes.NoteAcceptorStatusResponse; }
        }
    }
}
