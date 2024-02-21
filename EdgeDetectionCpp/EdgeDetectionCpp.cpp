#include "pch.h" 
#include <utility>
#include <limits.h>
#include "EdgeDetectionCpp.h"

void edgeDetection(char* values, int width, int height, int pixelSize, char* convertedValues){
    int bytes = width * height * pixelSize;
    int stride = width * pixelSize;
    for (int i = stride; i < bytes - stride; i++) {
        int sum = 0;
        sum += values[i - stride] * -1;
        sum += values[i - pixelSize] * -1;
        sum += values[i] * 4;
        sum += values[i + pixelSize] * -1;
        sum += values[i + stride] * -1;
        convertedValues[i] = abs(sum);
    }
    for (int i = 0; i < stride; i++) {
        convertedValues[i] = convertedValues[i + stride];
    }
    for (int i = bytes - stride; i < bytes; i++) {
        convertedValues[i] = convertedValues[i - stride];
    }
}
