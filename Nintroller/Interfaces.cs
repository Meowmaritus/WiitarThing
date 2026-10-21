using System;
using System.Collections.Generic;
using static NintrollerLib.ControllerConfig;

namespace NintrollerLib
{
    public interface INintrollerState : IEnumerable<KeyValuePair<string, float>>
    {
        void Update(Nintroller device, byte[] data, long elapsedTicks);
        float GetValue(string input);
        void SetCalibration(Calibrations.CalibrationPreset preset);
        void SetCalibration(INintrollerState from);
        void SetCalibration(string calibrationString);
        string GetCalibrationString();
        bool CalibrationEmpty { get; }


        void SetConfig(IControllerConfig config);
        IControllerConfig GetConfig();

#if DEBUG
        bool DebugViewActive { get; set; }
#endif
    }

    public interface INintrollerParsable
    {
        void Parse(byte[] input, int offset = 0);
    }

    public interface INintrollerNormalizable
    {
        void Normalize();
    }

    public interface INintrollerBounds
    {
        bool InBounds(float x, float y = 0);
    }
}
