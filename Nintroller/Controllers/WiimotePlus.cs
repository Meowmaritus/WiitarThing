using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NintrollerLib
{
    public struct WiimotePlus : INintrollerState
    {
        public static class MOTION_PLUS
        {
            public const string GYROUP       = "mpGYROUP";
            public const string GYRODOWN     = "mpGYRODOWN";
            public const string GYROLEFT     = "mpGYROLEFT";
            public const string GYRORIGHT    = "mpGYRORIGHT";
            public const string GYROFORWARD  = "mpGYROFORWARD";
            public const string GYROBACKWARD = "mpGYROBACKWARD";
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

        Wiimote wiimote;
        //gyro

        public void Update(byte[] data)
        {
            throw new NotImplementedException();
        }

        public float GetValue(string input)
        {
            throw new NotImplementedException();
        }

        // TODO: Calibration - Balance Board Calibration
        public void SetCalibration(Calibrations.CalibrationPreset preset)
        {
            switch (preset)
            {
                case Calibrations.CalibrationPreset.Default:
                    break;

                case Calibrations.CalibrationPreset.Modest:
                    break;

                case Calibrations.CalibrationPreset.Extra:
                    break;

                case Calibrations.CalibrationPreset.Minimum:
                    break;

                case Calibrations.CalibrationPreset.None:
                    break;
            }
        }

        public void SetCalibration(INintrollerState from)
        {
            if (from.GetType() == typeof(WiimotePlus))
            {

            }
        }

        public void SetCalibration(string calibrationString)
        {

        }

        public string GetCalibrationString()
        {
            return "";
        }

        public bool CalibrationEmpty
        {
            get { return false; }
        }

        public IEnumerator<KeyValuePair<string, float>> GetEnumerator()
        {
            foreach (var input in wiimote)
            {
                yield return input;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
