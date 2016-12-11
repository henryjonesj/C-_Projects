using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BallyTech.Utility.Serialization;
using BallyTech.Gtm;

namespace BallyTech.QCom.Model.Egm
{
    [GenerateICSerializable]
    public partial class DeviceCollection : SerializableList<Device>
    {

        public T GetDevice<T>() where T: Device
        {
            return this.OfType<T>().SingleOrDefault();
        }

        private SerializableList<Device> DevicesAwaitingMeters
        {
            get { return this.FindAll((item) => item.AwaitingForMeters).ToSerializableList(); }
        }

        private SerializableList<Device> DevicesAwaitingForRomSignatureStatus
        {
            get { return this.FindAll((item) => item.AwaitingForRomSignatureVerification).ToSerializableList(); }
        }


        internal void NotifyMetersReceived(SerializableList<MeterId> meterIds)
        {            
            foreach (var device in DevicesAwaitingMeters)
            {
                device.OnMetersReceived(meterIds);
            }            
        }

        private IEnumerable<Device> DevicesAwaitingForGameIdle
        {
            get { return this.FindAll((item) => item.AwaitingForGameIdle).ToSerializableList(); }
        }

        internal void NotifyGameIdle()
        {            
            foreach (var device in DevicesAwaitingForGameIdle)
            {
                device.OnGameIdle();
            } 
        }

        internal void NotifyGameLinkStatus(LinkStatus linkStatus)
        {
            foreach (var device in this)
            {
                device.OnLinkStatusChanged(linkStatus);
            }
        }

        internal void NotifyRomSignatureInitiated()
        {
            foreach (var device in DevicesAwaitingMeters)
                device.NotifyRomSignatureVerificationInitiated();
        }


        internal void NotifyRomSignatureStatus(SoftwareVerificationCompletionStatus status)
        {
            foreach (var device in DevicesAwaitingForRomSignatureStatus)
            {
                if (status == SoftwareVerificationCompletionStatus.Success)
                    device.NotifyRomSignatureVerificationComplete();
                else
                    device.NotifyRomSignatureVerificationFailure();
            }
        }


        internal void NotifyMeterRequestTimerExpired()
        {
            foreach (var device in DevicesAwaitingMeters)
            {
                device.MeterRequestTimerExpired();
            }
        }

    	internal void NotifyMeterRequestSplitIntervalSurpassed()
        {
            foreach (var device in DevicesAwaitingMeters)
            {
                device.MeterRequestSplitIntervalSurpassed();
            }
        }

        internal bool AnyDeviceAwaitingForMeters
        {
            get { return DevicesAwaitingMeters.Count > 0; }
        }

        internal bool AnyDeviceAwaitingForGameIdle
        {
            get { return DevicesAwaitingForGameIdle.Count() > 0; }
        }

        internal bool AnyDeviceAwaitingForRomSignatureStatus
        {
            get { return DevicesAwaitingForRomSignatureStatus.Count() > 0; }
        }

        internal void Reset()
        {
            ForEach((device) => device.ForceReset());
        }

    }
}
