using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace NintrollerLib
{
    //Source of mapping info: https://wiibrew.org/wiki/Wiimote/Extension_Controllers/Guitar_Hero_World_Tour_(Wii)_Drums
    public struct Drums : INintrollerState
    {
        public static class InputNames
        {
            public const string G = "drmG";
            public const string R = "drmR";
            public const string Y = "drmY";
            public const string B = "drmB";
            public const string O = "drmO";
            public const string BASS = "drmBASS";

            public const string UP = "drmUP";
            public const string DOWN = "drmDOWN";
            public const string LEFT = "drmLEFT";
            public const string RIGHT = "drmRIGHT";

            public const string SELECT = "drmSELECT";
            public const string START = "drmSTART";
            public const string HOME = "drmHOME";
        }

        private bool SpecialButtonSelect => wiimote.buttons.A;
        private bool SpecialButtonTiltCalibMin => wiimote.buttons.One;
        private bool SpecialButtonTiltCalibMax => wiimote.buttons.Two;
        private bool SpecialButtonTouchOn => wiimote.buttons.Plus;
        private bool SpecialButtonTouchOff => wiimote.buttons.Minus;
        private bool SpecialButtonDebugDump => wiimote.buttons.Home;

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
        public bool G, R, Y, B, O, Bass;
        public bool Up, Down, Left, Right;
        public bool Plus, Minus;

#if DEBUG
        public byte[] DebugLastData;
#endif

#if DEBUG
        private bool DebugButton_Dump;
#endif

        public Drums(Wiimote wm)
        {
            this = new Drums();
            wiimote = wm;

#if DEBUG
            DebugLastData = new byte[] { 0 };
#endif

            Joy.Calibrate(Calibrations.Defaults.GuitarDefault.Joy);
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
        
        private const float GTR_JOY_DIGITAL_THRESH = 0.5f;

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

                bool whichBit1 = (data[offset + 2] & 0x20) == 0;
                bool whichBit2 = (data[offset + 2] & 0x10) == 0;
                bool whichBit3 = (data[offset + 2] & 0x08) == 0;
                bool whichBit4 = (data[offset + 2] & 0x04) == 0;
                bool whichBit5 = (data[offset + 2] & 0x02) == 0;
                
                //Only registering when there is velocity information, if not the game doesn't recognize when you do fast inputs.
                G = !whichBit1 && whichBit2 && whichBit3 && !whichBit4 && whichBit5;// && (data[offset + 5] & 0x10) == 0;
                R = !whichBit1 && !whichBit2 && whichBit3 && whichBit4 && !whichBit5;// && (data[offset + 5] & 0x40) == 0;
                Y = !whichBit1 && whichBit2 && whichBit3 && whichBit4 && !whichBit5;// && (data[offset + 5] & 0x20) == 0;
                B = whichBit1 && !whichBit2 && !whichBit3 && !whichBit4 && !whichBit5;// && (data[offset + 5] & 0x08) == 0;
                O = whichBit1 && !whichBit2 && !whichBit3 && !whichBit4 && whichBit5;// && (data[offset + 5] & 0x80) == 0;
                Bass = !whichBit1 && !whichBit2 && whichBit3 && !whichBit4 && !whichBit5;// && (data[offset + 5] & 0x04) == 0;
        
                /*if(whichBit1 || whichBit2 || whichBit3 || whichBit4 || whichBit5) {         
                  Console.WriteLine("RAW: "
                    + (!whichBit1 && !whichBit2 && whichBit3 && whichBit4 && !whichBit5 ? "R" : "")
                    + (!whichBit1 && whichBit2 && whichBit3 && whichBit4 && !whichBit5 ? "Y" : "")
                    + (whichBit1 && !whichBit2 && !whichBit3 && !whichBit4 && !whichBit5 ? "B" : "")
                    + (whichBit1 && !whichBit2 && !whichBit3 && !whichBit4 && whichBit5 ? "O" : "")
                    + (!whichBit1 && whichBit2 && whichBit3 && !whichBit4 && whichBit5 ? "G" : "")
                    + (!whichBit1 && !whichBit2 && whichBit3 && !whichBit4 && !whichBit5 ? "BASS" : "")

                    //Softness are 3 bits which contains a number from 0 - 7 including the force of the hit.
                    //0 is max force, and 7 is no hit at all.
                    + " | Softness: "
                    + ((data[offset + 3] & 0x80) == 0)
                    + "." + ((data[offset + 3] & 0x40) == 0)
                    + "." + ((data[offset + 3] & 0x20) == 0)
                  );
                }*/

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
                        Joy.maxX = Joy.rawX;
                    else if (Joy.rawX < Joy.minX)
                        Joy.minX = Joy.rawX;


                    if (Joy.rawY > Joy.maxY)
                        Joy.maxY = Joy.rawY;
                    else if (Joy.rawY < Joy.minY)
                        Joy.minY = Joy.rawY;


                    Joy.Normalize();

                    bool isJoyPressed = (((Joy.X * Joy.X) + (Joy.Y * Joy.Y)) >= (GTR_JOY_DIGITAL_THRESH * GTR_JOY_DIGITAL_THRESH));
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

            }

#if LOW_BANDWIDTH

#else
            wiimote.Update(data);

            // Wiimote is sideways so these are weird
            if (wiimote.buttons.Up)
                Left = true;
            else if (wiimote.buttons.Down)
                Right = true;

            if (wiimote.buttons.Right)
                Down = true;
            else if (wiimote.buttons.Left)
                Up = true;

            // A on the actual wiimote
            if (SpecialButtonSelect)
                Select = true;
            
#endif






#if DEBUG
            if (offset > 0)
            {
                if (SpecialButtonDebugDump)
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



            Joy.Calibrate(Calibrations.Defaults.GuitarDefault.Joy);

            //SetCalibration(Calibrations.Defaults.ClassicControllerProDefault);
        }

        public void SetCalibration(INintrollerState from)
        {
            //if (from.CalibrationEmpty)
            //{
            //    // don't apply empty calibrations
            //    return;
            //}

            //if (from.GetType() == typeof(Guitar))
            //{
            //    Joy.Calibrate(((Guitar)from).Joy);
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
            sb.Append("-drm");
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

            yield return new KeyValuePair<string, float>(InputNames.G, G ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.R, R ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.Y, Y ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.B, B ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.O, O ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.BASS, Bass ? 1.0f : 0.0f);

            yield return new KeyValuePair<string, float>(InputNames.UP, (Up ? 1.0f : 0.0f));
            yield return new KeyValuePair<string, float>(InputNames.DOWN, (Down ? 1.0f : 0.0f));
            yield return new KeyValuePair<string, float>(InputNames.LEFT, (Left ? 1.0f : 0.0f));
            yield return new KeyValuePair<string, float>(InputNames.RIGHT, (Right ? 1.0f : 0.0f));

            yield return new KeyValuePair<string, float>(InputNames.START, Start ? 1.0f : 0.0f);
            yield return new KeyValuePair<string, float>(InputNames.SELECT, Select ? 1.0f : 0.0f);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
