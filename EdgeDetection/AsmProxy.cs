using System;
using System.Runtime.InteropServices;

namespace EdgeDetection {
    class AsmProxy : IProxy {
        [DllImport(@"C:\Users\User\Downloads\EdgeDetection\x64\Debug\EdgeDetectionAsm.dll")]
        private static extern void edgeDetection(byte[] values, int width, int height, int pixelSize, byte[] convertedValues);
        public void executeEdgeDetection(byte[] values, int width, int height, int pixelSize, byte[] convertedValues) {
            edgeDetection(values, width, height, pixelSize, convertedValues);
        }
    }
}
