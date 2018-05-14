using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Shared.Windows;
using System.ComponentModel;
using System.Windows.Interop;

namespace WiinUSoft.Windows
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

        const int ERROR_SUCCESS = 0x00000000;
        const int ERROR_DEVICE_NOT_CONNECTED = 0x0000048F;
        const int WAIT_TIMEOUT = 0x00000102;
        const int ERROR_GEN_FAILURE = 0x0000001F;
        const int ERROR_NOT_AUTHENTICATED = 0x000004DC;
        const int ERROR_NOT_ENOUGH_MEMORY = 0x00000008;
        const int ERROR_REQ_NOT_ACCEP = 0x00000047;
        const int ERROR_ACCESS_DENIED = 0x00000005;
        const int ERROR_NOT_READY = 0x00000015;
        const int ERROR_VC_DISCONNECTED = 0x000000F0;
        const int ERROR_INVALID_PARAMETER = 0x00000057;
        const int ERROR_SERVICE_DOES_NOT_EXIST = 0x00000424;

        const int ERROR_NO_MORE_ITEMS = 0x00000103;

        static string GetBluetoothAuthenticationError(uint errCode)
        {
            string msg = "(ERROR CODE 0x" + errCode.ToString("X") + ")";

            switch ((int)errCode)
            {
                case ERROR_SUCCESS: msg = "Success."; break;
                case ERROR_DEVICE_NOT_CONNECTED: msg = "Wiimote broke connection."; break;
                case ERROR_GEN_FAILURE: msg = "Bluetooth Hardware Failure."; break;
                case ERROR_NOT_AUTHENTICATED: msg = "Failed to authenticate. Wiimote rejected auto-generated PIN."; break;
                case ERROR_NOT_ENOUGH_MEMORY: msg = "Not enough RAM to connect."; break;
                case WAIT_TIMEOUT: msg = "Wiimote not responding to Bluetooth pair signal..."; break;
                case ERROR_REQ_NOT_ACCEP: msg = "Max number of Bluetooth connections for this adapter has already been reached. Cannot pair any more devices."; break;
                case ERROR_ACCESS_DENIED: msg = "Couldn't get permission to pair."; break;
                case ERROR_NOT_READY: msg = "Unspecified error; Windows has refused to connect the Wiimote without telling us why."; break;
                case ERROR_VC_DISCONNECTED: msg = "Windows forced the connection to be dropped."; break;
                case ERROR_NO_MORE_ITEMS: msg = "Be patient; Wiimote restarted the pairing process for some reason..."; break;
            }

            return msg;
        }

        static string GetMacAddressStr(ulong address)
        {
            var bytes = BitConverter.GetBytes(address);
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < 6; i++)
                str.Append(bytes[i].ToString("X2") + " ");
            return str.ToString();
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

            var radioParams = new NativeImports.BLUETOOTH_FIND_RADIO_PARAMS();
            Guid HidServiceClass = Guid.Parse(NativeImports.HID_GUID);
            List<IntPtr> btRadios = new List<IntPtr>();
            IntPtr foundRadio;
            IntPtr foundResult;

            radioParams.Initialize();

            // Get first BT Radio
            foundResult = NativeImports.BluetoothFindFirstRadio(ref radioParams, out foundRadio);
            bool more = foundResult != IntPtr.Zero;

            do
            {
                if (foundRadio != IntPtr.Zero)
                {
                    btRadios.Add(foundRadio);
                }

                // Find more
                more = NativeImports.BluetoothFindNextRadio(ref radioParams, out foundRadio);
            } while (more);

            if (btRadios.Count > 0)
            {
                foreach (var radio in btRadios)
                {
                    IntPtr found;
                    var radioInfo = new NativeImports.BLUETOOTH_RADIO_INFO();
                    var deviceInfo = new NativeImports.BLUETOOTH_DEVICE_INFO();
                    var searchParams = new NativeImports.BLUETOOTH_DEVICE_SEARCH_PARAMS();

                    radioInfo.Initialize();
                    deviceInfo.Initialize();
                    searchParams.Initialize();

                    // Access radio information
                    WiitarDebug.Log("BEF - BluetoothGetRadioInfo");
                    uint getInfoError = NativeImports.BluetoothGetRadioInfo(radio, ref radioInfo);
                    WiitarDebug.Log("AFT - BluetoothGetRadioInfo");

                    if (getInfoError == 0)
                    {
                        // Set search parameters
                        searchParams.hRadio = radio;
                        searchParams.fIssueInquiry = true;
                        searchParams.fReturnUnknown = true;
                        searchParams.fReturnConnected = true;
                        searchParams.fReturnRemembered = true;
                        searchParams.fReturnAuthenticated = true;
                        searchParams.cTimeoutMultiplier = 2;

                        // Search for a device
                        WiitarDebug.Log("BEF - BluetoothFindFirstDevice");
                        found = NativeImports.BluetoothFindFirstDevice(ref searchParams, ref deviceInfo);
                        WiitarDebug.Log("AFT - BluetoothFindFirstDevice");

                        // Success
                        if (found != IntPtr.Zero)
                        {
                            do
                            {

                                if (deviceInfo.szName.StartsWith("Nintendo RVL-CNT-01"))
                                {
                                    //NativeImports.BluetoothRemoveDevice(ref deviceInfo.Address);

                                    //StringBuilder password = new StringBuilder();
                                    ////uint pcService = 16;
                                    ////Guid[] guids = new Guid[16];
                                    bool success = true;
                                    uint errForget = 0;

                                    if (/*!ConnectedDeviceAddresses.Contains(deviceInfo.Address) && */(deviceInfo.fRemembered || deviceInfo.fConnected))
                                    {
                                        WiitarDebug.Log("BEF - BluetoothRemoveDevice");
                                        errForget = NativeImports.BluetoothRemoveDevice(ref deviceInfo.Address);
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

                            } while (NativeImports.BluetoothFindNextDevice(found, ref deviceInfo));
                        }
                    }
                }
            }

            WiitarDebug.Log("FUNC END - RemoveAllWiimotes");
        }

        public void Sync()
        {
            WiitarDebug.Log("FUNC BEGIN - Sync");

            WiitarDebug.Log("BEF - BLUETOOTH_FIND_RADIO_PARAMS");
            var radioParams = new NativeImports.BLUETOOTH_FIND_RADIO_PARAMS();
            WiitarDebug.Log("AFT - BLUETOOTH_FIND_RADIO_PARAMS");
            Guid HidServiceClass = Guid.Parse(NativeImports.HID_GUID);
            List<IntPtr> btRadios = new List<IntPtr>();
            IntPtr foundRadio;
            IntPtr foundResult;

            radioParams.Initialize();

            // Get first BT Radio
            WiitarDebug.Log("BEF - BluetoothGetRadioInfo");
            foundResult = NativeImports.BluetoothFindFirstRadio(ref radioParams, out foundRadio);
            WiitarDebug.Log("AFT - BluetoothGetRadioInfo");
            bool more = foundResult != IntPtr.Zero;

            do
            {
                if (foundRadio != IntPtr.Zero)
                {
                    btRadios.Add(foundRadio);
                }

                // Find more
                WiitarDebug.Log("BEF - BluetoothFindNextRadio");
                more = NativeImports.BluetoothFindNextRadio(ref radioParams, out foundRadio);
                WiitarDebug.Log("AFT - BluetoothFindNextRadio");
            } while (more);

            if (btRadios.Count > 0)
            {
                Prompt("Searching for controllers...", isBold: true);

                // Search until cancelled or at least one device is paired
                while (!Cancelled && Count == 0)
                {
                    foreach (var radio in btRadios)
                    {
                        IntPtr found;

                        WiitarDebug.Log("BEF - BLUETOOTH_RADIO_INFO");
                        var radioInfo = new NativeImports.BLUETOOTH_RADIO_INFO();
                        WiitarDebug.Log("AFT - BLUETOOTH_RADIO_INFO");

                        WiitarDebug.Log("BEF - BLUETOOTH_DEVICE_INFO");
                        var deviceInfo = new NativeImports.BLUETOOTH_DEVICE_INFO();
                        WiitarDebug.Log("AFT - BLUETOOTH_DEVICE_INFO");

                        WiitarDebug.Log("BEF - BLUETOOTH_DEVICE_SEARCH_PARAMS");
                        var searchParams = new NativeImports.BLUETOOTH_DEVICE_SEARCH_PARAMS();
                        WiitarDebug.Log("AFT - BLUETOOTH_DEVICE_SEARCH_PARAMS");

                        radioInfo.Initialize();
                        deviceInfo.Initialize();
                        searchParams.Initialize();

                        // Access radio information
                        WiitarDebug.Log("BEF - BluetoothGetRadioInfo");
                        uint getInfoError = NativeImports.BluetoothGetRadioInfo(radio, ref radioInfo);
                        WiitarDebug.Log("AFT - BluetoothGetRadioInfo");

                        // Success
                        if (getInfoError == 0)
                        {
                            // Set search parameters
                            searchParams.hRadio = radio;
                            searchParams.fIssueInquiry = true;
                            searchParams.fReturnUnknown = true;
                            searchParams.fReturnConnected = false;
                            searchParams.fReturnRemembered = true;
                            searchParams.fReturnAuthenticated = false;
                            searchParams.cTimeoutMultiplier = 2;

                            // Search for a device
                            WiitarDebug.Log("BEF - BluetoothFindFirstDevice");
                            found = NativeImports.BluetoothFindFirstDevice(ref searchParams, ref deviceInfo);
                            WiitarDebug.Log("AFT - BluetoothFindFirstDevice");

                            // Success
                            if (found != IntPtr.Zero)
                            {
                                do
                                {
                                    // Note: Switch Pro Controller is simply called "Pro Controller"
                                    if (deviceInfo.szName.StartsWith("Nintendo RVL-CNT-01"))
                                    {
//#if DEBUG
//                                        var str_fRemembered = deviceInfo.fRemembered ? ", but it is already synced!" : "";
//#else
//                                        if (deviceInfo.fRemembered)
//                                        {
//                                            continue;
//                                        }
//                                        var str_fRemembered = "";
//#endif

                                        var str_fRemembered = deviceInfo.fRemembered ? ", but it is already synced!" : ". Attempting to pair now...";

                                        if (deviceInfo.szName.Equals("Nintendo RVL-CNT-01"))
                                        {
                                            Prompt("Found Wiimote (\"" + deviceInfo.szName + "\")" + str_fRemembered, isBold: !deviceInfo.fRemembered, isItalic: deviceInfo.fRemembered, isSmall: deviceInfo.fRemembered);
                                        }
                                        else if (deviceInfo.szName.Equals("Nintendo RVL-CNT-01-TR"))
                                        {
                                            Prompt("Found 2nd-Gen Wiimote+ (\"" + deviceInfo.szName + "\")" + str_fRemembered, isBold: !deviceInfo.fRemembered, isItalic: deviceInfo.fRemembered, isSmall: deviceInfo.fRemembered);
                                        }
                                        else if (deviceInfo.szName.Equals("Nintendo RVL-CNT-01-UC"))
                                        {
                                            Prompt("Found Wii U Pro Controller (\"" + deviceInfo.szName + "\")" + str_fRemembered, isBold: !deviceInfo.fRemembered, isItalic: deviceInfo.fRemembered, isSmall: deviceInfo.fRemembered);
                                        }
                                        else
                                        {
                                            Prompt("Found Unknown Wii Device Type (\"" + deviceInfo.szName + "\")" + str_fRemembered, isBold: !deviceInfo.fRemembered, isItalic: deviceInfo.fRemembered, isSmall: deviceInfo.fRemembered);
                                        }

                                        if (deviceInfo.fRemembered)
                                        {
                                            continue;
                                        }
                                        

                                        StringBuilder password = new StringBuilder();
                                        uint pcService = 16;
                                        Guid[] guids = new Guid[16];
                                        bool success = true;

                                        var bytes = BitConverter.GetBytes(radioInfo.address);

                                        //// Create Password out of BT radio MAC address
                                        //if (BitConverter.IsLittleEndian)
                                        //{
                                        //    for (int i = 0; i < 6; i++)
                                        //    {
                                        //        password.Append((char)bytes[i]);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    for (int i = 7; i >= 2; i--)
                                        //    {
                                        //        password.Append((char)bytes[i]);
                                        //    }
                                        //}

                                        for (int i = 0; i < 6; i++)
                                        {
                                            if (bytes[i] > 0)
                                                password.Append((char)bytes[i]);
                                        }

                                        uint errForget = 0;
                                        uint errAuth = 0;
                                        uint errService = 0;
                                        uint errActivate = 0;


                                        //if (/*!ConnectedDeviceAddresses.Contains(deviceInfo.Address) && */(deviceInfo.fRemembered || deviceInfo.fConnected))
                                        //{
                                        //    // Remove current pairing
                                        //    Prompt("Device already in Bluetooth devices list. Removing from list before trying to sync...");
                                        //    errForget = NativeImports.BluetoothRemoveDevice(ref deviceInfo.Address);
                                        //    success = errForget == 0;

                                        //    if (success)
                                        //    {
                                        //        OnNewDeviceFound();
                                        //    }
                                        //}

                                        // Authenticate
                                        if (success)
                                        {
                                            WiitarDebug.Log("BEF - BluetoothAuthenticateDevice [SYNC]");
                                            errAuth = NativeImports.BluetoothAuthenticateDevice(IntPtr.Zero, radio, ref deviceInfo, password.ToString(), 6);
                                            WiitarDebug.Log("AFT - BluetoothAuthenticateDevice [SYNC]");
                                            //errAuth = NativeImports.BluetoothAuthenticateDeviceEx(IntPtr.Zero, radio, ref deviceInfo, null, NativeImports.AUTHENTICATION_REQUIREMENTS.MITMProtectionNotRequired);
                                            success = errAuth == 0;
                                        }

                                        //If it fails using SYNC method, try 1+2 method.
                                        if (!success)
                                        {
#if DEBUG
                                            Prompt("SYNC method didn't work. Trying 1+2 method...");
#endif

                                            var wiimoteBytes = BitConverter.GetBytes(deviceInfo.Address);

                                            password.Clear();

                                            for (int i = 0; i < 6; i++)
                                            {
                                                if (wiimoteBytes[i] > 0)
                                                    password.Append((char)wiimoteBytes[i]);
                                            }

                                            WiitarDebug.Log("BEF - BluetoothAuthenticateDevice [1+2]");
                                            errAuth = NativeImports.BluetoothAuthenticateDevice(IntPtr.Zero, radio, ref deviceInfo, password.ToString(), 6);
                                            WiitarDebug.Log("AFT - BluetoothAuthenticateDevice [1+2]");

                                            //errAuth = NativeImports.BluetoothAuthenticateDeviceEx(IntPtr.Zero, radio, ref deviceInfo, null, NativeImports.AUTHENTICATION_REQUIREMENTS.MITMProtectionNotRequired);
                                            success = errAuth == 0;
                                        }

                                        // Install PC Service
                                        if (success)
                                        {
                                            WiitarDebug.Log("BEF - BluetoothEnumerateInstalledServices");
                                            errService = NativeImports.BluetoothEnumerateInstalledServices(radio, ref deviceInfo, ref pcService, guids);
                                            WiitarDebug.Log("AFT - BluetoothEnumerateInstalledServices");
                                            success = errService == 0;
                                        }

                                        // Set to HID service
                                        if (success)
                                        {
                                            WiitarDebug.Log("BEF - BluetoothSetServiceState");
                                            errActivate = NativeImports.BluetoothSetServiceState(radio, ref deviceInfo, ref HidServiceClass, 0x01);
                                            WiitarDebug.Log("AFT - BluetoothSetServiceState");
                                            success = errActivate == 0;
                                        }

                                        if (success)
                                        {
                                            Prompt("Successfully Paired!", isBold: true);
                                            Count += 1;
                                        }
                                        else
                                        {
                                            var sb = new StringBuilder();
                                            //sb.AppendLine("Failed to pair.");

#if DEBUG
                                            sb.AppendLine("radio mac address: " + GetMacAddressStr(radioInfo.address));
                                            sb.AppendLine("wiimote mac address: " + GetMacAddressStr(deviceInfo.Address));
                                            sb.AppendLine("wiimote password: \"" + password.ToString() + "\"");
#endif

                                            
                                            if (errForget != 0)
                                            {
                                                sb.AppendLine(" >>> FAILED TO REMOVE DEVICE FROM BLUETOOTH DEVICES LIST. ERROR CODE 0x" + errForget.ToString("X"));
                                            }

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
#if DEBUG
                                    else
                                    {
                                        Prompt("(Found \"" + deviceInfo.szName + "\", but it is not a Wiimote)", isBold: false, isItalic: false, isSmall: true, isDebug: true);
                                    }
#endif

                                    WiitarDebug.Log("About to try BluetoothFindNextDevice...");
                                } while (NativeImports.BluetoothFindNextDevice(found, ref deviceInfo));
                            }
                        }
                        else
                        {
                            // Failed to get BT Radio info
                            Prompt("Found Bluetooth adapter but was unable to interact with it.");
                        }
                    }
                }

                // Close each Radio
                foreach (var openRadio in btRadios)
                {
                    WiitarDebug.Log("BEF - CloseHandle");
                    NativeImports.CloseHandle(openRadio);
                    WiitarDebug.Log("AFT - CloseHandle");
                }
            }
            else
            {
                // No (compatable) Bluetooth
                Prompt(
                    "No compatble Bluetooth Radios found (IF YOU SEE THIS MESSAGE, MENTION IT WHEN ASKING FOR HELP!).", isBold: true, isItalic: true);
                _notCompatable = true;
                return;
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

                //if (prompt.LineCount > 0)
                //    prompt.ScrollToLine(prompt.LineCount - 1);
                //prompt.ScrollToEnd();
            }));
        }

        //private void SetPrompt(string text)
        //{
        //    Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        prompt.Text = text;
        //        if (prompt.LineCount > 0)
        //            prompt.ScrollToLine(prompt.LineCount - 1);
        //        //prompt.ScrollToEnd();
        //    }));
        //}

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
