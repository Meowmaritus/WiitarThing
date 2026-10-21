using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        // The drum pad values the controller reports as buttons (fast hits can get ignored)
        public bool ButtonG;
        public bool ButtonR;
        public bool ButtonY;
        public bool ButtonB;
        public bool ButtonO;
        public bool ButtonBass;

        // The drum pad values the controller reports as MIDI (processed / debounced / etc)
        public bool MidiG;
        public bool MidiR;
        public bool MidiY;
        public bool MidiB;
        public bool MidiO;
        public bool MidiBass;

        public long PadHitCooldown1;
        public long PadHitCooldown2;

        // MIDI note IDs - TODO: check if these would ever be wrong for any drumkit?
        public const int MidiNoteG = 45;
        public const int MidiNoteR = 38;
        public const int MidiNoteY = 46;
        public const int MidiNoteB = 48;
        public const int MidiNoteO = 49;
        public const int MidiNoteBass = 36;

        public DigitalJoySnapbackFilter SnapbackFilterJoyX;
        public DigitalJoySnapbackFilter SnapbackFilterJoyY;

        public InputHitSustainer HitSustainerG;
        public InputHitSustainer HitSustainerR;
        public InputHitSustainer HitSustainerY;
        public InputHitSustainer HitSustainerB;
        public InputHitSustainer HitSustainerO;
        public InputHitSustainer HitSustainerBass;

        public ControllerConfig.DrumsConfig Config;
        public bool NeedsConfigLoad;


#if DEBUG
        public byte[] DebugLastData;

        private bool DebugButton_Dump;

        public Queue<string> DebugMidiPacketReadLog;
        const int DebugMidiPacketReadLogMaxCount = 20;
        public int DebugMidiPacketReadIndex;

        public int DebugLastMidiNote;
        public int DebugLastMidiVelocity;
        public int DebugLastMidiChannel;

        public int DebugTimerValue;

        public bool DebugLastButtonG;
        public bool DebugLastButtonR;
        public bool DebugLastButtonY;
        public bool DebugLastButtonB;
        public bool DebugLastButtonO;
        public bool DebugLastButtonBass;

#endif

        public Drums(Wiimote wm)
        {
            this = new Drums();
            wiimote = wm;

            HitSustainerG = new InputHitSustainer();
            HitSustainerR = new InputHitSustainer();
            HitSustainerY = new InputHitSustainer();
            HitSustainerB = new InputHitSustainer();
            HitSustainerO = new InputHitSustainer();
            HitSustainerBass = new InputHitSustainer();

            SnapbackFilterJoyX = new DigitalJoySnapbackFilter();
            SnapbackFilterJoyY = new DigitalJoySnapbackFilter();

#if DEBUG
            DebugLastData = new byte[] { 0 };

            DebugMidiPacketReadLog = new Queue<string>();
#endif
            Config = new ControllerConfig.DrumsConfig();
            NeedsConfigLoad = true;
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
        
        private const float GTR_JOY_DIGITAL_THRESH = 0.8f;

        private static float _MapRange(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }


#if DEBUG
        private void DebugEnqueuePacketReadLog(string s)
        {
            if (DebugMidiPacketReadLog.Count >= DebugMidiPacketReadLogMaxCount)
                DebugMidiPacketReadLog.Dequeue();
            DebugMidiPacketReadLog.Enqueue($"[{DebugMidiPacketReadIndex}] {s}");

            DebugMidiPacketReadIndex++;
        }
#endif

        public void Update(Nintroller device, byte[] data, long elapsedTicks)
        {
#if DEBUG
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

            if (PadHitCooldown1 > 0)
                PadHitCooldown1 -= elapsedTicks;

            if (PadHitCooldown2 > 0)
                PadHitCooldown2 -= elapsedTicks;

            if (offset > 0)
            {

                int midiNote = (((data[offset + 2] >> 1) & 0x7F)) ^ 0b1111111;

                int midiVelocityBit3 = (data[offset + 2] & 1) << 3;
                int midiVelocityBit4to6 = (data[offset + 3] >> 1) & 0b01110000;
                int midiVelocityBit2 = (data[offset + 3] & 1) << 2;
                int midiVelocityBit1 = (data[offset + 4] & (1 << 7)) >> 6;
                int midiVelocityBit0 = data[offset + 4] & 1;

                int midiVelocity = (midiVelocityBit0 | midiVelocityBit1 | midiVelocityBit2 | midiVelocityBit3 | midiVelocityBit4to6) ^ 0b1111111;

                int midiChannel = (((data[offset + 3] >> 1) & 0b1111) ^ 0b1111) + 1;


                bool canPadHit = PadHitCooldown1 <= 0 || PadHitCooldown2 <= 0;
                bool padHit = false;

                ButtonG = (data[offset + 5] & (1 << 4)) == 0;
                ButtonR = (data[offset + 5] & (1 << 6)) == 0;
                ButtonY = (data[offset + 5] & (1 << 5)) == 0;
                ButtonB = (data[offset + 5] & (1 << 3)) == 0;
                ButtonO = (data[offset + 5] & (1 << 7)) == 0;
                ButtonBass = (data[offset + 5] & (1 << 2)) == 0;

                if (midiNote == MidiNoteG && midiVelocity >= Config.MinVelocityG && canPadHit)
                {
                    HitSustainerG.QueueHit(Config.DropBouncesG);
                    padHit = true;
                }

                if (midiNote == MidiNoteR && midiVelocity >= Config.MinVelocityR && canPadHit)
                {
                    HitSustainerR.QueueHit(Config.DropBouncesR);
                    padHit = true;
                }

                if (midiNote == MidiNoteY && midiVelocity >= Config.MinVelocityY && canPadHit)
                {
                    HitSustainerY.QueueHit(Config.DropBouncesY);
                    padHit = true;
                }

                if (midiNote == MidiNoteB && midiVelocity >= Config.MinVelocityB && canPadHit)
                {
                    HitSustainerB.QueueHit(Config.DropBouncesB);
                    padHit = true;
                }

                if (midiNote == MidiNoteO && midiVelocity >= Config.MinVelocityO && canPadHit)
                {
                    HitSustainerO.QueueHit(Config.DropBouncesO);
                    padHit = true;
                }

                if (padHit)
                {
                    if (PadHitCooldown1 <= 0)
                        PadHitCooldown1 = Config.PadHitCooldownMax;
                    else if (PadHitCooldown2 <= 0)
                        PadHitCooldown2 = Config.PadHitCooldownMax;
                }

                if (midiNote == MidiNoteBass && midiVelocity >= Config.MinVelocityBass)
                    HitSustainerBass.QueueHit(Config.DropBouncesBass);
#if DEBUG


                if (midiNote != DebugLastMidiNote || midiVelocity != DebugLastMidiVelocity || midiChannel != DebugLastMidiChannel)
                {
                    DebugEnqueuePacketReadLog($"MIDI <Note: {midiNote} | Velocity: {midiVelocity} | Channel: {midiChannel}>");
                }

                if (ButtonG != DebugLastButtonG || ButtonR != DebugLastButtonR || ButtonY != DebugLastButtonY ||
                    ButtonB != DebugLastButtonB || ButtonO != DebugLastButtonO || ButtonBass != DebugLastButtonBass)
                {
                    string buttonsString = "Button <"
                        + (ButtonG ? "G" : "-")
                        + (ButtonR ? "R" : "-")
                        + (ButtonY ? "Y" : "-")
                        + (ButtonB ? "B" : "-")
                        + (ButtonO ? "O" : "-")
                        + (ButtonBass ? "P" : "-")
                        + $"> Cooldowns <{PadHitCooldown1:0000} {PadHitCooldown2:0000}>";
                    DebugEnqueuePacketReadLog(buttonsString);
                }

                DebugLastButtonG = ButtonG;
                DebugLastButtonR = ButtonR;
                DebugLastButtonY = ButtonY;
                DebugLastButtonB = ButtonB;
                DebugLastButtonO = ButtonO;
                DebugLastButtonBass = ButtonBass;



                DebugLastMidiNote = midiNote;
                DebugLastMidiVelocity = midiVelocity;
                DebugLastMidiChannel = midiChannel;
#endif

                //ZR = (data[offset + 5] & 0x04) == 0;
                Plus = (data[offset + 4] & 0x04) == 0;
                Minus = (data[offset + 4] & 0x10) == 0;
                //Home = (data[offset + 4] & 0x08) == 0;

                Up = false;
                Down = false;
                Left = false;
                Right = false;

                if (data[offset] != 0 || data[offset + 1] != 0)
                {
                    // Joysticks
                    Joy.rawX = (byte)(data[offset] & 0x3F);
                    Joy.rawY = (byte)(data[offset + 1] & 0x03F);

                    Joy.AutoCalibrate(device);

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

                SnapbackFilterJoyX.Update(elapsedTicks, Config.SnapbackFilterTicks, ref Left, ref Right);
                SnapbackFilterJoyY.Update(elapsedTicks, Config.SnapbackFilterTicks, ref Up, ref Down);

            }


            MidiG = HitSustainerG.Update(elapsedTicks, Config.HitSustainTicks, Config.HitReleaseTicks);
            MidiR = HitSustainerR.Update(elapsedTicks, Config.HitSustainTicks, Config.HitReleaseTicks);
            MidiY = HitSustainerY.Update(elapsedTicks, Config.HitSustainTicks, Config.HitReleaseTicks);
            MidiB = HitSustainerB.Update(elapsedTicks, Config.HitSustainTicks, Config.HitReleaseTicks);
            MidiO = HitSustainerO.Update(elapsedTicks, Config.HitSustainTicks, Config.HitReleaseTicks);
            MidiBass = HitSustainerBass.Update(elapsedTicks, Config.HitSustainTicks, Config.HitReleaseTicks);

            if (Config.ReadAsMIDI)
            {
                G = MidiG;
                R = MidiR;
                Y = MidiY;
                B = MidiB;
                O = MidiO;
                Bass = MidiBass;
            }
            else
            {
                G = ButtonG;
                R = ButtonR;
                Y = ButtonY;
                B = ButtonB;
                O = ButtonO;
                Bass = ButtonBass;
            }


#if LOW_BANDWIDTH

#else
            wiimote.Update(device, data, elapsedTicks);

            // Wiimote is sideways so these are weird
            if (wiimote.buttons.Up)
                Left = true;
            else if (wiimote.buttons.Down)
                Right = true;

            // These are opposite of how guitar is because the Wii remote slots in the opposite way around.
            if (wiimote.buttons.Right)
                Up = true;
            else if (wiimote.buttons.Left)
                Down = true;

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

        public void SetConfig(ControllerConfig.IControllerConfig config)
        {
            if (config is ControllerConfig.DrumsConfig drumsConfig)
            {
                Config = drumsConfig;
                Joy.Calibrate(Config.JoyCalibration);
                NeedsConfigLoad = false;
            }
            
        }

        public ControllerConfig.IControllerConfig GetConfig()
        {
            if (NeedsConfigLoad)
                return null;
            Config.JoyCalibration.Calibrate(Joy);
            return Config;
        }
    }
}
