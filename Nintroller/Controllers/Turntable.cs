using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace NintrollerLib
{
    public struct Turntable : INintrollerState
    {
        public static class InputNames
        {
            public const string UP = "ttbUP";
            public const string DOWN = "ttbDOWN";
            public const string LEFT = "ttbLEFT";
            public const string RIGHT = "ttbRIGHT";

            public const string LUP = "ttbLUP";
            public const string LDOWN = "ttbLDOWN";
            public const string LLEFT = "ttbLLEFT";
            public const string LRIGHT = "ttbLRIGHT";

            public const string LX = "ttbLX";
            public const string LY = "ttbLY";

            public const string LTABLECLKWISE = "ttbLTABLECLKWISE";
            public const string LTABLECTRCLKWISE = "ttbLTABLECTRCLKWISE";
            public const string LTABLE = "ttbLTABLE";

            public const string RTABLECLKWISE = "ttbRTABLECLKWISE";
            public const string RTABLECTRCLKWISE = "ttbRTABLECTRCLKWISE";
            public const string RTABLE = "ttbRTABLE";

            public const string LG = "ttbLG";
            public const string LR = "ttbLR";
            public const string LB = "ttbLB";
            public const string LBUTTONS = "ttbLBUTTONS";

            public const string RG = "ttbRG";
            public const string RR = "ttbRR";
            public const string RB = "ttbRB";
            public const string RBUTTONS = "ttbRBUTTONS";

            public const string DIALCLKWISE = "ttbDIALCLKWISE";
            public const string DIALCTRCLKWISE = "ttbDIALCTRCLKWISE";
            public const string DIAL = "ttbDIAL";
            public const string DIALT = "ttbDIALT";

            public const string CROSSFADERLEFT = "ttbCROSSFADERLEFT";
            public const string CROSSFADERRIGHT = "ttbCROSSFADERRIGHT";
            public const string CROSSFADER = "ttbCROSSFADER";
            public const string CROSSFADERT = "ttbCROSSFADERT";

            public const string EUPHORIA = "ttbEUPHORIA";
            public const string SELECT = "ttbSELECT";
            public const string START = "ttbSTART";
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
        public Joystick Joy, JoyTableLR, JoyDialCrossfade;
        public Trigger Crossfader, Dial;
        public bool RG, RR, RB, LG, LR, LB;
        public Trigger RButtons, LButtons;
        public bool Euphoria;
        public bool Plus, Minus;

#if DEBUG
        public byte[] DebugLastData;
#endif

#if DEBUG
        private bool DebugButton_Dump;
#endif

        public Turntable(Wiimote wm)
        {
            this = new Turntable();
            wiimote = wm;

#if DEBUG
            DebugLastData = new byte[] { 0 };
#endif
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
                byte rawLButtons, rawRButtons, rawLTable, rawRTable, rawDial, rawCrossfader;
                // Joystick
                Joy.rawX = (byte)(data[offset] & 0x3F);
                Joy.rawY = (byte)(data[offset + 1] & 0x3F);
                Joy.Normalize();

                // Buttons
                rawLButtons = (byte)(7 - (
                    ((data[offset + 5] & 0x80) >> 5) |
                    ((data[offset + 4] & 0x20) >> 4) |
                    ((data[offset + 5] & 0x08) >> 3)));
                rawRButtons = (byte)(7 - (
                    (data[offset + 5] & 0x04) |
                    (data[offset + 4] & 0x02) |
                    ((data[offset + 5] & 0x20) >> 5)));
                LG = (rawLButtons & 0x01) != 0;
                LR = (rawLButtons & 0x02) != 0;
                LB = (rawLButtons & 0x04) != 0;
                RG = (rawRButtons & 0x01) != 0;
                RR = (rawRButtons & 0x02) != 0;
                RB = (rawRButtons & 0x04) != 0;
                Euphoria = (data[offset + 5] & 0x10) == 0;
                Plus = (data[offset + 4] & 0x04) == 0;
                Minus = (data[offset + 4] & 0x10) == 0;

                // Button X360 triggers
                RButtons.rawValue = rawRButtons;
                RButtons.Normalize();
                LButtons.rawValue = rawLButtons;
                LButtons.Normalize();

                // Turntables
                // Wii range is 0-32 per direction.
                // 360 range is 0-128 per direction out of 0-32768
                // for 360 range, add 8160 to each rawTable value
                rawLTable = (byte)(((
                    ((data[offset + 4] & 0x01) << 5) |
                    (data[offset + 3] & 0x1f)
                    ) + 32) % 64);
                rawRTable = (byte)(((
                    ((data[offset + 2] & 0x01) << 5) |
                    ((data[offset] & 0xc0) >> 3) |
                    ((data[offset + 1] & 0xc0) >> 5) |
                    ((data[offset + 2] & 0x80) >> 7)
                    ) + 32) % 64);
                JoyTableLR.rawX = rawLTable;
                JoyTableLR.rawY = rawRTable;
                JoyTableLR.Normalize();

                // Dial & Crossfader Joystick
                rawDial = (byte)(
                    ((data[offset + 2] & 0x60) >> 2) |
                    ((data[offset + 3] & 0xe0) >> 5)
                    );
                rawCrossfader = (byte)((data[offset + 2] & 0x1e) >> 1);
                JoyDialCrossfade.rawX = rawDial;
                JoyDialCrossfade.rawY = rawCrossfader;
                JoyDialCrossfade.Normalize();

                // Dial Trigger
                Dial.rawValue = rawDial;
                Dial.Normalize();

                // Crossfader Trigger
                Crossfader.rawValue = rawCrossfader;
                Crossfader.Normalize();
            }

            wiimote.Update(data);

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

            // only default calibration is supported right now
            SetCalibration(Calibrations.Defaults.TurntableDefault);

            /*
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    SetCalibration(Calibrations.Defaults.TurntableDefault);
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    SetCalibration(Calibrations.Moderate.TurntableModest);
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    SetCalibration(Calibrations.Extras.TurntableExtra);
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    SetCalibration(Calibrations.Minimum.TurntableMinimal);
                    break;

                case Calibrations.CalibrationPreset.None:
                    SetCalibration(Calibrations.None.TurntableRaw);
                    break;
            }
            */
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.CalibrationEmpty)
            {
                // don't apply empty calibrations
                return;
            }

            if (from.GetType() == typeof(Turntable))
            {
                Joy.Calibrate(((Turntable)from).Joy);
                JoyTableLR.Calibrate(((Turntable)from).JoyTableLR);
                JoyDialCrossfade.Calibrate(((Turntable)from).JoyDialCrossfade);
                LButtons.Calibrate(((Turntable)from).LButtons);
                RButtons.Calibrate(((Turntable)from).RButtons);
                Dial.Calibrate(((Turntable)from).Dial);
                Crossfader.Calibrate(((Turntable)from).Crossfader);
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
                else if (component.StartsWith("jtb"))
                {
                    string[] joyConfig = component.Split(new char[] { '|' });

                    for (int j = 1; j < joyConfig.Length; j++)
                    {
                        int value = 0;
                        if (int.TryParse(joyConfig[j], out value))
                        {
                            switch (j)
                            {
                                case 1: JoyTableLR.centerX = value; break;
                                case 2: JoyTableLR.minX = value; break;
                                case 3: JoyTableLR.maxX = value; break;
                                case 4: JoyTableLR.deadX = value; break;
                                case 5: JoyTableLR.centerY = value; break;
                                case 6: JoyTableLR.minY = value; break;
                                case 7: JoyTableLR.maxY = value; break;
                                case 8: JoyTableLR.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("jdc"))
                {
                    string[] joyConfig = component.Split(new char[] { '|' });

                    for (int j = 1; j < joyConfig.Length; j++)
                    {
                        int value = 0;
                        if (int.TryParse(joyConfig[j], out value))
                        {
                            switch (j)
                            {
                                case 1: JoyDialCrossfade.centerX = value; break;
                                case 2: JoyDialCrossfade.minX = value; break;
                                case 3: JoyDialCrossfade.maxX = value; break;
                                case 4: JoyDialCrossfade.deadX = value; break;
                                case 5: JoyDialCrossfade.centerY = value; break;
                                case 6: JoyDialCrossfade.minY = value; break;
                                case 7: JoyDialCrossfade.maxY = value; break;
                                case 8: JoyDialCrossfade.deadY = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("lb"))
                {
                    string[] lButtonsConfig = component.Split(new char[] { '|' });

                    for (int t = 1; t < lButtonsConfig.Length; t++)
                    {
                        int value = 0;
                        if (int.TryParse(lButtonsConfig[t], out value))
                        {
                            switch (t)
                            {
                                case 1: LButtons.min = value; break;
                                case 2: LButtons.max = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("rb"))
                {
                    string[] rButtonsConfig = component.Split(new char[] { '|' });

                    for (int t = 1; t < rButtonsConfig.Length; t++)
                    {
                        int value = 0;
                        if (int.TryParse(rButtonsConfig[t], out value))
                        {
                            switch (t)
                            {
                                case 1: RButtons.min = value; break;
                                case 2: RButtons.max = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("cf"))
                {
                    string[] crossfaderConfig = component.Split(new char[] { '|' });

                    for (int t = 1; t < crossfaderConfig.Length; t++)
                    {
                        int value = 0;
                        if (int.TryParse(crossfaderConfig[t], out value))
                        {
                            switch (t)
                            {
                                case 1: Crossfader.min = value; break;
                                case 2: Crossfader.max = value; break;
                                default: break;
                            }
                        }
                    }
                }
                else if (component.StartsWith("di"))
                {
                    string[] dialConfig = component.Split(new char[] { '|' });

                    for (int t = 1; t < dialConfig.Length; t++)
                    {
                        int value = 0;
                        if (int.TryParse(dialConfig[t], out value))
                        {
                            switch (t)
                            {
                                case 1: Dial.min = value; break;
                                case 2: Dial.max = value; break;
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
            sb.Append("-ttb");
            sb.Append(":joy");
                sb.Append("|"); sb.Append(Joy.centerX);
                sb.Append("|"); sb.Append(Joy.minX);
                sb.Append("|"); sb.Append(Joy.maxX);
                sb.Append("|"); sb.Append(Joy.deadX);
                sb.Append("|"); sb.Append(Joy.centerY);
                sb.Append("|"); sb.Append(Joy.minY);
                sb.Append("|"); sb.Append(Joy.maxY);
                sb.Append("|"); sb.Append(Joy.deadY);
            sb.Append(":jtb");
                sb.Append("|"); sb.Append(JoyTableLR.centerX);
                sb.Append("|"); sb.Append(JoyTableLR.minX);
                sb.Append("|"); sb.Append(JoyTableLR.maxX);
                sb.Append("|"); sb.Append(JoyTableLR.deadX);
                sb.Append("|"); sb.Append(JoyTableLR.centerY);
                sb.Append("|"); sb.Append(JoyTableLR.minY);
                sb.Append("|"); sb.Append(JoyTableLR.maxY);
                sb.Append("|"); sb.Append(JoyTableLR.deadY);
            sb.Append(":jdc");
                sb.Append("|"); sb.Append(JoyDialCrossfade.centerX);
                sb.Append("|"); sb.Append(JoyDialCrossfade.minX);
                sb.Append("|"); sb.Append(JoyDialCrossfade.maxX);
                sb.Append("|"); sb.Append(JoyDialCrossfade.deadX);
                sb.Append("|"); sb.Append(JoyDialCrossfade.centerY);
                sb.Append("|"); sb.Append(JoyDialCrossfade.minY);
                sb.Append("|"); sb.Append(JoyDialCrossfade.maxY);
                sb.Append("|"); sb.Append(JoyDialCrossfade.deadY);
            sb.Append(":lb");
                sb.Append("|"); sb.Append(LButtons.min);
                sb.Append("|"); sb.Append(LButtons.max);
            sb.Append(":rb");
                sb.Append("|"); sb.Append(RButtons.min);
                sb.Append("|"); sb.Append(RButtons.max);
            sb.Append(":cf");
                sb.Append("|"); sb.Append(Crossfader.min);
                sb.Append("|"); sb.Append(Crossfader.max);
            sb.Append(":di");
                sb.Append("|"); sb.Append(Dial.min);
                sb.Append("|"); sb.Append(Dial.max);

            return sb.ToString();
        }

        public bool CalibrationEmpty
        {
            get
            {
                if (Joy.maxX == 0 && Joy.maxY == 0 && JoyTableLR.maxX == 0 && JoyTableLR.maxY == 0 && JoyDialCrossfade.maxX == 0 && JoyDialCrossfade.maxY == 0)
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

            Joy.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.LX, Joy.X);
            yield return new KeyValuePair<string, float>(InputNames.LY, Joy.Y);

            // analog
            yield return new KeyValuePair<string, float>(InputNames.LUP, Joy.Y > 0f ? Joy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LDOWN, Joy.Y < 0f ? -Joy.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LLEFT, Joy.X < 0f ? -Joy.X : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LRIGHT, Joy.X > 0f ? Joy.X : 0.0f);

            // digital
            yield return new KeyValuePair<string, float>(InputNames.UP, Joy.Y > 0.5f ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.DOWN, Joy.Y < -0.5f ? -1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LEFT, Joy.X < -0.5f ? -1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RIGHT, Joy.X > 0.5f ? 1.0f : 0.0f);

            JoyTableLR.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.LTABLECLKWISE, JoyTableLR.X > 0f ? JoyTableLR.X : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LTABLECTRCLKWISE, JoyTableLR.X < 0f ? -JoyTableLR.X : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LTABLE, JoyTableLR.X);

            yield return new KeyValuePair<string, float>(InputNames.RTABLECLKWISE, JoyTableLR.Y > 0f ? JoyTableLR.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RTABLECTRCLKWISE, JoyTableLR.Y < 0f ? -JoyTableLR.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RTABLE, JoyTableLR.Y);

            RButtons.Normalize();
            LButtons.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.RG, RG ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RR, RR ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RB, RB ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.RBUTTONS, RButtons.value);

            yield return new KeyValuePair<string, float>(InputNames.LG, LG ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LR, LR ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LB, LB ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.LBUTTONS, LButtons.value);

            JoyDialCrossfade.Normalize();
            Dial.Normalize();
            Crossfader.Normalize();
            yield return new KeyValuePair<string, float>(InputNames.DIALCLKWISE, JoyDialCrossfade.X > 0f ? JoyDialCrossfade.X : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.DIALCTRCLKWISE, JoyDialCrossfade.X < 0f ? -JoyDialCrossfade.X : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.DIAL, JoyDialCrossfade.X);
            yield return new KeyValuePair<string, float>(InputNames.DIALT, Dial.value);

            yield return new KeyValuePair<string, float>(InputNames.CROSSFADERLEFT, JoyDialCrossfade.Y < 0f ? -JoyDialCrossfade.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.CROSSFADERRIGHT, JoyDialCrossfade.Y > 0f ? JoyDialCrossfade.Y : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.CROSSFADER, JoyDialCrossfade.Y);
            yield return new KeyValuePair<string, float>(InputNames.CROSSFADERT, Crossfader.value);

            yield return new KeyValuePair<string, float>(InputNames.EUPHORIA, Euphoria ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.START, Start ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.SELECT, Select ? 1.0f : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
