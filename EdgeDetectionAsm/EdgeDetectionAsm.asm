.data
.code
edgeDetection proc
    ; rcx - Memory address of the 'values' array
    ; rdx - Width of the bitmap
    ; r8 - Height of the bitmap
    ; r9 - Pixel size
    ; [rsp+8*5] - Memory address of the 'convertedValues' array

    mov rsi, rcx ; Load the memory address of the 'values' array into a register RSI
    mov rdi, [rsp+8*5] ; Load the memory address of the 'convertedValues' array into a register RDI

    ; Calculate the length of the arrays (width * height * pixelSize)
    mov rcx, rdx       ; Load the memory address of the width into a register RCX
    imul rcx, r9       ; rcx = width * pixelSize
    mov r10, rcx       ; Load the stride into a register R10
    imul rcx, r8       ; rcx = width * pixelSize * height 

    mov rax, r10 ; Initialize a loop counter to stride
    sub rcx, r10 ; Subtract stride from array length

    ; Calculate i
    mov rbx, rsi    
    add rbx, rax

   ; Loop through the array to copy
    loopCopy:
        ; Check if we've reached the end of the array
        cmp rax, rcx
        jae doneCopy

        ; xmm0 += values[i] * 4
        pmovzxbd xmm0, [rbx]
        paddd xmm0, xmm0
        paddd xmm0, xmm0
        
        ; xmm0 -= values[i - stride]
        mov r11, rbx
        sub r11, r10
        pmovzxbd xmm1, [r11]
        psubd xmm0, xmm1

        ; xmm0 -= values[i - pixelSize]
        mov r11, rbx
        sub r11, r9
        pmovzxbd xmm1, [r11]
        psubd xmm0, xmm1

        ; xmm0 -= values[i + pixelSize]
        pmovzxbd xmm1, [rbx + r9]
        psubd xmm0, xmm1

        ; xmm0 -= values[i + stride]
        pmovzxbd xmm1, [rbx + r10]
        psubd xmm0, xmm1
        
        ; If value in xmm0 is negative, replace it with 0
        movapd xmm2, xmm0   ; Move xmm0 to xmm2
        pxor xmm1, xmm1     ; xmm1 = 0.0
        pcmpgtd xmm2, xmm1  ; Compare xmm1 with xmm0, xmm1 will have 0 for negative values
        pand xmm0, xmm2     ; Replace negative values with 0

        movupd [rdi + rax], xmm0

        ; Add 4 to loop counter
        add rax, 4
        add rbx, 4

        ; Repeat the loop
        jmp loopCopy
    doneCopy:
ret
edgeDetection endp
end
