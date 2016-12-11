using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using BallyTech.Utility;

namespace BallyTech.QCom.Model
{
    [GenerateICSerializable]
    public partial class EgmTiltHandler : IBoundSlotObserver<bool>
    {
        private string currentTiltMessage;



        public string CurrentTiltMessage
        {
            get { return currentTiltMessage; }
        }

        public bool IsEgmInTiltCondition
        {
            get { return (currentTiltMessage != string.Empty); }
        }

        public void AddTilt(string tiltMessage)
        {
            currentTiltMessage = tiltMessage;
        }

        public void AddTilt(EgmEvent egmTilt)
        {
            AddTilt(egmTilt.ToString());
        }

        public void ClearTilt()
        {
            currentTiltMessage = string.Empty;
        }

        #region IBoundSlotObserver<bool> Members

        public void ValueChanged(BoundSlot<bool> sender, bool oldValue, bool newValue)
        {
            if (newValue) ClearTilt();
        }

        #endregion
    }
}
