using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TDC_Console_net_marshalled
{
	/// <summary> Cube device-context servo. </summary>
	public class TCubeDCServo
	{
		#region data structures
		/// <summary> Velocity parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct VelocityParameters
		{
			public Int32 minVelocity;
			public Int32 acceleration;
			public Int32 maxVelocity;
		}

		/// <summary> Jog parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct JogParameters
		{
			public Int16 mode;
			public UInt32 iStepSize;
			public VelocityParameters velParams;
			public Int16 stopMode;
		}

		/// <summary> Homing parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct HomingParameters
		{
			public Int16 direction;
			public UInt16 uiLimitSwitch;
			public UInt32 uiVelocity;
			public UInt32 uiOffsetDistance;
		}

		/// <summary> Button parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ButtonParameters
		{
			public Int16 iButMode;
			public Int32 leftButtonPosition;
			public Int32 rightButtonPosition;
			public Int16 timeout;
			Int16 wUnused;
		}

		/// <summary> Potentiometer parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PotentiometerParams
		{
			/// <summary> Constructor. </summary>
			/// <param name="iSize"> (optional) zero-based index of the size. </param>
			public PotentiometerParams(int iSize = 4)
			{
				_steps = new step[iSize];
			}

			/// <summary> Step. </summary>
			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			public struct step
			{
				public UInt16 thresholdDeflection;
				public UInt32 velocity;
			}

			/// <summary> The steps. </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			public step[] _steps;
		}

		/// <summary> Limit switch parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct LimitSwitchParameters
		{
			public UInt16 uiClockwiseHardwareLimit;
			public UInt16 uiAnticlockwiseHardwareLimit;
			public UInt32 uiClockwisePosition;
			public UInt32 uiAnticlockwisePosition;
			public UInt16 uiSoftwareModes;
		}

		/// <summary> Dcpid parameters. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DCPIDParameters
		{
			public Int32 proportionalGain;
			public Int32 integralGain;
			public Int32 differentialGain;
			public Int32 integralLimit;
			public UInt16 parameterFilter;
		}

		/// <summary> Callback delegate. </summary>
		public delegate void CallbackDelegate();

		#endregion

		#region class data
		/// <summary> The serial no. </summary>
		public readonly string _serialNo;
		#endregion

		#region Member Functions
		/// <summary> Constructor. </summary>
		/// <param name="serialNo"> The serial no. </param>
		public TCubeDCServo(string serialNo)
		{
			_serialNo = serialNo;
		}

		/// <summary> Gets the device. </summary>
		/// <returns> . </returns>
		public string Device()
		{
			return _serialNo;
		}

		/// <summary> Opens the device. </summary>
		/// <returns> . </returns>
		public short Open()
		{ return NativeMethods.CC_Open(_serialNo); }

		/// <summary>	Query if this object is open. </summary>
		/// <returns>	true if open, false if not. </returns>
		/// <summary>	Determines if we can check connection. </summary>
		/// <returns>	Success. </returns>
		public bool CheckConnection()
		{ return NativeMethods.CC_CheckConnection(_serialNo); }

		/// <summary> Opens the device. </summary>
		/// <returns> . </returns>
		public bool StartPolling(Int32 ms)
		{ return NativeMethods.CC_StartPolling(_serialNo, ms); }

		/// <summary> Opens the device. </summary>
		/// <returns> . </returns>
		public void StopPolling()
		{ NativeMethods.CC_StopPolling(_serialNo); }

		/// <summary> Closes the device. </summary>
		public void Close()
		{ NativeMethods.CC_Close(_serialNo); }

		/// <summary> Identifies the object (Flashes Ident LED). </summary>
		public void Identify()
		{ NativeMethods.CC_Identify(_serialNo); }

		/// <summary> Gets the hardware information. </summary>
		/// <param name="modelNo">			 [in,out] The model no. </param>
		/// <param name="uiType">			 [in,out] The type. </param>
		/// <param name="numChannels">		 [in,out] Number of channels. </param>
		/// <param name="notes">			 [in,out] The notes. </param>
		/// <param name="uiFirmwareVersion"> [in,out] The firmware version. </param>
		/// <param name="uiHardwareVersion"> [in,out] The hardware version. </param>
		/// <param name="uiModState">		 [in,out] State of the modifier. </param>
		/// <returns> The hardware information. </returns>
		public short GetHardwareInfo(ref string modelNo, ref UInt16 uiType, ref UInt16 numChannels, ref string notes, ref UInt32 uiFirmwareVersion, ref UInt16 uiHardwareVersion, ref UInt16 uiModState)
        {
            StringBuilder sbModelNo = new StringBuilder(16);
            StringBuilder sbNotes = new StringBuilder(100);
            short retval = NativeMethods.CC_GetHardwareInfo(_serialNo, sbModelNo, (UInt32)sbModelNo.Capacity, ref(uiType), ref(numChannels),
                                                            sbNotes, (UInt32)sbNotes.Capacity, ref(uiFirmwareVersion), ref(uiHardwareVersion), ref(uiModState));
            notes = sbNotes.ToString();
            modelNo = sbModelNo.ToString();
            return retval;
        }

		/// <summary> Gets the number of positions. </summary>
		/// <returns> The number positions. </returns>
		public int GetNumberPositions()
		{ return NativeMethods.CC_GetNumberPositions(_serialNo); }

		/// <summary> Move to a position. </summary>
		/// <param name="index"> Zero-based index of the. </param>
		/// <returns> . </returns>
		public short MoveToPosition(int index)
		{ return NativeMethods.CC_MoveToPosition(_serialNo, index); }

		/// <summary> Gets the last read position. </summary>
		/// <returns> The position. </returns>
		public int GetPosition()
		{ return NativeMethods.CC_GetPosition(_serialNo); }

		/// <summary> Homes the device. </summary>
		/// <returns> . </returns>
		public short Home()
		{ return NativeMethods.CC_Home(_serialNo); }

		/// <summary> Determine if we can stop. </summary>
		/// <returns> <c>true</c> if we can stop.. </returns>
		public bool CanStop() { return true; }

		/// <summary> Gets the LED options. </summary>
		/// <returns> The led options. </returns>
		public ushort GetLEDswitches()
		{ return NativeMethods.CC_GetLEDswitches(_serialNo); }

		/// <summary> Sets the LED options. </summary>
		/// <param name="usLEDswitches"> The LED options. </param>
		/// <returns> . </returns>
		public short SetLEDswitches(ushort usLEDswitches)
		{ return NativeMethods.CC_SetLEDswitches(_serialNo, usLEDswitches); }

		/// <summary> Jogs the device in the given direction. </summary>
		/// <param name="usDirection"> The direction. </param>
		/// <returns> . </returns>
		public short Jog(ushort usDirection)
		{ return NativeMethods.CC_MoveJog(_serialNo, usDirection); }

		/// <summary> Move relative by the supplied amount. </summary>
		/// <param name="displacement"> the amount to move positive or negative. </param>
		/// <returns> . </returns>
		public short MoveRelative(int displacement)
		{ return NativeMethods.CC_MoveRelative(_serialNo, displacement); }

		/// <summary> Move continuously in the given direction. </summary>
		/// <param name="direction"> The direction. </param>
		/// <returns> . </returns>
		public short MoveAtVelocity(ushort direction)
		{ return NativeMethods.CC_MoveAtVelocity(_serialNo, direction); }

		/// <summary> Stops a move immediately. </summary>
		/// <returns> . </returns>
		public short StopImmediate()
		{ return NativeMethods.CC_StopImmediate(_serialNo); }

		/// <summary> Stops a move using the current profile. </summary>
		/// <returns> . </returns>
		public short StopProfiled()
		{ return NativeMethods.CC_StopProfiled(_serialNo); }

		/// <summary> Gets the homing velocity. </summary>
		/// <returns> The homing velocity. </returns>
		public uint GetHomingVelocity()
		{ return NativeMethods.CC_GetHomingVelocity(_serialNo); }

		/// <summary> Sets the home velocity. </summary>
		/// <param name="uiVelocity"> The velocity. </param>
		/// <returns> . </returns>
		public short SetHomeVelocity(uint uiVelocity)
		{ return NativeMethods.CC_SetHomingVelocity(_serialNo, uiVelocity); }

		/// <summary> Gets the jog mode. </summary>
		/// <param name="mode">	    [in,out] The mode. </param>
		/// <param name="stopMode"> [in,out] The stop mode. </param>
		/// <returns> The jog mode. </returns>
		public short GetJogMode(ref short mode, ref short stopMode)
		{ return NativeMethods.CC_GetJogMode(_serialNo, ref(mode), ref(stopMode)); }

		/// <summary> Sets the jog mode. </summary>
		/// <param name="mode">	    The mode. </param>
		/// <param name="stopMode"> The stop mode. </param>
		/// <returns> . </returns>
		public short SetJogMode(short mode, short stopMode)
		{ return NativeMethods.CC_SetJogMode(_serialNo, mode, stopMode); }

		/// <summary> Gets the jog step size. </summary>
		/// <returns> The jog step size. </returns>
		public uint GetJogStepSize()
		{ return NativeMethods.CC_GetJogStepSize(_serialNo); }

		/// <summary> Sets the jog step size. </summary>
		/// <param name="uiStep"> Amount to increment by. </param>
		/// <returns> . </returns>
		public short SetJogStepSize(uint uiStep)
		{ return NativeMethods.CC_SetJogStepSize(_serialNo, uiStep); }

		/// <summary> Gets the jog velocity parameters. </summary>
		/// <param name="iAccn">   [in,out] Zero-based index of the accn. </param>
		/// <param name="maxVelocity"> [in,out] Zero-based index of the maximum velocity. </param>
		/// <returns> The jog velocity parameters. </returns>
		public short GetJogVelParams(ref int iAccn, ref int maxVelocity)
		{ return NativeMethods.CC_GetJogVelParams(_serialNo, ref(iAccn), ref(maxVelocity)); }

		/// <summary> Sets the jog velocity parameters. </summary>
		/// <param name="iAccn">   Zero-based index of the accn. </param>
		/// <param name="maxVelocity"> Zero-based index of the maximum velocity. </param>
		/// <returns> . </returns>
		public short SetJogVelParams(int iAccn, int maxVelocity)
		{ return NativeMethods.CC_SetJogVelParams(_serialNo, iAccn, maxVelocity); }

		/// <summary> Gets the velocity parameters. </summary>
		/// <param name="iAccn">   [in,out] Zero-based index of the accn. </param>
		/// <param name="maxVelocity"> [in,out] Zero-based index of the maximum velocity. </param>
		/// <returns> The velocity parameters. </returns>
		public short GetVelParams(ref int iAccn, ref int maxVelocity)
		{ return NativeMethods.CC_GetVelParams(_serialNo, ref(iAccn), ref(maxVelocity)); }

		/// <summary> Sets the velocity parameters. </summary>
		/// <param name="iAccn">   Zero-based index of the accn. </param>
		/// <param name="maxVelocity"> Zero-based index of the maximum velocity. </param>
		/// <returns> . </returns>
		public short SetVelParams(int iAccn, int maxVelocity)
		{ return NativeMethods.CC_SetVelParams(_serialNo, iAccn, maxVelocity); }

		/// <summary> Gets the backlash. </summary>
		/// <returns> The backlash. </returns>
		public int GetBacklash()
		{ return NativeMethods.CC_GetBacklash(_serialNo); }

		/// <summary> Sets the backlash. </summary>
		/// <param name="iDistance"> Zero-based index of the distance. </param>
		/// <returns> . </returns>
		public short SetBacklash(int iDistance)
		{ return NativeMethods.CC_SetBacklash(_serialNo, iDistance); }

		/// <summary> Gets the position counter. </summary>
		/// <returns> The position counter. </returns>
		public int GetPositionCounter()
		{ return NativeMethods.CC_GetPositionCounter(_serialNo); }

		/// <summary> Sets the position counter. </summary>
		/// <param name="iCount"> Number of. </param>
		/// <returns> . </returns>
		public short SetPositionCounter(int iCount)
		{ return NativeMethods.CC_SetPositionCounter(_serialNo, iCount); }

		/// <summary> Gets the encoder counter. </summary>
		/// <returns> The encoder counter. </returns>
		public int GetEncoderCounter()
		{ return NativeMethods.CC_GetEncoderCounter(_serialNo); }

		/// <summary> Sets the encoder counter. </summary>
		/// <param name="iCount"> Number of. </param>
		/// <returns> . </returns>
		public short SetEncoderCounter(int iCount)
		{ return NativeMethods.CC_SetEncoderCounter(_serialNo, iCount); }

		/// <summary> Gets the limit switch parameters. </summary>
		/// <param name="usClockwiseHardwareLimit">	    [in,out] The clockwise hardware limit. </param>
		/// <param name="usAnticlockwiseHardwareLimit"> [in,out] The anticlockwise hardware limit. </param>
		/// <param name="uiClockwisePosition">		    [in,out] The clockwise position. </param>
		/// <param name="uiAnticlockwisePosition">	    [in,out] The anticlockwise position. </param>
		/// <param name="usSoftwareModes">			    [in,out] The software modes. </param>
		/// <returns> The limit switch parameters. </returns>
		public short GetLimitSwitchParams(ref ushort usClockwiseHardwareLimit, ref ushort usAnticlockwiseHardwareLimit, ref uint uiClockwisePosition, ref uint uiAnticlockwisePosition, ref ushort usSoftwareModes)
		{ return NativeMethods.CC_GetLimitSwitchParams(_serialNo, ref(usClockwiseHardwareLimit), ref(usAnticlockwiseHardwareLimit), ref(uiClockwisePosition), ref(uiAnticlockwisePosition), ref(usSoftwareModes)); }

		/// <summary> Sets the limit switch parameters. </summary>
		/// <param name="usClockwiseHardwareLimit">	    The clockwise hardware limit. </param>
		/// <param name="usAnticlockwiseHardwareLimit"> The anticlockwise hardware limit. </param>
		/// <param name="uiClockwisePosition">		    The clockwise position. </param>
		/// <param name="uiAnticlockwisePosition">	    The anticlockwise position. </param>
		/// <param name="usSoftwareModes">			    The software modes. </param>
		/// <returns> . </returns>
		public short SetLimitSwitchParams(ushort usClockwiseHardwareLimit, ushort usAnticlockwiseHardwareLimit, uint uiClockwisePosition, uint uiAnticlockwisePosition, ushort usSoftwareModes)
		{ return NativeMethods.CC_SetLimitSwitchParams(_serialNo, usClockwiseHardwareLimit, usAnticlockwiseHardwareLimit, uiClockwisePosition, uiAnticlockwisePosition, usSoftwareModes); }

		/// <summary> Gets the button parameters. </summary>
		/// <param name="iButMode">	    [in,out] Zero-based index of the but mode. </param>
		/// <param name="leftButtonPosition">  [in,out] Zero-based index of the left but position. </param>
		/// <param name="rightButtonPosition"> [in,out] Zero-based index of the right but position. </param>
		/// <param name="timeout">	    [in,out] The time out. </param>
		/// <returns> The button parameters. </returns>
		public short GetButtonParams(ref ushort iButMode, ref int leftButtonPosition, ref int rightButtonPosition, ref short timeout)
		{ return NativeMethods.CC_GetButtonParams(_serialNo, ref(iButMode), ref(leftButtonPosition), ref(rightButtonPosition), ref(timeout)); }

		/// <summary> Sets the button parameters. </summary>
		/// <param name="iButMode">	    Zero-based index of the but mode. </param>
		/// <param name="leftButtonPosition">  Zero-based index of the left but position. </param>
		/// <param name="rightButtonPosition"> Zero-based index of the right but position. </param>
		/// <param name="timeout">	    The time out. </param>
		/// <returns> . </returns>
		public short SetButtonParams(ushort iButMode, int leftButtonPosition, int rightButtonPosition, short timeout)
		{ return NativeMethods.CC_SetButtonParams(_serialNo, iButMode, leftButtonPosition, rightButtonPosition, timeout); }

		/// <summary> Gets the device information block. </summary>
		/// <returns> The device information block. </returns>
		public InstrumentManager.DeviceInfo GetDeviceInfo()
		{
			return InstrumentManager.GetDeviceInfo(_serialNo);
		}

		/// <summary> Gets the velocity parameters. </summary>
		/// <returns> The velocity parameters. </returns>
		public VelocityParameters GetVelParams()
		{
			int iSize = Marshal.SizeOf(typeof(VelocityParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetVelParamsBlock(_serialNo, iPtr);
			VelocityParameters velParams = (VelocityParameters)(Marshal.PtrToStructure(iPtr, typeof(VelocityParameters)));
			Marshal.FreeHGlobal(iPtr);
			return velParams;
		}

		/// <summary> Sets the velocity parameters. </summary>
		/// <param name="velParams"> Options for controlling the velocity. </param>
		/// <returns> . </returns>
		public short SetVelParams(VelocityParameters velParams)
		{
			int iSize = Marshal.SizeOf(typeof(VelocityParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(velParams, iPtr, false);
			short result = NativeMethods.CC_SetVelParamsBlock(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the homing parameters. </summary>
		/// <returns> The homing parameters. </returns>
		public HomingParameters GetHomingParams()
		{
			int iSize = Marshal.SizeOf(typeof(HomingParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetHomingParamsBlock(_serialNo, iPtr);
			HomingParameters homingParams = (HomingParameters)(Marshal.PtrToStructure(iPtr, typeof(HomingParameters)));
			return homingParams;
		}

		/// <summary> Sets the homing parameters. </summary>
		/// <param name="homingParams"> Options for controlling the homing. </param>
		/// <returns> . </returns>
		public short SetHomingParams(HomingParameters homingParams)
		{
			int iSize = Marshal.SizeOf(typeof(HomingParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(homingParams, iPtr, false);
			short result = NativeMethods.CC_SetHomingParamsBlock(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the jog parameters. </summary>
		/// <returns> The jog parameters. </returns>
		public JogParameters GetJogParams()
		{
			int iSize = Marshal.SizeOf(typeof(JogParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetJogParamsBlock(_serialNo, iPtr);
			JogParameters jogParams = (JogParameters)(Marshal.PtrToStructure(iPtr, typeof(JogParameters)));
			return jogParams;
		}

		/// <summary> Sets the jog parameters. </summary>
		/// <param name="jogParams"> Options for controlling the jog. </param>
		/// <returns> . </returns>
		public short SetJogParams(JogParameters jogParams)
		{
			int iSize = Marshal.SizeOf(typeof(JogParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(jogParams, iPtr, false);
			short result = NativeMethods.CC_SetJogParamsBlock(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the button parameters. </summary>
		/// <returns> The button parameters. </returns>
		public ButtonParameters GetButtonParams()
		{
			int iSize = Marshal.SizeOf(typeof(ButtonParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetButtonParamsBlock(_serialNo, iPtr);
			ButtonParameters buttonParams = (ButtonParameters)(Marshal.PtrToStructure(iPtr, typeof(ButtonParameters)));
			return buttonParams;
		}

		/// <summary> Sets the button parameters. </summary>
		/// <param name="buttonParams"> Options for controlling the button. </param>
		/// <returns> . </returns>
		public short SetButtonParams(ButtonParameters buttonParams)
		{
			int iSize = Marshal.SizeOf(typeof(ButtonParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(buttonParams, iPtr, false);
			short result = NativeMethods.CC_SetButtonParamsBlock(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the potentiometer parameters. </summary>
		/// <returns> The potentiometer parameters. </returns>
		public PotentiometerParams GetPotentiometerParams()
		{
			int iSize = Marshal.SizeOf(typeof(PotentiometerParams));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetPotentiometerParamsBlock(_serialNo, iPtr);
			PotentiometerParams potentiometerParams = (PotentiometerParams)(Marshal.PtrToStructure(iPtr, typeof(PotentiometerParams)));
			return potentiometerParams;
		}

		/// <summary> Sets the potentiometer parameters. </summary>
		/// <param name="potentiometerParams"> Options for controlling the potentiometer. </param>
		/// <returns> . </returns>
		public short SetPotentiometerParams(PotentiometerParams potentiometerParams)
		{
			int iSize = Marshal.SizeOf(typeof(PotentiometerParams));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(potentiometerParams, iPtr, false);
			short result = NativeMethods.CC_SetPotentiometerParamsBlock(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the limit switch parameters. </summary>
		/// <returns> The limit switch parameters. </returns>
		public LimitSwitchParameters GetLimitSwitchParams()
		{
			int iSize = Marshal.SizeOf(typeof(LimitSwitchParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetLimitSwitchParamsBlock(_serialNo, iPtr);
			LimitSwitchParameters limitSwitchParams = (LimitSwitchParameters)(Marshal.PtrToStructure(iPtr, typeof(LimitSwitchParameters)));
			return limitSwitchParams;
		}

		/// <summary> Sets the limit switch parameters. </summary>
		/// <param name="limitSwitchParams"> Options for controlling the limit switch. </param>
		/// <returns> . </returns>
		public short SetLimitSwitchParams(LimitSwitchParameters limitSwitchParams)
		{
			int iSize = Marshal.SizeOf(typeof(LimitSwitchParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(limitSwitchParams, iPtr, false);
			short result = NativeMethods.CC_SetLimitSwitchParamsBlock(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the PID parameters. </summary>
		/// <returns> The PID parameters. </returns>
		public DCPIDParameters GetDCPIDParams()
		{
			int iSize = Marshal.SizeOf(typeof(DCPIDParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.CC_GetDCPIDParams(_serialNo, iPtr);
			DCPIDParameters DCPIDParams = (DCPIDParameters)(Marshal.PtrToStructure(iPtr, typeof(DCPIDParameters)));
			return DCPIDParams;
		}

		/// <summary> Sets the PID parameters. </summary>
		/// <param name="DCPIDParams"> Options for controlling the PID. </param>
		/// <returns> . </returns>
		public short SetDCPIDParams(DCPIDParameters DCPIDParams)
		{
			int iSize = Marshal.SizeOf(typeof(DCPIDParameters));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			Marshal.StructureToPtr(DCPIDParams, iPtr, false);
			short result = NativeMethods.CC_SetDCPIDParams(_serialNo, iPtr);
			Marshal.FreeHGlobal(iPtr);
			return result;
		}

		/// <summary> Gets the potentiometer parameters. </summary>
		/// <param name="iStepup">			    Zero-based index of the stepup. </param>
		/// <param name="wThresholdDeflection"> [in,out] The threshold deflection. </param>
		/// <param name="dwVelocity">		    [in,out] The velocity. </param>
		/// <returns> The potentiometer parameters. </returns>
		public short GetPotentiometerParams(short iStepup, ref UInt16 wThresholdDeflection, ref UInt32 dwVelocity)
		{ return NativeMethods.CC_GetPotentiometerParams(_serialNo, iStepup, ref(wThresholdDeflection), ref(dwVelocity)); }

		/// <summary> Sets the potentiometer parameters. </summary>
		/// <param name="iStep">				 Amount to increment by. </param>
		/// <param name="usThresholdDeflection"> The threshold deflection. </param>
		/// <param name="ulVelocity">			 The ul velocity. </param>
		/// <returns> . </returns>
		public short SetPotentiometerParams(short iStep, UInt16 usThresholdDeflection, UInt32 ulVelocity)
		{ return NativeMethods.CC_SetPotentiometerParams(_serialNo, iStep, usThresholdDeflection, ulVelocity); }

		/// <summary> Gets the suspend move messages. </summary>
		/// <returns> . </returns>
		public short SuspendMoveMessages()
		{ return NativeMethods.CC_SuspendMoveMessages(_serialNo); }

		/// <summary> Resume move messages. </summary>
		/// <returns> . </returns>
		public short ResumeMoveMessages()
		{ return NativeMethods.CC_ResumeMoveMessages(_serialNo); }

		/// <summary> Requests that the current position be read from the device. </summary>
		/// <returns> . </returns>
		public short RequestPosition()
		{ return NativeMethods.CC_RequestPosition(_serialNo); }

		/// <summary> Request that the status bits be read from the device. </summary>
		/// <returns> . </returns>
		public short RequestStatusBits()
		{ return NativeMethods.CC_RequestStatusBits(_serialNo); }

		/// <summary> Gets the last status bits. </summary>
		/// <returns> The status bits. </returns>
		public uint GetStatusBits()
		{ return NativeMethods.CC_GetStatusBits(_serialNo); }

		/// <summary> Request that the settings be read from the device. </summary>
		/// <returns> . </returns>
		public short RequestSettings()
		{ return NativeMethods.CC_RequestSettings(_serialNo); }

		/// <summary> Gets the software version. </summary>
		/// <returns> The software version. </returns>
		public uint GetSoftwareVersion()
		{ return NativeMethods.CC_GetSoftwareVersion(_serialNo); }

		/// <summary> Enables the channel. </summary>
		/// <returns> . </returns>
		public short EnableChannel()
		{ return NativeMethods.CC_EnableChannel(_serialNo); }

		/// <summary> Cc message queue size. </summary>
		/// <returns> . </returns>
		public void ClearMessageQueue()
		{ NativeMethods.CC_ClearMessageQueue(_serialNo); }

		/// <summary> Cc message queue size. </summary>
		/// <param name="callback"> The callback. </param>
		public void RegisterMessageCallback( MulticastDelegate callback)
		{ NativeMethods.CC_RegisterMessageCallback(_serialNo, callback); }


		/// <summary> Cc message queue size. </summary>
		/// <returns> . </returns>
		public int MessageQueueSize()
		{ return NativeMethods.CC_MessageQueueSize(_serialNo); }

		/// <summary> Cc get next message. </summary>
		/// <param name="messageType"> [in,out] Type of the message. </param>
		/// <param name="messageId">   [in,out] Identifier for the message. </param>
		/// <param name="messageData"> [in,out] Information describing the message. </param>
		/// <returns> Success. </returns>
		public bool GetNextMessage(out UInt16 messageType, out UInt16 messageId, out UInt32 messageData)
		{ return NativeMethods.CC_GetNextMessage(_serialNo, out messageType, out messageId, out messageData); }

		/// <summary> Cc wait for message. </summary>
		/// <param name="messageType"> [in,out] Type of the message. </param>
		/// <param name="messageId">   [in,out] Identifier for the message. </param>
		/// <param name="messageData"> [in,out] Information describing the message. </param>
		/// <returns> Success. </returns>
		public bool WaitForMessage(out UInt16 messageType, out UInt16 messageId, out UInt32 messageData)
		{ return NativeMethods.CC_WaitForMessage(_serialNo, out messageType, out messageId, out messageData); }

		/// <summary> Enables the last message timer. </summary>
		/// <param name="enable">	  true to enable, false to disable. </param>
		/// <param name="msgTimeout"> The message timeout. </param>
		/// <returns> Success. </returns>
		public void EnableLastMsgTimer(bool enable, Int32 msgTimeout)
		{ NativeMethods.CC_EnableLastMsgTimer(_serialNo, enable, msgTimeout); }

		/// <summary> Query if this object has last message timer overrun. </summary>
		/// <returns> true if last message timer overrun, false if not. </returns>
		public bool HasLastMsgTimerOverrun()
		{ return NativeMethods.CC_HasLastMsgTimerOverrun(_serialNo); }

		/// <summary> Time since last message received. </summary>
		/// <param name="lastMsgTime"> Time of the last message. </param>
		/// <returns> Success. </returns>
		public bool TimeSinceLastMsgReceived(out Int64 lastMsgTime)
		{ return NativeMethods.CC_TimeSinceLastMsgReceived(_serialNo, out lastMsgTime); }

		#endregion

		#region internal Marshalling Functions

		private static class NativeMethods
		{
			private const string _thorLabsTCubeDCServo = "Thorlabs.MotionControl.TCube.DCServo.DLL";

			/// <summary> Determines if a given Cc open. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_Open([MarshalAs(UnmanagedType.LPStr)]String serialNo);

			// Close device

			/// <summary> Cc close. </summary>
			/// <param name="serialNo"> The serial no. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern void CC_Close([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc Check connection. </summary>
			/// <param name="serialNo"> The serial no. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool CC_CheckConnection([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc start polling. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="ms">	    The milliseconds. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool CC_StartPolling([MarshalAs(UnmanagedType.LPStr)]string serialNo, Int32 ms);

			/// <summary> Cc stop polling. </summary>
			/// <param name="serialNo"> The serial no. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern void CC_StopPolling([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc identify. </summary>
			/// <param name="serialNo"> The serial no. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern void CC_Identify([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get le dswitches. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern ushort CC_GetLEDswitches([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc set le dswitches. </summary>
			/// <param name="serialNo">		 The serial no. </param>
			/// <param name="usLEDswitches"> The le dswitches. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetLEDswitches([MarshalAs(UnmanagedType.LPStr)]string serialNo, UInt16 usLEDswitches);

			/// <summary> Cc get hardware information. </summary>
			/// <param name="serialNo">			 The serial no. </param>
			/// <param name="modelNo">			 [in,out] The model no. </param>
			/// <param name="uiType">			 [in,out] The type. </param>
			/// <param name="numChannels">		 [in,out] Number of channels. </param>
			/// <param name="notes">			 [in,out] The notes. </param>
			/// <param name="uiFirmwareVersion"> [in,out] The firmware version. </param>
			/// <param name="uiHardwareVersion"> [in,out] The hardware version. </param>
			/// <param name="uiModState">		 [in,out] State of the modifier. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
            public static extern short CC_GetHardwareInfo([MarshalAs(UnmanagedType.LPStr)]string serialNo, StringBuilder modelNo, UInt32 szModelNo, ref UInt16 uiType, ref UInt16 numChannels,
                                                                        StringBuilder notes, UInt32 szNotes, ref UInt32 uiFirmwareVersion, ref UInt16 uiHardwareVersion, ref UInt16 uiModState);

			/// <summary> Cc get number positions. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_GetNumberPositions([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc move to position. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="index">    Zero-based index of the. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_MoveToPosition([MarshalAs(UnmanagedType.LPStr)]string serialNo, int index);

			/// <summary> Cc get position. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_GetPosition([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			// Move device to home position

			/// <summary> Cc home. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_Home([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get homing velocity. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint CC_GetHomingVelocity([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc set homing velocity. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iVel">	    Zero-based index of the velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetHomingVelocity([MarshalAs(UnmanagedType.LPStr)]string serialNo, uint iVel);

			/// <summary> Cc move relative. </summary>
			/// <param name="serialNo">		 The serial no. </param>
			/// <param name="displacement"> Zero-based index of the displacement. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_MoveRelative([MarshalAs(UnmanagedType.LPStr)]string serialNo, int displacement);

			/// <summary> Cc move at velocity. </summary>
			/// <param name="serialNo">   The serial no. </param>
			/// <param name="wDirection"> The direction. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_MoveAtVelocity([MarshalAs(UnmanagedType.LPStr)]string serialNo, ushort wDirection);

			/// <summary> Cc stop immediate. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_StopImmediate([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc stop profiled. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_StopProfiled([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get jog mode. </summary>
			/// <param name="serialNo">  The serial no. </param>
			/// <param name="mode">	 [in,out] Zero-based index of the mode. </param>
			/// <param name="stopMode"> [in,out] Zero-based index of the stop mode. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetJogMode([MarshalAs(UnmanagedType.LPStr)]string serialNo, ref short mode, ref short stopMode);

			/// <summary> Cc set jog mode. </summary>
			/// <param name="serialNo">  The serial no. </param>
			/// <param name="mode">	 Zero-based index of the mode. </param>
			/// <param name="stopMode"> Zero-based index of the stop mode. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetJogMode([MarshalAs(UnmanagedType.LPStr)]string serialNo, short mode, short stopMode);

			/// <summary> Cc get jog step size. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint CC_GetJogStepSize([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc set jog step size. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="uiStep">   Amount to increment by. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetJogStepSize([MarshalAs(UnmanagedType.LPStr)]string serialNo, uint uiStep);

			/// <summary> Cc get jog velocity parameters. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iAccn">    [in,out] Zero-based index of the accn. </param>
			/// <param name="maxVelocity">  [in,out] Zero-based index of the maximum velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetJogVelParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, ref int iAccn, ref int maxVelocity);

			/// <summary> Cc set jog velocity parameters. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iAccn">    Zero-based index of the accn. </param>
			/// <param name="maxVelocity">  Zero-based index of the maximum velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetJogVelParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, int iAccn, int maxVelocity);

			/// <summary> Cc move jog. </summary>
			/// <param name="serialNo">    The serial no. </param>
			/// <param name="usDirection"> The direction. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_MoveJog([MarshalAs(UnmanagedType.LPStr)]string serialNo, ushort usDirection);

			/// <summary> Cc get velocity parameters. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iAccn">    [in,out] Zero-based index of the accn. </param>
			/// <param name="maxVelocity">  [in,out] Zero-based index of the maximum velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetVelParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, ref int iAccn, ref int maxVelocity);

			/// <summary> Cc set velocity parameters. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iAccn">    Zero-based index of the accn. </param>
			/// <param name="maxVelocity">  Zero-based index of the maximum velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetVelParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, int iAccn, int maxVelocity);

			/// <summary> Cc get backlash. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_GetBacklash([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc set backlash. </summary>
			/// <param name="serialNo">  The serial no. </param>
			/// <param name="iDistance"> Zero-based index of the distance. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetBacklash([MarshalAs(UnmanagedType.LPStr)]string serialNo, int iDistance);

			/// <summary> Cc get position counter. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_GetPositionCounter([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc set position counter. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iCount">   Number of. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetPositionCounter([MarshalAs(UnmanagedType.LPStr)]string serialNo, int iCount);

			/// <summary> Cc get encoder counter. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_GetEncoderCounter([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc set encoder counter. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="iCount">   Number of. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetEncoderCounter([MarshalAs(UnmanagedType.LPStr)]string serialNo, int iCount);

			/// <summary> Cc get limit switch parameters. </summary>
			/// <param name="serialNo">					    The serial no. </param>
			/// <param name="usClockwiseHardwareLimit">	    [in,out] The clockwise hardware limit. </param>
			/// <param name="usAnticlockwiseHardwareLimit"> [in,out] The anticlockwise hardware limit. </param>
			/// <param name="uiClockwisePosition">		    [in,out] The clockwise position. </param>
			/// <param name="uiAnticlockwisePosition">	    [in,out] The anticlockwise position. </param>
			/// <param name="usSoftwareModes">			    [in,out] The software modes. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetLimitSwitchParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, ref ushort usClockwiseHardwareLimit, ref ushort usAnticlockwiseHardwareLimit, ref uint uiClockwisePosition,
			                                                    ref uint uiAnticlockwisePosition, ref ushort usSoftwareModes);

			/// <summary> Cc set limit switch parameters. </summary>
			/// <param name="serialNo">					    The serial no. </param>
			/// <param name="usClockwiseHardwareLimit">	    The clockwise hardware limit. </param>
			/// <param name="usAnticlockwiseHardwareLimit"> The anticlockwise hardware limit. </param>
			/// <param name="uiClockwisePosition">		    The clockwise position. </param>
			/// <param name="uiAnticlockwisePosition">	    The anticlockwise position. </param>
			/// <param name="usSoftwareModes">			    The software modes. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetLimitSwitchParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, ushort usClockwiseHardwareLimit, ushort usAnticlockwiseHardwareLimit, uint uiClockwisePosition,
			                                                    uint uiAnticlockwisePosition, ushort usSoftwareModes);

			/// <summary> Cc get button parameters. </summary>
			/// <param name="serialNo">	    The serial no. </param>
			/// <param name="iButMode">	    [in,out] Zero-based index of the but mode. </param>
			/// <param name="leftButtonPosition">  [in,out] Zero-based index of the left but position. </param>
			/// <param name="rightButtonPosition"> [in,out] Zero-based index of the right but position. </param>
			/// <param name="timeout">	    [in,out] The time out. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetButtonParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, ref ushort iButMode, ref int leftButtonPosition, ref int rightButtonPosition, ref short timeout);

			/// <summary> Cc set button parameters. </summary>
			/// <param name="serialNo">	    The serial no. </param>
			/// <param name="iButMode">	    Zero-based index of the but mode. </param>
			/// <param name="leftButtonPosition">  Zero-based index of the left but position. </param>
			/// <param name="rightButtonPosition"> Zero-based index of the right but position. </param>
			/// <param name="timeout">	    The time out. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetButtonParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, ushort iButMode, int leftButtonPosition, int rightButtonPosition, short timeout);

			/// <summary> Cc get potentiometer parameters. </summary>
			/// <param name="serialNo">				 The serial no. </param>
			/// <param name="istep">				 The istep. </param>
			/// <param name="usThresholdDeflection"> [in,out] The threshold deflection. </param>
			/// <param name="ulVelocity">			 [in,out] The ul velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetPotentiometerParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, short istep, ref UInt16 usThresholdDeflection, ref UInt32 ulVelocity);

			/// <summary> Cc set potentiometer parameters. </summary>
			/// <param name="serialNo">				 The serial no. </param>
			/// <param name="istep">				 The istep. </param>
			/// <param name="usThresholdDeflection"> The threshold deflection. </param>
			/// <param name="ulVelocity">			 The ul velocity. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetPotentiometerParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, short istep, UInt16 usThresholdDeflection, UInt32 ulVelocity);

			/// <summary> Cc suspend move messages. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SuspendMoveMessages([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc resume move messages. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_ResumeMoveMessages([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc request position. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_RequestPosition([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc request status bits. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_RequestStatusBits([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get status bits. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint CC_GetStatusBits([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc request settings. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_RequestSettings([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get software version. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint CC_GetSoftwareVersion([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc enable channel. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_EnableChannel([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get velocity parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetVelParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set velocity parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetVelParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc get homing parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetHomingParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set homing parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetHomingParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc get jog parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetJogParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set jog parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetJogParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc get button parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetButtonParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set button parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetButtonParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc get potentiometer parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetPotentiometerParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set potentiometer parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetPotentiometerParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc get limit switch parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetLimitSwitchParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set limit switch parameters block. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetLimitSwitchParamsBlock([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc get dcpid parameters. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_GetDCPIDParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc set dcpid parameters. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern short CC_SetDCPIDParams([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			/// <summary> Cc message queue size. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_ClearMessageQueue([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc message queue size. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="callback"> The callback. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern void CC_RegisterMessageCallback([MarshalAs(UnmanagedType.LPStr)]string serialNo, MulticastDelegate callback);

			/// <summary> Cc message queue size. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern int CC_MessageQueueSize([MarshalAs(UnmanagedType.LPStr)]string serialNo);

			/// <summary> Cc get next message. </summary>
			/// <param name="serialNo">    The serial no. </param>
			/// <param name="messageType"> [in,out] Type of the message. </param>
			/// <param name="messageId">   [in,out] Identifier for the message. </param>
			/// <param name="messageData"> [in,out] Information describing the message. </param>
			/// <returns> Success. </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern bool CC_GetNextMessage([MarshalAs(UnmanagedType.LPStr)]string serialNo, out UInt16 messageType, out UInt16 messageId, out UInt32 messageData);

			/// <summary> Cc wait for message. </summary>
			/// <param name="serialNo">    The serial no. </param>
			/// <param name="messageType"> [in,out] Type of the message. </param>
			/// <param name="messageId">   [in,out] Identifier for the message. </param>
			/// <param name="messageData"> [in,out] Information describing the message. </param>
			/// <returns> Success. </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping=false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern bool CC_WaitForMessage([MarshalAs(UnmanagedType.LPStr)]string serialNo, out UInt16 messageType, out UInt16 messageId, out UInt32 messageData);

			/// <summary> Cc time since last message received. </summary>
			/// <param name="serialNo">    The serial no. </param>
			/// <param name="lastMsgTime"> Time of the last message. </param>
			/// <returns> Success. </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern bool CC_TimeSinceLastMsgReceived([MarshalAs(UnmanagedType.LPStr)]string serialNo, out Int64 lastMsgTime);

			/// <summary> Cc enable last message timer. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="enabled">  true to enable, false to disable. </param>
			/// <param name="timeout">  The time out. </param>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			public static extern void CC_EnableLastMsgTimer([MarshalAs(UnmanagedType.LPStr)]string serialNo, bool enabled, Int64 timeout);

			/// <summary> Cc has last message timer overrun. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <returns> Success. </returns>
			[DllImport(_thorLabsTCubeDCServo, SetLastError = true, BestFitMapping = false, ExactSpelling = false,
				CallingConvention = CallingConvention.Cdecl)]
			[return: MarshalAs(UnmanagedType.I1)]
			public static extern bool CC_HasLastMsgTimerOverrun([MarshalAs(UnmanagedType.LPStr)]string serialNo);
		}

		#endregion
	}
}
