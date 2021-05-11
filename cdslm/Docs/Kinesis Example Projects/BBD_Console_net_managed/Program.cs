using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.Benchtop.BrushlessMotorCLI;

namespace BBD_Console_net_managed
{
	/// <summary> Program showing examples of how to set settings a perform basic commands. </summary>
	/// <remarks> This example shows 3 examples of how to move / home a device. <br/>
	/// 		  Home_1 / MoveTo_1 - Create callback function to maintain a wait handle to wait for completion. <br /> 
	/// 		  Home_2 / MoveTo_2 - Use devices supplied callback handler and wait function to wait for completion. <br />
	/// 		  Home_3 / MoveTo_3 - Use the internal timeout mechanism. <br /> </remarks>
	class Program
	{
		static void Main(string[] args)
		{
			// get parameters from command line
			int argc = args.Count();
			if (argc < 1)
			{
				Console.WriteLine("Usage = BBD_Console_net_managed [serial_no] [channel] [position: optional (0 - 50)] [velocity: optional (0 - 5)]");
				Console.ReadKey();
				return;
			}

			short channel = 1;

			if (argc > 2)
			{
				channel = short.Parse(args[1]);
			}

			decimal position = 0m;
			if (argc > 2)
			{
				position = decimal.Parse(args[2]);
			}

			decimal velocity = 0m;
			if (argc > 3)
			{
				velocity = decimal.Parse(args[3]);
			}

			string serialNo = args[0];

			try
			{
				// build device list
				DeviceManagerCLI.BuildDeviceList();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception raised by BuildDeviceList {0}", ex);
				Console.ReadKey();
				return;
			}

			// get available TCube DC Servos and check our serial number is correct
			List<string> serialNumbers = DeviceManagerCLI.GetDeviceList(BenchtopBrushlessMotor.DevicePrefix);
			if (!serialNumbers.Contains(serialNo))
			{
				Console.WriteLine("{0} is not a valid serial number", serialNo);
				Console.ReadKey();
				return;
			}

			// create the device
			BenchtopBrushlessMotor device = BenchtopBrushlessMotor.CreateDevice(serialNo) as BenchtopBrushlessMotor;
			if (device == null)
			{
				Console.WriteLine("{0} is not a BenchtopBrushlessMotor", serialNo);
				Console.ReadKey();
				return;
			}

			//BrushlessMotorChannel benchtopChannel = device.GetChannel(channel); 
			BrushlessMotorChannel benchtopChannel = device[channel] as BrushlessMotorChannel; 
			if (benchtopChannel == null)
			{
				Console.WriteLine("{0} is not a valid channel number", channel);
				Console.ReadKey();
				return;
			}

			// connect device
			try
			{
				Console.WriteLine("Opening device {0}", serialNo);
				device.Connect(serialNo);

				if (!benchtopChannel.IsSettingsInitialized())
				{
					benchtopChannel.WaitForSettingsInitialized(5000);
				}

				// display info about device
				DeviceInfo di = device.GetDeviceInfo();
				Console.WriteLine("Device {0} = {1}", di.SerialNumber, di.Name);

				// start the device polling
				benchtopChannel.StartPolling(250);
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to open device {0} - {1}", ex.DeviceID, ex.Message);
				Console.ReadKey();
				return;
			}

			DeviceUnitConverter deviceUnitConverter;

			try
			{
				// call GetMotorConfiguration on the device to initialize the DeviceUnitConverter object required for real unit parameters
				MotorConfiguration motorSettings = benchtopChannel.GetMotorConfiguration(serialNo);

				// test code to test get / sert of parameters using real world units
				TestVelocityParameters(benchtopChannel);
				TestJogParameters(benchtopChannel);
				TestHomingParameters(benchtopChannel);
				TestLimitParameters(benchtopChannel);

				motorSettings.UpdateCurrentConfiguration();
				deviceUnitConverter = benchtopChannel.UnitConverter;

			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed prepare settings {0} - {1}", ex.DeviceID, ex.Message);
				Console.ReadKey();
				return;
			}

			try
			{
				if (!Home_1(benchtopChannel))
				{
					Console.WriteLine("Failed to home device");
					Console.ReadKey();
					return;
				}
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to Home device settings {0} - {1}", ex.DeviceID, ex.Message);
				Console.ReadKey();
				return;
			}

			try
			{
				// if position is set
				if (position != 0)
				{
					// update velocity if required using real world methods
					if (velocity != 0)
					{
						VelocityParameters velPars = benchtopChannel.GetVelocityParams();
						velPars.MaxVelocity = velocity;
						benchtopChannel.SetVelocityParams(velPars);
					}

					if (!MoveTo_1(benchtopChannel, position, deviceUnitConverter))
					{
						Console.WriteLine("Failed to set position");
						Console.ReadKey();
					}
				}
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to Move device settings {0} - {1}", ex.DeviceID, ex.Message);
				Console.ReadKey();
				return;
			}

			try
			{
				device.Disconnect(true);
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to Disconnect {0} - {1}", ex.DeviceID, ex.Message);
			}

			Console.ReadKey();
		}

		/// <summary> Move to. </summary>
		/// <param name="device">			   The device. </param>
		/// <param name="position">			   The position. </param>
		/// <param name="deviceUnitConverter"> The device unit converter. </param>
		public static bool MoveTo_1(BrushlessMotorChannel device, decimal position, DeviceUnitConverter deviceUnitConverter)
		{
			// create wait handler
			ManualResetEvent waitEvent = new ManualResetEvent(false);
			int iPos = deviceUnitConverter.RealToDeviceUnit(position, DeviceUnitConverter.UnitType.Length);
			Console.WriteLine("Moving Device to {0} ({1})", position, iPos);
			// call moveto function, passing in event handler
			// could alternatively use method with in-built wait handler by passing a timeout instead of callback
			waitEvent.Reset();
			// clear errors
			device.ClearDeviceExceptions();
			device.MoveTo_DeviceUnit(iPos, p =>
			{
				Console.WriteLine("Message Id {0}", p);
				waitEvent.Set();
			});
			if (!waitEvent.WaitOne(60000))
			{
				return false;
			}
			// check for exceptions thrown by background thread during process
			device.ThrowLastDeviceException();

			StatusBase status = device.Status;
			Decimal newPos = status.Position;
			Console.WriteLine("Device Moved to {0}({1})", newPos, position);
			return true;
		}

		/// <summary> Homes the given device. </summary>
		/// <param name="device"> The device. </param>
		public static bool Home_1(BrushlessMotorChannel device)
		{
			Console.WriteLine("Homing device");
			// create wait handler
			ManualResetEvent waitEvent = new ManualResetEvent(false);
			waitEvent.Reset();
			// clear errors
			device.ClearDeviceExceptions();
			// call home function, passing in event handler
			// could alternatively use method with in-built wait handler by passing a timeout instead of callback
			device.Home(p => waitEvent.Set());
			if (!waitEvent.WaitOne(60000))
			{
				// timed out
				return false;
			}
			// check for exceptions thrown by background thread during process
			device.ThrowLastDeviceException();
			// check status
			bool homed = device.Status.IsHomed;
			Console.WriteLine("Device Homed {0}", homed);
			return true;
		}

		/// <summary> Move to. </summary>
		/// <param name="device">			   The device. </param>
		/// <param name="position">			   The position. </param>
		/// <param name="deviceUnitConverter"> The device unit converter. </param>
		public static bool MoveTo_2(BrushlessMotorChannel device, decimal position, DeviceUnitConverter deviceUnitConverter)
		{
			int iPos = deviceUnitConverter.RealToDeviceUnit(position, DeviceUnitConverter.UnitType.Length);
			Console.WriteLine("Moving Device to {0} ({1})", position, iPos);

			// call moveto function, passing in message complete handler provided by device
			Action<UInt64> workDone = device.InitializeWaitHandler();
			device.MoveTo_DeviceUnit(iPos, workDone);
			device.Wait(60000);

			StatusBase status = device.Status;
			Decimal newPos = status.Position;
			Console.WriteLine("Device Moved to {0}({1})", newPos, position);
			return true;
		}

		/// <summary> Homes the given device. </summary>
		/// <param name="device"> The device. </param>
		public static bool Home_2(BrushlessMotorChannel device)
		{
			Console.WriteLine("Homing device");

			// call home function, passing in message complete handler provided by device
			Action<UInt64> workDone = device.InitializeWaitHandler();
			// call home function, passing in event handler
			device.Home(workDone);
			device.Wait(60000);

			// check status
			bool homed = device.Status.IsHomed;
			Console.WriteLine("Device Homed {0}", homed);
			return true;
		}

		/// <summary> Move to. </summary>
		/// <param name="device">			   The device. </param>
		/// <param name="position">			   The position. </param>
		/// <param name="deviceUnitConverter"> The device unit converter. </param>
		public static bool MoveTo_3(BrushlessMotorChannel device, decimal position, DeviceUnitConverter deviceUnitConverter)
		{
			int iPos = deviceUnitConverter.RealToDeviceUnit(position, DeviceUnitConverter.UnitType.Length);
			Console.WriteLine("Moving Device to {0} ({1})", position, iPos);

			device.MoveTo_DeviceUnit(iPos, 60000);

			StatusBase status = device.Status;
			Decimal newPos = status.Position;
			Console.WriteLine("Device Moved to {0}({1})", newPos, position);
			return true;
		}

		/// <summary> Homes the given device. </summary>
		/// <param name="device"> The device. </param>
		public static bool Home_3(BrushlessMotorChannel device)
		{
			Console.WriteLine("Homing device");

			// call home function, passing in event handler
			device.Home(60000);

			// check status
			bool homed = device.Status.IsHomed;
			Console.WriteLine("Device Homed {0}", homed);
			return true;
		}

		/// <summary> Tests velocity parameters. </summary>
		/// <param name="device"> The device. </param>
		public static void TestVelocityParameters(IGenericAdvancedMotor device)
		{
			try
			{
				VelocityParameters_DeviceUnit originalVelocityParameters = device.GetVelocityParams_DeviceUnit();
				VelocityParameters realVP = device.GetVelocityParams();
				realVP.Acceleration += 0.5m;
				realVP.MaxVelocity += 0.5m;
				device.SetVelocityParams(realVP);
				Thread.Sleep(250);

				realVP = device.GetVelocityParams();

				device.SetVelocityParams_DeviceUnit(originalVelocityParameters);
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to update settings {0} - {1}", ex.DeviceID, ex.Message);
			}
		}

		/// <summary> Tests jog parameters. </summary>
		/// <param name="device"> The device. </param>
		public static void TestJogParameters(IGenericAdvancedMotor device)
		{
			try
			{
				JogParameters_DeviceUnit originalJogParameters = device.GetJogParams_DeviceUnit();
				JogParameters realJP = device.GetJogParams();
				realJP.StepSize += 1.0m;
				realJP.VelocityParams.MaxVelocity += 0.5m;
				realJP.VelocityParams.Acceleration += 0.5m;
				device.SetJogParams(realJP);
				Thread.Sleep(250);
				realJP = device.GetJogParams();

				device.SetJogParams_DeviceUnit(originalJogParameters);
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to update settings {0} - {1}", ex.DeviceID, ex.Message);
			}
		}

		/// <summary> Tests homing parameters. </summary>
		/// <param name="device"> The device. </param>
		public static void TestHomingParameters(IGenericAdvancedMotor device)
		{
			try
			{
				HomeParameters_DeviceUnit originalHomeParameters = device.GetHomingParams_DeviceUnit();
				HomeParameters realHP = device.GetHomingParams();
				realHP.Velocity += 0.5m;
				realHP.OffsetDistance += 1.0m;
				device.SetHomingParams(realHP);
				Thread.Sleep(250);
				realHP = device.GetHomingParams();

				device.SetHomingParams_DeviceUnit(originalHomeParameters);
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to update settings {0} - {1}", ex.DeviceID, ex.Message);
			}
		}

		/// <summary> Tests limit parameters. </summary>
		/// <param name="device"> The device. </param>
		public static void TestLimitParameters(IGenericAdvancedMotor device)
		{
			try
			{
				LimitSwitchParameters_DeviceUnit originalLimitParameters = device.GetLimitSwitchParams_DeviceUnit();
				LimitSwitchParameters realLP = device.GetLimitSwitchParams();
				realLP.AnticlockwisePosition += 2.0m;
				realLP.ClockwisePosition += 2.0m;
				device.SetLimitSwitchParams(realLP);
				Thread.Sleep(250);
				realLP = device.GetLimitSwitchParams();

				device.SetLimitSwitchParams_DeviceUnit(originalLimitParameters);
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to update settings {0} - {1}", ex.DeviceID, ex.Message);
			}
		}
	}
}
