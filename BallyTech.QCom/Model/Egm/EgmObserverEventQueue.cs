using System;
using BallyTech.Gtm;
using BallyTech.Utility.Serialization;
using System.Linq;

namespace BallyTech.QCom.Model.Egm
{
    /// <summary>
    ///   Event-queuing proxy for IEgmObserver.
    /// </summary>
    [GenerateICSerializable]
    public partial class EgmObserverEventQueue : SerializableList<EgmObserverEventQueue.CommandBase>
    {
        private IEgmMeters _meterSnapshot;

        private void Execute(CommandBase command)
        {
            Add(command);
        }

        public void ForwardAll(IEgmObserver target, IEgmMeters currentMeters)
        {
            while (Count > 0)
            {
                var toSend = this.ToList();
                Clear();

                foreach (var item in toSend)
                {
                    item.Preprocess(_meterSnapshot, currentMeters);
                    item.Invoke(target);
                }
            }

            _meterSnapshot = currentMeters;
        }

        #region Nested type: CommandBase

        [GenerateICSerializable]
        public abstract partial class CommandBase
        {
            public virtual void Preprocess(IEgmMeters snapshot, IEgmMeters currentSnapshot)
            {
            }

            public abstract void Invoke(IEgmObserver target);
        }

        #endregion
    }
}