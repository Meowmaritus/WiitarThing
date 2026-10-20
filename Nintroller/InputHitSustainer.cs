using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintrollerLib
{
    public class InputHitSustainer
    {
        private enum States
        {
            Wait = 0,
            Sustain = 1,
            Release = 2,
        }
        private States State = States.Wait;
        private long StateElapsedTime = 0;
        private int NumQueuedHits = 0;
        public bool OutputValue = false;

        public void QueueHit(bool dropBounces)
        {
            // If dropBounces is set to true then it must be in wait state to process input
            if (State == States.Wait || !dropBounces)
                NumQueuedHits++;
        }

        private void SwitchState(States state)
        {
            State = state;
            StateElapsedTime = 0;
        }

        public bool Update(long elapsedTicks, long sustainTicks, long releaseTicks)
        {
            StateElapsedTime += elapsedTicks;

            switch (State)
            {
                case States.Wait:
                    OutputValue = false;
                    if (NumQueuedHits > 0)
                    {
                        NumQueuedHits--;
                        OutputValue = true;
                        SwitchState(States.Sustain);
                    }

                    break;

                case States.Sustain:
                    OutputValue = true;
                    if (StateElapsedTime >= sustainTicks)
                    {
                        OutputValue = false;
                        SwitchState(States.Release);
                    }

                    break;

                case States.Release:
                    OutputValue = false;
                    if (StateElapsedTime >= releaseTicks)
                    {
                        SwitchState(States.Wait);
                    }

                    break;
            }




            return OutputValue;
        }


        

    }
}
