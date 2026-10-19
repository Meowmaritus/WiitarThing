using System;
using System.Collections.Generic;
using System.Linq;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using NintrollerLib;

namespace WiitarThing.Holders
{
    public class XInputHolder : Holder
    {
        private struct StateReport
        {
            public float A;
            public float B;
            public float X;
            public float Y;

            public float Up;
            public float Down;
            public float Left;
            public float Right;

            public float LeftBumper;
            public float RightBumper;
            public float LeftStickClick;
            public float RightStickClick;

            public float Start;
            public float Back;
            public float Guide;

            public float LeftStickX;
            public float LeftStickY;
            public float RightStickX;
            public float RightStickY;

            public float LeftTrigger;
            public float RightTrigger;
        }

        static internal bool[] availabe = { true, true, true, true };

        internal int minRumble = 20;
        internal int rumbleLeft = 0;
        internal int rumbleDecrement = 10;

        private XBus bus;
        private bool connected;
        private int ID;
        private ushort vid = 0;
        private ushort pid = 0;

        public static Dictionary<string, string> GetDefaultMapping(ControllerType type)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            // TODO: finish default mapping (Acc, IR, Balance Board, ect) (not for 1st release)
            switch (type)
            {
                case ControllerType.ProController:
                    result.Add(ProController.InputNames.A, Inputs.Xbox360.A);
                    result.Add(ProController.InputNames.B, Inputs.Xbox360.B);
                    result.Add(ProController.InputNames.X, Inputs.Xbox360.X);
                    result.Add(ProController.InputNames.Y, Inputs.Xbox360.Y);

                    result.Add(ProController.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(ProController.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(ProController.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(ProController.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(ProController.InputNames.L, Inputs.Xbox360.LB);
                    result.Add(ProController.InputNames.R, Inputs.Xbox360.RB);
                    result.Add(ProController.InputNames.ZL, Inputs.Xbox360.LT);
                    result.Add(ProController.InputNames.ZR, Inputs.Xbox360.RT);

                    result.Add(ProController.InputNames.LUP, Inputs.Xbox360.LUP);
                    result.Add(ProController.InputNames.LDOWN, Inputs.Xbox360.LDOWN);
                    result.Add(ProController.InputNames.LLEFT, Inputs.Xbox360.LLEFT);
                    result.Add(ProController.InputNames.LRIGHT, Inputs.Xbox360.LRIGHT);

                    result.Add(ProController.InputNames.RUP, Inputs.Xbox360.RUP);
                    result.Add(ProController.InputNames.RDOWN, Inputs.Xbox360.RDOWN);
                    result.Add(ProController.InputNames.RLEFT, Inputs.Xbox360.RLEFT);
                    result.Add(ProController.InputNames.RRIGHT, Inputs.Xbox360.RRIGHT);

                    result.Add(ProController.InputNames.LS, Inputs.Xbox360.LS);
                    result.Add(ProController.InputNames.RS, Inputs.Xbox360.RS);
                    result.Add(ProController.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(ProController.InputNames.START, Inputs.Xbox360.START);
                    result.Add(ProController.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    break;

                case ControllerType.ClassicControllerPro:
                    result.Add(ClassicControllerPro.InputNames.A, Inputs.Xbox360.A);
                    result.Add(ClassicControllerPro.InputNames.B, Inputs.Xbox360.B);
                    result.Add(ClassicControllerPro.InputNames.X, Inputs.Xbox360.X);
                    result.Add(ClassicControllerPro.InputNames.Y, Inputs.Xbox360.Y);

                    result.Add(ClassicControllerPro.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(ClassicControllerPro.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(ClassicControllerPro.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(ClassicControllerPro.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(ClassicControllerPro.InputNames.L, Inputs.Xbox360.LB);
                    result.Add(ClassicControllerPro.InputNames.R, Inputs.Xbox360.RB);
                    result.Add(ClassicControllerPro.InputNames.ZL, Inputs.Xbox360.LT);
                    result.Add(ClassicControllerPro.InputNames.ZR, Inputs.Xbox360.RT);

                    result.Add(ClassicControllerPro.InputNames.LUP, Inputs.Xbox360.LUP);
                    result.Add(ClassicControllerPro.InputNames.LDOWN, Inputs.Xbox360.LDOWN);
                    result.Add(ClassicControllerPro.InputNames.LLEFT, Inputs.Xbox360.LLEFT);
                    result.Add(ClassicControllerPro.InputNames.LRIGHT, Inputs.Xbox360.LRIGHT);

                    result.Add(ClassicControllerPro.InputNames.RUP, Inputs.Xbox360.RUP);
                    result.Add(ClassicControllerPro.InputNames.RDOWN, Inputs.Xbox360.RDOWN);
                    result.Add(ClassicControllerPro.InputNames.RLEFT, Inputs.Xbox360.RLEFT);
                    result.Add(ClassicControllerPro.InputNames.RRIGHT, Inputs.Xbox360.RRIGHT);

                    result.Add(ClassicControllerPro.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(ClassicControllerPro.InputNames.START, Inputs.Xbox360.START);
                    result.Add(ClassicControllerPro.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.RIGHT);
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.A);
                    result.Add(Wiimote.InputNames.B, Inputs.Xbox360.B);
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.LS);
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.RS);
                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    break;

                case ControllerType.ClassicController:
                    result.Add(ClassicController.InputNames.B, Inputs.Xbox360.B);
                    result.Add(ClassicController.InputNames.A, Inputs.Xbox360.A);
                    result.Add(ClassicController.InputNames.Y, Inputs.Xbox360.X);
                    result.Add(ClassicController.InputNames.X, Inputs.Xbox360.Y);

                    result.Add(ClassicController.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(ClassicController.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(ClassicController.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(ClassicController.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(ClassicController.InputNames.ZL, Inputs.Xbox360.LB);
                    result.Add(ClassicController.InputNames.ZR, Inputs.Xbox360.RB);
                    result.Add(ClassicController.InputNames.LT, Inputs.Xbox360.LT);
                    result.Add(ClassicController.InputNames.RT, Inputs.Xbox360.RT);

                    result.Add(ClassicController.InputNames.LUP, Inputs.Xbox360.LUP);
                    result.Add(ClassicController.InputNames.LDOWN, Inputs.Xbox360.LDOWN);
                    result.Add(ClassicController.InputNames.LLEFT, Inputs.Xbox360.LLEFT);
                    result.Add(ClassicController.InputNames.LRIGHT, Inputs.Xbox360.LRIGHT);

                    result.Add(ClassicController.InputNames.RUP, Inputs.Xbox360.RUP);
                    result.Add(ClassicController.InputNames.RDOWN, Inputs.Xbox360.RDOWN);
                    result.Add(ClassicController.InputNames.RLEFT, Inputs.Xbox360.RLEFT);
                    result.Add(ClassicController.InputNames.RRIGHT, Inputs.Xbox360.RRIGHT);

                    result.Add(ClassicController.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(ClassicController.InputNames.START, Inputs.Xbox360.START);
                    result.Add(ClassicController.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.RIGHT);
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.A);
                    result.Add(Wiimote.InputNames.B, Inputs.Xbox360.B);
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.LS);
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.RS);
                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    break;

                //case ControllerType.Nunchuk:
                //case ControllerType.NunchukB:
                //    result.Add(Nunchuk.InputNames.UP,    Inputs.Xbox360.LUP);
                //    result.Add(Nunchuk.InputNames.DOWN,  Inputs.Xbox360.LDOWN);
                //    result.Add(Nunchuk.InputNames.LEFT,  Inputs.Xbox360.LLEFT);
                //    result.Add(Nunchuk.InputNames.RIGHT, Inputs.Xbox360.LRIGHT);
                //    result.Add(Nunchuk.InputNames.Z,     Inputs.Xbox360.RT);
                //    result.Add(Nunchuk.InputNames.C,     Inputs.Xbox360.LT);
                //    result.Add(Nunchuk.InputNames.TILT_RIGHT, "");
                //    result.Add(Nunchuk.InputNames.TILT_LEFT, "");
                //    result.Add(Nunchuk.InputNames.TILT_UP, "");
                //    result.Add(Nunchuk.InputNames.TILT_DOWN, "");
                //    result.Add(Nunchuk.InputNames.ACC_SHAKE_X, "");
                //    result.Add(Nunchuk.InputNames.ACC_SHAKE_Y, "");
                //    result.Add(Nunchuk.InputNames.ACC_SHAKE_Z, "");

                //    result.Add(Wiimote.InputNames.UP,    Inputs.Xbox360.UP);
                //    result.Add(Wiimote.InputNames.DOWN,  Inputs.Xbox360.DOWN);
                //    result.Add(Wiimote.InputNames.LEFT,  Inputs.Xbox360.LB);
                //    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.RB);
                //    result.Add(Wiimote.InputNames.A,     Inputs.Xbox360.A);
                //    result.Add(Wiimote.InputNames.B,     Inputs.Xbox360.B);
                //    result.Add(Wiimote.InputNames.ONE,   Inputs.Xbox360.X);
                //    result.Add(Wiimote.InputNames.TWO,   Inputs.Xbox360.Y);
                //    result.Add(Wiimote.InputNames.PLUS,  Inputs.Xbox360.BACK);
                //    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                //    result.Add(Wiimote.InputNames.HOME,  Inputs.Xbox360.GUIDE);
                //    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                //    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                //    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                //    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                //    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                //    result.Add(Wiimote.InputNames.TILT_UP, "");
                //    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                //    break;

                case ControllerType.Nunchuk:
                case ControllerType.NunchukB:
                case ControllerType.Wiimote:
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.DOWN);

                    result.Add(Wiimote.InputNames.B, Inputs.Xbox360.A); //Green
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.B); //Red
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.Y); //Yellow
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.X); //Blue
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.LB); //Orange

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.BACK); //SP

                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    //result.Add(Wiimote.InputNames.LEFT,  Inputs.Xbox360.DOWN);






                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, Inputs.Xbox360.RRIGHT);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, Inputs.Xbox360.RRIGHT);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, Inputs.Xbox360.RRIGHT);
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    result.Add(Wiimote.InputNames.IR_RIGHT, "");
                    result.Add(Wiimote.InputNames.IR_LEFT, "");
                    result.Add(Wiimote.InputNames.IR_UP, "");
                    result.Add(Wiimote.InputNames.IR_DOWN, "");
                    break;

                case ControllerType.Guitar:

                    result.Add(Guitar.InputNames.G, Inputs.Xbox360.A);
                    result.Add(Guitar.InputNames.R, Inputs.Xbox360.B);
                    result.Add(Guitar.InputNames.Y, Inputs.Xbox360.Y);
                    result.Add(Guitar.InputNames.B, Inputs.Xbox360.X);
                    result.Add(Guitar.InputNames.O, Inputs.Xbox360.LB);

                    result.Add(Guitar.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Guitar.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Guitar.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Guitar.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(Guitar.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(Guitar.InputNames.START, Inputs.Xbox360.START);
                    result.Add(Guitar.InputNames.HOME, Inputs.Xbox360.GUIDE);

                    result.Add(Guitar.InputNames.WHAMMYLOW, Inputs.Xbox360.RLEFT);
                    result.Add(Guitar.InputNames.WHAMMYHIGH, Inputs.Xbox360.RRIGHT);

                    result.Add(Guitar.InputNames.TILTLOW, Inputs.Xbox360.RDOWN);
                    result.Add(Guitar.InputNames.TILTHIGH, Inputs.Xbox360.RUP);

                    //result.Add(Wiimote.InputNames.UP, "");
                    //result.Add(Wiimote.InputNames.DOWN, "");
                    //result.Add(Wiimote.InputNames.LEFT, "");
                    //result.Add(Wiimote.InputNames.RIGHT, "");
                    //result.Add(Wiimote.InputNames.A, "");
                    //result.Add(Wiimote.InputNames.B, "");
                    //result.Add(Wiimote.InputNames.ONE, "");
                    //result.Add(Wiimote.InputNames.TWO, "");
                    //result.Add(Wiimote.InputNames.PLUS, "");
                    //result.Add(Wiimote.InputNames.MINUS, "");
                    //result.Add(Wiimote.InputNames.HOME, "");
                    //result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    //result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    //result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    //result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    //result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    //result.Add(Wiimote.InputNames.TILT_UP, "");
                    //result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    //result.Add(Wiimote.InputNames.IR_RIGHT, "");
                    //result.Add(Wiimote.InputNames.IR_LEFT, "");
                    //result.Add(Wiimote.InputNames.IR_UP, "");
                    //result.Add(Wiimote.InputNames.IR_DOWN, "");

                    break;

                case ControllerType.Drums:
                    result.Add(Drums.InputNames.G, Inputs.Xbox360.A);
                    result.Add(Drums.InputNames.R, Inputs.Xbox360.B);
                    result.Add(Drums.InputNames.Y, Inputs.Xbox360.Y);
                    result.Add(Drums.InputNames.B, Inputs.Xbox360.X);
                    result.Add(Drums.InputNames.O, Inputs.Xbox360.RB);
                    result.Add(Drums.InputNames.BASS, Inputs.Xbox360.LB);

                    result.Add(Drums.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Drums.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Drums.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Drums.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(Drums.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(Drums.InputNames.START, Inputs.Xbox360.START);
                    result.Add(Drums.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    break;

                case ControllerType.Turntable:

                    result.Add(Turntable.InputNames.RG, Inputs.Xbox360.A);
                    result.Add(Turntable.InputNames.RR, Inputs.Xbox360.B);
                    result.Add(Turntable.InputNames.RB, Inputs.Xbox360.X);
                    result.Add(Turntable.InputNames.LG, Inputs.Xbox360.A);
                    result.Add(Turntable.InputNames.LR, Inputs.Xbox360.B);
                    result.Add(Turntable.InputNames.LB, Inputs.Xbox360.X);

                    result.Add(Turntable.InputNames.UP, Inputs.Xbox360.UP);
                    result.Add(Turntable.InputNames.DOWN, Inputs.Xbox360.DOWN);
                    result.Add(Turntable.InputNames.LEFT, Inputs.Xbox360.LEFT);
                    result.Add(Turntable.InputNames.RIGHT, Inputs.Xbox360.RIGHT);

                    result.Add(Turntable.InputNames.LTABLECTRCLKWISE, Inputs.Xbox360.LLEFT);
                    result.Add(Turntable.InputNames.LTABLECLKWISE, Inputs.Xbox360.LRIGHT);
                    result.Add(Turntable.InputNames.RTABLECTRCLKWISE, Inputs.Xbox360.LDOWN);
                    result.Add(Turntable.InputNames.RTABLECLKWISE, Inputs.Xbox360.LUP);

                    result.Add(Turntable.InputNames.EUPHORIA, Inputs.Xbox360.Y);
                    result.Add(Turntable.InputNames.SELECT, Inputs.Xbox360.BACK);
                    result.Add(Turntable.InputNames.START, Inputs.Xbox360.START);

                    result.Add(Turntable.InputNames.DIALCLKWISE, Inputs.Xbox360.RRIGHT);
                    result.Add(Turntable.InputNames.DIALCTRCLKWISE, Inputs.Xbox360.RLEFT);
                    result.Add(Turntable.InputNames.CROSSFADERLEFT, Inputs.Xbox360.RDOWN);
                    result.Add(Turntable.InputNames.CROSSFADERRIGHT, Inputs.Xbox360.RUP);

                    result.Add(Turntable.InputNames.RBUTTONS, Inputs.Xbox360.RT);
                    result.Add(Turntable.InputNames.LBUTTONS, Inputs.Xbox360.LT);

                    result.Add(Wiimote.InputNames.UP, Inputs.Xbox360.LEFT);
                    result.Add(Wiimote.InputNames.DOWN, Inputs.Xbox360.RIGHT);
                    result.Add(Wiimote.InputNames.LEFT, Inputs.Xbox360.DOWN);
                    result.Add(Wiimote.InputNames.RIGHT, Inputs.Xbox360.UP);
                    result.Add(Wiimote.InputNames.A, Inputs.Xbox360.X);
                    result.Add(Wiimote.InputNames.B, "");
                    result.Add(Wiimote.InputNames.ONE, Inputs.Xbox360.B);
                    result.Add(Wiimote.InputNames.TWO, Inputs.Xbox360.A);
                    result.Add(Wiimote.InputNames.PLUS, Inputs.Xbox360.BACK);
                    result.Add(Wiimote.InputNames.MINUS, Inputs.Xbox360.START);
                    result.Add(Wiimote.InputNames.HOME, Inputs.Xbox360.GUIDE);
                    result.Add(Wiimote.InputNames.ACC_SHAKE_X, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Y, "");
                    result.Add(Wiimote.InputNames.ACC_SHAKE_Z, "");
                    result.Add(Wiimote.InputNames.TILT_RIGHT, "");
                    result.Add(Wiimote.InputNames.TILT_LEFT, "");
                    result.Add(Wiimote.InputNames.TILT_UP, "");
                    result.Add(Wiimote.InputNames.TILT_DOWN, "");
                    break;
            }

            return result;
        }

        public XInputHolder()
        {
            //Values = new Dictionary<string, float>();
            Values = new System.Collections.Concurrent.ConcurrentDictionary<string, float>();
            Mappings = new Dictionary<string, string>();
            Flags = new Dictionary<string, bool>();

            //if (!Flags.ContainsKey(Inputs.Flags.RUMBLE))
            //{
            //    Flags.Add(Inputs.Flags.RUMBLE, false);
            //}
            //
            //if (!Values.ContainsKey(Inputs.Flags.RUMBLE))
            //{
            //    Values.TryAdd(Inputs.Flags.RUMBLE, 0f);
            //}
        }

        public XInputHolder(ControllerType t) : this()
        {
            Mappings = GetDefaultMapping(t);
        }

        public void SetType(ControllerType t)
        {
            switch (t)
            {
                case ControllerType.Guitar:
                    vid = 0x1430;
                    pid = 0x4734;
                    break;
                case ControllerType.Drums:
                    vid = 0x1430;
                    pid = 0x0805;
                    break;
                case ControllerType.Turntable:
                    vid = 0x1430;
                    pid = 0x1705;
                    break;
                default:
                    break;
            }
            if (connected)
            {
                int prevID = ID;
                RemoveXInput(prevID);
                ConnectXInput(prevID);
            }
        }

        public override void Update()
        {
            if (!connected)
            {
                return;
            }

            var controller = bus.GetController(ID);
            if (controller == null)
            {
                return;
            }

            var report = new StateReport();

            foreach (KeyValuePair<string, string> map in Mappings)
            {
                if (!Values.TryGetValue(map.Key, out float value))
                {
                    continue;
                }

                switch (map.Value)
                {
                    case Inputs.Xbox360.A:      report.A += value; break;
                    case Inputs.Xbox360.B:      report.B += value; break;
                    case Inputs.Xbox360.X:      report.X += value; break;
                    case Inputs.Xbox360.Y:      report.Y += value; break;

                    case Inputs.Xbox360.UP:     report.Up += value; break;
                    case Inputs.Xbox360.DOWN:   report.Down += value; break;
                    case Inputs.Xbox360.LEFT:   report.Left += value; break;
                    case Inputs.Xbox360.RIGHT:  report.Right += value; break;

                    case Inputs.Xbox360.LB:     report.LeftBumper += value; break;
                    case Inputs.Xbox360.RB:     report.RightBumper += value; break;
                    case Inputs.Xbox360.LS:     report.LeftStickClick += value; break;
                    case Inputs.Xbox360.RS:     report.RightStickClick += value; break;

                    case Inputs.Xbox360.START:  report.Start += value; break;
                    case Inputs.Xbox360.BACK:   report.Back += value; break;
                    case Inputs.Xbox360.GUIDE:  report.Guide += value; break;

                    case Inputs.Xbox360.LLEFT:  report.LeftStickX -= value; break;
                    case Inputs.Xbox360.LRIGHT: report.LeftStickX += value; break;
                    case Inputs.Xbox360.LUP:    report.LeftStickY += value; break;
                    case Inputs.Xbox360.LDOWN:  report.LeftStickY -= value; break;

                    case Inputs.Xbox360.RLEFT:  report.RightStickX -= value; break;
                    case Inputs.Xbox360.RRIGHT: report.RightStickX += value; break;
                    case Inputs.Xbox360.RUP:    report.RightStickY += value; break;
                    case Inputs.Xbox360.RDOWN:  report.RightStickY -= value; break;

                    case Inputs.Xbox360.LT:     report.LeftTrigger += value; break;
                    case Inputs.Xbox360.RT:     report.RightTrigger += value; break;
                }
            }

            controller.SetButtonState(Xbox360Button.A, report.A > 0f);
            controller.SetButtonState(Xbox360Button.B, report.B > 0f);
            controller.SetButtonState(Xbox360Button.X, report.X > 0f);
            controller.SetButtonState(Xbox360Button.Y, report.Y > 0f);

            controller.SetButtonState(Xbox360Button.Up, report.Up > 0f);
            controller.SetButtonState(Xbox360Button.Down, report.Down > 0f);
            controller.SetButtonState(Xbox360Button.Left, report.Left > 0f);
            controller.SetButtonState(Xbox360Button.Right, report.Right > 0f);

            controller.SetButtonState(Xbox360Button.LeftShoulder, report.LeftBumper > 0f);
            controller.SetButtonState(Xbox360Button.RightShoulder, report.RightBumper > 0f);
            controller.SetButtonState(Xbox360Button.LeftThumb, report.LeftStickClick > 0f);
            controller.SetButtonState(Xbox360Button.RightThumb, report.RightStickClick > 0f);

            controller.SetButtonState(Xbox360Button.Start, report.Start > 0f);
            controller.SetButtonState(Xbox360Button.Back, report.Back > 0f);
            controller.SetButtonState(Xbox360Button.Guide, report.Guide > 0f);
            
            controller.SetAxisValue(Xbox360Axis.LeftThumbX, GetRawAxis(report.LeftStickX));
            controller.SetAxisValue(Xbox360Axis.LeftThumbY, GetRawAxis(report.LeftStickY));
            controller.SetAxisValue(Xbox360Axis.RightThumbX, GetRawAxis(report.RightStickX));
            controller.SetAxisValue(Xbox360Axis.RightThumbY, GetRawAxis(report.RightStickY));

            controller.SetSliderValue(Xbox360Slider.LeftTrigger, GetRawTrigger(report.LeftTrigger));
            controller.SetSliderValue(Xbox360Slider.RightTrigger, GetRawTrigger(report.RightTrigger));

            controller.SubmitReport();
        }

        private void OnRumble(object sender, Xbox360FeedbackReceivedEventArgs args)
        {
            int strength = (args.LargeMotor << 8) | args.SmallMotor;
            Flags[Inputs.Flags.RUMBLE] = strength > minRumble;
            RumbleAmount = strength > minRumble ? strength : 0;
        }

        public override void Close()
        {
            RemoveXInput(ID);
        }

        public override void AddMapping(ControllerType controller)
        {
            var additional = GetDefaultMapping(controller);

            foreach (KeyValuePair<string, string> map in additional)
            {
                SetMapping(map.Key, map.Value);
            }

            SetType(controller);
        }

        public bool ConnectXInput(int id)
        {
            if (id < 0 || id > 3)
            {
                WiitarDebug.Log($"Attempted to connect invalid user index {id}!");
                return false;
            }

            availabe[id] = false;
            bus = XBus.Default;
            bus.Unplug(id);
            bus.Plugin(id, vid, pid);
            var controller = bus.GetController(id);
            if (controller == null)
            {
                RemoveXInput(id);
                return false;
            }
            controller.FeedbackReceived += OnRumble;

            ID = id;
            connected = true;
            return true;
        }

        public bool RemoveXInput(int id)
        {
            if (id < 0 || id > 3)
            {
                WiitarDebug.Log($"Attempted to remove invalid user index {id}!");
                return false;
            }

            availabe[id] = true;
            Flags[Inputs.Flags.RUMBLE] = false;
            if (bus.Unplug(id))
            {
                ID = -1;
                connected = false;
                return true;
            }

            return false;
        }

        public short GetRawAxis(float axis)
        {
            if (axis > 1f)
            {
                return short.MaxValue;
            }
            else if (axis < -1f)
            {
                return short.MinValue;
            }

            return (short)(axis * short.MaxValue);
        }

        public byte GetRawTrigger(float trigger)
        {
            if (trigger > 1f)
            {
                return byte.MaxValue;
            }
            else if (trigger < -1f)
            {
                return byte.MinValue;
            }

            return (byte)(trigger * byte.MaxValue);
        }
    }

    public class XBus
    {
        private static XBus defaultInstance;
        private ViGEmClient viGEmClient;
        private Dictionary<int, IXbox360Controller> targets;
        private List<IXbox360Controller> connected;

        // Default Bus
        public static XBus Default
        {
            get
            {
                // if it hasn't been created create one
                if (defaultInstance == null)
                {
                    defaultInstance = new XBus();
                }

                return defaultInstance;
            }
        }

        public ViGEmClient Client
        {
            get
            {
                return viGEmClient;
            }
            private set
            {
                viGEmClient = value;
            }
        }

        public XBus()
        {
            Client = new ViGEmClient();
            targets = new Dictionary<int, IXbox360Controller>();
            connected = new List<IXbox360Controller>();
            App.Current.Exit += StopDevice;
        }

        private void StopDevice(object sender, System.Windows.ExitEventArgs e)
        {
            if (defaultInstance != null)
            {
                foreach (IXbox360Controller controller in targets.Values)
                {
                    if (connected.Contains(controller))
                    {
                        controller.Disconnect();
                        connected.Remove(controller);
                    }
                }
                Client.Dispose();
            }
        }

        public void Plugin(int id, ushort vid, ushort pid)
        {
            if (targets.ContainsKey(id))
            {
                return;
            }

            IXbox360Controller controller;
            if (vid != 0 && pid != 0)
            {
                controller = Client.CreateXbox360Controller(vid, pid);
            }
            else
            {
                controller = Client.CreateXbox360Controller();
            }

            controller.AutoSubmitReport = false;
            controller.Connect();
            targets.Add(id, controller);
            connected.Add(controller);
        }

        public bool Unplug(int id)
        {
            if (targets.ContainsKey(id) && targets[id] != null)
            {
                if (connected.Contains(targets[id]))
                {
                    targets[id].Disconnect();
                    connected.Remove(targets[id]);
                    targets.Remove(id);
                    return true;
                }
                return false;
            }

            return false;
        }

        public IXbox360Controller GetController(int id)
        {
            if (!targets.TryGetValue(id, out var controller))
            {
                return null;
            }

            return controller;
        }
    }
}
