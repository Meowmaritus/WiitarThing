using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace NintrollerLib
{
    internal static class NativeImports
    {
        public enum Win32Error : int
        {
            Success = 0,
            InvalidHandle = 6,
            IoPending = 997,
        }

        #region kernel32.dll

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly          = 0x00000001,
            Hidden            = 0x00000002,
            System            = 0x00000004,
            Directory         = 0x00000010,
            Archive           = 0x00000020,
            Device            = 0x00000040,
            Normal            = 0x00000080,
            Temporary         = 0x00000100,
            SparseFile        = 0x00000200,
            ReparsePoint      = 0x00000400,
            Compressed        = 0x00000800,
            Offline           = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted         = 0x00004000,
            Write_Through     = 0x80000000,
            Overlapped        = 0x40000000,
            NoBuffering       = 0x20000000,
            RandomAccess      = 0x10000000,
            SequentialScan    = 0x08000000,
            DeleteOnClose     = 0x04000000,
            BackupSemantics   = 0x02000000,
            PosixSemantics    = 0x01000000,
            OpenReparsePoint  = 0x00200000,
            OpenNoRecall      = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        /// <summary>
        /// A generic safe handle for any handle that is closed via CloseHandle.
        /// </summary>
        public class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeObjectHandle() : base(true)
            {
            }

            public SafeObjectHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] EFileAttributes flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static bool ReadFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            uint nNumberofBytesToRead,
            out uint lpNumberOfBytesRead,
            ref NativeOverlapped lpOverlapped);

        /// <summary>
        /// Use to fix sending data to TR controllers
        /// (broken in Windows 7)
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool WriteFile(
            SafeFileHandle hFile,            // HANDLE
            byte[] lpBuffer,                 // LPCVOID
            uint nNumberOfBytesToWrite,      // DWORD
            out uint lpNumberOfBytesWritten, // LPDWORD
            ref NativeOverlapped lpOverlapped);


        /// <summary>
        /// Async Callback for WriteFileEx
        /// </summary>
        public delegate void WriteFileCompletionDelegate(
            uint dwErrorCode,
            uint dwNumberOfBytesTransfered,
            ref NativeOverlapped lpOverlapped);

        /// <summary>
        /// Like WriteFile but provides an asynchronous callback
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static bool WriteFileEx(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            ref NativeOverlapped lpOverlapped,
            WriteFileCompletionDelegate lpCompletionRoutine);

        /// <summary>
        /// Gets the results of an overlapped operation
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool GetOverlappedResult(
            SafeFileHandle hFile,                // HANDLE
            in NativeOverlapped lpOverlapped,    // LPOVERLAPPED
            out uint lpNumberOfBytesTransferred, // LPDWORD
            bool bWait);                         // BOOL

        /// <summary>
        /// Gets the results of an overlapped operation
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool CancelIoEx(
            SafeFileHandle hFile,                // HANDLE
            in NativeOverlapped lpOverlapped);   // LPOVERLAPPED

        #endregion

        #region setupapi.dll

        /// <summary>
        /// Provided to SetupDiGetClassDevs to specify what to included in the device information
        /// </summary>
        [Flags]
        public enum DIGCF : int     // Device Information Group Control Flag?
        {
            /// <summary>
            /// The device that is associated with the system default device interface
            /// (only valid with DIGCF_DEVICEINTERFACE)
            /// </summary>
            Default         = 0x00000001,

            /// <summary>
            /// Devices that are currently present
            /// </summary>
            Present         = 0x00000002,

            /// <summary>
            /// Devices that are installed for the specified device setup or interface classes
            /// </summary>
            AllClasses      = 0x00000004,

            /// <summary>
            /// Devices that are part of the current hardware profile
            /// </summary>
            Profile         = 0x00000008,

            /// <summary>
            /// Devices that support device interfaces for the specified device classes.
            /// (Must be set if a device instance ID is specified)
            /// </summary>
            DeviceInterface = 0x00000010
        }

        // Used for BT Stack detection
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;

            public static SP_DEVINFO_DATA Create()
            {
                return new SP_DEVINFO_DATA()
                {
                    cbSize = (uint)Marshal.SizeOf<SP_DEVINFO_DATA>()
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            public IntPtr reserved;

            public static SP_DEVICE_INTERFACE_DATA Create()
            {
                return new SP_DEVICE_INTERFACE_DATA()
                {
                    cbSize = Marshal.SizeOf<SP_DEVICE_INTERFACE_DATA>()
                };
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public uint size;           // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string devicePath;

            public static SP_DEVICE_INTERFACE_DETAIL_DATA Create()
            {
                return new SP_DEVICE_INTERFACE_DETAIL_DATA()
                {
                    // TODO: This is what I get if I do sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_A)
                    // or sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_A) in C/C++
                    // size = 8
                    // Determine which of these is correct
                    size = (uint)(IntPtr.Size == 8 ? 8 : 5) // 4 + Marshal.SystemDefaultCharSize)
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public uint pid;
        };

        /// <summary>
        /// A safe handle for SetupDi device info lists.
        /// </summary>
        public class SafeDeviceInfoListHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeDeviceInfoListHandle() : base(true)
            {
            }

            public SafeDeviceInfoListHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return SetupDiDestroyDeviceInfoList(handle);
            }
        }

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeDeviceInfoListHandle SetupDiCreateDeviceInfoList(
            in Guid ClassGuid,
            IntPtr hwndParent);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeDeviceInfoListHandle SetupDiGetClassDevs(
            in Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
            IntPtr hwndParent,
            uint Flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiOpenDeviceInfo(
            SafeDeviceInfoListHandle DevInfoSet,
            string DeviceInstanceId,
            IntPtr hWndParent,
            uint OpenFlags,
            out SP_DEVINFO_DATA DeviceInfoData);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            SafeDeviceInfoListHandle DeviceInfoSet,
            IntPtr DeviceInfoData, //ref SP_DEVINFO_DATA devInfo,
            in Guid InterfaceClassGuid,
            int MemberIndex,
            out SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            SafeDeviceInfoListHandle DeviceInfoSet,
            in SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            IntPtr DeviceInterfaceDetailData,
            uint DeviceInterfaceDetailDataSize,
            out uint RequiredSize,
            IntPtr DeviceInfoData);

        [DllImport(@"setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            SafeDeviceInfoListHandle DeviceInfoSet,
            in SP_DEVICE_INTERFACE_DATA DeviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData,
            uint DeviceInterfaceDetailDataSize,
            out uint RequiredSize,
            out SP_DEVINFO_DATA DeviceInfoData);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceProperty(
            SafeDeviceInfoListHandle DeviceInfoSet,
            in SP_DEVINFO_DATA DeviceInfoData,
            in DEVPROPKEY PropertyKey,
            out ulong PropertyType,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder PropertyBuffer,
            int PropertyBufferSize,
            out int RequiredSize,
            uint Flags);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern int CM_Get_Device_ID(
            uint dnDevInst,
            StringBuilder buffer,
            int bufferLen,
            int flags);

        [DllImport("setupapi.dll")]
        public static extern int CM_Get_Parent(
            out uint pdnDevInst,
            uint dnDevInst,
            int ulFlags);

        [DllImport("setupapi.dll")]
        public static extern int CM_Get_DevNode_Status(
            out int pulStatus,
            out int pulProblemNumber,
            int dnDevInst,
            int ulFlags);

        #endregion

        #region hid.dll

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;

            public static HIDD_ATTRIBUTES Create()
            {
                return new HIDD_ATTRIBUTES()
                {
                    Size = Marshal.SizeOf<HIDD_ATTRIBUTES>()
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDP_CAPS
        {
            public ushort Usage;
            public ushort UsagePage;
            public ushort InputReportByteLength;
            public ushort OutputReportByteLength;
            public ushort FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)]
            private ushort[] unused; // There's more fields here but we don't use them
        };

        /// <summary>
        /// A safe handle for HID preparsed data.
        /// </summary>
        public class SafeHidPreparsedDataHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeHidPreparsedDataHandle() : base(true)
            {
            }

            public SafeHidPreparsedDataHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return HidD_FreePreparsedData(handle);
            }
        }

        [DllImport(@"hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void HidD_GetHidGuid(
            out Guid gHid);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(
            SafeFileHandle HidDeviceObject,
            out HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public extern static bool HidD_GetInputReport(
            SafeFileHandle HidDeviceObject,
            byte[] ReportBuffer,
            uint ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public extern static bool HidD_SetOutputReport(
            SafeFileHandle HidDeviceObject,
            byte[] lpReportBuffer,
            uint ReportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public extern static bool HidD_GetPreparsedData(
            SafeFileHandle HidDeviceObject,
            out SafeHidPreparsedDataHandle PreparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        public extern static bool HidD_FreePreparsedData(
            IntPtr PreparsedData);

        [DllImport("hid.dll")]
        public extern static int HidP_GetCaps(
            SafeHidPreparsedDataHandle PreparsedData,
            out HIDP_CAPS Capabilities);

        #endregion

        #region bthprops.cpl

        public static readonly Guid HidServiceClassGuid = Guid.Parse("00001124-0000-1000-8000-00805F9B34FB");

        [Flags]
        public enum BluetoothServiceFlag : uint
        {
            Disable = 0x00,
            Enable = 0x01
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BLUETOOTH_DEVICE_INFO
        {
            public uint dwSize;
            public ulong Address;
            public uint ulClassofDevice;
            public bool fConnected;
            public bool fRemembered;
            public bool fAuthenticated;
            public SYSTEMTIME stLastSeen;
            public SYSTEMTIME stLastUsed;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
            public string szName;

            public static BLUETOOTH_DEVICE_INFO Create()
            {
                return new BLUETOOTH_DEVICE_INFO()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO))
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_DEVICE_SEARCH_PARAMS
        {
            public uint dwSize;
            public bool fReturnAuthenticated;
            public bool fReturnRemembered;
            public bool fReturnUnknown;
            public bool fReturnConnected;
            public bool fIssueInquiry;
            public byte cTimeoutMultiplier;
            public IntPtr hRadio;

            public static BLUETOOTH_DEVICE_SEARCH_PARAMS Create()
            {
                return new BLUETOOTH_DEVICE_SEARCH_PARAMS()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_SEARCH_PARAMS))
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_FIND_RADIO_PARAMS
        {
            public uint dwSize;

            public static BLUETOOTH_FIND_RADIO_PARAMS Create()
            {
                return new BLUETOOTH_FIND_RADIO_PARAMS()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_FIND_RADIO_PARAMS))
                };
            }
        }

        private const int BLUETOOTH_MAX_NAME_SIZE = 248;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BLUETOOTH_RADIO_INFO
        {
            public uint dwSize;
            public ulong address;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BLUETOOTH_MAX_NAME_SIZE)]
            public string szName;
            public uint ulClassOfDevice;
            public ushort lmpSubversion;
            public ushort manufacturer;

            public string Address
            {
                get
                {
                    var bytes = BitConverter.GetBytes(address);
                    StringBuilder str = new StringBuilder();
                    for (int i = bytes.Length - 1; i >= 0; i--)
                        str.Append(bytes[i].ToString("X2"));
                    return str.ToString();
                }
            }

            public static BLUETOOTH_RADIO_INFO Create()
            {
                return new BLUETOOTH_RADIO_INFO()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_RADIO_INFO))
                };
            }
        }

        /// <summary>
        /// A safe handle for Bluetooth radio searches (handles returned by BluetoothFindFirstRadio).
        /// </summary>
        public class SafeBluetoothRadioHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeBluetoothRadioHandle() : base(true)
            {
            }

            public SafeBluetoothRadioHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return BluetoothFindRadioClose(handle);
            }
        }

        /// <summary>
        /// A safe handle for Bluetooth device searches (handles returned by BluetoothFindFirstDevice).
        /// </summary>
        public class SafeBluetoothDeviceHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeBluetoothDeviceHandle() : base(true)
            {
            }

            public SafeBluetoothDeviceHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return BluetoothFindDeviceClose(handle);
            }
        }

        [DllImport("bthprops.cpl")]
        public static extern uint BluetoothGetRadioInfo(
            SafeObjectHandle hRadio,
            ref BLUETOOTH_RADIO_INFO pRadioInfo);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern SafeBluetoothRadioHandle BluetoothFindFirstRadio(
            in BLUETOOTH_FIND_RADIO_PARAMS pbtfrp,
            out SafeObjectHandle phRadio);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextRadio(
            SafeBluetoothRadioHandle hFind,
            out SafeObjectHandle phRadio);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindRadioClose(
            IntPtr hFind);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern SafeBluetoothDeviceHandle BluetoothFindFirstDevice(
            in BLUETOOTH_DEVICE_SEARCH_PARAMS pbtsp,
            ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextDevice(
            SafeBluetoothDeviceHandle hFind,
            ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindDeviceClose(
            IntPtr hFind);

        [DllImport("bthprops.cpl")]
        public static extern uint BluetoothRemoveDevice(in ulong pAddress);

        [DllImport("bthprops.cpl", CharSet = CharSet.Unicode)]
        public static extern uint BluetoothAuthenticateDevice(
            IntPtr hwndParent,
            SafeObjectHandle hRadio,
            ref BLUETOOTH_DEVICE_INFO pbtdi,
            [MarshalAs(UnmanagedType.LPWStr)] string pszPasskey,
            uint ulPasskeyLength);

        [DllImport("bthprops.cpl")]
        public static extern uint BluetoothEnumerateInstalledServices(
            SafeObjectHandle hRadio,
            in BLUETOOTH_DEVICE_INFO pbtdi,
            ref uint pcServiceInout,
            Guid[] pGuidServices);

        [DllImport("bthprops.cpl")]
        public static extern uint BluetoothSetServiceState(
            SafeObjectHandle hRadio,
            in BLUETOOTH_DEVICE_INFO pbtdi,
            in Guid pGuidService,
            [MarshalAs(UnmanagedType.U4)] BluetoothServiceFlag dwServiceFlags);

        [DllImport("bthprops.cpl", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BluetoothEnableDiscovery(
            SafeObjectHandle hRadio,
            [MarshalAs(UnmanagedType.Bool)] bool fEnabled);

        #endregion
    }
}
