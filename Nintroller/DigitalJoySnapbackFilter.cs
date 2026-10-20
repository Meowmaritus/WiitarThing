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
            StateElapsedTime += deltaTicks;
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
                    // Continually restart snapback filter so it's ready to prevent snapback when input is released.
                    if (inputA)
                        StateElapsedTime = 0;
                    // If snapback filter period is over, go back to waiting.
                    if (StateElapsedTime >= filterDurationTicks)
                        SwitchState(States.Wait);
                    break;

                case States.OutputtingB:
                    inputA = false;
                    // Continually restart snapback filter so it's ready to prevent snapback when input is released.
                    if (inputB)
                        StateElapsedTime = 0;
                    // If snapback filter period is over, go back to waiting.
                    if (StateElapsedTime >= filterDurationTicks)
                        SwitchState(States.Wait);
                    break;
            }
        }
    }
}
