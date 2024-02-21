using System;
using System.Collections.Generic;
using System.Text;

namespace EdgeDetection {
    interface IProxy {
        void executeEdgeDetection(byte[] values, int width, int height, int pixelSize, byte[] convertedValues);
    }
}
