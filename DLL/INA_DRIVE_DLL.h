 #ifndef __INA_DRIVE_DLL_H__
#define __INA_DRIVE_DLL_H__

#ifdef INA_DRIVE_DLL_EXPORTS
#define DLLFunction __declspec(dllexport)
#else
#define DLLFunction __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C" {
#endif
	// -- COMMUNICATION, 드라이버에 상관 없는 함수
	DLLFunction		int			INA_DRIVE_INITIALIZE(int &nIndex, int nPort, int nBaudrate = 115200, int nDatabit = 8, int nParitybit = 2, int nStopbit = 0); //-- SERIAL INITIALIZE 함수
	DLLFunction		int			INA_DRIVE_UNINITIALIZE(int nIndex);											//-- SERIAL UNINITIALIZE 함수
	DLLFunction		int			INA_DRIVE_WRITE(int nIndex, char * pBuff, int nSize);							//-- SERIAL WRITE 함수
	DLLFunction		int			INA_DRIVE_READ(int nIndex, char * pBuff, unsigned long &ulSize);				//-- SERIAL READ 함수
	DLLFunction		int			INA_DRIVE_GET_CRC16(int nIndex, char *pBuff,int nSize, unsigned char &byCRC1 ,unsigned char &byCRC2); //-- CRC 16값을 얻기 위한 함수
	DLLFunction		int			INA_DRIVE_SET_DELAY(int nIndex, int nDelay);									// -- 내부 Delay default 20m/s  			

	//-- DRIVER INPUT CMD for AZ
	DLLFunction	  int			INA_AZ_SET_START(int nIndex, int nSlaveNo, int nDataNo);					// -- START- Toggle 방식
	DLLFunction     int			INA_AZ_SET_ZHOME(int nIndex, int nSlaveNo);									// -- ZHOME 
	DLLFunction     int			INA_AZ_SET_STOP(int nIndex, int nSlaveNo);									// -- STOP - Toggle 방식
	DLLFunction     int			INA_AZ_SET_FREE(int nIndex, int nSlaveNo, int nOnOff);						// -- FREE - Motor On: 0, Off: 1
	DLLFunction     int			INA_AZ_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- FORWARD ON/OFF
	DLLFunction     int			INA_AZ_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- REVERSE ON/OFF
	DLLFunction     int			INA_AZ_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG+
	DLLFunction     int			INA_AZ_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG-
	DLLFunction	  int			INA_AZ_SET_M0(int nIndex, int nSlaveNo, int nOnOff);												// -- M0
	DLLFunction	  int			INA_AZ_SET_M1(int nIndex, int nSlaveNo, int nOnOff);												// -- M1
	DLLFunction	  int			INA_AZ_SET_M2(int nIndex, int nSlaveNo, int nOnOff);												// -- M2	
	DLLFunction     int			INA_AZ_SET_CLEAR(int nIndex, int nSlaveNo);									// -- BIT CLEAR
	//-- DRIVER INPUT CMD for AZ mini
	DLLFunction	  int			INA_AZM_SET_START(int nIndex, int nSlaveNo, int nDataNo);					// -- START- Toggle 방식
	DLLFunction     int			INA_AZM_SET_ZHOME(int nIndex, int nSlaveNo);									// -- ZHOME 
	DLLFunction     int			INA_AZM_SET_STOP(int nIndex, int nSlaveNo);									// -- STOP - Toggle 방식
	DLLFunction     int			INA_AZM_SET_FREE(int nIndex, int nSlaveNo, int nOnOff);						// -- FREE - Motor On: 0, Off: 1
	DLLFunction     int			INA_AZM_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- FORWARD ON/OFF
	DLLFunction     int			INA_AZM_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- REVERSE ON/OFF
	DLLFunction     int			INA_AZM_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG+
	DLLFunction     int			INA_AZM_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG-
	DLLFunction	  int			INA_AZM_SET_M0(int nIndex, int nSlaveNo, int nOnOff);												// -- M0
	DLLFunction	  int			INA_AZM_SET_M1(int nIndex, int nSlaveNo, int nOnOff);												// -- M1
	DLLFunction	  int			INA_AZM_SET_M2(int nIndex, int nSlaveNo, int nOnOff);												// -- M2	
	DLLFunction     int			INA_AZM_SET_CLEAR(int nIndex, int nSlaveNo);									// -- BIT CLEAR
	//-- DRIVER INPUT CMD for CVD
	DLLFunction	  int			INA_CVD_SET_START(int nIndex, int nSlaveNo, int nDataNo);					// -- START- Toggle 방식
	DLLFunction     int			INA_CVD_SET_HOME(int nIndex, int nSlaveNo);									// -- HOME 
	DLLFunction     int			INA_CVD_SET_STOP(int nIndex, int nSlaveNo);									// -- STOP - Toggle 방식
	DLLFunction     int			INA_CVD_SET_AWO(int nIndex, int nSlaveNo, int nOnOff);						// -- AWO - Motor On: 0, Off: 1
	DLLFunction     int			INA_CVD_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- FORWARD ON/OFF
	DLLFunction     int			INA_CVD_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- REVERSE ON/OFF
	DLLFunction     int			INA_CVD_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG+
	DLLFunction     int			INA_CVD_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG-
	DLLFunction	  int			INA_CVD_SET_M0(int nIndex, int nSlaveNo, int nOnOff);												// -- M0
	DLLFunction	  int			INA_CVD_SET_M1(int nIndex, int nSlaveNo, int nOnOff);												// -- M1
	DLLFunction	  int			INA_CVD_SET_M2(int nIndex, int nSlaveNo, int nOnOff);												// -- M2	
	DLLFunction     int			INA_CVD_SET_CLEAR(int nIndex, int nSlaveNo);								// -- BIT CLEAR
	//-- DRIVER INPUT CMD for CVF
	DLLFunction	  int			INA_CVF_SET_START(int nIndex, int nSlaveNo, int nDataNo);					// -- START- Toggle 방식
	DLLFunction     int			INA_CVF_SET_HOME(int nIndex, int nSlaveNo);									// -- HOME 
	DLLFunction     int			INA_CVF_SET_STOP(int nIndex, int nSlaveNo);									// -- STOP - Toggle 방식
	DLLFunction     int			INA_CVF_SET_AWO(int nIndex, int nSlaveNo, int nOnOff);						// -- AWO - Motor On: 0, Off: 1
	DLLFunction     int			INA_CVF_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- FORWARD ON/OFF
	DLLFunction     int			INA_CVF_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- REVERSE ON/OFF
	DLLFunction     int			INA_CVF_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG+
	DLLFunction     int			INA_CVF_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG-
	DLLFunction	  int			INA_CVF_SET_M0(int nIndex, int nSlaveNo, int nOnOff);												// -- M0
	DLLFunction	  int			INA_CVF_SET_M1(int nIndex, int nSlaveNo, int nOnOff);												// -- M1
	DLLFunction	  int			INA_CVF_SET_M2(int nIndex, int nSlaveNo, int nOnOff);												// -- M2	
	DLLFunction     int			INA_CVF_SET_CLEAR(int nIndex, int nSlaveNo);								// -- BIT CLEAR
	//-- DRIVER INPUT COMMAND for RK2
	DLLFunction		int		INA_RK_SET_START(int nIndex, int nSlaveNo, int nDataNo);				// -- START - Toggle 방식
	DLLFunction		int		INA_RK_SET_HOME(int nIndex, int nSlaveNo);								// -- HOME - Toggle 방식
	DLLFunction		int		INA_RK_SET_STOP(int nIndex, int nSlaveNo);								// -- STOP - Toggle 방식
	DLLFunction		int		INA_RK_SET_FREE(int nIndex, int nSlaveNo, int nOnOff);					// -- FREE - Motor On: 0, Off: 1
	DLLFunction		int		INA_RK_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);		// -- FORWARD ON/OFF
	DLLFunction		int		INA_RK_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);		// -- REVERSE ON/OFF
	DLLFunction		int		INA_RK_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);					// -- JOG+
	DLLFunction		int		INA_RK_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);					// -- JOG-
	DLLFunction		int		INA_RK_SET_M0(int nIndex, int nSlaveNo, int nOnOff);					// -- M0
	DLLFunction		int		INA_RK_SET_M1(int nIndex, int nSlaveNo, int nOnOff);					// -- M1
	DLLFunction		int		INA_RK_SET_M2(int nIndex, int nSlaveNo, int nOnOff);					// -- M2
	DLLFunction		int		INA_RK_SET_CLEAR(int nIndex, int nSlaveNo);								// -- BIT CLEAR
	//-- DRIVER INPUT COMMAND for CRD
	DLLFunction		int			INA_CRK_SET_START(int nIndex, int nSlaveNo, int nDataNo);				// -- START - Toggle 방식 + C-ON 유지
	DLLFunction		int			INA_CRK_SET_HOME(int nIndex, int nSlaveNo);								// -- HOME - Toggle 방식 + C-ON 유지
	DLLFunction		int			INA_CRK_SET_STOP(int nIndex, int nSlaveNo);								// -- STOP - Toggle 방식 + C-ON 유지
	DLLFunction		int			INA_CRK_SET_MOTOR_C_ONOFF(int nIndex, int nSlaveNo, int nOnOff);		// -- MOTOR - C-On: 0, C-Off: 1
	DLLFunction		int			INA_CRK_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);		// -- FORWARD ON/OFF + C-ON 유지
	DLLFunction		int			INA_CRK_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);		// -- REVERSE ON/OFF + C-ON 유지
	DLLFunction		int			INA_CRK_SET_CLEAR(int nIndex, int nSlaveNo);							// -- 0x001E
	//-- DRIVER INPUT CMD for AR
	DLLFunction	  int			INA_AR_SET_START(int nIndex, int nSlaveNo, int nDataNo);					// -- START- Toggle 방식
	DLLFunction     int			INA_AR_SET_HOME(int nIndex, int nSlaveNo);									// -- HOME 
	DLLFunction     int			INA_AR_SET_STOP(int nIndex, int nSlaveNo);									// -- STOP - Toggle 방식
	DLLFunction     int			INA_AR_SET_FREE(int nIndex, int nSlaveNo, int nOnOff);						// -- FREE - Motor On: 0, Off: 1
	DLLFunction     int			INA_AR_SET_FWD(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- FORWARD ON/OFF
	DLLFunction     int			INA_AR_SET_RVS(int nIndex, int nSlaveNo, int nDataNo, int nOnOff);			// -- REVERSE ON/OFF
	DLLFunction     int			INA_AR_SET_JOG_P(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG+
	DLLFunction     int			INA_AR_SET_JOG_N(int nIndex, int nSlaveNo, int nOnOff);						// -- JOG-
	DLLFunction	  int			INA_AR_SET_M0(int nIndex, int nSlaveNo, int nOnOff);												// -- M0
	DLLFunction	  int			INA_AR_SET_M1(int nIndex, int nSlaveNo, int nOnOff);												// -- M1
	DLLFunction	  int			INA_AR_SET_M2(int nIndex, int nSlaveNo, int nOnOff);												// -- M2	
	DLLFunction     int			INA_AR_SET_CLEAR(int nIndex, int nSlaveNo);									// -- BIT CLEAR



	
	// -- 레지스터 어드레스 ( 10진수 값 )을 넣어 원하는 쿼리 송신, 상위와 하위가 같이 보내지므로 상위 어드레스를 기입하셔야 합니다.
	DLLFunction		int			INA_DRIVE_SET(int nIndex, int nSlaveNo, int nID, int nValue );
	DLLFunction		int			INA_DRIVE_GET(int nIndex, int nSlaveNo, int nID);

	DLLFunction		int			INA_DRIVE_SET_LOW(int nIndex, int nSlaveNo, int nID, int nValue);
	DLLFunction		int			INA_DRIVE_GET_LOW(int nIndex, int nSlaveNo, int nID);

	// -IO 읽기 쓰기
	DLLFunction		int			INA_AZ_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo, int nBitNo, int nOnOff);
	DLLFunction		int			INA_AZ_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);
	DLLFunction		int			INA_AZM_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo, int nBitNo, int nOnOff);
	DLLFunction		int			INA_AZM_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);
	DLLFunction		int			INA_CVD_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo, int nBitNo, int nOnOff);
	DLLFunction		int			INA_CVD_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);
	DLLFunction		int			INA_CVF_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo, int nBitNo, int nOnOff);
	DLLFunction		int			INA_CVF_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);
	DLLFunction		int			INA_RK_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo, int nBitNo, int nOnOff);
	DLLFunction		int			INA_RK_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);
	DLLFunction		int			INA_AR_SET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo, int nBitNo, int nOnOff);
	DLLFunction		int			INA_AR_GET_CURRENT_DRIVER_INPUT_LOW(int nIndex, int nSlaveNo);
	

	//-- DIRECT DATA OPERTATION 
	DLLFunction		int			INA_AZ_SET_DIRECT_DATA_OPERATION(int nIndex, int nSlaveNo, int nDataNo, int nOpType, int nPos, int nSpd, int nAccRate, int nDecRate, int nCurrent, int nTrigger);	
	DLLFunction		int			INA_AZM_SET_DIRECT_DATA_OPERATION(int nIndex, int nSlaveNo, int nDataNo, int nOpType, int nPos, int nSpd, int nAccRate, int nDecRate, int nCurrent, int nTrigger);
	DLLFunction		int			INA_CVD_SET_DIRECT_DATA_OPERATION(int nIndex, int nSlaveNo, int nDataNo, int nOpType, int nPos, int nSpd, int nAccRate, int nDecRate, int nCurrent, int nTrigger);
	DLLFunction		int			INA_CVF_SET_DIRECT_DATA_OPERATION(int nIndex, int nSlaveNo, int nDataNo, int nOpType, int nPos, int nSpd, int nAccRate, int nDecRate, int nCurrent, int nTrigger);

	//-- OPERATING DATA R/W COMMAND for AZ
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

	//-- OPERATING DATA R/W COMMAND for AZM
	DLLFunction		int			INA_AZM_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_AZM_SET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_AZM_GET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo);


	//-- OPERATING DATA R/W COMMAND for CVD
	DLLFunction		int			INA_CVD_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVD_SET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVD_GET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo);


	//-- OPERATING DATA R/W COMMAND for CVF
	DLLFunction		int			INA_CVF_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_ACC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_DEC_RATE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_OPERATING_CURRENT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_DELAY_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_LINK(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_NEXT_DATA_NO(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_AREA_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_AREA_WIDTH(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_LOOP_COUNT(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_LOOP_OFFSET(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CVF_SET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CVF_GET_DATA_LOOP_END(int nIndex, int nSlaveNo, int nDataNo);

	//-- OPERATING DATA R/W COMMAND for RK
	DLLFunction		int		INA_RK_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_OPERATING_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_OPERATING_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_ACC_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_ACC_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_DEC_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_DEC_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_SEQUENTIAL_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_SEQUENTIAL_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_RK_SET_DATA_DWELL_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_RK_GET_DATA_DWELL_TIME(int nIndex, int nSlaveNo, int nDataNo);

	//-- OPERATING DATA R/W COMMAND for CRK
	DLLFunction		int			INA_CRK_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_OPERATING_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_OPERATING_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_SEQUENTIAL_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_SEQUENTIAL_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_ACC_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_ACC_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_DEC_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_DEC_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int			INA_CRK_SET_DATA_DWELL_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int			INA_CRK_GET_DATA_DWELL_TIME(int nIndex, int nSlaveNo, int nDataNo);

	//-- OPERATING DATA R/W COMMAND for AR
	DLLFunction		int		INA_AR_SET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_VELOCITY(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_POSITION_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_OPERATING_MODE(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_OPERATING_MODE(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_ACC_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_ACC_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_DEC_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_DEC_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_SEQUENTIAL_POSITION(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_SEQUENTIAL_POSITION(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_DWELL_TIME(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_DWELL_TIME(int nIndex, int nSlaveNo, int nDataNo);

	DLLFunction		int		INA_AR_SET_DATA_PUSH_CURRENT(int nIndex, int nSlaveNo, int nDataNo, int nValue);
	DLLFunction		int		INA_AR_GET_DATA_PUSH_CURRENT(int nIndex, int nSlaveNo, int nDataNo);
	
}
#endif // __INA_DRIVE_DLL_H__