using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace NintrollerLib
{
    public struct Wiimote : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public CoreButtons buttons;
        public Accelerometer accelerometer;
        public IR irSensor;
        //INintrollerState extension;

        public Wiimote(byte[] rawData)
        {
            buttons = new CoreButtons();
            accelerometer = new Accelerometer();
            irSensor = new IR();
            //extension = null;

#if DEBUG
            _debugViewActive = false;
#endif

            Update(rawData);
        }

        public void Update(byte[] data)
        {
            buttons.Parse(data, 1);
            accelerometer.Parse(data, 3);
            irSensor.Parse(data, 3);

            accelerometer.Normalize();
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    //accelerometer.Calibrate(Calibrations.Defaults.WiimoteDefault.accelerometer);
                    SetCalibration(Calibrations.Defaults.WiimoteDefault);
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    SetCalibration(Calibrations.Moderate.WiimoteModest);
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    SetCalibration(Calibrations.Extras.WiimoteExtra);
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    SetCalibration(Calibrations.Minimum.WiimoteMinimal);
                    break;

                case Calibrations.CalibrationPreset.None:
                    SetCalibration(Calibrations.None.WiimoteRaw);
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.CalibrationEmpty)
            {
                // don't apply empty calibrations
                return;
            }

            if (from.GetType() == typeof(Wiimote))
            {
                accelerometer.Calibrate(((Wiimote)from).accelerometer);
                irSensor.boundingArea = ((Wiimote)from).irSensor.boundingArea;
            }
            else if (from.GetType() == typeof(Nunchuk))
            {
                accelerometer.Calibrate(((Nunchuk)from).wiimote.accelerometer);
                irSensor.boundingArea = ((Nunchuk)from).wiimote.irSensor.boundingArea;
            }
            else if (from.GetType() == typeof(ClassicController))
            {
                accelerometer.Calibrate(((ClassicController)from).wiimote.accelerometer);
                irSensor.boundingArea = ((ClassicController)from).wiimote.irSensor.boundingArea;
            }
            else if (from.GetType() == typeof(ClassicControllerPro))
            {
                accelerometer.Calibrate(((ClassicControllerPro)from).wiimote.accelerometer);
                irSensor.boundingArea = ((ClassicControllerPro)from).wiimote.irSensor.boundingArea;
            }
        }

        public void SetCalibration(string calibrationString)
        {
            if (calibrationString.Count(c => c == '0') > 5)
            {
                // don't set empty calibrations
                return;
            }

            string[] components = calibrationString.Split(new char[] {':'});

            foreach (string component in components)
            {
                if (component.StartsWith("acc"))
                {
                    string[] accConfig = component.Split(new char[] { '|' });

                    for (int a = 1; a < accConfig.Length; a++)
                    {
                        int value = 0;
                        if (int.TryParse(accConfig[a], out value))
                        {
                            switch (a)
                            {
                                case 1:  accelerometer.centerX = value; break;
                                case 2:  accelerometer.minX    = value; break;
                                case 3:  accelerometer.maxX    = value; break;
                                case 4:  accelerometer.deadX   = value; break;
                                case 5:  accelerometer.centerY = value; break;
                                case 6:  accelerometer.minY    = value; break;
                                case 7:  accelerometer.maxY    = value; break;
                                case 8:  accelerometer.deadY   = value; break;
                                case 9:  accelerometer.centerZ = value; break;
                                case 10: accelerometer.minZ    = value; break;
                                case 11: accelerometer.maxZ    = value; break;
                                case 12: accelerometer.deadZ   = value; break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("irSqr"))
                {
                    SquareBoundry sBoundry = new SquareBoundry();
                    string[] sqrConfig = component.Split(new char[] { '|' });

                    for (int s = 1; s < sqrConfig.Length; s++)
                    {
                        int value = 0;
                        if (int.TryParse(sqrConfig[s], out value))
                        {
                            switch (s)
                            {
                                case 1: sBoundry.center_x = value; break;
                                case 2: sBoundry.center_y = value; break;
                                case 3: sBoundry.width = value; break;
                                case 4: sBoundry.height = value; break;
                            }
                        }
                    }

                    irSensor.boundingArea = sBoundry;
                }
                else if (component.StartsWith("irCir"))
                {
                    CircularBoundry sBoundry = new CircularBoundry();
                    string[] cirConfig = component.Split(new char[] { '|' });

                    for (int c = 1; c < cirConfig.Length; c++)
                    {
                        int value = 0;
                        if (int.TryParse(cirConfig[c], out value))
                        {
                            switch (c)
                            {
                                case 1: sBoundry.center_x = value; break;
                                case 2: sBoundry.center_y = value; break;
                                case 3: sBoundry.radius = value; break;
                            }
                        }
                    }

                    irSensor.boundingArea = sBoundry;
                }
            }
        }

        /// <summary>
        /// Creates a string containing the calibration settings for the Wiimote.
        /// String is in the following format 
        /// -wm:acc|centerX|minX|minY|deadX|centerY|[...]:ir
        /// </summary>
        /// <returns>String representing the Wiimote's calibration settings.</returns>
        public string GetCalibrationString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-wm");
                sb.Append(":acc");
                    sb.Append("|"); sb.Append(accelerometer.centerX);
                    sb.Append("|"); sb.Append(accelerometer.minX);
                    sb.Append("|"); sb.Append(accelerometer.maxX);
                    sb.Append("|"); sb.Append(accelerometer.deadX);

                    sb.Append("|"); sb.Append(accelerometer.centerY);
                    sb.Append("|"); sb.Append(accelerometer.minY);
                    sb.Append("|"); sb.Append(accelerometer.maxY);
                    sb.Append("|"); sb.Append(accelerometer.deadY);

                    sb.Append("|"); sb.Append(accelerometer.centerZ);
                    sb.Append("|"); sb.Append(accelerometer.minZ);
                    sb.Append("|"); sb.Append(accelerometer.maxZ);
                    sb.Append("|"); sb.Append(accelerometer.deadZ);
                
            if (irSensor.boundingArea != null)
            {
                if (irSensor.boundingArea is SquareBoundry)
                {
                    SquareBoundry sqr = (SquareBoundry)irSensor.boundingArea;
                    sb.Append(":irSqr");
                        sb.Append("|"); sb.Append(sqr.center_x);
                        sb.Append("|"); sb.Append(sqr.center_y);
                        sb.Append("|"); sb.Append(sqr.width);
                        sb.Append("|"); sb.Append(sqr.height);
                }
                else if (irSensor.boundingArea is CircularBoundry)
                {
                    CircularBoundry cir = (CircularBoundry)irSensor.boundingArea;
                    sb.Append(":irCir");
                        sb.Append("|"); sb.Append(cir.center_x);
                        sb.Append("|"); sb.Append(cir.center_y);
                        sb.Append("|"); sb.Append(cir.radius);
                }
            }

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get 
            { 
                if (accelerometer.maxX == 0 && accelerometer.maxY == 0 && accelerometer.maxZ == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            // Buttons
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.PLUS, buttons.Plus ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.MINUS, buttons.Minus ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.HOME, buttons.Home ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.A, buttons.A ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.B, buttons.B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.ONE, buttons.One ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.TWO, buttons.Two ? 1.0f : 0.0f);

            // D-Pad
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.UP, buttons.Up ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.DOWN, buttons.Down ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.LEFT, buttons.Left ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.RIGHT, buttons.Right ? 1.0f : 0.0f);

            // IR Sensor
            irSensor.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.IR_X, irSensor.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.IR_Y, irSensor.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.IR_UP, irSensor.Y > 0 ? irSensor.Y : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.IR_DOWN, irSensor.Y > 0 ? -irSensor.Y : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.IR_LEFT, irSensor.X < 0 ? -irSensor.X : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.IR_RIGHT, irSensor.X > 0 ? irSensor.X : 0);

            // Accelerometer
            accelerometer.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.ACC_X, accelerometer.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.ACC_Y, accelerometer.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.ACC_Z, accelerometer.Z);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.TILT_LEFT, accelerometer.X < 0 ? -accelerometer.X : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.TILT_RIGHT, accelerometer.X > 0 ? accelerometer.X : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.TILT_UP, accelerometer.Y > 0 ? accelerometer.Y : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.TILT_DOWN, accelerometer.Y < 0 ? -accelerometer.Y : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.FACE_UP, accelerometer.Z > 0 ? accelerometer.Z : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WIIMOTE.FACE_DOWN, accelerometer.Z < 0 ? -accelerometer.Z : 0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct Nunchuk : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public Wiimote wiimote;
        public Accelerometer accelerometer;
        public Joystick joystick;
        public bool C, Z;

        public Nunchuk(Wiimote wm)
        {
            this = new Nunchuk();
            wiimote = wm;
        }

        public Nunchuk(byte[] rawData)
        {
            wiimote = new Wiimote(rawData);
            accelerometer = new Accelerometer();
            joystick = new Joystick();

            C = Z = false;

#if DEBUG
            _debugViewActive = false;
#endif

            Update(rawData);
        }

        public void Update(byte[] data)
        {
            int offset = 0;
            switch((InputReport)data[0])
            {
                case InputReport.BtnsExt:
                case InputReport.BtnsExtB:
                    offset = 3;
                    break;
                case InputReport.BtnsAccExt:
                    offset = 6;
                    break;
                case InputReport.BtnsIRExt:
                    offset = 13;
                    break;
                case InputReport.BtnsAccIRExt:
                    offset = 16;
                    break;
                case InputReport.ExtOnly:
                    offset = 1;
                    break;
                case InputReport.Status:
                    offset = -1;
                    break;
                default:
                    return;
            }

            if (offset > 0)
            {
                // Buttons
                C = (data[offset + 5] & 0x02) == 0;
                Z = (data[offset + 5] & 0x01) == 0;

                // Joystick
                joystick.rawX = data[offset];
                joystick.rawY = data[offset + 1];

                // Accelerometer
                accelerometer.Parse(data, offset + 2);

                // Normalize
                joystick.Normalize();
                accelerometer.Normalize();
            }

            wiimote.Update(data);
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            wiimote.SetCalibration(preset);

            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    SetCalibration(Calibrations.Defaults.NunchukDefault);
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    SetCalibration(Calibrations.Moderate.NunchukModest);
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    SetCalibration(Calibrations.Extras.NunchukExtra);
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    SetCalibration(Calibrations.Minimum.NunchukMinimal);
                    break;

                case Calibrations.CalibrationPreset.None:
                    SetCalibration(Calibrations.None.NunchukRaw);
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.CalibrationEmpty)
            {
                // don't apply empty calibrations
                return;
            }

            if (from.GetType() == typeof(Nunchuk))
            {
                accelerometer.Calibrate(((Nunchuk)from).accelerometer);
                joystick.Calibrate(((Nunchuk)from).joystick);
            }
            else if (from.GetType() == typeof(Wiimote))
            {
                wiimote.SetCalibration(from);
            }
        }

        public void SetCalibration(string calibrationString)
        {
            if (calibrationString.Count(c => c == '0') > 5)
            {
                // don't set empty calibrations
                return;
            }

            string[] components = calibrationString.Split(new char[] { ':' });

            foreach (string component in components)
            {
                if (component.StartsWith("joy"))
                {
                    string[] joyConfig = component.Split(new char[] { '|' });

                    for (int j = 1; j < joyConfig.Length; j++)
                    {
                        int value = 0;
                        if (int.TryParse(joyConfig[j], out value))
                        {
                            switch (j)
                            {
                                case 1: joystick.centerX = value; break;
                                case 2: joystick.minX    = value; break;
                                case 3: joystick.maxX    = value; break;
                                case 4: joystick.deadX   = value; break;
                                case 5: joystick.centerY = value; break;
                                case 6: joystick.minY    = value; break;
                                case 7: joystick.maxY    = value; break;
                                case 8: joystick.deadY   = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("acc"))
                {
                    string[] accConfig = component.Split(new char[] { '|' });

                    for (int a = 1; a < accConfig.Length; a++)
                    {
                        int value = 0;
                        if (int.TryParse(accConfig[a], out value))
                        {
                            switch (a)
                            {
                                case 1:  accelerometer.centerX = value; break;
                                case 2:  accelerometer.minX    = value; break;
                                case 3:  accelerometer.maxX    = value; break;
                                case 4:  accelerometer.deadX   = value; break;
                                case 5:  accelerometer.centerY = value; break;
                                case 6:  accelerometer.minY    = value; break;
                                case 7:  accelerometer.maxY    = value; break;
                                case 8:  accelerometer.deadY   = value; break;
                                case 9:  accelerometer.centerZ = value; break;
                                case 10: accelerometer.minZ    = value; break;
                                case 11: accelerometer.maxZ    = value; break;
                                case 12: accelerometer.deadZ   = value; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }

        public string GetCalibrationString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-nun");
                sb.Append(":joy");
                    sb.Append("|"); sb.Append(joystick.centerX);
                    sb.Append("|"); sb.Append(joystick.minX);
                    sb.Append("|"); sb.Append(joystick.maxX);
                    sb.Append("|"); sb.Append(joystick.deadX);

                    sb.Append("|"); sb.Append(joystick.centerY);
                    sb.Append("|"); sb.Append(joystick.minY);
                    sb.Append("|"); sb.Append(joystick.maxY);
                    sb.Append("|"); sb.Append(joystick.deadY);
                sb.Append(":acc");
                    sb.Append("|"); sb.Append(accelerometer.centerX);
                    sb.Append("|"); sb.Append(accelerometer.minX);
                    sb.Append("|"); sb.Append(accelerometer.maxX);
                    sb.Append("|"); sb.Append(accelerometer.deadX);

                    sb.Append("|"); sb.Append(accelerometer.centerY);
                    sb.Append("|"); sb.Append(accelerometer.minY);
                    sb.Append("|"); sb.Append(accelerometer.maxY);
                    sb.Append("|"); sb.Append(accelerometer.deadY);

                    sb.Append("|"); sb.Append(accelerometer.centerZ);
                    sb.Append("|"); sb.Append(accelerometer.minZ);
                    sb.Append("|"); sb.Append(accelerometer.maxZ);
                    sb.Append("|"); sb.Append(accelerometer.deadZ);

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get
            {
                if (accelerometer.maxX == 0 && accelerometer.maxY == 0 && accelerometer.maxZ == 0)
                {
                    return true;
                }
                else if (joystick.maxX == 0 && joystick.maxY == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            // Wiimote
            foreach (var input in wiimote)
            {
                yield return input;
            }
            
            // Buttons
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.C, C ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.Z, Z ? 1.0f : 0.0f);

            // Joystick
            joystick.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.JOY_X, joystick.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.JOY_Y, joystick.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.UP, joystick.Y > 0 ? joystick.Y : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.DOWN, joystick.Y > 0 ? 0 : -joystick.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.LEFT, joystick.X > 0 ? 0 : -joystick.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.RIGHT, joystick.X > 0 ? joystick.X : 0);

            // Accelerometer
            accelerometer.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.ACC_X, accelerometer.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.ACC_Y, accelerometer.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.ACC_Z, accelerometer.Z);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.TILT_LEFT, accelerometer.X > 0 ? 0 : -accelerometer.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.TILT_RIGHT, accelerometer.X > 0 ? accelerometer.X : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.TILT_UP, accelerometer.Y > 0 ? accelerometer.Y : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.TILT_DOWN, accelerometer.Y > 0 ? 0 : -accelerometer.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.FACE_UP, accelerometer.Z > 0 ? accelerometer.Z : 0);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.NUNCHUK.FACE_DOWN, accelerometer.Z > 0 ? 0 : -accelerometer.Z);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct ClassicController : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public Wiimote wiimote;
        public Joystick LJoy, RJoy;
        public Trigger L, R;
        public bool A, B, X, Y;
        public bool Up, Down, Left, Right;
        public bool ZL, ZR, LFull, RFull;
        public bool Plus, Minus, Home;

        public ClassicController(Wiimote wm)
        {
            this = new ClassicController();
            wiimote = wm;
        }

        public bool Start
        {
            get { return Plus; }
            set { Plus = value; }
        }

        public bool Select
        {
            get { return Minus; }
            set { Minus = value; }
        }

        public void Update(byte[] data)
        {
            int offset = 0;
            switch ((InputReport)data[0])
            {
                case InputReport.BtnsExt:
                case InputReport.BtnsExtB:
                    offset = 3;
                    break;
                case InputReport.BtnsAccExt:
                    offset = 6;
                    break;
                case InputReport.BtnsIRExt:
                    offset = 13;
                    break;
                case InputReport.BtnsAccIRExt:
                    offset = 16;
                    break;
                case InputReport.ExtOnly:
                    offset = 1;
                    break;
                default:
                    return;
            }

            if (offset > 0)
            {
                // Buttons
                A     = (data[offset + 5] & 0x10) == 0;
                B     = (data[offset + 5] & 0x40) == 0;
                X     = (data[offset + 5] & 0x08) == 0;
                Y     = (data[offset + 5] & 0x20) == 0;
                LFull = (data[offset + 4] & 0x20) == 0;  // Until the Click
                RFull = (data[offset + 4] & 0x02) == 0;  // Until the Click
                ZL    = (data[offset + 5] & 0x80) == 0;
                ZR    = (data[offset + 5] & 0x04) == 0;
                Plus  = (data[offset + 4] & 0x04) == 0;
                Minus = (data[offset + 4] & 0x10) == 0;
                Home  = (data[offset + 4] & 0x08) == 0;

                // Dpad
                Up    = (data[offset + 5] & 0x01) == 0;
                Down  = (data[offset + 4] & 0x40) == 0;
                Left  = (data[offset + 5] & 0x02) == 0;
                Right = (data[offset + 4] & 0x80) == 0;

                // Joysticks
                LJoy.rawX = (byte)(data[offset] & 0x3F);
                LJoy.rawY = (byte)(data[offset + 1] & 0x03F);
                RJoy.rawX = (byte)(data[offset + 2] >> 7 | (data[offset + 1] & 0xC0) >> 5 | (data[offset] & 0xC0) >> 3);
                RJoy.rawY = (byte)(data[offset + 2] & 0x1F);

                // Triggers
                L.rawValue = (byte)(((data[offset + 2] & 0x60) >> 2) | (data[offset + 3] >> 5));
                R.rawValue = (byte)(data[offset + 3] & 0x1F);
                L.full = LFull;
                R.full = RFull;

                // Normalize
                LJoy.Normalize();
                RJoy.Normalize();
                L.Normalize();
                R.Normalize();
            }

            wiimote.Update(data);
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            wiimote.SetCalibration(preset);

            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    //LJoy.Calibrate(Calibrations.Defaults.ClassicControllerDefault.LJoy);
                    //RJoy.Calibrate(Calibrations.Defaults.ClassicControllerDefault.RJoy);
                    //L.Calibrate(Calibrations.Defaults.ClassicControllerDefault.L);
                    //R.Calibrate(Calibrations.Defaults.ClassicControllerDefault.R);
                    SetCalibration(Calibrations.Defaults.ClassicControllerDefault);
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    SetCalibration(Calibrations.Moderate.ClassicControllerModest);
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    SetCalibration(Calibrations.Extras.ClassicControllerExtra);
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    SetCalibration(Calibrations.Minimum.ClassicControllerMinimal);
                    break;

                case Calibrations.CalibrationPreset.None:
                    SetCalibration(Calibrations.None.ClassicControllerRaw);
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.CalibrationEmpty)
            {
                // don't apply empty calibrations
                return;
            }

            if (from.GetType() == typeof(ClassicController))
            {
                LJoy.Calibrate(((ClassicController)from).LJoy);
                RJoy.Calibrate(((ClassicController)from).RJoy);
                L.Calibrate(((ClassicController)from).L);
                R.Calibrate(((ClassicController)from).R);
            }
            else if (from.GetType() == typeof(Wiimote))
            {
                wiimote.SetCalibration(from);
            }
        }

        public void SetCalibration(string calibrationString)
        {
            if (calibrationString.Count(c => c == '0') > 5)
            {
                // don't set empty calibrations
                return;
            }

            string[] components = calibrationString.Split(new char[] { ':' });

            foreach (string component in components)
            {
                if (component.StartsWith("joyL"))
                {
                    string[] joyLConfig = component.Split(new char[] { '|' });

                    for (int jL = 1; jL < joyLConfig.Length; jL++)
                    {
                        int value = 0;
                        if (int.TryParse(joyLConfig[jL], out value))
                        {
                            switch (jL)
                            {
                                case 1: LJoy.centerX = value; break;
                                case 2: LJoy.minX = value; break;
                                case 3: LJoy.maxX = value; break;
                                case 4: LJoy.deadX = value; break;
                                case 5: LJoy.centerY = value; break;
                                case 6: LJoy.minY = value; break;
                                case 7: LJoy.maxY = value; break;
                                case 8: LJoy.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("joyR"))
                {
                    string[] joyRConfig = component.Split(new char[] { '|' });

                    for (int jR = 1; jR < joyRConfig.Length; jR++)
                    {
                        int value = 0;
                        if (int.TryParse(joyRConfig[jR], out value))
                        {
                            switch (jR)
                            {
                                case 1: RJoy.centerX = value; break;
                                case 2: RJoy.minX = value; break;
                                case 3: RJoy.maxX = value; break;
                                case 4: RJoy.deadX = value; break;
                                case 5: RJoy.centerY = value; break;
                                case 6: RJoy.minY = value; break;
                                case 7: RJoy.maxY = value; break;
                                case 8: RJoy.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("tl"))
                {
                    string[] triggerLConfig = component.Split(new char[] { '|' });

                    for (int tl = 1; tl < triggerLConfig.Length; tl++)
                    {
                        int value = 0;
                        if (int.TryParse(triggerLConfig[tl], out value))
                        {
                            switch (tl)
                            {
                                case 1: L.min = value; break;
                                case 2: L.max = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("tr"))
                {
                    string[] triggerRConfig = component.Split(new char[] { '|' });

                    for (int tr = 1; tr < triggerRConfig.Length; tr++)
                    {
                        int value = 0;
                        if (int.TryParse(triggerRConfig[tr], out value))
                        {
                            switch (tr)
                            {
                                case 1: R.min = value; break;
                                case 2: R.max = value; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }

        public string GetCalibrationString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-cla");
            sb.Append(":joyL");
                sb.Append("|"); sb.Append(LJoy.centerX);
                sb.Append("|"); sb.Append(LJoy.minX);
                sb.Append("|"); sb.Append(LJoy.maxX);
                sb.Append("|"); sb.Append(LJoy.deadX);
                sb.Append("|"); sb.Append(LJoy.centerY);
                sb.Append("|"); sb.Append(LJoy.minY);
                sb.Append("|"); sb.Append(LJoy.maxY);
                sb.Append("|"); sb.Append(LJoy.deadY);
            sb.Append(":joyR");
                sb.Append("|"); sb.Append(RJoy.centerX);
                sb.Append("|"); sb.Append(RJoy.minX);
                sb.Append("|"); sb.Append(RJoy.maxX);
                sb.Append("|"); sb.Append(RJoy.deadX);
                sb.Append("|"); sb.Append(RJoy.centerY);
                sb.Append("|"); sb.Append(RJoy.minY);
                sb.Append("|"); sb.Append(RJoy.maxY);
                sb.Append("|"); sb.Append(RJoy.deadY);
            sb.Append(":tl");
                sb.Append("|"); sb.Append(L.min);
                sb.Append("|"); sb.Append(L.max);
            sb.Append(":tr");
                sb.Append("|"); sb.Append(R.min);
                sb.Append("|"); sb.Append(R.max);

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get
            {
                if (LJoy.maxX == 0 && LJoy.maxY == 0 && RJoy.maxX == 0 && RJoy.maxY == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            // Wiimote
            foreach (var input in wiimote)
            {
                yield return input;
            }

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.A, A ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.X, X ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.Y, Y ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.L, L.value > 0 ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.R, R.value > 0 ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.ZL, ZL ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.ZR, ZR ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.UP, Up ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.DOWN, Down ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LEFT, Left ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RIGHT, Right ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.START, Start ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.SELECT, Select ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.HOME, Home ? 1.0f : 0.0f);

            L.Normalize();
            R.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LFULL, L.full ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RFULL, R.full ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LT, L.value);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RT, R.value);

            LJoy.Normalize();
            RJoy.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LX, LJoy.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LY, LJoy.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RX, RJoy.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RY, RJoy.X);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LUP, LJoy.Y > 0f ? LJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LDOWN, LJoy.Y > 0f ? 0.0f : -LJoy.Y); // These are inverted
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LLEFT, LJoy.X > 0f ? 0.0f : -LJoy.X); // because they
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.LRIGHT, LJoy.X > 0f ? LJoy.X : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RUP, RJoy.Y > 0f ? RJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RDOWN, RJoy.Y > 0f ? 0.0f : -RJoy.Y); // represents how far the
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RLEFT, RJoy.X > 0f ? 0.0f : -RJoy.X); // input is left or down
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER.RRIGHT, RJoy.X > 0f ? RJoy.X : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct ClassicControllerPro : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public Wiimote wiimote;
        public Joystick LJoy, RJoy;
        public bool A, B, X, Y;
        public bool Up, Down, Left, Right;
        public bool L, R, ZL, ZR;
        public bool Plus, Minus, Home;

        public ClassicControllerPro(Wiimote wm)
        {
            this = new ClassicControllerPro();
            wiimote = wm;
        }

        public bool Start
        {
            get { return Plus; }
            set { Plus = value; }
        }

        public bool Select
        {
            get { return Minus; }
            set { Minus = value; }
        }

        public void Update(byte[] data)
        {
            int offset = 0;
            switch ((InputReport)data[0])
            {
                case InputReport.BtnsExt:
                case InputReport.BtnsExtB:
                    offset = 3;
                    break;
                case InputReport.BtnsAccExt:
                    offset = 6;
                    break;
                case InputReport.BtnsIRExt:
                    offset = 13;
                    break;
                case InputReport.BtnsAccIRExt:
                    offset = 16;
                    break;
                case InputReport.ExtOnly:
                    offset = 1;
                    break;
                default:
                    return;
            }

            if (offset > 0)
            {
                // Buttons
                A     = (data[offset + 5] & 0x10) == 0;
                B     = (data[offset + 5] & 0x40) == 0;
                X     = (data[offset + 5] & 0x08) == 0;
                Y     = (data[offset + 5] & 0x20) == 0;
                L     = (data[offset + 4] & 0x20) == 0;
                R     = (data[offset + 4] & 0x02) == 0;
                ZL    = (data[offset + 5] & 0x80) == 0;
                ZR    = (data[offset + 5] & 0x04) == 0;
                Plus  = (data[offset + 4] & 0x04) == 0;
                Minus = (data[offset + 4] & 0x10) == 0;
                Home  = (data[offset + 4] & 0x08) == 0;

                // Dpad
                Up    = (data[offset + 5] & 0x01) == 0;
                Down  = (data[offset + 4] & 0x40) == 0;
                Left  = (data[offset + 5] & 0x02) == 0;
                Right = (data[offset + 4] & 0x80) == 0;

                // Joysticks
                LJoy.rawX = (byte)(data[offset] & 0x3F);
                LJoy.rawY = (byte)(data[offset + 1] & 0x03F);
                RJoy.rawX = (byte)(data[offset + 2] >> 7 | (data[offset + 1] & 0xC0) >> 5 | (data[offset] & 0xC0) >> 3);
                RJoy.rawY = (byte)(data[offset + 2] & 0x1F);

                // Normalize
                LJoy.Normalize();
                RJoy.Normalize();
            }

            wiimote.Update(data);
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            wiimote.SetCalibration(preset);

            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    //LJoy.Calibrate(Calibrations.Defaults.ClassicControllerProDefault.LJoy);
                    //RJoy.Calibrate(Calibrations.Defaults.ClassicControllerProDefault.RJoy);
                    SetCalibration(Calibrations.Defaults.ClassicControllerProDefault);
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    SetCalibration(Calibrations.Moderate.ClassicControllerProModest);
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    SetCalibration(Calibrations.Extras.ClassicControllerProExtra);
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    SetCalibration(Calibrations.Minimum.ClassicControllerProMinimal);
                    break;

                case Calibrations.CalibrationPreset.None:
                    SetCalibration(Calibrations.None.ClassicControllerProRaw);
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.CalibrationEmpty)
            {
                // don't apply empty calibrations
                return;
            }

            if (from.GetType() == typeof(ClassicControllerPro))
            {
                LJoy.Calibrate(((ClassicControllerPro)from).LJoy);
                RJoy.Calibrate(((ClassicControllerPro)from).RJoy);
            }
            else if (from.GetType() == typeof(Wiimote))
            {
                wiimote.SetCalibration(from);
            }
        }

        public void SetCalibration(string calibrationString)
        {
            if (calibrationString.Count(c => c == '0') > 5)
            {
                // don't set empty calibrations
                return;
            }

            string[] components = calibrationString.Split(new char[] { ':' });

            foreach (string component in components)
            {
                if (component.StartsWith("joyL"))
                {
                    string[] joyLConfig = component.Split(new char[] { '|' });

                    for (int jL = 1; jL < joyLConfig.Length; jL++)
                    {
                        int value = 0;
                        if (int.TryParse(joyLConfig[jL], out value))
                        {
                            switch (jL)
                            {
                                case 1: LJoy.centerX = value; break;
                                case 2: LJoy.minX = value; break;
                                case 3: LJoy.maxX = value; break;
                                case 4: LJoy.deadX = value; break;
                                case 5: LJoy.centerY = value; break;
                                case 6: LJoy.minY = value; break;
                                case 7: LJoy.maxY = value; break;
                                case 8: LJoy.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("joyR"))
                {
                    string[] joyRConfig = component.Split(new char[] { '|' });

                    for (int jR = 1; jR < joyRConfig.Length; jR++)
                    {
                        int value = 0;
                        if (int.TryParse(joyRConfig[jR], out value))
                        {
                            switch (jR)
                            {
                                case 1: RJoy.centerX = value; break;
                                case 2: RJoy.minX = value; break;
                                case 3: RJoy.maxX = value; break;
                                case 4: RJoy.deadX = value; break;
                                case 5: RJoy.centerY = value; break;
                                case 6: RJoy.minY = value; break;
                                case 7: RJoy.maxY = value; break;
                                case 8: RJoy.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }

        public string GetCalibrationString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-ccp");
                sb.Append(":joyL");
                    sb.Append("|"); sb.Append(LJoy.centerX);
                    sb.Append("|"); sb.Append(LJoy.minX);
                    sb.Append("|"); sb.Append(LJoy.maxX);
                    sb.Append("|"); sb.Append(LJoy.deadX);
                    sb.Append("|"); sb.Append(LJoy.centerY);
                    sb.Append("|"); sb.Append(LJoy.minY);
                    sb.Append("|"); sb.Append(LJoy.maxY);
                    sb.Append("|"); sb.Append(LJoy.deadY);
                sb.Append(":joyR");
                    sb.Append("|"); sb.Append(RJoy.centerX);
                    sb.Append("|"); sb.Append(RJoy.minX);
                    sb.Append("|"); sb.Append(RJoy.maxX);
                    sb.Append("|"); sb.Append(RJoy.deadX);
                    sb.Append("|"); sb.Append(RJoy.centerY);
                    sb.Append("|"); sb.Append(RJoy.minY);
                    sb.Append("|"); sb.Append(RJoy.maxY);
                    sb.Append("|"); sb.Append(RJoy.deadY);

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get
            {
                if (LJoy.maxX == 0 && LJoy.maxY == 0 && RJoy.maxX == 0 && RJoy.maxY == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            foreach (var input in wiimote)
            {
                yield return input;
            }

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.A, A ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.X, X ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.Y, Y ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.L, L ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.R, R ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.ZL, ZL ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.ZR, ZR ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.UP, Up ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.DOWN, Down ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LEFT, Left ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RIGHT, Right ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.START, Start ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.SELECT, Select ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.HOME, Home ? 1.0f : 0.0f);

            LJoy.Normalize();
            RJoy.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LX, LJoy.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LY, LJoy.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RX, RJoy.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RY, RJoy.X);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LUP, LJoy.Y > 0f ? LJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LDOWN, LJoy.Y > 0f ? 0.0f : -LJoy.Y); // These are inverted
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LLEFT, LJoy.X > 0f ? 0.0f : -LJoy.X); // because they
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.LRIGHT, LJoy.X > 0f ? LJoy.X : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RUP, RJoy.Y > 0f ? RJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RDOWN, RJoy.Y > 0f ? 0.0f : -RJoy.Y); // represents how far the
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RLEFT, RJoy.X > 0f ? 0.0f : -RJoy.X); // input is left or down
            yield return new KeyValuePair<string, float>(INPUT_NAMES.CLASSIC_CONTROLLER_PRO.RRIGHT, RJoy.X > 0f ? RJoy.X : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct ProController : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public Joystick LJoy, RJoy;
        public bool A, B, X, Y;
        public bool Up, Down, Left, Right;
        public bool L, R, ZL, ZR;
        public bool Plus, Minus, Home;
        public bool LStick, RStick;
        public bool charging, usbConnected;

        public bool Start
        {
            get { return Plus; }
            set { Plus = value; }
        }

        public bool Select
        {
            get { return Minus; }
            set { Minus = value; }
        }

        public void Update(byte[] data)
        {
            int offset = 0;

            switch ((InputReport)data[0])
            {
                case InputReport.ExtOnly:
                    offset = 1;
                    break;
                case InputReport.BtnsExt:
                case InputReport.BtnsExtB:
                    offset = 3;
                    break;
                case InputReport.BtnsAccExt:
                    offset = 6;
                    break;
                case InputReport.BtnsIRExt:
                    offset = 13;
                    break;
                case InputReport.BtnsAccIRExt:
                    offset = 16;
                    break;
                case InputReport.Status:
                    Plus  = (data[1] & 0x04) == 0;
                    Home  = (data[1] & 0x08) == 0;
                    Minus = (data[1] & 0x10) == 0;
                    Down  = (data[1] & 0x40) == 0;
                    Right = (data[1] & 0x80) == 0;
                    Up    = (data[2] & 0x01) == 0;
                    Left  = (data[2] & 0x02) == 0;
                    A     = (data[2] & 0x10) == 0;
                    B     = (data[2] & 0x40) == 0;
                    return;
                default:
                    return;
            }

            // Buttons
            A      = (data[offset +  9] & 0x10) == 0;
            B      = (data[offset +  9] & 0x40) == 0;
            X      = (data[offset +  9] & 0x08) == 0;
            Y      = (data[offset +  9] & 0x20) == 0;
            L      = (data[offset +  8] & 0x20) == 0;
            R      = (data[offset +  8] & 0x02) == 0;
            ZL     = (data[offset +  9] & 0x80) == 0;
            ZR     = (data[offset +  9] & 0x04) == 0;
            Plus   = (data[offset +  8] & 0x04) == 0;
            Minus  = (data[offset +  8] & 0x10) == 0;
            Home   = (data[offset +  8] & 0x08) == 0;
            LStick = (data[offset + 10] & 0x02) == 0;
            RStick = (data[offset + 10] & 0x01) == 0;

            // DPad
            Up    = (data[offset + 9] & 0x01) == 0;
            Down  = (data[offset + 8] & 0x40) == 0;
            Left  = (data[offset + 9] & 0x02) == 0;
            Right = (data[offset + 8] & 0x80) == 0;

            // Joysticks
            LJoy.rawX = BitConverter.ToInt16(data, offset);
            LJoy.rawY = BitConverter.ToInt16(data, offset + 4);
            RJoy.rawX = BitConverter.ToInt16(data, offset + 2);
            RJoy.rawY = BitConverter.ToInt16(data, offset + 6);

            // Other
            charging     = (data[offset + 10] & 0x04) == 0;
            usbConnected = (data[offset + 10] & 0x08) == 0;

            // Normalize
            LJoy.Normalize();
            RJoy.Normalize();
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    //LJoy.Calibrate(Calibrations.Defaults.ProControllerDefault.LJoy);
                    //RJoy.Calibrate(Calibrations.Defaults.ProControllerDefault.RJoy);
                    SetCalibration(Calibrations.Defaults.ProControllerDefault);
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    SetCalibration(Calibrations.Moderate.ProControllerModest);
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    SetCalibration(Calibrations.Extras.ProControllerExtra);
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    SetCalibration(Calibrations.Minimum.ProControllerMinimal);
                    break;

                case Calibrations.CalibrationPreset.None:
                    SetCalibration(Calibrations.None.ProControllerRaw);
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.CalibrationEmpty)
            {
                // don't apply empty calibrations
                return;
            }

            if (from.GetType() == typeof(ProController))
            {
                LJoy.Calibrate(((ProController)from).LJoy);
                RJoy.Calibrate(((ProController)from).RJoy);
            }
        }

        public void SetCalibration(string calibrationString)
        {
            if (calibrationString.Count(c => c == '0') > 5)
            {
                // don't set empty calibrations
                return;
            }

            string[] components = calibrationString.Split(new char[] { ':' });

            foreach (string component in components)
            {
                if (component.StartsWith("joyL"))
                {
                    string[] joyLConfig = component.Split(new char[] { '|' });

                    for (int jL = 1; jL < joyLConfig.Length; jL++)
                    {
                        int value = 0;
                        if (int.TryParse(joyLConfig[jL], out value))
                        {
                            switch (jL)
                            {
                                case 1: LJoy.centerX = value; break;
                                case 2: LJoy.minX    = value; break;
                                case 3: LJoy.maxX    = value; break;
                                case 4: LJoy.deadX   = value; break;
                                case 5: LJoy.centerY = value; break;
                                case 6: LJoy.minY    = value; break;
                                case 7: LJoy.maxY    = value; break;
                                case 8: LJoy.deadY   = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("joyR"))
                {
                    string[] joyRConfig = component.Split(new char[] { '|' });

                    for (int jR = 1; jR < joyRConfig.Length; jR++)
                    {
                        int value = 0;
                        if (int.TryParse(joyRConfig[jR], out value))
                        {
                            switch (jR)
                            {
                                case 1: RJoy.centerX = value; break;
                                case 2: RJoy.minX    = value; break;
                                case 3: RJoy.maxX    = value; break;
                                case 4: RJoy.deadX   = value; break;
                                case 5: RJoy.centerY = value; break;
                                case 6: RJoy.minY    = value; break;
                                case 7: RJoy.maxY    = value; break;
                                case 8: RJoy.deadY   = value; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }

        public string GetCalibrationString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-pro");
            sb.Append(":joyL");
            sb.Append("|"); sb.Append(LJoy.centerX);
            sb.Append("|"); sb.Append(LJoy.minX);
            sb.Append("|"); sb.Append(LJoy.maxX);
            sb.Append("|"); sb.Append(LJoy.deadX);
            sb.Append("|"); sb.Append(LJoy.centerY);
            sb.Append("|"); sb.Append(LJoy.minY);
            sb.Append("|"); sb.Append(LJoy.maxY);
            sb.Append("|"); sb.Append(LJoy.deadY);
            sb.Append(":joyR");
            sb.Append("|"); sb.Append(RJoy.centerX);
            sb.Append("|"); sb.Append(RJoy.minX);
            sb.Append("|"); sb.Append(RJoy.maxX);
            sb.Append("|"); sb.Append(RJoy.deadX);
            sb.Append("|"); sb.Append(RJoy.centerY);
            sb.Append("|"); sb.Append(RJoy.minY);
            sb.Append("|"); sb.Append(RJoy.maxY);
            sb.Append("|"); sb.Append(RJoy.deadY);

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get
            {
                if (LJoy.maxX == 0 && LJoy.maxY == 0 && RJoy.maxX == 0 && RJoy.maxY == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.A, A ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.X, X ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.Y, Y ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.L,  L  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.R,  R  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.ZL, ZL ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.ZR, ZR ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.UP,    Up    ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.DOWN,  Down  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LEFT,  Left  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RIGHT, Right ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.START,  Start  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.SELECT, Select ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.HOME,   Home   ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LS, LStick ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RS, RStick ? 1.0f : 0.0f);

            LJoy.Normalize();
            RJoy.Normalize();
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LX, LJoy.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LY, LJoy.Y);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RX, RJoy.X);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RY, RJoy.X);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LUP,    LJoy.Y > 0f ? LJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LDOWN,  LJoy.Y > 0f ? 0.0f : -LJoy.Y); // These are inverted
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LLEFT,  LJoy.X > 0f ? 0.0f : -LJoy.X); // because they
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.LRIGHT, LJoy.X > 0f ? LJoy.X : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RUP,    RJoy.Y > 0f ? RJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RDOWN,  RJoy.Y > 0f ? 0.0f : -RJoy.Y); // represents how far the
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RLEFT,  RJoy.X > 0f ? 0.0f : -RJoy.X); // input is left or down
            yield return new KeyValuePair<string, float>(INPUT_NAMES.PRO_CONTROLLER.RRIGHT, RJoy.X > 0f ? RJoy.X : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct BalanceBoard : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public void Update(byte[] data)
        {
            throw new NotImplementedException();
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        // TODO: Calibration - Balance Board Calibration
        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    break;

                case Calibrations.CalibrationPreset.None:
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.GetType() == typeof(BalanceBoard))
            {
                
            }
        }

        public void SetCalibration(string calibrationString)
        {

        }

        public string GetCalibrationString()
        {
            return "";
        }


        public bool CalibrationEmpty
        {
            get { return false; }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            yield return new KeyValuePair<string, float>("bb", 0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct WiimotePlus : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        Wiimote wiimote;
        //gyro

        public void Update(byte[] data)
        {
            throw new NotImplementedException();
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        // TODO: Calibration - Balance Board Calibration
        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    break;

                case Calibrations.CalibrationPreset.None:
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.GetType() == typeof(WiimotePlus))
            {

            }
        }

        public void SetCalibration(string calibrationString)
        {

        }

        public string GetCalibrationString()
        {
            return "";
        }

        public bool CalibrationEmpty
        {
            get { return false; }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            foreach (var input in wiimote)
            {
                yield return input;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

       
    }

    public struct WiiGuitar : INintrollerState
    {
#if DEBUG
        private bool _debugViewActive;
        public bool DebugViewActive
        {
            get
            {
                return _debugViewActive;
            }
            set
            {
                _debugViewActive = value;
            }
        }
#endif

        public Wiimote wiimote;
        public Joystick Joy;
        public bool G, R, Y, B, O;
        public bool Up, Down, Left, Right;
        public bool Plus, Minus;

        public bool IsGH3;
        public bool IsGH3SetYet { get; private set; }

        private byte oldTouchStripValue;

        public float WhammyHigh;
        public float WhammyLow;

        public float TiltHigh;
        public float TiltLow;

#if DEBUG
        public byte[] DebugLastData;
#endif

#if DEBUG
        private bool DebugButton_Dump;
#endif

        private byte CALIB_Whammy_Min;
        private byte CALIB_Whammy_Max;

        public WiiGuitar(Wiimote wm)
        {
            this = new WiiGuitar();
            wiimote = wm;

            CALIB_Whammy_Min = 0xFF;
            CALIB_Whammy_Max = 0;

            CALIB_Enable_TouchStrip = false;

            oldTouchStripValue = WGT_TOUCH_STRIP_None;

            IsGH3SetYet = false;
            IsGH3 = false;

            CALIB_Tilt_Neutral = 0;
            CALIB_Tilt_TiltedREEEE = (float)(Math.PI / 2);

#if DEBUG
            DebugLastData = new byte[] { 0 };
#endif

            Joy.Calibrate(Calibrations.Defaults.WiiGuitarDefault.Joy);
        }

        public bool Start
        {
            get { return Plus; }
            set { Plus = value; }
        }

        public bool Select
        {
            get { return Minus; }
            set { Minus = value; }
        }

        public bool CALIB_Enable_TouchStrip;

        public float CALIB_Tilt_Neutral;
        public float CALIB_Tilt_TiltedREEEE;
        public float CALIB_Tilt_StartingZ;

        private const byte WGT_TOUCH_STRIP_None = 0x0F;
        private const byte WGT_TOUCH_STRIP_Green = 0x04;
        private const byte WGT_TOUCH_STRIP_Green2 = 0x05;
        private const byte WGT_TOUCH_STRIP_GreenToRed = 0x06;
        private const byte WGT_TOUCH_STRIP_GreenToRed2 = 0x07;
        private const byte WGT_TOUCH_STRIP_GreenToRed3 = 0x08;
        private const byte WGT_TOUCH_STRIP_GreenToRed4 = 0x09;
        private const byte WGT_TOUCH_STRIP_Red = 0x0A;
        private const byte WGT_TOUCH_STRIP_Red2 = 0x0B;
        private const byte WGT_TOUCH_STRIP_Red3 = 0x0C;
        private const byte WGT_TOUCH_STRIP_RedToYellow = 0x0D;
        private const byte WGT_TOUCH_STRIP_RedToYellow2 = 0x0E;
        //private const byte WGT_TOUCH_STRIP_RedToYellow3 = 0x0F; //conflicts with None
        private const byte WGT_TOUCH_STRIP_RedToYellow4 = 0x10;
        private const byte WGT_TOUCH_STRIP_RedToYellow5 = 0x11;
        private const byte WGT_TOUCH_STRIP_Yellow = 0x12;
        private const byte WGT_TOUCH_STRIP_Yellow2 = 0x13;
        private const byte WGT_TOUCH_STRIP_YellowToBlue = 0x14;
        private const byte WGT_TOUCH_STRIP_YellowToBlue2 = 0x15;
        private const byte WGT_TOUCH_STRIP_YellowToBlue3 = 0x16;
        private const byte WGT_TOUCH_STRIP_Blue = 0x17;
        private const byte WGT_TOUCH_STRIP_Blue2 = 0x18;
        private const byte WGT_TOUCH_STRIP_Blue3 = 0x19;
        private const byte WGT_TOUCH_STRIP_BlueToOrange = 0x1A;
        private const byte WGT_TOUCH_STRIP_BlueToOrange2 = 0x1B;
        private const byte WGT_TOUCH_STRIP_BlueToOrange3 = 0x1C;
        private const byte WGT_TOUCH_STRIP_BlueToOrange4 = 0x1D;
        private const byte WGT_TOUCH_STRIP_BlueToOrange5 = 0x1E;
        private const byte WGT_TOUCH_STRIP_Orange = 0x1F;

        private const float WGT_JOY_DIGITAL_THRESH = 0.5f;

        //private const byte WGT_WHAMMY_MIN = 0x10;
        //private const byte WGT_WHAMMY_MAX = 0x1B;

        private static float _MapRange(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public void Update(byte[] data)
        {
#if DEBUG
            //DebugLastData = new byte[data.Length];

            //for (int i = 0; i < data.Length; i++)
            //{
            //    DebugLastData[i] = data[i];
            //}

            DebugLastData = data;
#endif

            int offset = 0;
            switch ((InputReport)data[0])
            {
                case InputReport.BtnsExt:
                case InputReport.BtnsExtB:
                    offset = 3;
                    break;
                case InputReport.BtnsAccExt:
                    offset = 6;
                    break;
                case InputReport.BtnsIRExt:
                    offset = 13;
                    break;
                case InputReport.BtnsAccIRExt:
                    offset = 16;
                    break;
                case InputReport.ExtOnly:
                    offset = 1;
                    break;
                default:
                    return;
            }

            if (offset > 0)
            {
                if (!IsGH3SetYet)
                {
                    IsGH3 = (data[offset] & 0x80) == 0x80; //0b10000000
                    IsGH3SetYet = true;
                }

                //Console.Write("WII GUITAR DATA: ");
                //for (int i = offset; i < data.Length; i++)
                //    Console.Write(data[i].ToString("X2") + " ");

                //Console.WriteLine();

                // Buttons
                G = (data[offset + 5] & 0x10) == 0;
                R = (data[offset + 5] & 0x40) == 0;
                Y = (data[offset + 5] & 0x08) == 0;
                B = (data[offset + 5] & 0x20) == 0;
                //L = (data[offset + 4] & 0x20) == 0;
                //R = (data[offset + 4] & 0x02) == 0;
                O = (data[offset + 5] & 0x80) == 0;
                //ZR = (data[offset + 5] & 0x04) == 0;
                Plus = (data[offset + 4] & 0x04) == 0;
                Minus = (data[offset + 4] & 0x10) == 0;
                //Home = (data[offset + 4] & 0x08) == 0;

                // Dpad
                Up = (data[offset + 5] & 0x01) == 0;
                Down = (data[offset + 4] & 0x40) == 0;
                //Left = (data[offset + 5] & 0x02) == 0;
                //Right = (data[offset + 4] & 0x80) == 0;

                //Up = false;
                //Down = false;
                Left = false;
                Right = false;

                if (data[offset] != 0 || data[offset + 1] != 0)
                {
                    // Joysticks
                    Joy.rawX = (byte)(data[offset] & 0x3F);
                    Joy.rawY = (byte)(data[offset + 1] & 0x03F);

                    if (Joy.rawX > Joy.maxX)
                    {
                        Joy.maxX = Joy.rawX;
                    }
                    else if (Joy.rawX < Joy.minX)
                    {
                        Joy.minX = Joy.rawX;
                    }

                    if (Joy.rawY > Joy.maxY)
                    {
                        Joy.maxY = Joy.rawY;
                    }
                    else if (Joy.rawY < Joy.minY)
                    {
                        Joy.minY = Joy.rawY;
                    }

                    Joy.Normalize();

                    bool isJoyPressed = (((Joy.X * Joy.X) + (Joy.Y * Joy.Y)) >= (WGT_JOY_DIGITAL_THRESH * WGT_JOY_DIGITAL_THRESH));
                    double joyDirection = (int)((Math.Atan2(Joy.Y, Joy.X) + (Math.PI / 2)) / (Math.PI / 8));
                    int joyDirStep = (int)(Math.Abs(joyDirection));

                    if (isJoyPressed)
                    {
                        if (joyDirection < 0)
                        {
                            switch (joyDirStep)
                            {
                                case 0: //N
                                    Down = true;
                                    break;
                                case 1: //NE
                                case 2: //NE
                                    Down = true;
                                    Left = true;
                                    break;
                                case 3: //E
                                case 4: //E
                                    Left = true;
                                    break;
                                case 5: //SE
                                case 6: //SE
                                    Left = true;
                                    Up = true;
                                    break;
                                case 7: //S
                                case 8: //S
                                    Up = true;
                                    break;
                                case 9: //SW
                                case 10: //SW
                                    Up = true;
                                    Right = true;
                                    break;
                                case 11: //W
                                case 12: //W
                                    Right = true;
                                    break;

                            }
                        }
                        else
                        {
                            switch (joyDirStep)
                            {
                                case 0: //N
                                    Down = true;
                                    break;
                                case 1: //NW
                                case 2: //NW
                                    Down = true;
                                    Right = true;
                                    break;
                                case 3: //W
                                case 4: //W
                                    Right = true;
                                    break;
                                case 5: //SW
                                case 6: //SW
                                    Right = true;
                                    Up = true;
                                    break;
                                case 7: //S
                                case 8: //S
                                    Up = true;
                                    break;
                                case 9: //SE
                                case 10: //SE
                                    Up = true;
                                    Left = true;
                                    break;
                                case 11: //E
                                case 12: //E
                                    Left = true;
                                    break;
                            }
                        }


                    }
                }

                

                if ((!IsGH3) && CALIB_Enable_TouchStrip)
                {
                    if (G || R || Y || B || O)
                    {
                        if (data[offset + 2] != WGT_TOUCH_STRIP_None && oldTouchStripValue == WGT_TOUCH_STRIP_None)
                        {
                            Down = true;
                        }
                    }
                    else
                    {
                        switch (data[offset + 2] & 0x1F)
                        {
                            case WGT_TOUCH_STRIP_Green:
                            case WGT_TOUCH_STRIP_Green2:
                                G = true;
                                break;
                            case WGT_TOUCH_STRIP_GreenToRed:
                            case WGT_TOUCH_STRIP_GreenToRed2:
                            case WGT_TOUCH_STRIP_GreenToRed3:
                            case WGT_TOUCH_STRIP_GreenToRed4:
                                G = true;
                                R = true;
                                break;
                            case WGT_TOUCH_STRIP_Red:
                            case WGT_TOUCH_STRIP_Red2:
                            case WGT_TOUCH_STRIP_Red3:
                                R = true;
                                break;
                            case WGT_TOUCH_STRIP_RedToYellow:
                            case WGT_TOUCH_STRIP_RedToYellow2:
                            //case WGT_TOUCH_STRIP_RedToYellow3: //conflicts with None
                            case WGT_TOUCH_STRIP_RedToYellow4:
                            case WGT_TOUCH_STRIP_RedToYellow5:
                                R = true;
                                Y = true;
                                break;
                            case WGT_TOUCH_STRIP_Yellow:
                            case WGT_TOUCH_STRIP_Yellow2:
                                Y = true;
                                break;
                            case WGT_TOUCH_STRIP_YellowToBlue:
                            case WGT_TOUCH_STRIP_YellowToBlue2:
                            case WGT_TOUCH_STRIP_YellowToBlue3:
                                Y = true;
                                B = true;
                                break;
                            case WGT_TOUCH_STRIP_Blue:
                            case WGT_TOUCH_STRIP_Blue2:
                            case WGT_TOUCH_STRIP_Blue3:
                                B = true;
                                break;
                            case WGT_TOUCH_STRIP_BlueToOrange:
                            case WGT_TOUCH_STRIP_BlueToOrange2:
                            case WGT_TOUCH_STRIP_BlueToOrange3:
                            case WGT_TOUCH_STRIP_BlueToOrange4:
                            case WGT_TOUCH_STRIP_BlueToOrange5:
                                B = true;
                                O = true;
                                break;
                            case WGT_TOUCH_STRIP_Orange:
                                O = true;
                                break;
                        }
                    }

                    oldTouchStripValue = data[offset + 2];
                }

                //// Normalize
                //Joy.Normalize();

                //if (Joy.Y > 0.7f)
                //    Up = true;
                //else if (Joy.Y < -0.7f)
                //    Down = true;

                //Left = Joy.X < -0.7f;
                //Right = Joy.X > 0.7f;

                byte currentWhammyValue = (byte)(data[offset + 3] & 0x1F);

                if (currentWhammyValue < CALIB_Whammy_Min)
                    CALIB_Whammy_Min = currentWhammyValue;

                if (currentWhammyValue > CALIB_Whammy_Max)
                    CALIB_Whammy_Max = currentWhammyValue;

                float whammy = (2.0f * (1.0f * (currentWhammyValue - CALIB_Whammy_Min) / (CALIB_Whammy_Max - CALIB_Whammy_Min)) - 1);

                WhammyHigh = Math.Max(whammy, 0);
                WhammyLow = Math.Min(whammy, 0);


                //Console.Write("WII GUITAR:");
                //Console.Write($"Frets:{(A ? "_" : "-")}{(B ? "_" : "-")}{(X ? "_" : "-")}{(Y ? "_" : "-")}{(ZL ? "_" : "-")}");

                //Console.Write($"    Joy1=[{LJoy.X},{LJoy.Y}]    ");
                //Console.Write($"Joy2=[{RJoy.X},{RJoy.Y}]    ");

                //Console.WriteLine();
                

            }

#if LOW_BANDWIDTH

#else
            wiimote.Update(data);

            if (wiimote.buttons.Up)
            {
                Left = true;
            }
            else if (wiimote.buttons.Down)
            {
                Right = true;
            }

            if (wiimote.buttons.Right)
            {
                Down = true;
            }
            else if (wiimote.buttons.Left)
            {
                Up = true;
            }

            if (wiimote.buttons.A)
            {
                Select = true;
            }

            if (wiimote.buttons.One)
            {
                wiimote.accelerometer.centerX = wiimote.accelerometer.rawX;
                wiimote.accelerometer.centerY = wiimote.accelerometer.rawY;
                wiimote.accelerometer.centerZ = wiimote.accelerometer.rawZ - 32;

                wiimote.accelerometer.minX = wiimote.accelerometer.centerX - 32;
                wiimote.accelerometer.maxX = wiimote.accelerometer.centerX + 32;

                wiimote.accelerometer.minY = wiimote.accelerometer.centerY - 32;
                wiimote.accelerometer.maxY = wiimote.accelerometer.centerY + 32;

                wiimote.accelerometer.minZ = wiimote.accelerometer.centerZ - 32;
                wiimote.accelerometer.maxZ = wiimote.accelerometer.centerZ + 32;

                wiimote.accelerometer.deadX = 0;
                wiimote.accelerometer.deadY = 0;
                wiimote.accelerometer.deadZ = 0;

                wiimote.accelerometer.Normalize();

                CALIB_Tilt_StartingZ = 0;

                CALIB_Tilt_Neutral = 0;// (float)Math.Atan2(Math.Abs(wiimote.accelerometer.Y), Math.Abs(wiimote.accelerometer.X)) * Math.Max(Math.Min(1 - Math.Abs(wiimote.accelerometer.Z - CALIB_Tilt_StartingZ), 1), 0);

                CALIB_Tilt_TiltedREEEE = (float)(CALIB_Tilt_Neutral + Math.PI / 2);
            }

            float tiltAngle = (float)Math.Atan2(Math.Abs(wiimote.accelerometer.Y), Math.Abs(wiimote.accelerometer.X)) * Math.Max(Math.Min(1 - Math.Abs(wiimote.accelerometer.Z - CALIB_Tilt_StartingZ), 1), 0);


            if (wiimote.buttons.Two)
            {
                CALIB_Tilt_TiltedREEEE = tiltAngle;
            }

            float tilt = _MapRange(tiltAngle, CALIB_Tilt_Neutral, CALIB_Tilt_TiltedREEEE, 0, 1);

            TiltHigh = Math.Min(Math.Max(tilt, 0), 1);
            TiltLow = Math.Max(Math.Min(tilt, 0), -1);

            if (!IsGH3)
            {
                if (wiimote.buttons.Plus && !CALIB_Enable_TouchStrip)
                    CALIB_Enable_TouchStrip = true;
                else if (wiimote.buttons.Minus && CALIB_Enable_TouchStrip)
                    CALIB_Enable_TouchStrip = false;
            }
#endif






#if DEBUG
            if (offset > 0)
            {
                if (wiimote.buttons.Home)
                {
                    if (!DebugButton_Dump)
                    {
                        DebugButton_Dump = true;

                        //var sb = new StringBuilder();

                        //sb.AppendLine("Wii Guitar data packet dump:");

                        //for (int i = 0; i < data.Length; i++)
                        //{
                        //    sb.Append(data[i].ToString("X2") + " ");
                        //}

                        //MessageBox.Show(sb.ToString(), "DEBUG: WII GUITAR DUMP", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        DebugViewActive = true;
                    }
                }
                else
                {
                    DebugButton_Dump = false;
                }

                
            }
#endif

        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            wiimote.SetCalibration(preset);

            //switch (preset)
            //{
            //    case Calibrations.CalibrationPreset.Default:
            //        //LJoy.Calibrate(Calibrations.Defaults.ClassicControllerProDefault.LJoy);
            //        //RJoy.Calibrate(Calibrations.Defaults.ClassicControllerProDefault.RJoy);
            //        SetCalibration(Calibrations.Defaults.ClassicControllerProDefault);
            //        break;

            //    case Calibrations.CalibrationPreset.Modest:
            //        SetCalibration(Calibrations.Moderate.ClassicControllerProModest);
            //        break;

            //    case Calibrations.CalibrationPreset.Extra:
            //        SetCalibration(Calibrations.Extras.ClassicControllerProExtra);
            //        break;

            //    case Calibrations.CalibrationPreset.Minimum:
            //        SetCalibration(Calibrations.Minimum.ClassicControllerProMinimal);
            //        break;

            //    case Calibrations.CalibrationPreset.None:
            //        SetCalibration(Calibrations.None.ClassicControllerProRaw);
            //        break;
            //}



            Joy.Calibrate(Calibrations.Defaults.WiiGuitarDefault.Joy);

            //SetCalibration(Calibrations.Defaults.ClassicControllerProDefault);
        }

        public void SetCalibration(INintrollerState from)
        {
            //if (from.CalibrationEmpty)
            //{
            //    // don't apply empty calibrations
            //    return;
            //}

            //if (from.GetType() == typeof(WiiGuitar))
            //{
            //    Joy.Calibrate(((WiiGuitar)from).Joy);
            //}
            //else if (from.GetType() == typeof(ClassicControllerPro))
            //{
            //    Joy.Calibrate(((ClassicControllerPro)from).LJoy);
            //}
            //else if (from.GetType() == typeof(Wiimote))
            //{
            //    wiimote.SetCalibration(from);
            //}
        }

        public void SetCalibration(string calibrationString)
        {
            if (calibrationString.Count(c => c == '0') > 5)
            {
                // don't set empty calibrations
                return;
            }

            string[] components = calibrationString.Split(new char[] { ':' });

            foreach (string component in components)
            {
                if (component.StartsWith("joy"))
                {
                    string[] joyLConfig = component.Split(new char[] { '|' });

                    for (int jL = 1; jL < joyLConfig.Length; jL++)
                    {
                        int value = 0;
                        if (int.TryParse(joyLConfig[jL], out value))
                        {
                            switch (jL)
                            {
                                case 1: Joy.centerX = value; break;
                                case 2: Joy.minX = value; break;
                                case 3: Joy.maxX = value; break;
                                case 4: Joy.deadX = value; break;
                                case 5: Joy.centerY = value; break;
                                case 6: Joy.minY = value; break;
                                case 7: Joy.maxY = value; break;
                                case 8: Joy.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
            }
        }

        public string GetCalibrationString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("-wgt");
            sb.Append(":joy");
            sb.Append("|"); sb.Append(Joy.centerX);
            sb.Append("|"); sb.Append(Joy.minX);
            sb.Append("|"); sb.Append(Joy.maxX);
            sb.Append("|"); sb.Append(Joy.deadX);
            sb.Append("|"); sb.Append(Joy.centerY);
            sb.Append("|"); sb.Append(Joy.minY);
            sb.Append("|"); sb.Append(Joy.maxY);
            sb.Append("|"); sb.Append(Joy.deadY);

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get
            {
                if (Joy.maxX == 0 && Joy.maxY == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            foreach (var input in wiimote)
            {
                yield return input;
            }

            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.G, G ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.R, R ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.Y, Y ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.O, O ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.UP, (Up ? 1.0f : 0.0f));
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.DOWN, (Down ? 1.0f : 0.0f));
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.LEFT, (Left ? 1.0f : 0.0f));
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.RIGHT, (Right ? 1.0f : 0.0f));

            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.START, Start ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.SELECT, Select ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.WHAMMYHIGH, WhammyHigh);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.WHAMMYLOW, WhammyLow);

            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.TILTHIGH, TiltHigh);
            yield return new KeyValuePair<string, float>(INPUT_NAMES.WII_GUITAR.TILTLOW, TiltLow);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
