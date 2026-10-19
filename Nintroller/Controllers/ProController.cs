using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NintrollerLib
{
    public struct ProController : INintrollerState
    {
        public static class InputNames
        {
            public const string A      = "proA";
            public const string B      = "proB";
            public const string X      = "proX";
            public const string Y      = "proY";

            public const string UP     = "proUP";
            public const string DOWN   = "proDOWN";
            public const string LEFT   = "proLEFT";
            public const string RIGHT  = "proRIGHT";

            public const string L      = "proL";
            public const string R      = "proR";
            public const string ZL     = "proZL";
            public const string ZR     = "proZR";

            public const string LX     = "proLX";
            public const string LY     = "proLY";
            public const string RX     = "proRX";
            public const string RY     = "proRY";

            public const string LUP    = "proLUP";
            public const string LDOWN  = "proLDOWN";
            public const string LLEFT  = "proLLEFT";
            public const string LRIGHT = "proLRIGHT";

            public const string RUP    = "proRUP";
            public const string RDOWN  = "proRDOWN";
            public const string RLEFT  = "proRLEFT";
            public const string RRIGHT = "proRRIGHT";

            public const string LS     = "proLS";
            public const string RS     = "proRS";

            public const string SELECT = "proSELECT";
            public const string START  = "proSTART";
            public const string HOME   = "proHOME";
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
            yield return new KeyValuePair<string, float>(InputNames.A, A ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.X, X ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.Y, Y ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.L,  L  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.R,  R  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.ZL, ZL ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.ZR, ZR ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.UP,    Up    ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.DOWN,  Down  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LEFT,  Left  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RIGHT, Right ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.START,  Start  ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.SELECT, Select ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.HOME,   Home   ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.LS, LStick ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RS, RStick ? 1.0f : 0.0f);

            LJoy.Normalize();
            RJoy.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.LX, LJoy.X);
            yield return new KeyValuePair<string, float>(InputNames.LY, LJoy.Y);
            yield return new KeyValuePair<string, float>(InputNames.RX, RJoy.X);
            yield return new KeyValuePair<string, float>(InputNames.RY, RJoy.X);

            yield return new KeyValuePair<string, float>(InputNames.LUP,    LJoy.Y > 0f ? LJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LDOWN,  LJoy.Y > 0f ? 0.0f : -LJoy.Y); // These are inverted
            yield return new KeyValuePair<string, float>(InputNames.LLEFT,  LJoy.X > 0f ? 0.0f : -LJoy.X); // because they
            yield return new KeyValuePair<string, float>(InputNames.LRIGHT, LJoy.X > 0f ? LJoy.X : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.RUP,    RJoy.Y > 0f ? RJoy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RDOWN,  RJoy.Y > 0f ? 0.0f : -RJoy.Y); // represents how far the
            yield return new KeyValuePair<string, float>(InputNames.RLEFT,  RJoy.X > 0f ? 0.0f : -RJoy.X); // input is left or down
            yield return new KeyValuePair<string, float>(InputNames.RRIGHT, RJoy.X > 0f ? RJoy.X : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
