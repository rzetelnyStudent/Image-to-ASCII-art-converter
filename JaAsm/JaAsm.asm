.data
asciicodes db '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', ' '   
	

.code
convertLineAsm proc
;Arguments:
;RCX - lineNr, RDX - imageInBytesPtr, R8 - textLinePtr, R9 - ASCII_IM_W 

LOCAL startPos:QWORD, imageInBytesPtr:QWORD, textLinePtr:QWORD
LOCAL tempGrayScale[2]:oword	  ; temporary table for storing conversion results

mov imageInBytesPtr, RDX
mov textLinePtr, R8

mov RAX, RCX
mul R9  
mov RCX, RAX		    ; startPosAscii = lineNr * asciiImWidth (startPosAscii is a pointer in output char array
mov R11, 3
mul R11
mov startPos, RAX		; startPos = lineNr * asciiImWidth * 3  (startpos is a pointer for the image line to be converted)

xor R10, R10              ; R10 is loop counter
mov RAX, 3
mul R9
mov R9, RAX             ; 3*ascii_image_width (R9 is LOOP counter top)


startloop:	
	mov RBX, imageInBytesPtr        
	add RBX, startPos   
	add RBX, R10        ; RBX = imageInBytesPtr + startPos + R10(loop counter), RBX is a pointer to image in bytes
	xor R11, R11

	; populate xmm3. Note that SSE don't support div for integers. The override is to multiply with scale
	mov R15D, 10923		; 10923 is a scale for pmulhrsw (multiply xmm with scale). Equivalent to div /3
	pinsrw xmm3, R15D, 0     
	pinsrw xmm3, R15D, 1
	pinsrw xmm3, R15D, 2
	pinsrw xmm3, R15D, 3
	pinsrw xmm3, R15D, 4
	pinsrw xmm3, R15D, 5
	pinsrw xmm3, R15D, 6
	pinsrw xmm3, R15D, 7

	pxor xmm5, xmm5		; prepare regs
	pxor xmm6, xmm6		
	pxor xmm7, xmm7		
	pxor xmm8, xmm8		
	pxor xmm9, xmm9		
	pxor xmm10, xmm10		

	; load pixels to xmms. Note that my image is a byte array RGBRGBRGB....
	; I load bytes, but they will later be treated as words (that's why I index xmms 0,2,4...)
	pinsrb xmm5, byte ptr [RBX], 0     ; load 1. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 0     ; load 1. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 0     ; load 1. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 2     ; load 2. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 2     ; load 2. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 2     ; load 2. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 4     ; load 3. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 4     ; load 3. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 4     ; load 3. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 6     ; load 4. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 6     ; load 4. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 6     ; load 4. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 8     ; load 5. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 8     ; load 5. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 8     ; load 5. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 10     ; load 6. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 10     ; load 6. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 10     ; load 6. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 12     ; load 7. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 12     ; load 7. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 12     ; load 7. pixel B
	inc RBX

	pinsrb xmm5, byte ptr [RBX], 14    ; load 8. pixel R
	inc RBX
	pinsrb xmm7, byte ptr [RBX], 14    ; load 8. pixel G
	inc RBX
	pinsrb xmm9, byte ptr [RBX], 14     ; load 8. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 0     ; load 9. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 0     ; load 9. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 0     ; load 9. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 2     ; load 10. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 2     ; load 10. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 2     ; load 10. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 4     ; load 11. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 4     ; load 11. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 4     ; load 11. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 6     ; load 12. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 6     ; load 12. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 6     ; load 12. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 8     ; load 13. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 8     ; load 13. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 8     ; load 13. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 10     ; load 14. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 10     ; load 14. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 10     ; load 14. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 12     ; load 15. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 12     ; load 15. pixel G
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 12     ; load 15. pixel B
	inc RBX

	pinsrb xmm6, byte ptr [RBX], 14     ; load 16. pixel R
	inc RBX
	pinsrb xmm8, byte ptr [RBX], 14     ; load 16. pixel g
	inc RBX
	pinsrb xmm10, byte ptr [RBX], 14     ; load 16. pixel B
	inc RBX

	; Convert to gray scale:
	; R+G+B, sum all components
	paddw xmm5, xmm7
	paddw xmm9, xmm5
	paddw xmm8, xmm6
	paddw xmm10, xmm8

	; take average (equivalent to div by 3)
	pmulhrsw xmm9, xmm3
	pmulhrsw xmm10, xmm3

	; Store conversion results (xmm9 and xmm10) in tempGrayScale array.
	lea R11, tempGrayScale
	movdqu oword ptr [R11], xmm9
	mov RAX, 16
	add R11, RAX
	movdqu oword ptr [R11], xmm10

	; Now map gray scale results to ascii chars:
	; for (int i=0; i < 32; i+=2) {
	xor R13, R13     ; R13 is write loop counter
writeLoop:

	mov RAX, R13
	lea R12, tempGrayScale      
	add RAX, R12	   ; pointer to tempGrayScale = &tempGrayScale[0] + loop_counter
	xor R14, R14
	mov R14W, word ptr [RAX]     ; treat as words

	; convert gray scale to ascii chars array index
	mov RAX, 10			
	mul R14				
	xor RDX, RDX
	mov R14, 255	
	div R14				; index (RAX) = gray_scale * 10 / 255

	mov R14, RAX
	lea R11, asciicodes
	add R14, R11
	xor RAX, RAX
	mov AL, byte ptr [R14]		; R14 holds the index
	movzx R14, AL		; R14 holds ascii char code

	; write to output char array aproperiate ascii char (R14)
	xor RDX, RDX
	mov RAX, R10
	mov R11, 3
	div R11				; outer loop counter / 3
	add RAX, RCX		; RAX = outer loop counter / 3 + start_pos_ascii
	mov R12, 2
	mul R12				; * 2 because C# holds chars as uint16
	add RAX, textLinePtr    ; outer loop counter / 3 + start_pos_ascii + textLinePtr 
	add RAX, R13		; R13 - write_loop_counter
	mov word ptr [RAX], R14W     ; save ascii char to output array
	inc R13
	inc R13
	cmp R13, 32     
    jl writeLoop 


	mov R12, 48      ; every iteration converts 16 pixels to ascii. 16 pixels = 48 RGB bytes
	add R10, R12
	cmp R10, R9     ; R9 is loop counter top
    jl startloop     ; Loop while less

mov RAX, 0		; return 0 (trying unsuccessfully to remove null reference exception)
ret 0
convertLineAsm endp
end