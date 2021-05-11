using System;
using System.Runtime.InteropServices;

namespace TDC_Console_net_marshalled
{
	/// <summary> Manager for instruments. </summary>
	public class InstrumentManager
	{
		#region data structures

		/// <summary> Information about the device. </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct DeviceInfo
		{
			/// <summary> device Type ID. </summary>
			public UInt32 typeID;

			/// <summary> The description. </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 65)] private readonly char[] _description;

			/// <summary> The serial no of the device. </summary>
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)] private readonly char[] _serialNo;

			/// <summary> The PID for the device. </summary>
			public UInt32 PID;

			/// <summary> true if this object is known type. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isKnownType;

			/// <summary> true if this object is a motor. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isMotor;

			/// <summary> true if this object is a piezzo motor. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isPiezzoMotor;

			/// <summary> true if this object is a DC motor. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isDCMotor;

			/// <summary> true if this object is a stepper motor. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isStepperMotor;

			/// <summary> true if this object is a laser. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isLaser;

			/// <summary> true if this object is a custom type. </summary>
			[MarshalAs(UnmanagedType.I1)] public bool isCustomType;

			/// <summary> Gets the description. </summary>
			/// <returns> . </returns>
			public string Description()
			{
				string s = new string(_description);
				return s.Substring(0, s.IndexOf('\0'));
			}

			/// <summary> Gets the serial number. </summary>
			/// <returns> . </returns>
			public string SerialNumber()
			{
				string s = new string(_serialNo);
				return s.Substring(0, s.IndexOf('\0'));
			}
		}

		#endregion

		#region member functions

		/// <summary> Builds the Thorlabs Instruments device list. </summary>
		/// <returns> . </returns>
		public static short BuildDeviceList()
		{
			return NativeMethods.TLI_BuildDeviceList();
		}

		/// <summary> Gets the Thorlabs Instruments device list size. </summary>
		/// <returns> . </returns>
		public static short GetDeviceListSize()
		{
			return NativeMethods.TLI_GetDeviceListSize();
		}

		/// <summary> Get the Thorlabs Instruments device list. </summary>
		/// <returns> An array of device serial numbers. </returns>
		public static string[] GetDeviceList()
		{
			string[] serialNumbers;
			if (NativeMethods.TLI_GetDeviceList(out serialNumbers) != 0)
			{
				return new string[0];
			}
			return serialNumbers;
		}

		/// <summary> Get the Thorlabs Instruments device list. </summary>
		/// <param name="typeID"> Identifier for the type. </param>
		/// <returns> An array of device serial numbers matching the supplied TypeID. </returns>
		public static string[] GetDeviceListByType(int typeID)
		{
			string[] serialNumbers;
			if (NativeMethods.TLI_GetDeviceListByType(out serialNumbers, typeID) != 0)
			{
				return new string[0];
			}
			return serialNumbers;
		}

		/// <summary> Get the Thorlabs Instruments device list. </summary>
		/// <param name="typeIDs"> Identifiers for the types. </param>
		/// <returns> An array of device serial numbers matching the supplied TypeIDs. </returns>
		public static string[] GetDeviceListByTypes(int[] typeIDs)
		{
			string[] serialNumbers;
			if (NativeMethods.TLI_GetDeviceListByTypes(out serialNumbers, typeIDs, typeIDs.Length) != 0)
			{
				return new string[0];
			}
			return serialNumbers;
		}

		/// <summary> Gets the device information block. </summary>
		/// <returns> The device information block. </returns>
		public static DeviceInfo GetDeviceInfo(string serialNo)
		{
			int iSize = Marshal.SizeOf(typeof (DeviceInfo));
			IntPtr iPtr = Marshal.AllocHGlobal(iSize);
			NativeMethods.TLI_GetDeviceInfo(serialNo, iPtr);
			DeviceInfo di = (DeviceInfo)(Marshal.PtrToStructure(iPtr, typeof(DeviceInfo)));
			Marshal.FreeHGlobal(iPtr);
			return di;
		}

		#endregion

		#region internal marshalled functions

		private static class NativeMethods
		{
			private const string _thorLabsDeviceManager = "Thorlabs.MotionControl.DeviceManager.DLL";

			/// <summary> Builds the Thorlabs Instruments device list. </summary>
			/// <returns> . </returns>
			[DllImport(_thorLabsDeviceManager, CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			public static extern short TLI_BuildDeviceList();

			/// <summary> Gets the Thorlabs Instruments device list size. </summary>
			/// <returns> . </returns>
			[DllImport(_thorLabsDeviceManager, CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			public static extern short TLI_GetDeviceListSize();

			/// <summary> Get the Thorlabs Instruments device list. </summary>
			/// <param name="managedStringArray"> the list of serial numbers available. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsDeviceManager, CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			public static extern short TLI_GetDeviceList([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] managedStringArray);

			/// <summary> Get the Thorlabs Instruments device list by type. </summary>
			/// <param name="managedStringArray"> the list of matching serial numbers. </param>
			/// <param name="typeID">	  Identifier for the type. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsDeviceManager, CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			public static extern short TLI_GetDeviceListByType([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] managedStringArray, int typeID);

			/// <summary> Get the Thorlabs Instruments device list by types. </summary>
			/// <param name="managedStringArray"> the list of matching serial numbers. </param>
			/// <param name="typeID">	  Identifier for the type. </param>
			/// <param name="length">	  The length. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsDeviceManager, CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
			public static extern short TLI_GetDeviceListByTypes([MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_BSTR)] out string[] managedStringArray, int[] typeID,
			                                                     int length);

			/// <summary> Get the device information for the selected device. </summary>
			/// <param name="serialNo"> The serial no. </param>
			/// <param name="pStruct">  The device information structure. </param>
			/// <returns> . </returns>
			[DllImport(_thorLabsDeviceManager, SetLastError = true, BestFitMapping = false, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
			public static extern short TLI_GetDeviceInfo([MarshalAs(UnmanagedType.LPStr)]string serialNo, IntPtr pStruct);

			#endregion
		}
	}

}
