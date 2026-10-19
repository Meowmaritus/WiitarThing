using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using NintrollerLib;

namespace WiitarThing
{
    using static NativeImports;

    public class DeviceInfo
    {
        public enum BtStack
        {
            Microsoft,
            Toshiba,
            Other
        }

        public string DeviceID
        {
            get
            {
                return string.IsNullOrEmpty(DevicePath) ? InstanceGUID.ToString() : DevicePath;
            }
        }

        // For Wii/U Controllers
        public string DevicePath { get; set; }
        public ControllerType Type { get; set; }

        // For Joysticks
        public Guid InstanceGUID { get; set; } = Guid.Empty;
        public string VID { get; set; }
        public string PID { get; set; }

        public bool SameDevice(string identifier)
        {
            if (!string.IsNullOrEmpty(DevicePath))
            {
                return identifier == DevicePath;
            }
            else
            {
                return identifier == InstanceGUID.ToString();
            }
        }

        public bool SameDevice(Guid guid)
        {
            if (InstanceGUID != Guid.Empty)
            {
                return guid.Equals(InstanceGUID);
            }

            return false;
        }

        public static List<DeviceInfo> GetPaths()
        {
            var result = new List<DeviceInfo>();
            Guid guid;
            int index = 0;
            SafeFileHandle handle;

            // Get GUID of the HID class
            HidD_GetHidGuid(out guid);

            // handle for HID devices
            var hDevInfo = SetupDiGetClassDevs(in guid, null, IntPtr.Zero, (uint)(DIGCF.DeviceInterface | DIGCF.Present));

            SP_DEVICE_INTERFACE_DATA diData = SP_DEVICE_INTERFACE_DATA.Create();

            // Step through all devices
            while (SetupDiEnumDeviceInterfaces(hDevInfo, IntPtr.Zero, in guid, index, out diData))
            {
                uint size;

                // Get Device Buffer Size
                SetupDiGetDeviceInterfaceDetail(hDevInfo, in diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

                // Create Detail Struct
                SP_DEVICE_INTERFACE_DETAIL_DATA diDetail = SP_DEVICE_INTERFACE_DETAIL_DATA.Create();

                SP_DEVINFO_DATA deviceInfoData = SP_DEVINFO_DATA.Create();

                // Populate Detail Struct
                if (SetupDiGetDeviceInterfaceDetail(hDevInfo, in diData, ref diDetail, size, out size, out deviceInfoData))
                {
                    // Open read/write handle
                    handle = CreateFile(diDetail.devicePath, FileAccess.ReadWrite, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);

                    // Create Attributes Structure
                    HIDD_ATTRIBUTES attrib = new HIDD_ATTRIBUTES();
                    attrib.Size = Marshal.SizeOf(attrib);

                    // Populate Attributes
                    if (HidD_GetAttributes(handle, out attrib))
                    {
                        // Check if this is a compatable device
                        if (attrib.VendorID == 0x057e && (attrib.ProductID == 0x0306 || attrib.ProductID == 0x0330))
                        {
                            // TODO: Debug
                            //var associatedStack = CheckBtStack(deviceInfoData);
                            //var associatedStack = BtStack.Microsoft;

                            //var associatedStack = BluetoothEnableDiscovery(IntPtr.Zero, true) ? BtStack.Microsoft : BtStack.Toshiba;
                            //
                            //if (!AssociatedStack.ContainsKey(diDetail.devicePath))
                            //{
                            //    AssociatedStack.Add(diDetail.devicePath, associatedStack);
                            //}

                            result.Add(new DeviceInfo
                            {
                                DevicePath = diDetail.devicePath,
                                Type = attrib.ProductID == 0x0330 ? ControllerType.ProController : ControllerType.Wiimote
                            });
                        }
                    }

                    handle.Close();
                }
                else
                {
                    // Failed
                }

                index += 1;
            }

            // Clean Up
            hDevInfo.Dispose();

            return result;
        }

        private static readonly DEVPROPKEY DEVPKEY_Device_DriverProvider = new DEVPROPKEY
        {
            // DEVPROP_TYPE_STRING
            fmtid = new Guid(0xa8b865dd, 0x2e3d, 0x4094, 0xad, 0x97, 0xe5, 0x93, 0xa7, 0xc, 0x75, 0xd6),
            pid = 9
        };

        public static BtStack CheckBtStack(SP_DEVINFO_DATA data)
        {
            // Assume it is the Microsoft Stack
            BtStack resultStack = BtStack.Microsoft;
            SP_DEVINFO_DATA parentData = SP_DEVINFO_DATA.Create();

            int status = 0;
            int problemNum = 0;

            var result = CM_Get_DevNode_Status(out status, out problemNum, (int)data.DevInst, 0);

            if (result != 0) return resultStack; // Failed

            uint parentDevice;

            result = CM_Get_Parent(out parentDevice, data.DevInst, 0);

            if (result != 0) return resultStack; // Failed

            StringBuilder parentId = new StringBuilder(200);

            result = CM_Get_Device_ID(parentDevice, parentId, parentId.Capacity, 0);

            if (result != 0) return resultStack; // Failed

            string id = parentId.ToString();

            Guid g = Guid.Empty;
            HidD_GetHidGuid(out g);
            var parentDeviceInfo = SetupDiCreateDeviceInfoList(in g, IntPtr.Zero);

            // TODO: This fails, something not right
            bool success = SetupDiOpenDeviceInfo(parentDeviceInfo, id, IntPtr.Zero, 0, out parentData);

            if (success)
            {
                int requiredSize = 0;
                ulong devicePropertyType;

                SetupDiGetDeviceProperty(parentDeviceInfo, parentData, DEVPKEY_Device_DriverProvider, out devicePropertyType, null, 0, out requiredSize, 0);

                StringBuilder buffer = new StringBuilder(requiredSize);
                success = SetupDiGetDeviceProperty(parentDeviceInfo, parentData, DEVPKEY_Device_DriverProvider, out devicePropertyType, buffer, requiredSize, out requiredSize, 0);

                if (success)
                {
                    string classProvider = buffer.ToString();
                    if (classProvider == "TOSHIBA")
                    {
                        // Toshiba Stack
                        resultStack = BtStack.Toshiba;
                    }
                }
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
            }

            parentDeviceInfo.Dispose();

            return resultStack;
        }
    }
}
