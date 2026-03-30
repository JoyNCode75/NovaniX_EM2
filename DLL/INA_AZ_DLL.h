 #ifndef __INA_AZ_DLL_H__
#define __INA_AZ_DLL_H__

#ifdef INA_AZ_DLL_EXPORTS
#define DLLFunction __declspec(dllexport)
#else
#define DLLFunction __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif
	// -- COMMUNICATION
	DLLFunction		int			INA_AZ_INITIALIZE(int &nIndex, int nPort, int nBaudrate = 115200, int nDatabit = 8, int nParitybit = 2, int nStopbit = 0); //-- SERIAL INITIALIZE 함수
	DLLFunction		int			INA_AZ_UNINITIALIZE(int nIndex);											//-- SERIAL UNINITIALIZE 함수
	DLLFunction		int			INA_AZ_WRITE(int nIndex, char * pBuff, int nSize);							//-- SERIAL WRITE 함수
	DLLFunction		int			INA_AZ_READ(int nIndex, char * pBuff, unsigned long &ulSize);				//-- SERIAL READ 함수
	DLLFunction		int			INA_AZ_GET_CRC16(int nIndex, char *pBuff,int nSize, unsigned char &byCRC1 ,unsigned char &byCRC2); //-- CRC 16값을 얻기 위한 함수
	DLLFunction		int			INA_AZ_SET_DELAY(int nIndex, int nDelay);									// -- 내부 Delay default 35m/s  			

	//-- DRIVER INPUT CMD(0x007C/0x007D) 
	DLLFunction	  int			INA_AZ_SET_START(int nIndex, int nSlaveNo, int nDataNo);					// -- START- Toggle 방식
	DLLFunction     int			INA_AZ_SET_HOME(int nIndex, int nSlaveNo);									// -- HOME - Toggle 방식
	DLLFunction     int			INA_AZ_SET_STOP(int nIndex, int nSlaveNo);									// -- STOP - Toggle 방식
	DLLFunction     int			INA_AZ_SET_FREE(int nIndex, int nSlaveNo, int nOnOff);						// -- FREE - Motor On: 0, Off: 1
	DLLFunction     int			INA_AZ_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- FORWARD ON/OFF
	DLLFunction     int			INA_AZ_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- REVERSE ON/OFF
	DLLFunction     int			INA_AZ_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG+
	DLLFunction     int			INA_AZ_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG-
	DLLFunction     int			INA_AZ_SET_MS0(int nIndex, int nSlaveNo, int nOnOff);						// -- MS0
	DLLFunction     int			INA_AZ_SET_MS1(int nIndex, int nSlaveNo, int nOnOff);						// -- MS1
	DLLFunction     int			INA_AZ_SET_MS2(int nIndex, int nSlaveNo, int nOnOff);						// -- MS2	
	DLLFunction	  int			INA_AZ_SET_M0(int nIndex, int nSlaveNo, int nOnOff);												// -- M0
	DLLFunction	  int			INA_AZ_SET_M1(int nIndex, int nSlaveNo, int nOnOff);												// -- M1
	DLLFunction	  int			INA_AZ_SET_M2(int nIndex, int nSlaveNo, int nOnOff);												// -- M2	
	DLLFunction     int			INA_AZ_SET_CLEAR(int nIndex, int nSlaveNo);									// -- BIT CLEAR

	//-- MAINTENANCE COMMAND	 
	DLLFunction		int			INA_AZ_SET_ALARM_RESET(int nIndex, int nSlaveNo);							// -- 0180			
	DLLFunction		int			INA_AZ_SET_P_PRESET(int nIndex, int nSlaveNo);								// -- 008B		
	DLLFunction		int			INA_AZ_SET_NVMEMORY_WRITE(int nIndex, int nSlaveNo);						// -- 0192	
	
	//-- MONITORING COMMAND		
	DLLFunction		int			INA_AZ_GET_CURRENT_ALARM(int nIndex, int nSlaveNo);							// -- 0081
	DLLFunction		int			INA_AZ_GET_CURRENT_POSITION(int nIndex, int nSlaveNo);						// -- 00C6
	DLLFunction		int			INA_AZ_GET_CURRENT_VELOCITY(int nIndex, int nSlaveNo);						// -- 00C8		
	DLLFunction		int			INA_AZ_GET_CURRENT_VELOCITY_HZ(int nIndex, int nSlaveNo);					// -- 00CA		
	DLLFunction		int			INA_AZ_GET_CURRENT_FEEDBACK_POSITION(int nIndex, int nSlaveNo);				// -- 00CC	
	DLLFunction		int			INA_AZ_GET_CURRENT_FEEDBACK_VELOCITY(int nIndex, int nSlaveNo);				// -- 00CE	
	DLLFunction		int			INA_AZ_GET_CURRENT_FEEDBACK_VELOCITY_HZ(int nIndex, int nSlaveNo);			// -- 00D0		
	DLLFunction		int			INA_AZ_GET_CURRENT_TORQUE_MONITOR(int nIndex, int nSlaveNo);				// -- 00D6
	DLLFunction		int			INA_AZ_GET_CURRENT_DRIVER_TEMPERATURE(int nIndex, int nSlaveNo);//(1=0.1ºC)// -- 00F8
	DLLFunction		int			INA_AZ_GET_CURRENT_MOTOR_TEMPERATURE(int nIndex, int nSlaveNo);//(1=0.1ºC)	// -- 00FA
	DLLFunction		int			INA_AZ_GET_CURRENT_POWER_SUPPLY_VOLTAGE_DC(int nIndex, int nSlaveNo);//(1=0.1V)// -- 0148
	DLLFunction		int			INA_AZ_GET_CURRENT_ELASPED_TIME_FROM_BOOT(int nIndex, int nSlaveNo);		// -- 0152	
	DLLFunction		int			INA_AZ_GET_CURRENT_LOOP_COUNT(int nIndex, int nSlaveNo);//00E4

	//-- DIRECT DATA OPERTATION (제품 취급 설명서 260P 참조)
	DLLFunction		int			INA_AZ_SET_DIRECT_DATA_OPERATION(int nIndex, int nSlaveNo, int nDataNo, int nOpType, int nPos, int nSpd, int nAccRate, int nDecRate, int nCurrent, int nTrigger);	
	
	//-- GROUP TRANSFER
	DLLFunction		int			INA_AZ_SET_GROUP_TRANSFER(int nIndex, int nSlaveNo, int nValue);
	DLLFunction		int			INA_AZ_GET_GROUP_TRANSFER(int nIndex,int nSlaveNo);	
	
	////////////////////////////////////////////////////////////////////////////////////////////////////
	// -- Driver 입력 지령(0x007C/0x007D) 
	// ## Driver에 대한 입력 지령을 설정합니다.(Type: Read/Write방식)
	//---------------------------------------------------------------------------------------------------//
	// ## Bit15 : -	|| Bit14	: -	|| Bit13	: -	|| Bit12	: -	|| Bit11	: -	|| Bit10	: -	|| Bit9	: -	|| Bit8	: -	## //
	// ## Bit7	: -	|| Bit6		: -	|| Bit5		: -	|| Bit4		: -	|| Bit3		: -	|| Bit2		: -	|| Bit1	: -	|| Bit0	: -	## //
	//---------------------------------------------------------------------------------------------------//
	DLLFunction	    int			INA_AZ_SET_CURRENT_DRIVER_INPUT_HIGH(int nIndex, int nSlaveNo , int nBitNo, int nOnOff);// -- 007C	(nBitNo : 0 ~ 15 , nOnOff : 0(OFF) , 1(0N)) 
	DLLFunction	    int			INA_AZ_GET_CURRENT_DRIVER_INPUT_HIGH(int nIndex, int nSlaveNo);							// -- (return 값 BIT 연산 필요 , Sample 예제 참조)

	//-------------------------------------------------------------------------------------------------------------------------------------------------//
	// ## Bit15	: RVS	|| Bit14	: FWD	|| Bit13	: -JOG	|| Bit12	: +JOG	|| Bit11	: SSTART	|| Bit10	: MS2	|| Bit9	: MS1	|| Bit8	: MS0  ## //
	// ## Bit7	: 미사용|| Bit6		: FREE	|| Bit5		: STOP	|| Bit4		: HOME	|| Bit3		: START		|| Bit2	: M2		|| Bit1	: M1	|| Bit0	: M0 7 ## //
	//-------------------------------------------------------------------------------------------------------------------------------------------------//
	DLLFunction		int			INA_AZ_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo , int nBitNo, int nOnOff);	// -- 007D	(nBitNo : 0 ~ 15 , nOnOff : 0(OFF) , 1(0N)) 
	DLLFunction		int			INA_AZ_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);							// -- (return 값 BIT 연산 필요 , Sample 예제 참조)
	////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////
	// -- Driver 출력 지령(0x007E/0x007F) 
	// ## Driver의 출력상태를 가져옵니다.
	//------------------------------------------------------------------------------------------------//
	// ## Bit15	: -	|| Bit14	: -	|| Bit13	: -	|| Bit12	: -	|| Bit11	: -	|| Bit10	: -	|| Bit9	: -	|| Bit8	: -	## //
	// ## Bit7	: -	|| Bit6		: -	|| Bit5		: -	|| Bit4		: -	|| Bit3		: -	|| Bit2		: -	|| Bit1	: -	|| Bit0	: -	## //
	//------------------------------------------------------------------------------------------------//
	DLLFunction		int			INA_AZ_GET_CURRENT_DRIVER_OUTPUT_HIGH(int nIndex, int nSlaveNo);							// -- (return 값 BIT 연산 필요 , Sample 예제 참조)
	//------------------------------------------------------------------------------------------------------------------------------------------------------------------//
	// ## Bit15	: TLC	|| Bit14	: END	|| Bit13	: MOVE		|| Bit12	: TIM			|| Bit11	: AREA3		|| Bit10	: AREA2	|| Bit9	: AREA1	|| Bit8	: S-BSY	## //
	// ## Bit7	: ALM	|| Bit6		: WNG	|| Bit5		: READY		|| Bit4		: HOME-P		|| Bit3		: START-R	|| Bit2		: M2_R	|| Bit1	: M1_R	|| Bit0	: M0_R	## //
	//------------------------------------------------------------------------------------------------------------------------------------------------------------------//
	DLLFunction		int			INA_AZ_GET_CURRENT_DRIVER_OUTPUT_LOW(int nIndex, int nSlaveNo);								// -- (return 값 BIT 연산 필요 , Sample 예제 참조)
	////////////////////////////////////////////////////////////////////////////////////////////////////

	////////////////////////////////////////////////////////////////////////////////////////////////////
	// ## Direct I/O와 전자 Brake의 상태를 나타냅니다. 
	//--------------------------------------------------------------------------------------------------------------------------------------//
	// ## Bit15	: -	|| Bit14	: -	|| Bit13	: -			|| Bit12	: -		|| Bit11	: -			|| Bit10	: -			|| Bit9	: -			|| Bit8	: MB	## //
	// ## Bit7	: -	|| Bit6		: -	|| Bit5		: OUT5		|| Bit4		: OUT4	|| Bit3		: OUT3		|| Bit2		: OUT2		|| Bit1	: OUT1		|| Bit0	: OUT0	## //
	//----------------------------------------------------------------------------------------------------------------------------------//
	DLLFunction		int			INA_AZ_GET_CURRENT_DIRECT_IO_HIGH(int nIndex, int nSlaveNo);									// -- 00D4 (return 값 BIT 연산 필요 , Sample Code예제 참조)
	//--------------------------------------------------------------------------------------------------------------------------------------//
	//																			||--	하위	--||																						     
	// ## Bit15	: -	  || Bit14	: -		|| Bit13	: IN7	|| Bit12	: IN6	|| Bit11	: IN5		|| Bit10	: IN4			|| Bit9	: IN3	|| Bit8	: IN2	## //
	// ## Bit7	: IN1 || Bit6	: IN0	|| Bit5	: -			|| Bit4		: -		|| Bit3		: SLIT		|| Bit2		: HOMES			|| Bit1	: -LS	|| Bit0	: +LS	## //
	//-------------------------------------------------------------------------------------------------------------------------------------//
	DLLFunction		int			INA_AZ_GET_CURRENT_DIRECT_IO_LOW(int nIndex, int nSlaveNo);									// -- 00D4 (return 값 BIT 연산 필요 , Sample Code예제 참조)
	////////////////////////////////////////////////////////////////////////////////////////////////////

	//-- OPERATING DATA R/W COMMAND 
	DLLFunction		int			INA_AZ_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_LOW_IO_EVENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_LOW_IO_EVENT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZ_SET_DATA_HIGH_IO_EVENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZ_GET_DATA_HIGH_IO_EVENT(int nIndex, int nSlaveNo, int nDataNo);

	//-- ## Parameter Operation I/O event	
	DLLFunction		int			INA_AZ_SET_EVENT_LINK(int nIndex, int nSlaveNo, int nDataNo,int nValue);									
	DLLFunction		int			INA_AZ_GET_EVENT_LINK(int nIndex, int nSlaveNo, int nDataNo);								
			
	DLLFunction		int			INA_AZ_SET_EVENT_JUMP_DESTINATION(int nIndex, int nSlaveNo, int nDataNo,int nValue);						
	DLLFunction		int			INA_AZ_GET_EVENT_JUMP_DESTINATION(int nIndex, int nSlaveNo, int nDataNo);					
			
	DLLFunction		int			INA_AZ_SET_EVENT_WAITING_TIME(int nIndex, int nSlaveNo, int nDataNo,int nValue);							
	DLLFunction		int			INA_AZ_GET_EVENT_WAITING_TIME(int nIndex, int nSlaveNo, int nDataNo);						
			
	DLLFunction		int			INA_AZ_SET_EVENT_TRIGGER_IO(int nIndex, int nSlaveNo, int nDataNo,int nValue);							
	DLLFunction		int			INA_AZ_GET_EVENT_TRIGGER_IO(int nIndex, int nSlaveNo, int nDataNo);						
			
	DLLFunction		int			INA_AZ_SET_EVENT_TRIGGER_TYPE(int nIndex, int nSlaveNo, int nDataNo,int nValue);							
	DLLFunction		int			INA_AZ_GET_EVENT_TRIGGER_TYPE(int nIndex, int nSlaveNo, int nDataNo);						
			
	DLLFunction		int			INA_AZ_SET_EVENT_TRIGGER_COUNTER(int nIndex, int nSlaveNo, int nDataNo,int nValue);							
	DLLFunction		int			INA_AZ_GET_EVENT_TRIGGER_COUNTER(int nIndex, int nSlaveNo, int nDataNo);					
																			
	//-- ## Parameter Extended Operation Data Setting					
	DLLFunction		int			INA_AZ_SET_COMMON_ACCELERATION_RATE_OR_TIME(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_COMMON_ACCELERATION_RATE_OR_TIME(int nIndex, int nSlaveNo);				
			
	DLLFunction		int			INA_AZ_SET_COMMON_STOPPING_DECELERATION(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_COMMON_STOPPING_DECELERATION(int nIndex, int nSlaveNo);					
			
	DLLFunction		int			INA_AZ_SET_RATE_SELECTION(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_RATE_SELECTION(int nIndex, int nSlaveNo);									
			
	DLLFunction		int			INA_AZ_SET_REPEAT_START_OPERATION_DATA_NO(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_REPEAT_START_OPERATION_DATA_NO(int nIndex, int nSlaveNo);					
			
	DLLFunction		int			INA_AZ_SET_REPEAT_END_OPERATION_DATA_NO(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_REPEAT_END_OPERATION_DATA_NO(int nIndex, int nSlaveNo);					
			
	DLLFunction		int			INA_AZ_SET_REPEAT_TIME(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_REPEAT_TIME(int nIndex, int nSlaveNo);			

	//##Parameter Base Setting 
	DLLFunction		int			INA_AZ_SET_BASE_CURRENT(int nIndex, int nSlaveNo, int nValue);
	DLLFunction		int			INA_AZ_GET_BASE_CURRENT(int nIndex, int nSlaveNo);

	DLLFunction		int			INA_AZ_SET_BASE_CURRENT_SETTING_SOURCE(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_BASE_CURRENT_SETTING_SOURCE(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_STOP_CURRENT(int nIndex, int nSlaveNo, int nValue);															
	DLLFunction		int			INA_AZ_GET_STOP_CURRENT(int nIndex, int nSlaveNo);															

	DLLFunction		int			INA_AZ_SET_COMMAND_FILTER_SETTING(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_COMMAND_FILTER_SETTING(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_COMMAND_FILTER_TIME_CONSTANT(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_COMMAND_FILTER_TIME_CONSTANT(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_COMMAND_FILTER_SOURCE(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_COMMAND_FILTER_SOURCE(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_SMOOTH_DRIVE_FUNCTION(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_SMOOTH_DRIVE_FUNCTION(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_CURRENT_CONTROL_MODE(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_CURRENT_CONTROL_MODE(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_SERVO_EMULATION_RATIO(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_SERVO_EMULATION_RATIO(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_SVE_POSITION_LOOP_GAIN(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_SVE_POSITION_LOOP_GAIN(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_SVE_SPEED_LOOP_GAIN(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_SVE_SPEED_LOOP_GAIN(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_SVE_SPEED_LOOP_INTEGRAL_TIME_CONSTANT(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_SVE_SPEED_LOOP_INTEGRAL_TIME_CONSTANT(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_AUTOMATIC_CURRENT_CUTBACK_FUNCTION(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_AUTOMATIC_CURRENT_CUTBACK_FUNCTION(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_AUTOMATIC_CURRENT_CUTBACK_SWITCHING_TIME(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_AUTOMATIC_CURRENT_CUTBACK_SWITCHING_TIME(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_OPERATING_CURRENT_RAMP_UP_RATE(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_OPERATING_CURRENT_RAMP_UP_RATE(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_OPERATING_CURRENT_RAMP_DOWN_RATE(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_OPERATING_CURRENT_RAMP_DOWN_RATE(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_ELECTRONIC_DAMPER_FUNCTION(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_ELECTRONIC_DAMPER_FUNCTION(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_RESONANCE_SUPPRESSION_CONTROL_FREQUENCY(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_RESONANCE_SUPPRESSION_CONTROL_FREQUENCY(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_RESONANCE_SUPPRESSION_CONTROL_GAIN(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_RESONANCE_SUPPRESSION_CONTROL_GAIN(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_DEVIATION_ACCELERATION_SUPPRESSING_GAIN(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_DEVIATION_ACCELERATION_SUPPRESSING_GAIN(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_SOFTWARE_OVERTRAVEL(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_SOFTWARE_OVERTRAVEL(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_POSITIVE_SOFTWARE_LIMIT(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_POSITIVE_SOFTWARE_LIMIT(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_NEGATIVE_SOFTWARE_LIMIT(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_NEGATIVE_SOFTWARE_LIMIT(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_PRESET_POSITION(int nIndex, int nSlaveNo, int nValue);														
	DLLFunction		int			INA_AZ_GET_PRESET_POSITION(int nIndex, int nSlaveNo);														

	DLLFunction		int			INA_AZ_SET_STARTING_SPEED(int nIndex, int nSlaveNo, int nValue);															
	DLLFunction		int			INA_AZ_GET_STARTING_SPEED(int nIndex, int nSlaveNo);															

	DLLFunction		int			INA_AZ_SET_ACCELRATION_DECELERATION_UNIT(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_ACCELRATION_DECELERATION_UNIT(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_PERMISSION_OF_ABSOLUTE_POSITIONING_WITHOUT_SETTING_ABSOLUTE_COORDINATES(int nIndex, int nSlaveNo, int nValue);
	DLLFunction		int			INA_AZ_GET_PERMISSION_OF_ABSOLUTE_POSITIONING_WITHOUT_SETTING_ABSOLUTE_COORDINATES(int nIndex, int nSlaveNo);

	DLLFunction		int			INA_AZ_SET_DIRECT_DATA_OPERATION_ZERO_SPEED_COMMAND_ACTION(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_DIRECT_DATA_OPERATION_ZERO_SPEED_COMMAND_ACTION(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_DIRECT_DATA_OPERATION_TRIGGER_INITIAL_VALUE(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_DIRECT_DATA_OPERATION_TRIGGER_INITIAL_VALUE(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_DIRECT_DATA_OPERATION_DATA_DESTINATION_INITIAL_VALUE(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DIRECT_DATA_OPERATION_DATA_DESTINATION_INITIAL_VALUE(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_DIRECT_DATA_OPERATION_PARAMETER_INITIAL_VALUE_REFERENCE_NO(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_DIRECT_DATA_OPERATION_PARAMETER_INITIAL_VALUE_REFERENCE_NO(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_COMMAND_DATA_ACCESS_AREA(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_COMMAND_DATA_ACCESS_AREA(int nIndex, int nSlaveNo);		

	//## Parameter Motor & Mechanism  
	DLLFunction		int			INA_AZ_SET_MANUAL_SETTING_OF_THE_MECHANISM_SETTINGS(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_MANUAL_SETTING_OF_THE_MECHANISM_SETTINGS(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_MANUAL_SETTING_OF_GEAR_RATIO(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_MANUAL_SETTING_OF_GEAR_RATIO(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_INITIAL_COORDINATE_GENERATION_MANUAL_WRAP_SETTING(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INITIAL_COORDINATE_GENERATION_MANUAL_WRAP_SETTING(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_MECHANISM_LIMIT_PARAMETER_DISABLEMENT_SETTING(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_MECHANISM_LIMIT_PARAMETER_DISABLEMENT_SETTING(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_MECHANISM_PROTECTION_PARAMETER_DISABLEMENT_SETTING(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_MECHANISM_PROTECTION_PARAMETER_DISABLEMENT_SETTING(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_JOG_HOME_ZHOME_OPERATION_MANUAL_SETTING(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_JOG_HOME_ZHOME_OPERATION_MANUAL_SETTING(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_ELECTRONIC_GEAR_A(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_ELECTRONIC_GEAR_A(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_ELECTRONIC_GEAR_B(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_ELECTRONIC_GEAR_B(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_MOTOR_ROTATION_DIRECTION(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_MOTOR_ROTATION_DIRECTION(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_MECHANISM_TYPE(int nIndex, int nSlaveNo, int nValue);														
	DLLFunction		int			INA_AZ_GET_MECHANISM_TYPE(int nIndex, int nSlaveNo);														

	DLLFunction		int			INA_AZ_SET_MECHANICAL_LEAD(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_MECHANICAL_LEAD(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_WRAP_SETTING(int nIndex, int nSlaveNo, int nValue);														
	DLLFunction		int			INA_AZ_GET_WRAP_SETTING(int nIndex, int nSlaveNo);														

	DLLFunction		int			INA_AZ_SET_INITIAL_COORDINATE_GENERATION_WRAP_SETTING_RANGE(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INITIAL_COORDINATE_GENERATION_WRAP_SETTING_RANGE(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INITIAL_COORDINATE_GENERATION_WRAP_RANGE_OFFSET_RATIO(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_INITIAL_COORDINATE_GENERATION_WRAP_RANGE_OFFSET_RATIO(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_INITIAL_COORDINATE_GENERATION_WRAP_RANGE_OFFSET_VALUE(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_INITIAL_COORDINATE_GENERATION_WRAP_RANGE_OFFSET_VALUE(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_THE_NUMBER_OF_THE_RND_ZERO_OUTPUT_IN_WRAP_RANGE(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_THE_NUMBER_OF_THE_RND_ZERO_OUTPUT_IN_WRAP_RANGE(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_JOG_TRAVEL_AMOUNT(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_JOG_TRAVEL_AMOUNT(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_JOG_OPERATING_SPEED(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_JOG_OPERATING_SPEED(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_JOG_ACCELERATION_DECELERATION(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_JOG_ACCELERATION_DECELERATION(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_JOG_STARTING_SPEED(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_JOG_STARTING_SPEED(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_JOG_OPERATING_SPEED_HIGH(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_JOG_OPERATING_SPEED_HIGH(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_ZHOME_OPERATION_SPEED(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_ZHOME_OPERATION_SPEED(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_ZHOME_ACCELERATION_DECELERATION(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_ZHOME_ACCELERATION_DECELERATION(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_ZHOME_STARTING_SPEED(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_ZHOME_STARTING_SPEED(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_JOG_HOME_ZHOME_COMMAND_FILTER_TIME_CONSTANT(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_JOG_HOME_ZHOME_COMMAND_FILTER_TIME_CONSTANT(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_JOG_HOME_ZHOME_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_JOG_HOME_ZHOME_OPERATING_CURRENT(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_HOME_SEEKING_MODE(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_HOME_SEEKING_MODE(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_HOME_STARTING_DIRECTION(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_HOME_STARTING_DIRECTION(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_HOME_ACCELERATION_DECELERATION(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_HOME_ACCELERATION_DECELERATION(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_HOME_STARTING_SPEED(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_HOME_STARTING_SPEED(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_HOME_OPERATING_SPEED(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_HOME_OPERATING_SPEED(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_HOME_LAST_SPEED(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_HOME_LAST_SPEED(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_HOME_SLIT_DETECTION(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_HOME_SLIT_DETECTION(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_HOME_TIM_ZSG_SIGNAL_DETECTION(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_HOME_TIM_ZSG_SIGNAL_DETECTION(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_HOME_POSITION_OFFSET(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_HOME_POSITION_OFFSET(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_HOME_BACKWARD_STEPS_IN_2SENSOR_HOME_SEEKING(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_HOME_BACKWARD_STEPS_IN_2SENSOR_HOME_SEEKING(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_HOME_OPERATING_AMOUNT_IN_UNI_DIRECTIONAL_HOME_SEEKING(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_HOME_OPERATING_AMOUNT_IN_UNI_DIRECTIONAL_HOME_SEEKING(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_HOME_OPERATING_CURRENT_FOR_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_HOME_OPERATING_CURRENT_FOR_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_HOME_BACKWARD_STEPS_AFTER_FIRST_ENTRY_IN_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo, int nValue);	
	DLLFunction		int			INA_AZ_GET_HOME_BACKWARD_STEPS_AFTER_FIRST_ENTRY_IN_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo);	

	DLLFunction		int			INA_AZ_SET_HOME_PUSHING_TIME_IN_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_HOME_PUSHING_TIME_IN_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_HOME_BACKWARD_STEPS_IN_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo, int nValue);	
	DLLFunction		int			INA_AZ_GET_HOME_BACKWARD_STEPS_IN_PUSH_MOTION_HOME_SEEKING(int nIndex, int nSlaveNo);	
	
	//-- ## Parameter ETO & Alarm & Info
	DLLFunction		int			INA_AZ_SET_HWTO_MODE_SELECTION(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_HWTO_MODE_SELECTION(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_HWTO_DELEAY_TIME_OF_CHECKING_DUAL_SYSTEM(int nIndex, int nSlaveNo, int nValue);	
	DLLFunction		int			INA_AZ_GET_HWTO_DELEAY_TIME_OF_CHECKING_DUAL_SYSTEM(int nIndex, int nSlaveNo);	

	DLLFunction		int			INA_AZ_SET_ETO_RESET_INEFFECTIVE_PERIOD(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_ETO_RESET_INEFFECTIVE_PERIOD(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_ETO_RESET_ACTION_ETO_CLR(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_ETO_RESET_ACTION_ETO_CLR(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_ETO_RESET_ACTION_ALM_RST(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_ETO_RESET_ACTION_ALM_RST(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_ETO_RESET_ACTION_C_ON(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_ETO_RESET_ACTION_C_ON(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_ETO_RESET_ACTION_STOP(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_ETO_RESET_ACTION_STOP(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_OVERLOAD_ALARM(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_OVERLOAD_ALARM(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_EXCESSIVE_POSITION_DEVIATION_ALARM(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_EXCESSIVE_POSITION_DEVIATION_ALARM(int nIndex, int nSlaveNo);			

	DLLFunction		int			INA_AZ_SET_INFO_DRVTMP(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_DRVTMP(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_OLTIME(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_OLTIME(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_SPD(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_INFO_SPD(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_INFO_POSERR(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_POSERR(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_MRTMP(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_INFO_MRTMP(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_INFO_OVOLT_AC(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_OVOLT_AC(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_UVOLT_AC(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_UVOLT_AC(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_OVOLT_DC(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_OVOLT_DC(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_UVOLT_DC(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_INFO_UVOLT_DC(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_INFO_TRIP(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_INFO_TRIP(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_INFO_ODO(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_INFO_ODO(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_INFO_CULD0(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_INFO_CULD0(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_INFO_CULD1(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_INFO_CULD1(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_CULD_VALUE_AUTO_CLEAR(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_CULD_VALUE_AUTO_CLEAR(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_CULD_VALUE_COUNT_DIVISOR(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_CULD_VALUE_COUNT_DIVISOR(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_USRIO_OUTPUT_SELECTION(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_INFO_USRIO_OUTPUT_SELECTION(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_INFO_USRIO_OUTPUT_INVERSION(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_INFO_USRIO_OUTPUT_INVERSION(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_INFO_LED_DISPLAY(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_INFO_LED_DISPLAY(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_INFO_AUTO_CLEAR(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_INFO_AUTO_CLEAR(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_USRIO(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_USRIO(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_INFO_POSERR(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_INFO_POSERR(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_DRIVETMP(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_DRIVETMP(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_MTPTMP(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_MTPTMP(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_OVOLT(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_OVOLT(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_UVOLT(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_UVOLT(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_OLTIME(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_OLTIME(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_SPD(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_SPD(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_START(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_START(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_ZHOME(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_ZHOME(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_PR_REQ(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_PR_REQ(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_EGR_E(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_EGR_E(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_RND_E(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_RND_E(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_NET_E(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_NET_E(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_FW_OT(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_FW_OT(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_RV_OT(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_RV_OT(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_CULD0(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_CULD0(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_CULD1(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_CULD1(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_TRIP(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_TRIP(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_ODO(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_ODO(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_DSLMTD(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_DSLMTD(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_IOTEST(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_IOTEST(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_CFG(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_CFG(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_INFO_ACTION_INFO_RBT(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_INFO_ACTION_INFO_RBT(int nIndex, int nSlaveNo);		
	
	//-- ## Parameter I/O action
	DLLFunction		int			INA_AZ_SET_STOP_STOP_C_OFF_INPUT_ACTION(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_STOP_STOP_C_OFF_INPUT_ACTION(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_FW_LS_RV_LS_INPUT_ACTION(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_FW_LS_RV_LS_INPUT_ACTION(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_FW_BLK_RV_BLK_INPUT_ACTION(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_FW_BLK_RV_BLK_INPUT_ACTION(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_IN_POS_POSITIONING_COMPLETION_SIGNAL_RANGE(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_IN_POS_POSITIONING_COMPLETION_SIGNAL_RANGE(int nIndex, int nSlaveNo);			

	DLLFunction		int			INA_AZ_SET_IN_POS_POSITIONING_COMPLETION_SIGNAL_OFFSET(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_IN_POS_POSITIONING_COMPLETION_SIGNAL_OFFSET(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_D_SEL_DRIVE_START_FUNCTION(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_D_SEL_DRIVE_START_FUNCTION(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_TEACH_OPERATION_TYPE_SETTING(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_TEACH_OPERATION_TYPE_SETTING(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_ZSG_SIGNAL_WIDTH(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_ZSG_SIGNAL_WIDTH(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_RND_ZERO_SIGNAL_WIDTH(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_RND_ZERO_SIGNAL_WIDTH(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_RND_ZERO_SIGNAL_SOURCE(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_RND_ZERO_SIGNAL_SOURCE(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_MOVE_MINIMUM_ON_TIME(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_MOVE_MINIMUM_ON_TIME(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_PAUSE_STANDBY_CONDITION_SELECTION(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_PAUSE_STANDBY_CONDITION_SELECTION(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_PLS_XMODE_PULSE_MULTIPLYING_FACTOR(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_PLS_XMODE_PULSE_MULTIPLYING_FACTOR(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_CRNT_LMT_OPERATING_CURRENT_LIMIT_VALUE(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_CRNT_LMT_OPERATING_CURRENT_LIMIT_VALUE(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_SPD_LMT_SPEED_LIMIT_TYPE_SELECTION(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_SPD_LMT_SPEED_LIMIT_TYPE_SELECTION(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_SPD_LMT_SPEED_LIMIT_LATIO(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_SPD_LMT_SPEED_LIMIT_LATIO(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_SPD_LMT_SPEED_LIMIT_VALUE(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_SPD_LMT_SPEED_LIMIT_VALUE(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_JOG_C_TIME_FROM_JOG_P_TO_JOG(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_JOG_C_TIME_FROM_JOG_P_TO_JOG(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_JOG_C_TIME_FROM_JOG_TO_JOG_H(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_JOG_C_TIME_FROM_JOG_TO_JOG_H(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_PLS_LOST_CHECK_ALGORITHM(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_PLS_LOST_CHECK_ALGORITHM(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_MON_REQ0_OUTPUT_DATA_SELECTION(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_MON_REQ0_OUTPUT_DATA_SELECTION(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_MON_REQ1_OUTPUT_DATA_SELECTION(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_MON_REQ1_OUTPUT_DATA_SELECTION(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_PLS_OUT_DATA_SELECTION(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_PLS_OUT_DATA_SELECTION(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_PLS_OUT_MAXIMUM_FREQUENCY(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_PLS_OUT_MAXIMUM_FREQUENCY(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_VA_MODE_SELECTION(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_VA_MODE_SELECTION(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_VA_DETECTION_SPEED_RANGE(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_VA_DETECTION_SPEED_RANGE(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_MAREA_OUTPUT_SOURCE(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_MAREA_OUTPUT_SOURCE(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_AREA_POSITIVE_DIRECTION_POSITION_OFFSET(int nIndex, int nSlaveNo, int nNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_AREA_POSITIVE_DIRECTION_POSITION_OFFSET(int nIndex, int nSlaveNo, int nNo);			

	DLLFunction		int			INA_AZ_SET_AREA_NEGATIVE_DIRECTION_POSITION_DETECTION_RANGE(int nIndex, int nSlaveNo, int nNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_AREA_NEGATIVE_DIRECTION_POSITION_DETECTION_RANGE(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_AREA_RANGE_SETTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_AREA_RANGE_SETTING_MODE(int nIndex, int nSlaveNo, int nNo);							

	DLLFunction		int			INA_AZ_SET_AREA_POSITIONING_STANDARD(int nIndex, int nSlaveNo, int nNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_AREA_POSITIONING_STANDARD(int nIndex, int nSlaveNo, int nNo);								

	DLLFunction		int			INA_AZ_SET_D_SEL_OPERATION_NUMBER_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_D_SEL_OPERATION_NUMBER_SELECTION(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_D_END_OPERATION_NUMBER_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_D_END_OPERATION_NUMBER_SELECTION(int nIndex, int nSlaveNo, int nNo);	

	//-- ## Parameter Direct I/O setting
	//-- INPUT(NO. O ~ 9)
	DLLFunction		int			INA_AZ_SET_DIN_INPUT_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DIN_INPUT_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DIN_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DIN_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DIN_COMPOSITE_FUNCTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DIN_COMPOSITE_FUNCTION(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DIN_ON_SIGNAL_DEAD_TIME(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DIN_ON_SIGNAL_DEAD_TIME(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DIN_SHOT_SIGNAL(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DIN_SHOT_SIGNAL(int nIndex, int nSlaveNo, int nNo);	

	//-- OUTPUT(NO. O ~ 5)
	DLLFunction		int			INA_AZ_SET_DOUT_OUTPUT_FUNCTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DOUT_OUTPUT_FUNCTION(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DOUT_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DOUT_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DOUT_COMPOSITE_OUTPUT_FUNCTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DOUT_COMPOSITE_OUTPUT_FUNCTION(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DOUT_COMPOSITE_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DOUT_COMPOSITE_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo);	

	DLLFunction		int			INA_AZ_SET_DOUT_COMPOSITE_LOGICAL_COMBINATION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DOUT_COMPOSITE_LOGICAL_COMBINATION(int nIndex, int nSlaveNo, int nNo);	
	
	DLLFunction		int			INA_AZ_SET_DOUT_OFF_DELAY_TIME(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_DOUT_OFF_DELAY_TIME(int nIndex, int nSlaveNo, int nNo);

	//-- ## Parameter Remote-I/O function(NO. O ~ 15)
	DLLFunction		int			INA_AZ_SET_NET_IN_INPUT_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_NET_IN_INPUT_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo);				

	DLLFunction		int			INA_AZ_SET_NET_OUT_OUTPUT_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_NET_OUT_OUTPUT_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo);					

	DLLFunction		int			INA_AZ_SET_NET_IN_GROUP_ACTION_MODE_INITIAL_STATE(int nIndex, int nSlaveNo, int nNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_NET_IN_GROUP_ACTION_MODE_INITIAL_STATE(int nIndex, int nSlaveNo, int nNo);		

	DLLFunction		int			INA_AZ_SET_NET_OUT_OFF_DELAY_TIME(int nIndex, int nSlaveNo, int nNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_NET_OUT_OFF_DELAY_TIME(int nIndex, int nSlaveNo, int nNo);	

	//-- ## Parameter EXT-IN & VIR-IN & USR-OUT & I/F function
	DLLFunction		int			INA_AZ_SET_EXT_IN_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_EXT_IN_FUNCTION_SELECTION(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_EXT_IN_INVERTING_MODE(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_EXT_IN_INVERTING_MODE(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_EXT_IN_INTERLOCK_RELEASING_TIME(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_EXT_IN_INTERLOCK_RELEASING_TIME(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_EXT_IN_INTERLOCK_RELEASING_DURATION(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_EXT_IN_INTERLOCK_RELEASING_DURATION(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_EXT_IN_ON_MONITOR_TIME(int nIndex, int nSlaveNo, int nValue);										
	DLLFunction		int			INA_AZ_GET_EXT_IN_ON_MONITOR_TIME(int nIndex, int nSlaveNo);										

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_MODE_SELECTION(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_MODE_SELECTION(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_EXT_OUTA_FUNCTION_SELECTION_ON_IO_MODE(int nIndex, int nSlaveNo, int nValue);	
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_EXT_OUTA_FUNCTION_SELECTION_ON_IO_MODE(int nIndex, int nSlaveNo);	

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_EXT_OUTB_FUNCTION_SELECTION_ON_IO_MODE(int nIndex, int nSlaveNo, int nValue);	
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_EXT_OUTB_FUNCTION_SELECTION_ON_IO_MODE(int nIndex, int nSlaveNo);	

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_EXT_OUTA_INVERTING_MODE_ON_IO_MODE(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_EXT_OUTA_INVERTING_MODE_ON_IO_MODE(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_EXT_OUTB_INVERTING_MODE_ON_IO_MODE(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_EXT_OUTB_INVERTING_MODE_ON_IO_MODE(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_EXT_OUTA_OFF_DELAY_TIME_ON_IO_MODE(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_EXT_OUTA_OFF_DELAY_TIME_ON_IO_MODE(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_EXT_DIFFERENTIAL_EXT_OUTB_OFF_DELAY_TIME_ON_IO_MODE(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_EXT_DIFFERENTIAL_EXT_OUTB_OFF_DELAY_TIME_ON_IO_MODE(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_EXT_VIR_IN_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_EXT_VIR_IN_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo);								

	DLLFunction		int			INA_AZ_SET_EXT_VIR_IN_SOURCE_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_EXT_VIR_IN_SOURCE_SELECTION(int nIndex, int nSlaveNo, int nNo);		

	DLLFunction		int			INA_AZ_SET_EXT_VIR_IN_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_EXT_VIR_IN_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo);									

	DLLFunction		int			INA_AZ_SET_EXT_VIR_IN_ON_SIGNAL_DEAD_TIME(int nIndex, int nSlaveNo, int nNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_EXT_VIR_IN_ON_SIGNAL_DEAD_TIME(int nIndex, int nSlaveNo, int nNo);							

	DLLFunction		int			INA_AZ_SET_EXT_VIR_IN_1_SHOT_SIGNAL_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_EXT_VIR_IN_1_SHOT_SIGNAL_MODE(int nIndex, int nSlaveNo, int nNo);								

	DLLFunction		int			INA_AZ_SET_EXT_USER_OUT_SOURCE_A_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_EXT_USER_OUT_SOURCE_A_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nValue);				

	DLLFunction		int			INA_AZ_SET_EXT_USER_OUT_SOURCE_A_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_EXT_USER_OUT_SOURCE_A_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo);						

	DLLFunction		int			INA_AZ_SET_EXT_USER_OUT_SOURCE_B_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_EXT_USER_OUT_SOURCE_B_FUNCTION_SELECTION(int nIndex, int nSlaveNo, int nNo);					

	DLLFunction		int			INA_AZ_SET_EXT_USER_OUT_SOURCE_B_INVERTING_MODE(int nIndex, int nSlaveNo, int nNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_EXT_USER_OUT_SOURCE_B_INVERTING_MODE(int nIndex, int nSlaveNo, int nValue);						

	DLLFunction		int			INA_AZ_SET_EXT_USER_OUT_LOGICAL_OPERATION(int nIndex, int nSlaveNo, int nNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_EXT_USER_OUT_LOGICAL_OPERATION(int nIndex, int nSlaveNo, int nNo);		

	DLLFunction		int			INA_AZ_SET_EXT_PULSE_IF_MODE_SELECTION(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_EXT_PULSE_IF_MODE_SELECTION(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_EXT_RS485_IF_MODE_SELECTION(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_EXT_RS485_IF_MODE_SELECTION(int nIndex, int nSlaveNo);								

	DLLFunction		int			INA_AZ_SET_EXT_USB_ID_ENABLE(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_EXT_USB_ID_ENABLE(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_EXT_USB_ID(int nIndex, int nSlaveNo, int nValue);													
	DLLFunction		int			INA_AZ_GET_EXT_USB_ID(int nIndex, int nSlaveNo);													

	DLLFunction		int			INA_AZ_SET_EXT_USB_PID(int nIndex, int nSlaveNo, int nValue);												
	DLLFunction		int			INA_AZ_GET_EXT_USB_PID(int nIndex, int nSlaveNo);												

	DLLFunction		int			INA_AZ_SET_EXT_LED_OUT_MODE(int nIndex, int nSlaveNo, int nValue);											
	DLLFunction		int			INA_AZ_GET_EXT_LED_OUT_MODE(int nIndex, int nSlaveNo);											

	DLLFunction		int			INA_AZ_SET_EXT_LED_OUT_GREEN_FUNCTION(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_EXT_LED_OUT_GREEN_FUNCTION(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_EXT_LED_OUT_GREEN_INVERTING_MODE(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_EXT_LED_OUT_GREEN_INVERTING_MODE(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_EXT_LED_OUT_RED_FUNCTION(int nIndex, int nSlaveNo, int nValue);									
	DLLFunction		int			INA_AZ_GET_EXT_LED_OUT_RED_FUNCTION(int nIndex, int nSlaveNo);									

	DLLFunction		int			INA_AZ_SET_EXT_LED_OUT_RED_INVERTING_MODE(int nIndex, int nSlaveNo, int nValue);								
	DLLFunction		int			INA_AZ_GET_EXT_LED_OUT_RED_INVERTING_MODE(int nIndex, int nSlaveNo);								

	//-- ## Parameter Communication
	DLLFunction		int			INA_AZ_SET_MODBUS_SLAVE_ADDRESS(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_MODBUS_SLAVE_ADDRESS(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_MODBUS_BAUDRATE(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_MODBUS_BAUDRATE(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_MODBUS_BYTE_WORD_ORDER(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_MODBUS_BYTE_WORD_ORDER(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_MODBUS_COMMUNICATION_PARTITY(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_MODBUS_COMMUNICATION_PARTITY(int nIndex, int nSlaveNo);			

	DLLFunction		int			INA_AZ_SET_MODBUS_COMMUNICATION_STOP_BIT(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_MODBUS_COMMUNICATION_STOP_BIT(int nIndex, int nSlaveNo);			

	DLLFunction		int			INA_AZ_SET_MODBUS_COMMUNICATION_TIME_OUT(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_MODBUS_COMMUNICATION_TIME_OUT(int nIndex, int nSlaveNo);			

	DLLFunction		int			INA_AZ_SET_MODBUS_COMMUNICATION_ERROR_DETECTION(int nIndex, int nSlaveNo, int nValue);	
	DLLFunction		int			INA_AZ_GET_MODBUS_COMMUNICATION_ERROR_DETECTION(int nIndex, int nSlaveNo);	

	DLLFunction		int			INA_AZ_SET_MODBUS_TRANSMISSION_WAITING_TIME(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_MODBUS_TRANSMISSION_WAITING_TIME(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_MODBUS_SILENT_INTERVAL(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_MODBUS_SILENT_INTERVAL(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_MODBUS_SLAVE_ERROR_RESPONSE_MODE(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_MODBUS_SLAVE_ERROR_RESPONSE_MODE(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_MODBUS_INITIAL_GROUP_ID(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_MODBUS_INITIAL_GROUP_ID(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_MODBUS_TEST_MODE_TIMEOUT(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_MODBUS_TEST_MODE_TIMEOUT(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_NETC_GWV2_SLAVE_ID(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_NETC_GWV2_SLAVE_ID(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_NETC_INITIAL_GROUP_ID(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_NETC_INITIAL_GROUP_ID(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_GWV2_BAUDRATE(int nIndex, int nSlaveNo, int nValue);							
	DLLFunction		int			INA_AZ_GET_GWV2_BAUDRATE(int nIndex, int nSlaveNo);							

	DLLFunction		int			INA_AZ_SET_GWV2_FRAME_TIME(int nIndex, int nSlaveNo, int nValue);						
	DLLFunction		int			INA_AZ_GET_GWV2_FRAME_TIME(int nIndex, int nSlaveNo);						

	DLLFunction		int			INA_AZ_SET_GWV2_COMMUNICATION_CONNECTION_TIME(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_GWV2_COMMUNICATION_CONNECTION_TIME(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_GWV2_COMMUNICATION_TIMEOUT(int nIndex, int nSlaveNo, int nValue);				
	DLLFunction		int			INA_AZ_GET_GWV2_COMMUNICATION_TIMEOUT(int nIndex, int nSlaveNo);				

	DLLFunction		int			INA_AZ_SET_GWV2_COMMUNICATION_ERROR_DETECTION(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_GWV2_COMMUNICATION_ERROR_DETECTION(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_GWV2_TRANSMISSTION_WAITING_TIME(int nIndex, int nSlaveNo, int nValue);		
	DLLFunction		int			INA_AZ_GET_GWV2_TRANSMISSTION_WAITING_TIME(int nIndex, int nSlaveNo);		

	DLLFunction		int			INA_AZ_SET_GWV2_CONNECTION_CHECK(int nIndex, int nSlaveNo, int nValue);					
	DLLFunction		int			INA_AZ_GET_GWV2_CONNECTION_CHECK(int nIndex, int nSlaveNo);					

	DLLFunction		int			INA_AZ_SET_RS485_RECEIVE_PACKET_MONITOR(int nIndex, int nSlaveNo, int nValue);			
	DLLFunction		int			INA_AZ_GET_RS485_RECEIVE_PACKET_MONITOR(int nIndex, int nSlaveNo);	
}
#endif // __INA_AZ_DLL_H__