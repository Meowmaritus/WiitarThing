using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintrollerLib
{
    public class DigitalJoySnapbackFilter
    {
        public enum States
        {
            Wait = 0,
            OutputtingA = 1,
            OutputtingB = 2,
        }
        public States State = States.Wait;
        public long StateElapsedTime = 0;

        private void SwitchState(States state)
        {
            State = state;
            StateElapsedTime = 0;
        }

        public void Update(long deltaTicks, long filterDurationTicks, ref bool inputA, ref bool inputB)
        {
            switch (State)
            {
                case States.Wait:

                    if (inputA)
                    {
                        SwitchState(States.OutputtingA);
                        inputB = false;
                    }
                    else if (inputB)
                    {
                        SwitchState(States.OutputtingB);
                        inputA = false;
                    }

                    break;

                case States.OutputtingA:
                    inputB = false;
                    if (StateElapsedTime >= filterDurationTicks)
                        SwitchState(States.Wait);
                    break;

                case States.OutputtingB:
                    inputA = false;
                    if (StateElapsedTime >= filterDurationTicks)
                        SwitchState(States.Wait);
                    break;
            }
        }
    }
}
