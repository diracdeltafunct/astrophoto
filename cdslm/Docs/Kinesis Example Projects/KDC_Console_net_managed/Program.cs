using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.KCube.DCServoCLI;
using Thorlabs.MotionControl.TCube.DCServoCLI;

namespace DC_Console_net_managed
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
			TCubeDCServo.RegisterDevice();
			KCubeDCServo.RegisterDevice();

			// get parameters from command line
			int argc = args.Count();
			if (argc < 1)
			{
				Console.WriteLine("Usage = DC_Console_net_managed [serial_no] [position: optional (0 - 50)] [velocity: optional (0 - 5)]");
				Console.ReadKey();
				return;
			}

			decimal position = 0m;
			if (argc > 1)
			{
				position = decimal.Parse(args[1]);
			}

			decimal velocity = 0m;
			if (argc > 2)
			{
				velocity = decimal.Parse(args[2]);
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

			// get available KCube DC Servos and check our serial number is correct
			List<string> serialNumbers = DeviceManagerCLI.GetDeviceList(new List<int> { KCubeDCServo.DevicePrefix, TCubeDCServo.DevicePrefix} );
			if (!serialNumbers.Contains(serialNo))
			{
				if (serialNumbers.Count > 0)
				{
					serialNo = serialNumbers[0];
					Console.WriteLine("using serial number {0}", serialNo);
				}
				else
				{
					Console.WriteLine("{0} is not a valid serial number", serialNo);
					Console.ReadKey();
					return;
				}
			}

			// create the device
			IGenericCoreDeviceCLI device = DeviceFactory.CreateDevice(serialNo);
			IGenericAdvancedMotor motor = device as IGenericAdvancedMotor;
			if (motor == null)
			{
				Console.WriteLine("{0} is not a DCServo", serialNo);
				Console.ReadKey();
				return;
			}

			// connect device
			try
			{
				Console.WriteLine("Opening device {0}", serialNo);
				device.Connect(serialNo);

				if (!motor.IsSettingsInitialized())
				{
					motor.WaitForSettingsInitialized(5000);
				}

				// display info about device
				DeviceInfo di = device.GetDeviceInfo();
				Console.WriteLine("Device {0} = {1}", di.SerialNumber, di.Name);

				// start the device polling
				motor.StartPolling(250);
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
				MotorConfiguration motorSettings = motor.GetMotorConfiguration(serialNo);
				motorSettings.DeviceSettingsName = "PRM1-Z8";
				motorSettings.UpdateCurrentConfiguration();

				MotorDeviceSettings motorDeviceSettings = motor.MotorDeviceSettings;
				motor.SetSettings(motorDeviceSettings, true, false);

				// test code to test get / sert of parameters using real world units
				TestVelocityParameters(motor);
				TestJogParameters(motor);
				TestHomingParameters(motor);
				TestLimitParameters(motor);
				if(device is TCubeDCServo)
				{
					TestPotentiometerParameters(device as TCubeDCServo); // TDC Only
				}

				motorSettings.UpdateCurrentConfiguration();
				deviceUnitConverter = motor.UnitConverter;

			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed prepare settings {0} - {1}", ex.DeviceID, ex.Message);
				Console.ReadKey();
				return;
			}

			try
			{
				if (!Home_1(motor))
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
						VelocityParameters velPars = motor.GetVelocityParams();
						velPars.MaxVelocity = velocity;
						motor.SetVelocityParams(velPars);
					}

					if (!MoveTo_1(motor, position, deviceUnitConverter))
					{
						Console.WriteLine("Failed to set position");
						Console.ReadKey();
					}
				}
				else
				{
					char c = '\0';
					do
					{
						do
						{
							Console.WriteLine("Press a key");
							Console.WriteLine("0 to exit");
							Console.WriteLine("1 to test StopImmediate()");
							Console.WriteLine("2 to test Stop(5000)");
							Console.WriteLine("3 to test Stop(WaitEvent)");
							c = Console.ReadKey().KeyChar;
						} while (c < '0' || c > '3');

						if (c != '0')
						{
							motor.MoveContinuous(MotorDirection.Forward);
							Console.WriteLine("Press any key to stop");
							Console.ReadKey();
							StatusBase status;
							if (c == '1')
							{
								motor.Stop(5000);
							}
							if (c == '2')
							{
								motor.StopImmediate();
							}
							if (c == '3')
							{
								ManualResetEvent waitEvent = new ManualResetEvent(false);
								waitEvent.Reset();
								motor.Stop(p =>
								{
									Console.WriteLine("Message Id {0}", p);
									waitEvent.Set();
								});
								if (!waitEvent.WaitOne(5000))
								{
									Console.WriteLine("Failed to Stop");
								}
							}
							do
							{
								status = motor.Status;
								Console.WriteLine("Status says {0} ({1:X})", status.IsInMotion ? "Moving" : "Stopped", status.Status);
								Thread.Sleep(50);
							} while (status.IsInMotion);
						}
					} while (c != '0');

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
		public static bool MoveTo_1(IGenericAdvancedMotor device, decimal position, DeviceUnitConverter deviceUnitConverter)
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
		public static bool Home_1(IGenericAdvancedMotor device)
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
			Console.WriteLine("Device Homed");
			return true;
		}

		/// <summary> Move to. </summary>
		/// <param name="device">			   The device. </param>
		/// <param name="position">			   The position. </param>
		/// <param name="deviceUnitConverter"> The device unit converter. </param>
		public static bool MoveTo_2(IGenericAdvancedMotor device, decimal position, DeviceUnitConverter deviceUnitConverter)
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
		public static bool Home_2(IGenericAdvancedMotor device)
		{
			Console.WriteLine("Homing device");

			// call home function, passing in message complete handler provided by device
			Action<UInt64> workDone = device.InitializeWaitHandler();
			// call home function, passing in event handler
			device.Home(workDone);
			device.Wait(60000);

			// check status
			bool homed = device.Status.IsHomed;
			Console.WriteLine("Device Homed");
			return true;
		}

		/// <summary> Move to. </summary>
		/// <param name="device">			   The device. </param>
		/// <param name="position">			   The position. </param>
		/// <param name="deviceUnitConverter"> The device unit converter. </param>
		public static bool MoveTo_3(IGenericAdvancedMotor device, decimal position, DeviceUnitConverter deviceUnitConverter)
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
		public static bool Home_3(IGenericAdvancedMotor device)
		{
			Console.WriteLine("Homing device");

			// call home function, passing in event handler
			device.Home(60000);

			// check status
			bool homed = device.Status.IsHomed;
			Console.WriteLine("Device Homed");
			return true;
		}

		/// <summary> Tests potentiometer parameters. </summary>
		/// <param name="device"> The device. </param>
		public static void TestPotentiometerParameters(TCubeDCServo device)
		{
			try
			{
				PotentiometerParameters_DeviceUnit potentiometerParameters = device.GetPotentiometerParams_DeviceUnit();
				PotentiometerParameters realPotentiometerParameters = device.GetPotentiometerParams();
				realPotentiometerParameters[0].Velocity += 0.5m;
				realPotentiometerParameters[1].Velocity += 0.5m;
				realPotentiometerParameters[2].Velocity += 0.5m;
				realPotentiometerParameters[3].Velocity += 0.5m;
				device.SetPotentiometerParams(realPotentiometerParameters);
				Thread.Sleep(250);
				device.SetPotentiometerParams_DeviceUnit(potentiometerParameters);
				Thread.Sleep(250);
				PotentiometerParameters_DeviceUnit newPotentiometerParameters = device.GetPotentiometerParams_DeviceUnit();
			}
			catch (DeviceException ex)
			{
				Console.WriteLine("Failed to update settings {0} - {1}", ex.DeviceID, ex.Message);
			}
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
