# The purpose of the algorithm

The project involves writing an efficient algorithm for detecting edges in an image using a Laplace filter with coefficients defined in the LAPL1 mask:

| 0 | -1 | 0 |
| :-: | :-: | :-: |
| -1 | 4 | -1 |
| 0 | -1 | 0 |

The program will perform convolution operations between the image and the LAPL1 mask. This will transform each pixel in the image based on the surrounding 
pixels according to the weights from the mask.  After the convolution, the resulting image will be the image with the detected edges. The pixel values in 
this image will be larger where the edges are present. Compared to other filters, it is characterized by omnidirectionality - it detects edges in all directions. 
In addition, it results in sharper edges.

# Program input parameters
UI Input parameters:
- Color or gray image
- Choice of DLL library written in c++ or assembler
- Number of threads (between 1-64)

Parameters passed to the DLL library:
- RCX - Input bitmap
- rsp+8*5] - Output bitmap
- R8 - Height of the bitmap
- RDX - Width of the bitmap
- R9 - Size of the pixel

# Fragments of assembler DLL library source code
The following code fragment performs operations on the xmm0 register, which is one of the XMM registers (128-bit). The first line loads data from memory at the 
address stored in the rbx register into xmm0. Then the next two lines double the value of xmm0, performing addition itself. The final effect is to quadruple the 
value of xmm0. This increase is due to the middle value of the LAP1 mask.
```
pmovzxbd xmm0, [rbx] 		; Load data from the memory address stored in rbx into xmm0
paddd xmm0, xmm0		; Add the contents of xmm0 to itself (xmm0 = xmm0 + xmm0)
paddd xmm0, xmm0		; Add the contents of xmm0 to itself (xmm0 = xmm0 + xmm0)
```

The following code snippet performs operations on the xmm0 and xmm1 registers in the context of image processing. The first line loads data from memory, where the 
address is calculated as the sum of the contents of the rbx and r9 registers (where I find the pixel size), and the result is loaded into xmm1. The second line subtracts 
the contents of xmm1 from xmm0, which corresponds to the operation associated with the right part of the mask.
```
pmovzxbd xmm1, [rbx + r9]    ; Load data from the memory address calculated as rbx + r9 into xmm1
psubd xmm0, xmm1             ; Subtract the contents of xmm1 from xmm0 (xmm0 = xmm0 - xmm1)
```

The following code snippet performs operations on the xmm0, xmm1 and xmm2 registers. The first line copies the contents of xmm0 to xmm2. Next, it performs an XOR operation 
on xmm1 with itself, resulting in setting all the bits of xmm1 to 0. The third line is an instruction to compare xmm2 with zero (xmm1), setting the bits in xmm2 according 
to the result of the comparison (whether the elements of xmm2 are greater than 0). Finally, the fourth line performs a logical AND operation between xmm0 and xmm2, resulting 
in the preservation of only those elements of xmm0 that were greater than 0.
```
movapd xmm2, xmm0      ; Copy the contents of xmm0 to xmm2   	
pxor xmm1, xmm1        ; Set all bits in xmm1 to 0 by performing a bitwise XOR with itself
pcmpgtd xmm2, xmm1     ; Compare the values in xmm2 with 0 and set the corresponding bits in xmm2 based on the result of the comparison (greater-than)
pand xmm0, xmm2        ; Perform a bitwise AND operation between xmm0 and xmm2, resulting in xmm0 having only those elements that were greater than 0
```

The following code fragment is used to write the contents of the xmm0 registers to memory. The movupd instruction copies the data to memory, or more precisely to an address 
calculated as the sum of the contents of the rdi and rax registers. This is the step of writing the pixels that have been processed by the edge detection algorithm to the output bitmap.
```
movupd [rdi + rax], xmm0   	; Store the contents of xmm0 into the memory address calculated as rdi + rax
```

# User interface
### User interface before image processing:

![](https://github.com/KsaweryZietara/EdgeDetection/blob/main/Resources/UIBeforeProcessing.png)

### User interface after image processing:
![](https://github.com/KsaweryZietara/EdgeDetection/blob/main/Resources/UIAfterProcessing.png)

# Performance report
![](https://github.com/KsaweryZietara/EdgeDetection/blob/main/Resources/PerformanceReport.png)

# Conclusions
The implemented algorithm using the Laplace filter LAPL1 effectively detects edges in an image, featuring omnidirectionality. Input parameters, such as the choice of color or 
gray image, DLL libraries in C++ or assembler, and the number of threads, allow you to customize the operation of the program to meet different needs. The C++ language stands out 
for its readability and conciseness of code, which makes it easier to maintain and develop the project, while assembler shows an advantage in execution time as can be seen in the 
chart. The vector instructions shown in the code snippets allow for more efficient processing of data sets, enabling a certain operation to be performed on several data at once. 
Despite the performance advantage of assembler, the choice between it and C++ depends on specific design priorities, where assembler may be preferred for performance reasons and C++ 
for rapid code development.

# Bibliography
- [Algorithms](http://www.algorytm.org/przetwarzanie-obrazow/filtrowanie-obrazow.html)
- [ZMITAC](http://db.zmitac.aei.polsl.pl/PIA/PiA%20Ex.9.pdf)
- [SSE](https://www.songho.ca/misc/sse/sse.html)
- [x86 and amd64 instruction reference](https://www.felixcloutier.com/x86)
- [Interstellar image](https://images.squarespace-cdn.com/content/v1/5a78ab8490badee028bef0e9/1568935524292-TPSLMXHD9HE6PKN02YOG/Interstellar.jpg)
