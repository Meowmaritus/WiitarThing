using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintrollerLib
{
    

    public sealed class ControllerConfig
    {
        public interface IControllerConfig
        {
            void InitDefaults();
            IControllerConfig GetClone();
        }

        public struct EmptyConfig : IControllerConfig
        {
            public void InitDefaults() { }
            public IControllerConfig GetClone()
            {
                return new EmptyConfig();
            }
        }

        public class GuitarConfig : IControllerConfig
        {
            // Settings

            public long SnapbackFilterTicks;
            public float JoyDigitalThreshold;
            public bool EnableTouchStrip;

            // Calibrations

            public Joystick JoyCalibration;
            public Accelerometer WiimoteAccelerometerCalibration;
            public float TiltCalibration_Neutral;
            public float TiltCalibration_Weight;
            public float TiltCalibration_Tilted;
            public float TiltCalibration_StartingZ;
            public byte WhammyCalibration_Min;
            public byte WhammyCalibration_Max;

            public void InitDefaults()
            {
                SnapbackFilterTicks = 50 * TimeSpan.TicksPerMillisecond;
                JoyDigitalThreshold = 80f;
                EnableTouchStrip = true;

                JoyCalibration.Calibrate(Calibrations.Defaults.GuitarDefault.Joy);
                WiimoteAccelerometerCalibration.Calibrate(Calibrations.Defaults.WiimoteDefault.accelerometer);
                TiltCalibration_Neutral = 0;
                TiltCalibration_Tilted = (float)(Math.PI / 2);
                TiltCalibration_Weight = 0.35f;
                WhammyCalibration_Min = 0xFF;
                WhammyCalibration_Max = 0;
            }

            public IControllerConfig GetClone()
            {
                var clone = new GuitarConfig();

                clone.SnapbackFilterTicks = SnapbackFilterTicks;
                clone.JoyDigitalThreshold = JoyDigitalThreshold;
                clone.EnableTouchStrip = EnableTouchStrip;

                clone.JoyCalibration.Calibrate(JoyCalibration);
                clone.WiimoteAccelerometerCalibration.Calibrate(WiimoteAccelerometerCalibration);
                clone.TiltCalibration_Neutral = TiltCalibration_Neutral;
                clone.TiltCalibration_Weight = TiltCalibration_Weight;
                clone.TiltCalibration_Tilted = TiltCalibration_Tilted;
                clone.TiltCalibration_StartingZ = TiltCalibration_StartingZ;
                clone.WhammyCalibration_Min = WhammyCalibration_Min;
                clone.WhammyCalibration_Max = WhammyCalibration_Max;

                return clone;
            }
        }


        public class DrumsConfig : IControllerConfig
        {
            public long SnapbackFilterTicks;

            // Whether to read the drums as MIDI rather than buttons.
            // MIDI will probably work better for fast hits due to the button data's built-in very long debounce.
            public bool ReadAsMIDI;

            // The cooldown upon hitting each pad that prevents "third hand" pad ghost hits from happening on dodgy kits.
            public long PadHitCooldownMax;

            // Each drum hit will sustain a button output value of true for this many ticks.
            public long HitSustainTicks;

            // Minimum number of ticks to wait between outputting each drum hit.
            public long HitReleaseTicks;

            // Minimum MIDI velocity required for hits to register.
            public int MinVelocityG;
            public int MinVelocityR;
            public int MinVelocityY;
            public int MinVelocityB;
            public int MinVelocityO;
            public int MinVelocityBass;

            // Whether to ignore hits while the current hit is sustained.
            public bool DropBouncesG;
            public bool DropBouncesR;
            public bool DropBouncesY;
            public bool DropBouncesB;
            public bool DropBouncesO;
            public bool DropBouncesBass;


            // Calibrations

            public Joystick JoyCalibration;

            public void InitDefaults()
            {
                SnapbackFilterTicks = 50 * TimeSpan.TicksPerMillisecond;
                ReadAsMIDI = true;
                HitSustainTicks = TimeSpan.TicksPerMillisecond * 24; // about 42 NPS
                HitReleaseTicks = TimeSpan.TicksPerMillisecond * 6;
                PadHitCooldownMax = 30 * TimeSpan.TicksPerMillisecond;
                MinVelocityG = 0;
                MinVelocityR = 0;
                MinVelocityY = 0;
                MinVelocityB = 0;
                MinVelocityO = 0;
                MinVelocityBass = 0;
                DropBouncesG = true;
                DropBouncesR = true;
                DropBouncesY = true;
                DropBouncesB = true;
                DropBouncesO = true;
                DropBouncesBass = true;

                JoyCalibration.Calibrate(Calibrations.Defaults.GuitarDefault.Joy);
            }

            public IControllerConfig GetClone()
            {
                var clone = new DrumsConfig();

                clone.SnapbackFilterTicks = SnapbackFilterTicks;
                clone.ReadAsMIDI = ReadAsMIDI;
                clone.PadHitCooldownMax = PadHitCooldownMax;
                clone.HitSustainTicks = HitSustainTicks;
                clone.HitReleaseTicks = HitReleaseTicks;
                clone.MinVelocityG = MinVelocityG;
                clone.MinVelocityR = MinVelocityR;
                clone.MinVelocityY = MinVelocityY;
                clone.MinVelocityB = MinVelocityB;
                clone.MinVelocityO = MinVelocityO;
                clone.MinVelocityBass = MinVelocityBass;
                clone.DropBouncesG = DropBouncesG;
                clone.DropBouncesR = DropBouncesR;
                clone.DropBouncesY = DropBouncesY;
                clone.DropBouncesB = DropBouncesB;
                clone.DropBouncesO = DropBouncesO;
                clone.DropBouncesBass = DropBouncesBass;

                clone.JoyCalibration.Calibrate(JoyCalibration);

                return clone;
            }
        }



        public class TurntableConfig : IControllerConfig
        {
            public Joystick JoyCalibration, JoyTableLRCalibration, JoyDialCrossfadeCalibration;
            public Trigger CrossfaderCalibration, DialCalibration;
            public Trigger RButtonsCalibration, LButtonsCalibration;

            public void InitDefaults()
            {
                JoyCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.Joy);
                JoyTableLRCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.JoyTableLR);
                JoyDialCrossfadeCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.JoyDialCrossfade);
                CrossfaderCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.Crossfader);
                DialCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.Dial);
                RButtonsCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.RButtons);
                LButtonsCalibration.Calibrate(Calibrations.Defaults.TurntableDefault.LButtons);
            }

            public IControllerConfig GetClone()
            {
                var clone = new TurntableConfig();

                clone.JoyCalibration.Calibrate(JoyCalibration);
                clone.JoyTableLRCalibration.Calibrate(JoyTableLRCalibration);
                clone.JoyDialCrossfadeCalibration.Calibrate(JoyDialCrossfadeCalibration);
                clone.CrossfaderCalibration.Calibrate(CrossfaderCalibration);
                clone.DialCalibration.Calibrate(DialCalibration);
                clone.RButtonsCalibration.Calibrate(RButtonsCalibration);
                clone.LButtonsCalibration.Calibrate(LButtonsCalibration);
                
                return clone;
            }
        }

    }
}
