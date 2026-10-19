using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NintrollerLib
{
    public struct Nunchuk : INintrollerState
    {
        public static class InputNames
        {
            public const string C            = "nC";
            public const string Z            = "nZ";

            public const string JOY_X        = "nJoyX";
            public const string JOY_Y        = "nJoyY";

            public const string UP           = "nUP";
            public const string DOWN         = "nDOWN";
            public const string LEFT         = "nLEFT";
            public const string RIGHT        = "nRIGHT";

            public const string ACC_X        = "nAccX";
            public const string ACC_Y        = "nAccY";
            public const string ACC_Z        = "nAccZ";

            // Swinging
            public const string ACC_UP       = "nACCUP";
            public const string ACC_DOWN     = "nACCDOWN";
            public const string ACC_LEFT     = "nACCLEFT";
            public const string ACC_RIGHT    = "nACCRIGHT";
            public const string ACC_FORWARD  = "nACCFORWARD";
            public const string ACC_BACKWARD = "nACCBACKWARD";

            // Shaking
            public const string ACC_SHAKE_X  = "nSHAKEX";
            public const string ACC_SHAKE_Y  = "nSHAKEY";
            public const string ACC_SHAKE_Z  = "nSHAKEZ";

            // tilting the controler with the wrist
            public const string TILT_RIGHT   = "nTILTRIGHT";
            public const string TILT_LEFT    = "nTILTLEFT";
            public const string TILT_UP      = "nTILTUP";
            public const string TILT_DOWN    = "nTILTDOWN";
            public const string FACE_UP      = "nTILTFACEUP";
            public const string FACE_DOWN    = "nTILTFACEDOWN";
        }

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
            yield return new KeyValuePair<string, float>(InputNames.C, C ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.Z, Z ? 1.0f : 0.0f);

            // Joystick
            joystick.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.JOY_X, joystick.X);
            yield return new KeyValuePair<string, float>(InputNames.JOY_Y, joystick.Y);
            yield return new KeyValuePair<string, float>(InputNames.UP, joystick.Y > 0 ? joystick.Y : 0);
            yield return new KeyValuePair<string, float>(InputNames.DOWN, joystick.Y > 0 ? 0 : -joystick.Y);
            yield return new KeyValuePair<string, float>(InputNames.LEFT, joystick.X > 0 ? 0 : -joystick.X);
            yield return new KeyValuePair<string, float>(InputNames.RIGHT, joystick.X > 0 ? joystick.X : 0);

            // Accelerometer
            accelerometer.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.ACC_X, accelerometer.X);
            yield return new KeyValuePair<string, float>(InputNames.ACC_Y, accelerometer.Y);
            yield return new KeyValuePair<string, float>(InputNames.ACC_Z, accelerometer.Z);
            yield return new KeyValuePair<string, float>(InputNames.TILT_LEFT, accelerometer.X > 0 ? 0 : -accelerometer.X);
            yield return new KeyValuePair<string, float>(InputNames.TILT_RIGHT, accelerometer.X > 0 ? accelerometer.X : 0);
            yield return new KeyValuePair<string, float>(InputNames.TILT_UP, accelerometer.Y > 0 ? accelerometer.Y : 0);
            yield return new KeyValuePair<string, float>(InputNames.TILT_DOWN, accelerometer.Y > 0 ? 0 : -accelerometer.Y);
            yield return new KeyValuePair<string, float>(InputNames.FACE_UP, accelerometer.Z > 0 ? accelerometer.Z : 0);
            yield return new KeyValuePair<string, float>(InputNames.FACE_DOWN, accelerometer.Z > 0 ? 0 : -accelerometer.Z);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
