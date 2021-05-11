// Example_ISC000.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"

#include <stdlib.h>
#include <windows.h>
#include <conio.h>
#include <tchar.h>

#include "..\Includes\Thorlabs.MotionControl.IntegratedStepperMotors.h"

	bool IsTimeout(const char *serialNo)
	{
		if(ISC_HasLastMsgTimerOverrun(serialNo))
		{
			long long lastMsgTime;
			ISC_TimeSinceLastMsgReceived(serialNo, lastMsgTime);
			printf("\r\nno response from device %s , %lld\r\n", serialNo, lastMsgTime);
			char c = _getch();
			return true;
		}
		return false;
	}

	/// <summary> Main entry-point for this application. </summary>
	/// <param name="argc"> The argc. </param>
	/// <param name="argv"> The argv. </param>
	/// <returns> . </returns>
	int _tmain(int argc, _TCHAR* argv[])
	{
 
		if(argc < 1)
		{
			printf("Usage = Example_ISC [serial_no] [position: optional (0.0 - 300.0mm)] [velocity: optional (0.0 - 2.4)]\r\n");
			char c = _getch();
			return 1;
		}

		int serialNo = 45850894;
		if(argc > 1)
		{
			serialNo = _wtoi(argv[1]);
		}

		// get parameters from command line
		int position = 50 * 409600;
		if(argc > 2)
		{
			position = (int)(409600 * _wtof(argv[2]));
		}

		int velocity = 0;
		if(argc > 3)
		{
			velocity = (int)(409600 * _wtof(argv[3]));
		}

		// identify and access device
		char testSerialNo[16];
		sprintf_s(testSerialNo, "%d", serialNo);

		try
        {
 			// Build list of connected device
            if (TLI_BuildDeviceList() == 0)
            {
				// get device list size 
                short n = TLI_GetDeviceListSize();
				// get LTS serial numbers
                char serialNos[100];
				TLI_GetDeviceListByTypeExt(serialNos, 100, 45);

				// output list of matching devices
				char *next_token1 = NULL;
				char *p = strtok_s(serialNos, ",", &next_token1);
				bool matched = false;
				while(p != NULL)
                {
					TLI_DeviceInfo deviceInfo;
					// get device info from device
                    TLI_GetDeviceInfo(p, &deviceInfo);
					// get strings from device info structure
					char desc[65];
					strncpy(desc, deviceInfo.description, 64);
					desc[64] = '\0';
					char serialNo[9];
					strncpy(serialNo, deviceInfo.serialNo, 8);
					serialNo[8] = '\0';
					// output
					printf("Found Device %s=%s : %s\r\n", p, serialNo, desc);
					p = strtok_s(NULL, ",", &next_token1);
					if(strncmp(testSerialNo, serialNo, 8) == 0)
					{
						printf("requested device %s found\r\n", serialNo);
						matched = true;
					}
				}

				if(!matched)
				{
					printf("LTS %s not found\r\n", testSerialNo);
					char c = _getch();
					return 1;
				}

				// open device
		        if(ISC_Open(testSerialNo) == 0)
		        {
					// start the device polling at 200ms intervals
			        ISC_StartPolling(testSerialNo, 200);
					ISC_EnableLastMsgTimer(testSerialNo, true, 2000);

					// NOTE The following uses Sleep functions to simulate timing
					// In reality, the program should read the status to check that commands have been completed
			        Sleep(3000);
					// Home device
			        ISC_Home(testSerialNo);
					printf("Device %s homing\r\n", testSerialNo);
			        Sleep(300);
					DWORD bits = ISC_GetStatusBits(testSerialNo);
					while((bits & 0x200) == 0x200)
					{
						Sleep(50);
						if(IsTimeout(testSerialNo))
						{
							// stop polling
							ISC_StopPolling(testSerialNo);
							// close device
							ISC_Close(testSerialNo);
							return 0;
						}
						bits = ISC_GetStatusBits(testSerialNo);
					}

					// set velocity if desired
			        if(velocity > 0)
			        {
				        int currentVelocity, currentAcceleration;
				        ISC_GetVelParams(testSerialNo, &currentVelocity, &currentAcceleration);
				        ISC_SetVelParams(testSerialNo, velocity, currentAcceleration);
			        }

					// move to position (channel 1)
			        ISC_MoveToPosition(testSerialNo, position);
					printf("Device %s moving\r\n", testSerialNo);
			        Sleep(300);
					bits = ISC_GetStatusBits(testSerialNo);
					while((bits & 0xF0) != 0)
					{
						Sleep(50);
						if(IsTimeout(testSerialNo))
						{
							// stop polling
							ISC_StopPolling(testSerialNo);
							// close device
							ISC_Close(testSerialNo);
							return 0;
						}
						int pos = ISC_GetPosition(testSerialNo);
						printf("Device %s positio %d\r\n", testSerialNo, pos);
						bits = ISC_GetStatusBits(testSerialNo);
					}

					int pos = ISC_GetPosition(testSerialNo);
					printf("Device %s moved to %d\r\n", testSerialNo, pos);

					ISC_SetMoveRelativeDistance(testSerialNo, 409600);
					ISC_MoveRelativeDistance(testSerialNo);
			        Sleep(300);
					bits = ISC_GetStatusBits(testSerialNo);
					while((bits & 0xF0) != 0)
					{
						Sleep(50);
						if(IsTimeout(testSerialNo))
						{
							// stop polling
							ISC_StopPolling(testSerialNo);
							// close device
							ISC_Close(testSerialNo);
							return 0;
						}
						int pos = ISC_GetPosition(testSerialNo);
						printf("Device %s positio %d\r\n", testSerialNo, pos);
						bits = ISC_GetStatusBits(testSerialNo);
					}

					// stop polling
			        ISC_StopPolling(testSerialNo);
					// close device
			        ISC_Close(testSerialNo);
	            }
            }
        }
        catch (char * e)
        {
			printf("Error %s\r\n", e);
        }

		char c = _getch();
		return 0;
    }


