namespace NintrollerLib
{
    internal static class StaticBuffers
    {
        public static readonly byte[] SensitivityBlockInit = new byte[] { 0x08 };

        public static readonly byte[] SensitivityBlock1_Custom = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00, 0xC0 };
        public static readonly byte[] SensitivityBlock1_CustomHigh = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0x00, 0x41 };
        public static readonly byte[] SensitivityBlock1_CustomMax = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x0C };
        public static readonly byte[] SensitivityBlock1_Level1 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x64, 0x00, 0xFE };
        public static readonly byte[] SensitivityBlock1_Level2 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0x96, 0x00, 0xB4 };
        public static readonly byte[] SensitivityBlock1_Level3 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xaa, 0x00, 0x64 };
        public static readonly byte[] SensitivityBlock1_Level4 = new byte[] { 0x02, 0x00, 0x00, 0x71, 0x01, 0x00, 0xc8, 0x00, 0x36 };
        public static readonly byte[] SensitivityBlock1_Level5 = new byte[] { 0x07, 0x00, 0x00, 0x71, 0x01, 0x00, 0x72, 0x00, 0x20 };

        public static readonly byte[] SensitivityBlock2_Custom = new byte[] { 0x40, 0x00 };
        public static readonly byte[] SensitivityBlock2_CustomHigh = new byte[] { 0x40, 0x00 };
        public static readonly byte[] SensitivityBlock2_CustomMax = new byte[] { 0x00, 0x00 };
        public static readonly byte[] SensitivityBlock2_Level1 = new byte[] { 0xFD, 0x05 };
        public static readonly byte[] SensitivityBlock2_Level2 = new byte[] { 0xB3, 0x04 };
        public static readonly byte[] SensitivityBlock2_Level3 = new byte[] { 0x63, 0x03 };
        public static readonly byte[] SensitivityBlock2_Level4 = new byte[] { 0x35, 0x03 };
        public static readonly byte[] SensitivityBlock2_Level5 = new byte[] { 0x1F, 0x03 };

        // public static readonly byte[] SensitivityBlock3 = new byte[] { (byte)_irMode }; // Not constant
        public static readonly byte[] SensitivityBlock4 = new byte[] { 0x08 };

        public static readonly byte[] ExtensionInit1 = new byte[] { 0x55 };
        public static readonly byte[] ExtensionInit2 = new byte[] { 0x00 };

        public static readonly byte[] MotionPlusInit = new byte[] { 0x04 };
    }
}