using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;

namespace WiitarThing
{
    using static NativeImports;

    public class BluetoothDevice
    {
        private BluetoothRadio radio;
        private BLUETOOTH_DEVICE_INFO info;

        public string Name => info.szName;
        public ulong Address => info.Address;
        public uint DeviceClass => info.ulClassofDevice;
        public bool Connected => info.fConnected;
        public bool Remembered => info.fRemembered;
        public bool Authenticated => info.fAuthenticated;
        public DateTime LastSeen { get; }
        public DateTime LastUsed { get; }

        internal BluetoothDevice(BluetoothRadio radio, BLUETOOTH_DEVICE_INFO info)
        {
            this.radio = radio;
            this.info = info;

            LastSeen = new DateTime(info.stLastSeen.Year, info.stLastSeen.Month, info.stLastSeen.Day, info.stLastSeen.Hour, info.stLastSeen.Minute, info.stLastSeen.Second, info.stLastSeen.Milliseconds);
            LastUsed = new DateTime(info.stLastUsed.Year, info.stLastUsed.Month, info.stLastUsed.Day, info.stLastUsed.Hour, info.stLastUsed.Minute, info.stLastUsed.Second, info.stLastUsed.Milliseconds);
        }

        /// <summary>
        /// Authenticates this device with the given password.
        /// </summary>
        public uint Authenticate(string password)
        {
            return radio.AuthenticateDevice(ref info, password);
        }

        /// <summary>
        /// Removes this device from the system.
        /// </summary>
        public uint Remove()
        {
            return NativeImports.BluetoothRemoveDevice(in info.Address);
        }

        /// <summary>
        /// Gets an array of all enabled services on this device.
        /// </summary>
        public uint EnumerateInstalledServices(out Guid[] serviceGuids)
        {
            return radio.EnumerateInstalledServices(in info, out serviceGuids);
        }

        /// <summary>
        /// Sets the state of a service on this device.
        /// </summary>
        public uint SetServiceState(Guid serviceGuid, bool enable)
        {
            return radio.SetServiceState(in info, serviceGuid, enable);
        }
    }
}