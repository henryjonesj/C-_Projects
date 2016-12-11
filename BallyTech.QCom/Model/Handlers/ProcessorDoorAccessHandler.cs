using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;

namespace BallyTech.QCom.Model.Handlers
{
    [GenerateICSerializable]
    public partial class ProcessorDoorAccessHandler
    {
        internal QComModel Model { get; set; }
        
        public ProcessorDoorAccessHandler()
        {
        }

        public void HandleProcessorDoorAccess()
        {
           // .Model.Model.game
        }
    
    }
}
