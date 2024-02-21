using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EdgeDetection {
    class CppProxy : IProxy {
        [DllImport(@"C:\Users\User\Downloads\EdgeDetection\x64\Release\EdgeDetectionCpp.dll")]
        private static extern void edgeDetection(byte[] values, int width, int height, int pixelSize, byte[] convertedValues);

        public void executeEdgeDetection(byte[] values, int width, int height, int pixelSize, byte[] convertedValues) {
            edgeDetection(values, width, height, pixelSize, convertedValues);
        }
    }
}
