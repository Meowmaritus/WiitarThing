using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NintrollerLib;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace WiitarThing
{
    public enum DeviceState
    {
        None = 0,
        Discovered,
        Connected_XInput,
        Connected_VJoy
    }

    public delegate void ConnectStateChange(DeviceControl sender, DeviceState oldState, DeviceState newState);
    public delegate void ConnectionLost(DeviceControl sender);

    public partial class DeviceControl : UserControl
    {
        #region Members

        // private members
        private string devicePath;
        private Nintroller device;
        private DeviceState state;
        private IR     previousIR;
        private bool  snapIRpointer = false;
        private float rumbleAmount      = 0;
        private int   rumbleStepCount   = 0;
        private int   rumbleStepPeriod  = 10;
        private float rumbleSlowMult    = 0.5f;
        
        // internally public members
        internal Holders.Holder holder;
        internal Property       properties;
        internal int            targetXDevice = -1;
        internal bool           lowBatteryFired = false;
        internal bool           identifying = false;
        internal string         dName = "";
        internal System.Threading.Timer updateTimer;

        // constance
        internal const int UPDATE_SPEED = 25;

        // events
        public event ConnectStateChange OnConnectStateChange;
        public event ConnectionLost OnConnectionLost;

        #endregion

        #region Properties

        internal Nintroller Device
        {
            get { return device; }
            set
            {
                if (device != null)
                {
                    device.ExtensionChange -= device_ExtensionChange;
                    device.StateUpdate -= device_StateChange;
                    device.LowBattery -= device_LowBattery;

#if DEBUG
                    Device.StateUpdate -= Debug_Device_StateUpdate;
#endif
                }

                device = value;

                if (device != null)
                {
                    device.ExtensionChange += device_ExtensionChange;
                    device.StateUpdate += device_StateChange;
                    device.LowBattery += device_LowBattery;

#if DEBUG
                    Device.StateUpdate += Debug_Device_StateUpdate;
#endif
                }
            }
        }

        internal ControllerType DeviceType { get; private set; }

        internal string DevicePath
        {
            get { return devicePath; }
            private set { devicePath = value; }
        }

        internal bool Connected
        {
            get
            {
                if (device == null)
                    return false;

                return device.Connected;
            }
        }

        internal DeviceState ConnectionState
        {
            get
            {
                return state;
            }

            set
            {
                if (value != state)
                {
                    DeviceState previous = state;
                    SetState(value);

                    if (OnConnectStateChange != null)
                    {
                        OnConnectStateChange(this, previous, value);
                    }
                }
            }
        }

        #endregion

        public DeviceControl()
        {
            InitializeComponent();
        }

        public DeviceControl(Nintroller nintroller, string path)
            : this()
        {
            Device = nintroller;
            devicePath = path;

            Device.Disconnected += device_Disconnected;
        }

#if DEBUG
        private Windows.DebugDataWindow DebugDataWindowInstance = null;

        private void Debug_Device_StateUpdate(object sender, NintrollerStateEventArgs e)
        {
            if (e.state.DebugViewActive)
            {
                e.state.DebugViewActive = false;
                DebugViewActivate();
            }
        }

        private void DebugViewActivate()
        {
            if (DebugDataWindowInstance == null || !DebugDataWindowInstance.IsVisible)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DebugDataWindowInstance = new Windows.DebugDataWindow();
                    DebugDataWindowInstance.nintroller = Device;
                    DebugDataWindowInstance.RegisterNintrollerUpdate();
                    DebugDataWindowInstance.Show();

                }), System.Windows.Threading.DispatcherPriority.ContextIdle);


            }
        }
#endif

        public void RefreshState()
        {
            if (state != DeviceState.Connected_XInput)
                ConnectionState = DeviceState.Discovered;

            // Load Properties
            properties = UserPrefs.Instance.GetDevicePref(devicePath);
            if (properties != null)
            {
                SetName(string.IsNullOrWhiteSpace(properties.name) ? device.Type.ToString() : properties.name);
                ApplyCalibration(properties.calPref, properties.calString ?? "");
                snapIRpointer = properties.pointerMode != Property.PointerOffScreenMode.Center;
                if (!string.IsNullOrEmpty(properties.lastIcon))
                {
                    icon.Source = (ImageSource)Application.Current.Resources[properties.lastIcon];
                }
            }
            else
            {
                properties = new Property(devicePath);
                UpdateIcon(device.Type);
                SetName(device.Type.ToString());
            }
        }

        public void SetName(string newName)
        {
            dName = newName;
            labelName.Content = new TextBlock() { Text = newName };
        }

        public void Detatch()
        {
            device?.StopReading();
            holder?.Close();
            lowBatteryFired = false;
            ConnectionState = DeviceState.Discovered;
            Dispatcher.BeginInvoke
            (
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => statusGradient.Color = (Color)FindResource("AntemBlue")
            ));
        }

        public void SetState(DeviceState newState)
        {
            state = newState;
            if (updateTimer != null)
            {
                updateTimer.Dispose();
                updateTimer = null;
            }

            switch (newState)
            {
                case DeviceState.None:
                    btnIdentify.IsEnabled   = false;
                    btnProperties.IsEnabled = false;
                    btnXinput.IsEnabled     = false;
                    //btnVjoy.IsEnabled     = false;
                    //btnConfig.IsEnabled     = false;
                    btnDetatch.IsEnabled    = false;
                   // btnConfig.Visibility    = Visibility.Hidden;
                    btnDetatch.Visibility   = Visibility.Hidden;
                    btnDebugView.Visibility = Visibility.Hidden;
                    break;

                case DeviceState.Discovered:
                    btnIdentify.IsEnabled   = true;
                    btnProperties.IsEnabled = true;
                    btnXinput.IsEnabled     = true;
                    //btnVjoy.IsEnabled     = true;
                    //btnConfig.IsEnabled     = false;
                    btnDetatch.IsEnabled    = false;
                    //btnConfig.Visibility    = Visibility.Hidden;
                    btnDetatch.Visibility   = Visibility.Hidden;
                    btnDebugView.Visibility = Visibility.Hidden;
                    break;

                case DeviceState.Connected_XInput:
                    btnIdentify.IsEnabled   = true;
                    btnProperties.IsEnabled = true;
                    btnXinput.IsEnabled     = false;
                    //btnVjoy.IsEnabled     = false;
                    //btnConfig.IsEnabled     = true;
                    btnDetatch.IsEnabled    = true;
                    //btnConfig.Visibility    = Visibility.Visible;
                    btnDetatch.Visibility   = Visibility.Visible;

#if DEBUG
                    btnDebugView.Visibility = Visibility.Visible;
#else
                    btnDebugView.Visibility = Visibility.Hidden;
#endif

                    var xHolder = new Holders.XInputHolder(device.Type);
                    LoadProfile(properties.profile, xHolder);
                    xHolder.ConnectXInput(targetXDevice);
                    holder = xHolder;
                    device.SetPlayerLED(targetXDevice + 1);
                    updateTimer = new System.Threading.Timer(HolderUpdate, device, 1000, UPDATE_SPEED);
                    break;

                //case DeviceState.Connected_VJoy:
                //    btnIdentify.IsEnabled = true;
                //    btnProperties.IsEnabled = true;
                //    btnXinput.IsEnabled = false;
                //    btnVjoy.IsEnabled = false;
                //    btnConfig.IsEnabled = true;
                //    btnDetatch.IsEnabled = true;
                //    btnConfig.Visibility = System.Windows.Visibility.Visible;
                //    btnDetatch.Visibility = System.Windows.Visibility.Visible;

                //    // Instantiate VJoy Holder (not for 1st release)
                //    break;
            }
        }

        void device_ExtensionChange(object sender, NintrollerExtensionEventArgs e)
        {
            DeviceType = e.controllerType;

            if (holder != null)
            {
                holder.ClearAllValues();
                holder.ClearAllMappings();
                holder.AddMapping(DeviceType);
            }

            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, 
                new Action(() =>
                {
                    RefreshState();
                }
            ));
        }

        void device_LowBattery(object sender, LowBatteryEventArgs e)
        {
            SetBatteryStatus(e.batteryLevel == BatteryStatus.Low || e.batteryLevel == BatteryStatus.VeryLow);
        }

        void device_StateChange(object sender, NintrollerStateEventArgs e)
        {
            // Makes the timer wait
            if (updateTimer != null) updateTimer.Change(1000, UPDATE_SPEED);

            if (holder == null)
            {
                return;
            }

//            float intensity = 0;
//            if (holder.Values.TryGetValue(Flags.RUMBLE, out intensity))
//            {
//                rumbleAmount = (int)intensity;
                RumbleStep();
//            }

            switch (e.controllerType)
            {
                // TODO: Motion Plus Reading (not for 1st release)
                // TODO: Balance Board Reading (not for 1st release)
                // TODO: Musical Extension readings (not for 1st release)
                case ControllerType.ProController:
#region Pro Controller
                    ProController pro = (ProController)e.state;

                    holder.SetValue(ProController.InputNames.A, pro.A);
                    holder.SetValue(ProController.InputNames.B, pro.B);
                    holder.SetValue(ProController.InputNames.X, pro.X);
                    holder.SetValue(ProController.InputNames.Y, pro.Y);

                    holder.SetValue(ProController.InputNames.UP, pro.Up);
                    holder.SetValue(ProController.InputNames.DOWN, pro.Down);
                    holder.SetValue(ProController.InputNames.LEFT, pro.Left);
                    holder.SetValue(ProController.InputNames.RIGHT, pro.Right);

                    holder.SetValue(ProController.InputNames.L, pro.L);
                    holder.SetValue(ProController.InputNames.R, pro.R);
                    holder.SetValue(ProController.InputNames.ZL, pro.ZL);
                    holder.SetValue(ProController.InputNames.ZR, pro.ZR);

                    holder.SetValue(ProController.InputNames.START, pro.Plus);
                    holder.SetValue(ProController.InputNames.SELECT, pro.Minus);
                    holder.SetValue(ProController.InputNames.HOME, pro.Home);
                    holder.SetValue(ProController.InputNames.LS, pro.LStick);
                    holder.SetValue(ProController.InputNames.RS, pro.RStick);

                    holder.SetValue(ProController.InputNames.LRIGHT, pro.LJoy.X > 0 ? pro.LJoy.X : 0f);
                    holder.SetValue(ProController.InputNames.LLEFT,  pro.LJoy.X < 0 ? pro.LJoy.X * -1 : 0f);
                    holder.SetValue(ProController.InputNames.LUP,    pro.LJoy.Y > 0 ? pro.LJoy.Y : 0f);
                    holder.SetValue(ProController.InputNames.LDOWN,  pro.LJoy.Y < 0 ? pro.LJoy.Y * -1 : 0f);

                    holder.SetValue(ProController.InputNames.RRIGHT, pro.RJoy.X > 0 ? pro.RJoy.X : 0f);
                    holder.SetValue(ProController.InputNames.RLEFT,  pro.RJoy.X < 0 ? pro.RJoy.X * -1 : 0f);
                    holder.SetValue(ProController.InputNames.RUP,    pro.RJoy.Y > 0 ? pro.RJoy.Y : 0f);
                    holder.SetValue(ProController.InputNames.RDOWN,  pro.RJoy.Y < 0 ? pro.RJoy.Y * -1 : 0f);
#endregion
                    break;

                case ControllerType.Wiimote:
                    Wiimote wm = (Wiimote)e.state;
                    SetWiimoteInputs(wm);
                    break;

                case ControllerType.Nunchuk:
                case ControllerType.NunchukB:
#region Nunchuk
                    Nunchuk nun = (Nunchuk)e.state;

                    SetWiimoteInputs(nun.wiimote);

                    holder.SetValue(Nunchuk.InputNames.C, nun.C);
                    holder.SetValue(Nunchuk.InputNames.Z, nun.Z);

                    holder.SetValue(Nunchuk.InputNames.RIGHT, nun.joystick.X > 0 ? nun.joystick.X : 0f);
                    holder.SetValue(Nunchuk.InputNames.LEFT,  nun.joystick.X < 0 ? nun.joystick.X * -1 : 0f);
                    holder.SetValue(Nunchuk.InputNames.UP,    nun.joystick.Y > 0 ? nun.joystick.Y : 0f);
                    holder.SetValue(Nunchuk.InputNames.DOWN,  nun.joystick.Y < 0 ? nun.joystick.Y * -1 : 0f);

                    //TODO: Nunchuk Accelerometer (not for 1st release)
                    holder.SetValue(Nunchuk.InputNames.TILT_RIGHT, nun.accelerometer.X > 0 ? nun.accelerometer.X : 0f);
                    holder.SetValue(Nunchuk.InputNames.TILT_LEFT, nun.accelerometer.X < 0 ? nun.accelerometer.X * -1 : 0f);
                    holder.SetValue(Nunchuk.InputNames.TILT_UP, nun.accelerometer.Y > 0 ? nun.accelerometer.Y : 0f);
                    holder.SetValue(Nunchuk.InputNames.TILT_DOWN, nun.accelerometer.Y < 0 ? nun.accelerometer.Y * -1 : 0f);

                    holder.SetValue(Nunchuk.InputNames.ACC_SHAKE_X, nun.accelerometer.X > 1.15f);
                    holder.SetValue(Nunchuk.InputNames.ACC_SHAKE_Y, nun.accelerometer.Y > 1.15f);
                    holder.SetValue(Nunchuk.InputNames.ACC_SHAKE_Z, nun.accelerometer.Z > 1.15f);
#endregion
                    break;

                case ControllerType.ClassicController:
#region Classic Controller
                    ClassicController cc = (ClassicController)e.state;

                    SetWiimoteInputs(cc.wiimote);

                    holder.SetValue(ClassicController.InputNames.A, cc.A);
                    holder.SetValue(ClassicController.InputNames.B, cc.B);
                    holder.SetValue(ClassicController.InputNames.X, cc.X);
                    holder.SetValue(ClassicController.InputNames.Y, cc.Y);

                    holder.SetValue(ClassicController.InputNames.UP, cc.Up);
                    holder.SetValue(ClassicController.InputNames.DOWN, cc.Down);
                    holder.SetValue(ClassicController.InputNames.LEFT, cc.Left);
                    holder.SetValue(ClassicController.InputNames.RIGHT, cc.Right);

                    holder.SetValue(ClassicController.InputNames.L, cc.L.value > 0);
                    holder.SetValue(ClassicController.InputNames.R, cc.R.value > 0);
                    holder.SetValue(ClassicController.InputNames.ZL, cc.ZL);
                    holder.SetValue(ClassicController.InputNames.ZR, cc.ZR);

                    holder.SetValue(ClassicController.InputNames.START, cc.Start);
                    holder.SetValue(ClassicController.InputNames.SELECT, cc.Select);
                    holder.SetValue(ClassicController.InputNames.HOME, cc.Home);

                    holder.SetValue(ClassicController.InputNames.LFULL, cc.LFull);
                    holder.SetValue(ClassicController.InputNames.RFULL, cc.RFull);
                    holder.SetValue(ClassicController.InputNames.LT, cc.L.value > 0.1f ? cc.L.value : 0f);
                    holder.SetValue(ClassicController.InputNames.RT, cc.R.value > 0.1f ? cc.R.value : 0f);

                    holder.SetValue(ClassicController.InputNames.LRIGHT, cc.LJoy.X > 0 ? cc.LJoy.X : 0f);
                    holder.SetValue(ClassicController.InputNames.LLEFT, cc.LJoy.X < 0 ? cc.LJoy.X * -1 : 0f);
                    holder.SetValue(ClassicController.InputNames.LUP, cc.LJoy.Y > 0 ? cc.LJoy.Y : 0f);
                    holder.SetValue(ClassicController.InputNames.LDOWN, cc.LJoy.Y < 0 ? cc.LJoy.Y * -1 : 0f);

                    holder.SetValue(ClassicController.InputNames.RRIGHT, cc.RJoy.X > 0 ? cc.RJoy.X : 0f);
                    holder.SetValue(ClassicController.InputNames.RLEFT, cc.RJoy.X < 0 ? cc.RJoy.X * -1 : 0f);
                    holder.SetValue(ClassicController.InputNames.RUP, cc.RJoy.Y > 0 ? cc.RJoy.Y : 0f);
                    holder.SetValue(ClassicController.InputNames.RDOWN, cc.RJoy.Y < 0 ? cc.RJoy.Y * -1 : 0f);
#endregion
                    break;

                case ControllerType.ClassicControllerPro:
#region Classic Controller Pro
                    ClassicControllerPro ccp = (ClassicControllerPro)e.state;

                    SetWiimoteInputs(ccp.wiimote);

                    holder.SetValue(ClassicControllerPro.InputNames.A, ccp.A);
                    holder.SetValue(ClassicControllerPro.InputNames.B, ccp.B);
                    holder.SetValue(ClassicControllerPro.InputNames.X, ccp.X);
                    holder.SetValue(ClassicControllerPro.InputNames.Y, ccp.Y);

                    holder.SetValue(ClassicControllerPro.InputNames.UP, ccp.Up);
                    holder.SetValue(ClassicControllerPro.InputNames.DOWN, ccp.Down);
                    holder.SetValue(ClassicControllerPro.InputNames.LEFT, ccp.Left);
                    holder.SetValue(ClassicControllerPro.InputNames.RIGHT, ccp.Right);

                    holder.SetValue(ClassicControllerPro.InputNames.L, ccp.L);
                    holder.SetValue(ClassicControllerPro.InputNames.R, ccp.R);
                    holder.SetValue(ClassicControllerPro.InputNames.ZL, ccp.ZL);
                    holder.SetValue(ClassicControllerPro.InputNames.ZR, ccp.ZR);

                    holder.SetValue(ClassicControllerPro.InputNames.START, ccp.Start);
                    holder.SetValue(ClassicControllerPro.InputNames.SELECT, ccp.Select);
                    holder.SetValue(ClassicControllerPro.InputNames.HOME, ccp.Home);

                    holder.SetValue(ClassicControllerPro.InputNames.LRIGHT, ccp.LJoy.X > 0 ? ccp.LJoy.X : 0f);
                    holder.SetValue(ClassicControllerPro.InputNames.LLEFT, ccp.LJoy.X < 0 ? ccp.LJoy.X * -1 : 0f);
                    holder.SetValue(ClassicControllerPro.InputNames.LUP, ccp.LJoy.Y > 0 ? ccp.LJoy.Y : 0f);
                    holder.SetValue(ClassicControllerPro.InputNames.LDOWN, ccp.LJoy.Y < 0 ? ccp.LJoy.Y * -1 : 0f);

                    holder.SetValue(ClassicControllerPro.InputNames.RRIGHT, ccp.RJoy.X > 0 ? ccp.RJoy.X : 0f);
                    holder.SetValue(ClassicControllerPro.InputNames.RLEFT, ccp.RJoy.X < 0 ? ccp.RJoy.X * -1 : 0f);
                    holder.SetValue(ClassicControllerPro.InputNames.RUP, ccp.RJoy.Y > 0 ? ccp.RJoy.Y : 0f);
                    holder.SetValue(ClassicControllerPro.InputNames.RDOWN, ccp.RJoy.Y < 0 ? ccp.RJoy.Y * -1 : 0f);
#endregion
                    break;

                case ControllerType.Guitar:
#region Wii Guitar
                    Guitar gtr = (Guitar)e.state;

                    //SetWiimoteInputs(gtr.wiimote);

                    holder.SetValue(Guitar.InputNames.G, gtr.G);
                    holder.SetValue(Guitar.InputNames.R, gtr.R);
                    holder.SetValue(Guitar.InputNames.Y, gtr.Y);
                    holder.SetValue(Guitar.InputNames.B, gtr.B);
                    holder.SetValue(Guitar.InputNames.O, gtr.O);

                    holder.SetValue(Guitar.InputNames.UP, gtr.Up);
                    holder.SetValue(Guitar.InputNames.DOWN, gtr.Down);
                    holder.SetValue(Guitar.InputNames.LEFT, gtr.Left);
                    holder.SetValue(Guitar.InputNames.RIGHT, gtr.Right);

                    holder.SetValue(Guitar.InputNames.WHAMMYHIGH, gtr.WhammyHigh);
                    holder.SetValue(Guitar.InputNames.WHAMMYLOW, gtr.WhammyLow);

                    holder.SetValue(Guitar.InputNames.TILTHIGH, gtr.TiltHigh);
                    holder.SetValue(Guitar.InputNames.TILTLOW, gtr.TiltLow);

                    holder.SetValue(Guitar.InputNames.START, gtr.Start);
                    holder.SetValue(Guitar.InputNames.SELECT, gtr.Select);
#endregion
                    break;

                case ControllerType.Drums:
#region Wii Drums
                    Drums drm = (Drums)e.state;

                    holder.SetValue(Drums.InputNames.G, drm.G);
                    holder.SetValue(Drums.InputNames.R, drm.R);
                    holder.SetValue(Drums.InputNames.Y, drm.Y);
                    holder.SetValue(Drums.InputNames.B, drm.B);
                    holder.SetValue(Drums.InputNames.O, drm.O);
                    holder.SetValue(Drums.InputNames.BASS, drm.Bass);

                    holder.SetValue(Drums.InputNames.UP, drm.Up);
                    holder.SetValue(Drums.InputNames.DOWN, drm.Down);
                    holder.SetValue(Drums.InputNames.LEFT, drm.Left);
                    holder.SetValue(Drums.InputNames.RIGHT, drm.Right);

                    holder.SetValue(Drums.InputNames.START, drm.Start);
                    holder.SetValue(Drums.InputNames.SELECT, drm.Select);
#endregion
                break;

                case ControllerType.Turntable:
#region Wii Turntable
                    Turntable ttb = (Turntable)e.state;

                    SetWiimoteInputs(ttb.wiimote);

                    // analog
                    holder.SetValue(Turntable.InputNames.LUP, ttb.Joy.Y > 0 ? ttb.Joy.Y : 0f);
                    holder.SetValue(Turntable.InputNames.LDOWN, ttb.Joy.Y < 0 ? -ttb.Joy.Y : 0f);
                    holder.SetValue(Turntable.InputNames.LLEFT, ttb.Joy.X < 0 ? -ttb.Joy.X : 0f);
                    holder.SetValue(Turntable.InputNames.LRIGHT, ttb.Joy.X > 0 ? ttb.Joy.X : 0f);

                    // digital
                    holder.SetValue(Turntable.InputNames.UP, ttb.Joy.Y > 0.5f ? 1f : 0f);
                    holder.SetValue(Turntable.InputNames.DOWN, ttb.Joy.Y < -0.5f ? -1f : 0f);
                    holder.SetValue(Turntable.InputNames.LEFT, ttb.Joy.X < -0.5f ? -1f : 0f);
                    holder.SetValue(Turntable.InputNames.RIGHT, ttb.Joy.X > 0.5f ? 1f : 0f);

                    holder.SetValue(Turntable.InputNames.LTABLECLKWISE, ttb.JoyTableLR.X > 0 ? ttb.JoyTableLR.X : 0f);
                    holder.SetValue(Turntable.InputNames.LTABLECTRCLKWISE, ttb.JoyTableLR.X < 0 ? -ttb.JoyTableLR.X : 0f);

                    holder.SetValue(Turntable.InputNames.RTABLECLKWISE, ttb.JoyTableLR.Y > 0 ? ttb.JoyTableLR.Y : 0f);
                    holder.SetValue(Turntable.InputNames.RTABLECTRCLKWISE, ttb.JoyTableLR.Y < 0 ? -ttb.JoyTableLR.Y : 0f);

                    holder.SetValue(Turntable.InputNames.RG, ttb.RG);
                    holder.SetValue(Turntable.InputNames.RR, ttb.RR);
                    holder.SetValue(Turntable.InputNames.RB, ttb.RB);
                    holder.SetValue(Turntable.InputNames.RBUTTONS, ttb.RButtons.value);

                    holder.SetValue(Turntable.InputNames.LG, ttb.LG);
                    holder.SetValue(Turntable.InputNames.LR, ttb.LR);
                    holder.SetValue(Turntable.InputNames.LB, ttb.LB);
                    holder.SetValue(Turntable.InputNames.LBUTTONS, ttb.LButtons.value);

                    holder.SetValue(Turntable.InputNames.DIALCLKWISE, ttb.JoyDialCrossfade.X > 0 ? ttb.JoyDialCrossfade.X : 0f);
                    holder.SetValue(Turntable.InputNames.DIALCTRCLKWISE, ttb.JoyDialCrossfade.X < 0 ? -ttb.JoyDialCrossfade.X : 0f);
                    holder.SetValue(Turntable.InputNames.DIALT, ttb.Dial.value);

                    holder.SetValue(Turntable.InputNames.CROSSFADERLEFT, ttb.JoyDialCrossfade.Y < 0 ? -ttb.JoyDialCrossfade.Y : 0f);
                    holder.SetValue(Turntable.InputNames.CROSSFADERRIGHT, ttb.JoyDialCrossfade.Y > 0 ? ttb.JoyDialCrossfade.Y : 0f);
                    holder.SetValue(Turntable.InputNames.CROSSFADERT, ttb.Crossfader.value);

                    holder.SetValue(Turntable.InputNames.EUPHORIA, ttb.Euphoria);
                    holder.SetValue(Turntable.InputNames.SELECT, ttb.Select);
                    holder.SetValue(Turntable.InputNames.START, ttb.Start);
#endregion
                    break;
            }
            
            holder.Update();

            // Resumes the timer in case this method is not called withing 100ms
            if (updateTimer != null) updateTimer.Change(100, UPDATE_SPEED);
        }

        private void device_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                new Action(() =>
                {
                    Detatch();
                    OnConnectionLost?.Invoke(this);
                    MainWindow.Instance.ShowBalloon("Connection Lost", "Failed to communicate with controller. It may no longer be connected.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Error);
                }
            ));
        }

        private void SetWiimoteInputs(Wiimote wm)
        {
            wm.irSensor.Normalize();

            holder.SetValue(Wiimote.InputNames.A, wm.buttons.A);
            holder.SetValue(Wiimote.InputNames.B, wm.buttons.B);
            holder.SetValue(Wiimote.InputNames.ONE, wm.buttons.One);
            holder.SetValue(Wiimote.InputNames.TWO, wm.buttons.Two);

            holder.SetValue(Wiimote.InputNames.UP, wm.buttons.Up);
            holder.SetValue(Wiimote.InputNames.DOWN, wm.buttons.Down);
            holder.SetValue(Wiimote.InputNames.LEFT, wm.buttons.Left);
            holder.SetValue(Wiimote.InputNames.RIGHT, wm.buttons.Right);

            holder.SetValue(Wiimote.InputNames.MINUS, wm.buttons.Minus);
            holder.SetValue(Wiimote.InputNames.PLUS, wm.buttons.Plus);
            holder.SetValue(Wiimote.InputNames.HOME, wm.buttons.Home);

            holder.SetValue(Wiimote.InputNames.TILT_RIGHT, wm.accelerometer.X > 0 ? wm.accelerometer.X : 0);
            holder.SetValue(Wiimote.InputNames.TILT_LEFT, wm.accelerometer.X < 0 ? wm.accelerometer.X : 0);
            holder.SetValue(Wiimote.InputNames.TILT_UP, wm.accelerometer.Y > 0 ? wm.accelerometer.Y : 0);
            holder.SetValue(Wiimote.InputNames.TILT_DOWN, wm.accelerometer.Y < 0 ? wm.accelerometer.Y : 0);

            holder.SetValue(Wiimote.InputNames.ACC_SHAKE_X, wm.accelerometer.X > 1.15);
            holder.SetValue(Wiimote.InputNames.ACC_SHAKE_Y, wm.accelerometer.Y > 1.15);
            holder.SetValue(Wiimote.InputNames.ACC_SHAKE_Z, wm.accelerometer.Z > 1.15);

            if (snapIRpointer && !wm.irSensor.point1.visible && !wm.irSensor.point2.visible)
            {
                if (properties.pointerMode == Property.PointerOffScreenMode.SnapX ||
                    properties.pointerMode == Property.PointerOffScreenMode.SnapXY)
                {
                    wm.irSensor.X = previousIR.X;
                }

                if (properties.pointerMode == Property.PointerOffScreenMode.SnapY ||
                    properties.pointerMode == Property.PointerOffScreenMode.SnapXY)
                {
                    wm.irSensor.Y = previousIR.Y;
                }
            }

            holder.SetValue(Wiimote.InputNames.IR_RIGHT, wm.irSensor.X > 0 ? wm.irSensor.X : 0);
            holder.SetValue(Wiimote.InputNames.IR_LEFT, wm.irSensor.X < 0 ? wm.irSensor.X : 0);
            holder.SetValue(Wiimote.InputNames.IR_UP, wm.irSensor.Y > 0 ? wm.irSensor.Y : 0);
            holder.SetValue(Wiimote.InputNames.IR_DOWN, wm.irSensor.Y < 0 ? wm.irSensor.Y : 0);

            previousIR = wm.irSensor;
        }

        private void HolderUpdate(object holderState)
        {
            if (holder == null) return;

            holder.Update();

//            float intensity = 0;
//            if (holder.Values.TryGetValue(Inputs.Flags.RUMBLE, out intensity))
//            {
//                rumbleAmount = (int)intensity;
                RumbleStep();
//            }

            SetBatteryStatus(device.BatteryLevel == BatteryStatus.Low);
        }

        void RumbleStep()
        {
            if (identifying) return;

            bool currentRumbleState = device.RumbleEnabled;

            // disable rumble for turntables
            if (!properties.useRumble || device.Type == ControllerType.Turntable)
            {
                if (currentRumbleState) device.RumbleEnabled = false;
                return;
            }

            rumbleAmount = holder.RumbleAmount;

            float dutyCycle = 0;
            float modifier = properties.rumbleIntensity * 0.5f;

            if (rumbleAmount < 256)
            {
                dutyCycle = rumbleSlowMult * (float)rumbleAmount / 256f;
            }
            else
            {
                dutyCycle = (float)rumbleAmount / 65535f;
            }

            int stopStep = (int)Math.Round(modifier * dutyCycle * rumbleStepPeriod);

            if (rumbleStepCount < stopStep)
            {
                if (!currentRumbleState) device.RumbleEnabled = true;
            }
            else
            {
                if (currentRumbleState) device.RumbleEnabled = false;
            }

            rumbleStepCount += 1;

            if (rumbleStepCount >= rumbleStepPeriod)
            {
                rumbleStepCount = 0;
            }
        }

        private void SetBatteryStatus(bool isLow)
        {
            if (isLow && !lowBatteryFired)
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                        {
                            statusGradient.Color = (Color)FindResource("LowBattery");
                            if (MainWindow.Instance.trayIcon.Visibility == Visibility.Visible)
                            {
                                lowBatteryFired = true;
                                MainWindow.Instance.ShowBalloon
                                (
                                    "Battery Low",
                                    dName + (!dName.Equals(device.Type.ToString()) ? " (" + device.Type.ToString() + ") " : " ")
                                    + "is running low on battery life.",
                                    Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning,
                                    System.Media.SystemSounds.Hand
                                );
                            }
                        }
                ));
            }
            else if (!isLow && lowBatteryFired)
            {
                statusGradient = (GradientStop)FindResource("AntemBlue");
                lowBatteryFired = false;
            }
        }

        private void LoadProfile(string profilePath, Holders.Holder h)
        {
            Profile loadedProfile = null;

            if (!string.IsNullOrWhiteSpace(profilePath) && File.Exists(profilePath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Profile));

                    using (FileStream stream = File.OpenRead(profilePath))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        loadedProfile = serializer.Deserialize(reader) as Profile;
                        reader.Close();
                        stream.Close();
                    }
                }
                catch { }
            }

            if (loadedProfile == null)
            {
                loadedProfile = UserPrefs.Instance.defaultProfile;
            }

            if (loadedProfile != null)
            {
                for (int i = 0; i < Math.Min(loadedProfile.controllerMapKeys.Count, loadedProfile.controllerMapValues.Count); i++)
                {
                    h.SetMapping(loadedProfile.controllerMapKeys[i], loadedProfile.controllerMapValues[i]);
                    CheckIR(loadedProfile.controllerMapKeys[i]);
                }
            }
        }

        private void UpdateIcon(ControllerType cType)
        {
            switch (cType)
            {
                case ControllerType.ProController:
                    icon.Source = (ImageSource)Application.Current.Resources["ProIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "ProIcon");
                    break;
                case ControllerType.ClassicControllerPro:
                    icon.Source = (ImageSource)Application.Current.Resources["CCPIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "CCPIcon");
                    break;
                case ControllerType.ClassicController:
                    icon.Source = (ImageSource)Application.Current.Resources["CCIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "CCIcon");
                    break;
                case ControllerType.Nunchuk:
                case ControllerType.NunchukB:
                    icon.Source = (ImageSource)Application.Current.Resources["WNIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "WNIcon");
                    break;

                case ControllerType.Guitar:
                    icon.Source = (ImageSource)Application.Current.Resources["GTRIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "GTRIcon");
                    break;

                case ControllerType.Drums:
                    icon.Source = (ImageSource)Application.Current.Resources["DRMIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "DRMIcon");
                    break;

                case ControllerType.Turntable:
                    icon.Source = (ImageSource)Application.Current.Resources["TTBIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "TTBIcon");
                    break;

                default:
                    icon.Source = (ImageSource)Application.Current.Resources["WIcon"];
                    UserPrefs.Instance.UpdateDeviceIcon(devicePath, "WIcon");
                    break;
            }
        }

        private void ApplyCalibration(Property.CalibrationPreference calPref, string calString)
        {
            // Load calibration settings
            switch (calPref)
            {
                case Property.CalibrationPreference.Default:
                    device.SetCalibration(Calibrations.CalibrationPreset.Default);
                    break;

                case Property.CalibrationPreference.More:
                    device.SetCalibration(Calibrations.CalibrationPreset.Modest);
                    break;

                case Property.CalibrationPreference.Extra:
                    device.SetCalibration(Calibrations.CalibrationPreset.Extra);
                    break;

                case Property.CalibrationPreference.Minimal:
                    device.SetCalibration(Calibrations.CalibrationPreset.Minimum);
                    break;

                case Property.CalibrationPreference.Raw:
                    device.SetCalibration(Calibrations.CalibrationPreset.None);
                    break;

                case Property.CalibrationPreference.Custom:
                    CalibrationStorage calStor = new CalibrationStorage(calString);
                    device.SetCalibration(calStor.ProCalibration);
                    device.SetCalibration(calStor.NunchukCalibration);
                    device.SetCalibration(calStor.ClassicCalibration);
                    device.SetCalibration(calStor.ClassicProCalibration);
                    device.SetCalibration(calStor.WiimoteCalibration);
                    break;
            }
        }

        private void CheckIR(string assignment)
        {
            if (assignment.StartsWith("wIR") && device != null && device.IRMode == IRCamMode.Off)
            {
                if (device.Type == ControllerType.Wiimote ||
                    device.Type == ControllerType.Nunchuk ||
                    device.Type == ControllerType.NunchukB)
                {
                    device.IRMode = IRCamMode.Basic;
                }
            }
        }

#region UI Events
        private void btnXinput_Click(object sender, RoutedEventArgs e)
        {
            if (btnXinput.ContextMenu != null)
            {
                XOption1.IsEnabled = Holders.XInputHolder.availabe[0];
                XOption2.IsEnabled = Holders.XInputHolder.availabe[1];
                XOption3.IsEnabled = Holders.XInputHolder.availabe[2];
                XOption4.IsEnabled = Holders.XInputHolder.availabe[3];

                btnXinput.ContextMenu.PlacementTarget = btnXinput;
                btnXinput.ContextMenu.IsOpen = true;
            }
        }

        private void AssignToXinputPlayer(int player)
        {
            device.BeginReading();
            device.GetStatus();

            targetXDevice = player;
            ConnectionState = DeviceState.Connected_XInput;

            RefreshState();
        }

        private void XOption_Click(object sender, RoutedEventArgs e)
        {
            if (Device.Type != ControllerType.ProController)
                MessageBox.Show("Press 1+2 on the Wii remote and press OK to continue.", "Connect Wii Remote", MessageBoxButton.OK, MessageBoxImage.Information);

            if (device.DataStream.Open() && device.DataStream.CanRead)
            {
                int tmp = 0;
                if (int.TryParse(((MenuItem)sender).Name.Replace("XOption", ""), out tmp))
                {
                    AssignToXinputPlayer(tmp - 1);
                }
            }

            
        }

        private void btnDetatch_Click(object sender, RoutedEventArgs e)
        {
            Detatch();
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            var config = new ConfigWindow(holder.Mappings, device.Type);
            config.ShowDialog();
            if (config.result)
            {
                foreach (KeyValuePair<string, string> pair in config.map)
                {
                    holder.SetMapping(pair.Key, pair.Value);
                    CheckIR(pair.Key);
                }
            }
        }

        private void btnIdentify_Click(object sender, RoutedEventArgs e)
        {
            if (identifying)
                return;

            identifying = true;
            Task.Run(() =>
            {
                bool wasConnected = Connected;
                if (wasConnected || (device.DataStream.Open() && device.DataStream.CanRead))
                {
                    if (!wasConnected)
                        device.BeginReading();

                    device.RumbleEnabled = true;

                    // O___
                    // _O__
                    // __O_
                    // ___O
                    // __O_
                    // _O__
                    // O___
                    for (int i = -3; i < 4; i++)
                    {
                        int led = 4 - Math.Abs(i);
                        device.SetPlayerLED(led);
                        Thread.Sleep(75);
                    }

                    device.RumbleEnabled = false;
                    if (targetXDevice > -1)
                        device.SetPlayerLED(targetXDevice + 1);
                    else
                        device.SetBinaryLEDs(0b1001);

                    if (!wasConnected)
                        device.StopReading();
                }

                identifying = false;
            });
        }

        //private void btnVjoy_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (btnVjoy_image == null)
        //        return;

        //    if ((bool)e.NewValue)
        //    {
        //        btnVjoy_image.Opacity = 1.0;
        //    }
        //    else
        //    {
        //        btnVjoy_image.Opacity = 0.5;
        //    }
        //}

        private void btnXinput_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                btnXinput.Visibility = Visibility.Visible;
            }
            else
            {
                btnXinput.Visibility = Visibility.Collapsed;
            }
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e)
        {
            PropWindow win = new PropWindow(properties, device.Type.ToString());
            win.ShowDialog();

            if (win.customCalibrate)
            {
                CalibrateWindow cb = new CalibrateWindow(device);
                cb.ShowDialog();

                if (cb.doSave)
                {
                    win.props.calString = cb.Calibration.ToString();
                    win.ShowDialog();
                }
                else
                {
                    win.Show();
                }
            }

            if (win.doSave)
            {
                ApplyCalibration(win.props.calPref, win.props.calString);
                properties = new Property(win.props);
                snapIRpointer = properties.pointerMode != Property.PointerOffScreenMode.Center;
                SetName(properties.name);
                UserPrefs.Instance.AddDevicePref(properties);
                UserPrefs.SavePrefs();
            }
        }
#endregion

        private void btnDebugView_Click(object sender, RoutedEventArgs e)
        {
#if DEBUG
            DebugViewActivate();
#endif
        }
    }
}