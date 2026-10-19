using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NintrollerLib
{
    public struct ClassicControllerPro : INintrollerState
    {
        public static class InputNames
        {
            public const string A      = "ccpA";
            public const string B      = "ccpB";
            public const string X      = "ccpX";
            public const string Y      = "ccpY";

            public const string UP     = "ccpUP";
            public const string DOWN   = "ccpDOWN";
            public const string LEFT   = "ccpLEFT";
            public const string RIGHT  = "ccpRIGHT";

            public const string L      = "ccpL";
            public const string R      = "ccpR";
            public const string ZL     = "ccpZL";
            public const string ZR     = "ccpZR";

            public const string LX     = "ccpLX";
            public const string LY     = "ccpLY";
            public const string RX     = "ccpRX";
            public const string RY     = "ccpRY";

            public const string LUP    = "ccpLUP";
            public const string LDOWN  = "ccpLDOWN";
            public const string LLEFT  = "ccpLLEFT";
            public const string LRIGHT = "ccpLRIGHT";

            public const string RUP    = "ccpRUP";
            public const string RDOWN  = "ccpRDOWN";
            public const string RLEFT  = "ccpRLEFT";
            public const string RRIGHT = "ccpRRIGHT";

            public const string SELECT = "ccpSELECT";
            public const string START  = "ccpSTART";
            public const string HOME   = "ccpHOME";
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

            yield return new KeyValuePair<string, float>(InputNames.A, A ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.X, X ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.Y, Y ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.L, L ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.R, R ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.ZL, ZL ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.ZR, ZR ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.UP, Up ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.DOWN, Down ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LEFT, Left ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RIGHT, Right ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.START, Start ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.SELECT, Select ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.HOME, Home ? 1.0f : 0.0f);

            LJoy.Normalize();
            RJoy.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.LX, LJoy.X);
            yield return new KeyValuePair<string, float>(InputNames.LY, LJoy.Y);
            yield return new KeyValuePair<string, float>(InputNames.RX, RJoy.X);
            yield return new KeyValuePair<string, float>(InputNames.RY, RJoy.X);

            yield return new KeyValuePair<string, float>(InputNames.LUP, LJoy.Y > 0f ? LJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LDOWN, LJoy.Y > 0f ? 0.0f : -LJoy.Y); // These are inverted
            yield return new KeyValuePair<string, float>(InputNames.LLEFT, LJoy.X > 0f ? 0.0f : -LJoy.X); // because they
            yield return new KeyValuePair<string, float>(InputNames.LRIGHT, LJoy.X > 0f ? LJoy.X : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.RUP, RJoy.Y > 0f ? RJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RDOWN, RJoy.Y > 0f ? 0.0f : -RJoy.Y); // represents how far the
            yield return new KeyValuePair<string, float>(InputNames.RLEFT, RJoy.X > 0f ? 0.0f : -RJoy.X); // input is left or down
            yield return new KeyValuePair<string, float>(InputNames.RRIGHT, RJoy.X > 0f ? RJoy.X : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
