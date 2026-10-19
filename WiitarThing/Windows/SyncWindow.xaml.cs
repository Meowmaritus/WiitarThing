using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Interop;
using System.Linq;

namespace WiitarThing.Windows
{
    /// <summary>
    /// Interaction logic for SyncWindow.xaml
    /// </summary>
    public partial class SyncWindow : Window
    {
        public bool Cancelled { get; protected set; }
        public int Count { get; protected set; }

        //public List<ulong> ConnectedDeviceAddresses = new List<ulong>();

        bool _notCompatable = false;

        public SyncWindow()
        {
            InitializeComponent();
        }

        public event EventHandler NewDeviceFound;

        private enum BluetoothError
        {
            Success = 0x00000000,               // ERROR_SUCCESS, BTH_ERROR_SUCCESS
            Disconnected = 0x0000048F,          // ERROR_DEVICE_NOT_CONNECTED, BTH_ERROR_NO_CONNECTION
            Timeout = 0x00000102,               // WAIT_TIMEOUT, BTH_ERROR_PAGE_TIMEOUT, BTH_ERROR_CONNECTION_TIMEOUT, BTH_ERROR_LMP_RESPONSE_TIMEOUT
            HardwareFailure = 0x0000001F,       // ERROR_GEN_FAILURE, BTH_ERROR_HARDWARE_FAILURE
            AuthFailure = 0x000004DC,           // ERROR_NOT_AUTHENTICATED, BTH_ERROR_AUTHENTICATION_FAILURE
            OutOfMemory = 0x00000008,           // ERROR_NOT_ENOUGH_MEMORY, BTH_ERROR_MEMORY_FULL
            MaxConnections = 0x00000047,        // ERROR_REQ_NOT_ACCEP, BTH_ERROR_MAX_NUMBER_OF_CONNECTIONS
            PairingNotAllowed = 0x00000005,     // ERROR_ACCESS_DENIED, BTH_ERROR_PAIRING_NOT_ALLOWED
            ConnectionTerminated = 0x000000F0,  // ERROR_VC_DISCONNECTED, BTH_ERROR_LOCAL_HOST_TERMINATED_CONNECTION
            AlreadyAuthenticated = 0x00000103,  // ERROR_NO_MORE_ITEMS
            Unspecified = 0x00000015,           // ERROR_NOT_READY, BTH_ERROR_UNSPECIFIED_ERROR
        }

        static string GetBluetoothAuthenticationError(uint errCode)
        {
            string msg;
            switch ((BluetoothError)errCode)
            {
                case BluetoothError.Success: msg = "Success."; break;
                case BluetoothError.Disconnected: msg = "Wiimote broke connection."; break;
                case BluetoothError.HardwareFailure: msg = "Bluetooth hardware failure."; break;
                case BluetoothError.AuthFailure: msg = "Failed to authenticate. Wiimote rejected auto-generated PIN."; break;
                case BluetoothError.OutOfMemory: msg = "Not enough RAM to connect."; break;
                case BluetoothError.Timeout: msg = "Wiimote not responding to Bluetooth pair signal..."; break;
                case BluetoothError.MaxConnections: msg = "Max number of Bluetooth connections for this adapter has already been reached. Cannot pair any more devices."; break;
                case BluetoothError.PairingNotAllowed: msg = "Couldn't get permission to pair."; break;
                case BluetoothError.ConnectionTerminated: msg = "Windows forced the connection to be dropped."; break;
                case BluetoothError.AlreadyAuthenticated: msg = "Be patient; Wiimote restarted the pairing process for some reason..."; break;
                case BluetoothError.Unspecified: msg = "Unspecified error; Windows has refused to connect the Wiimote without telling us why."; break;
                default: msg = "Unhandled error!"; break;
            }

            msg += $" (error code: 0x{errCode:X8})";
            return msg;
        }

        static string GetPasswordFromMacAddress(ulong address)
        {
            var password = new StringBuilder();
            byte[] bytes = BitConverter.GetBytes(address);
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 6; i++)
                {
                    password.Append((char)bytes[i]);
                }
            }
            else
            {
                for (int i = 7; i >= 2; i--)
                {
                    password.Append((char)bytes[i]);
                }
            }

            return password.ToString();
        }

        protected void OnNewDeviceFound()
        {
            var handler = NewDeviceFound;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public static void RemoveAllWiimotes()
        {
            WiitarDebug.Log("FUNC BEGIN - RemoveAllWiimotes");

            var radioParams = NativeImports.BLUETOOTH_FIND_RADIO_PARAMS.Create();
            Guid HidServiceClass = NativeImports.HidServiceClassGuid;

            // Get Bluetooth radios
            var btRadios = BluetoothRadio.FindAllRadios();
            if (btRadios.Count > 0)
            {
                foreach (var radio in btRadios) using (radio)
                {
                    // Get devices on this radio
                    var devices = radio.FindAllDevices();
                    if (devices == null || devices.Count < 1)
                    {
                        continue;
                    }
                    
                    // Remove all wiimotes that are connected or paired
                    foreach (var device in devices)
                    {
                        if (device.Name.StartsWith("Nintendo RVL-CNT-01"))
                        {
                            bool success = true;
                            uint errForget = 0;

                            if (device.Remembered || device.Connected)
                            {
                                WiitarDebug.Log("BEF - BluetoothRemoveDevice");
                                errForget = device.Remove();
                                WiitarDebug.Log("AFT - BluetoothRemoveDevice");
                                success = errForget == 0;
                            }

#if DEBUG
                            if (!success)
                            {
                                MessageBox.Show("DEBUG - Failed to remove bluetooth device.");
                            }
#endif
                        }

                    }
                }
            }

            WiitarDebug.Log("FUNC END - RemoveAllWiimotes");
        }

        public void Sync()
        {
            WiitarDebug.Log("FUNC BEGIN - Sync");

            // Retrieve all radios on the system
            var btRadios = BluetoothRadio.FindAllRadios();
            if (btRadios.Count < 1)
            {
                // No compatible Bluetooth
                Prompt("No compatible Bluetooth Radios found, or Bluetooth is disabled! (IF YOU SEE THIS MESSAGE, MENTION IT WHEN ASKING FOR HELP!)",
                isBold: true, isItalic: true);
                _notCompatable = true;
                return;
            }

            Prompt("Searching for controllers...", isBold: true);

            // Search until cancelled or at least one device is paired
            while (!Cancelled && Count == 0)
            {
                foreach (var radio in btRadios)
                {
                    // Get radio info
                    if (!radio.TryGetInfo(out var radioInfo))
                    {
                        Prompt("Found Bluetooth adapter but was unable to interact with it.");
                        continue;
                    }

                    // Get devices on this radio
                    var devices = radio.FindAllDevices();
                    if (devices == null || devices.Count < 1)
                    {
                        continue;
                    }
                    
                    foreach (var device in devices)
                    {
                        // Check device name
                        // Note: Switch Pro Controller is simply called "Pro Controller"
                        string deviceName = device.Name;
                        if (!deviceName.StartsWith("Nintendo RVL-CNT-01"))
                        {
#if DEBUG
                            Prompt("(Found \"" + deviceName + "\", but it is not a Wiimote)", isBold: false, isItalic: false, isSmall: true, isDebug: true);
#endif
                            continue;
                        }

                        bool remembered = device.Remembered;
                        var str_fRemembered = remembered ? ", but it is already synced!" : ". Attempting to pair now...";

                        if (deviceName.Equals("Nintendo RVL-CNT-01"))
                        {
                            Prompt("Found Wiimote (\"" + deviceName + "\")" + str_fRemembered, isBold: !remembered, isItalic: remembered, isSmall: remembered);
                        }
                        else if (deviceName.Equals("Nintendo RVL-CNT-01-TR"))
                        {
                            Prompt("Found 2nd-Gen Wiimote+ (\"" + deviceName + "\")" + str_fRemembered, isBold: !remembered, isItalic: remembered, isSmall: remembered);
                        }
                        else if (deviceName.Equals("Nintendo RVL-CNT-01-UC"))
                        {
                            Prompt("Found Wii U Pro Controller (\"" + deviceName + "\")" + str_fRemembered, isBold: !remembered, isItalic: remembered, isSmall: remembered);
                        }
                        else
                        {
                            Prompt("Found Unknown Wii Device Type (\"" + deviceName + "\")" + str_fRemembered, isBold: !remembered, isItalic: remembered, isSmall: remembered);
                        }

                        // Skip already-paired devices
                        if (remembered)
                        {
                            continue;
                        }
                        
                        bool success = true;

                        uint errAuth = 0;
                        uint errService = 0;
                        uint errActivate = 0;

                        string password = GetPasswordFromMacAddress(radioInfo.address);

                        // Authenticate
                        if (success)
                        {
                            errAuth = device.Authenticate(password);
                            success = errAuth == 0;
                        }

                        // If the sync method didn't work, try the 1+2 method
                        if (!success)
                        {
#if DEBUG
                            Prompt("SYNC method didn't work. Trying 1+2 method...");
#endif

                            password = GetPasswordFromMacAddress(device.Address);
                            errAuth = device.Authenticate(password);
                            success = errAuth == 0;
                        }

                        // Get activated services
                        Guid[] guids = null;
                        if (success)
                        {
                            WiitarDebug.Log("BEF - BluetoothEnumerateInstalledServices");
                            errService = device.EnumerateInstalledServices(out guids);
                            WiitarDebug.Log("AFT - BluetoothEnumerateInstalledServices");
                            success = errService == 0;
                        }

                        // Activate HID service
                        if (success)
                        {
                            if (guids == null || !guids.Contains(NativeImports.HidServiceClassGuid))
                            {
                                WiitarDebug.Log("BEF - BluetoothSetServiceState");
                                errActivate = device.SetServiceState(NativeImports.HidServiceClassGuid, true);
                                WiitarDebug.Log("AFT - BluetoothSetServiceState");
                                success = errActivate == 0;
                            }
                        }

                        if (success)
                        {
                            Prompt("Successfully Paired!", isBold: true);
                            Count += 1;
                        }
                        else
                        {
                            var sb = new StringBuilder();

#if DEBUG
                            sb.AppendLine($"radio mac address: {radioInfo.address:X12}");
                            sb.AppendLine($"wiimote mac address: {device.Address:X12}");
                            sb.Append($"wiimote password: \"{password}\" (");
                            foreach (char ch in password)
                            {
                                if (ch > byte.MaxValue)
                                {
                                    sb.Append($"{ch & 0xFF:X2}-{(ch & 0xFF00) >> 8:X2}-");
                                }
                                else
                                {
                                    sb.Append($"{ch:X2}-");
                                }
                            }
                            // Remove trailing dash
                            sb.Remove(sb.Length - 1, 1);
                            sb.AppendLine(")");
#endif


                            if (errAuth != 0)
                            {
                                sb.AppendLine(GetBluetoothAuthenticationError(errAuth));
                            }

                            if (errService != 0)
                            {
                                sb.AppendLine(" >>> SERVICE ERROR: " + new Win32Exception((int)errService).Message);
                            }

                            if (errActivate != 0)
                            {
                                sb.AppendLine(" >>> ACTIVATION ERROR: " + new Win32Exception((int)errActivate).Message);
                            }

                            Prompt(sb.ToString(), isBold: true, isItalic: true);
                        }
                    }
                }
            }

            // Clean up radio handles
            foreach (var radio in btRadios)
            {
                radio.Dispose();
            }

            // Close this window
            Dispatcher.BeginInvoke((Action)(() => Close()));

            WiitarDebug.Log("FUNC END - Sync");
        }

        private void Prompt(string text, bool isBold = false, bool isItalic = false, bool isSmall = false, bool isDebug = false)
        {
            WiitarDebug.Log("SYNC WINDOW OUTPUT: \n\n" + text + "\n\n");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var newInline = new System.Windows.Documents.Run(text);

                newInline.FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal;
                newInline.FontStyle = isItalic ? FontStyles.Italic : FontStyles.Normal;

                if (isSmall)
                {
                    newInline.FontSize *= 0.75;
                }

                if (isDebug)
                {
                    newInline.Foreground = System.Windows.Media.Brushes.Gray;
                }

                var newParagraph = new System.Windows.Documents.Paragraph(newInline);

                newParagraph.Padding = new Thickness(0);
                newParagraph.Margin = new Thickness(0);


                prompt.Blocks.Add(newParagraph);

                promptBoxContainer.ScrollToEnd();
            }));
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_notCompatable)
            {
                Close();
            }

            Prompt("Stopping scan...");
            Cancelled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!Cancelled && Count == 0 && !_notCompatable)
            {
                Cancelled = true;
                Prompt("Stopping scan...");
                e.Cancel = true;
            }

            if (Count > 0)
            {
                MessageBox.Show("Device connected successfully. Give Windows up to a few minutes to install the drivers and it will show up in the list on the left.", "Device Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task t = new Task(() => Sync());
            t.Start();
        }
    }
}
