using System;
using System.Linq;
using System.Threading;

namespace TDC_Console_net_marshalled
{
	/// <summary> Example program  using .net to show how to connect to the C API DLLs </summary>
	/// <remarks> The TCubeDCServo class wraps the marshalling code.
	/// 		  This class shows how to set up a wait loop to send a command and wait for a response. </remarks>
	class Program
	{
		private const int MotorMessageType = 2;
		private const int MotorHomed = 0;
		private const int MotorMoved = 1;

		static void Main(string[] args)
		{
			// get parameters from command line
			int argc = args.Count();
			if (argc < 1)
			{
				Console.WriteLine("Usage = TDC_Console_net_managed [serial_no] [position: optional (0 - 1715200)] [velocity: optional (0 - 3838091)]");
				Console.ReadKey();
				return;
			}

			// get position
			int position = 250000;
			if (argc > 1)
			{
				position = int.Parse(args[1]);
			}

			// get velocity
			int velocity = 0;
			if (argc > 2)
			{
				velocity = int.Parse(args[2]);
			}

			// get serial no
			string serialNo = args[0];

			try
			{
				// build device list
				InstrumentManager.BuildDeviceList();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception raised by BuildDeviceList {0}", ex);
				Console.ReadKey();
				return;
			}

			// get available TCube DC Servos and check our serial number is correct
            string[] serialNumbers = InstrumentManager.GetDeviceListByType(83);
			if (!serialNumbers.Contains(serialNo))
			{
				Console.WriteLine("{0} is not a valid serial number", serialNo);
				Console.ReadKey();
				return;
			}

			// display info about device
			InstrumentManager.DeviceInfo di = InstrumentManager.GetDeviceInfo(serialNo);
			Console.WriteLine("Device {0} = {1}", di.SerialNumber(), di.Description());

			// open device
			Console.WriteLine("Opening device {0}", serialNo);
			TCubeDCServo device = new TCubeDCServo(serialNo);
			short err = device.Open();
			if (err != 0)
			{
				Console.WriteLine("Failed to open device {0}: error code = {1}", serialNo, err);
				Console.ReadKey();
				return;
			}

			// register a callback function to receive device messages
			device.RegisterMessageCallback(new TCubeDCServo.CallbackDelegate(MessageWaitingCallback));

			device.StartPolling(250);
			Thread.Sleep(500);
			device.EnableLastMsgTimer(true, 2000);

			// home the device
			Console.WriteLine("Homing device");
			device.ClearMessageQueue();
			device.Home();

			if (!WaitForMessage(device, MotorMessageType, MotorHomed))
			{
				return;
			}
			Console.WriteLine("Device Homed");

			// if position is set
			if (position != 0)
			{
				// update velocity if required
				if (velocity != 0)
				{
					TCubeDCServo.VelocityParameters velPars = device.GetVelParams();
					velPars.maxVelocity = velocity;
					device.SetVelParams(velPars);
				}

				// move to defined position
				Console.WriteLine("Moving Device to {0}", position);
				device.ClearMessageQueue();
				device.MoveToPosition(position);
				if (!WaitForMessage(device, MotorMessageType, MotorMoved))
				{
					return;
				}
				int newPos = GetPosition(device);
				Console.WriteLine("Device Moved to {0}({1})", newPos, position);
			}

			Console.ReadKey();
			return;
		}

		/// <summary> Gets a position. </summary>
		/// <param name="cube"> If non-null, the cube. </param>
		/// <returns> The position. </returns>
		private static int GetPosition(TCubeDCServo cube)
		{
			cube.RequestPosition();
			Thread.Sleep(50);
			return cube.GetPosition();
		}

		private static readonly ManualResetEvent _event = new ManualResetEvent(false);

		private static bool WaitForMessage(TCubeDCServo cube, int messageTypeID, int requiredMessageID)
		{
			while (true)
			{
				ushort messageType;
				ushort messageID;
				uint messageData;

				// if no messages waiting
				if (cube.MessageQueueSize() == 0)
				{
					// reset wait for message event and wait for a message
					_event.Reset();
					_event.WaitOne();
				}

				// get the message
				cube.GetNextMessage(out messageType, out messageID, out messageData);

				// if message is correct
				if ((messageType == messageTypeID) && (messageID == requiredMessageID))
				{
					return true;
				}

				if (cube.HasLastMsgTimerOverrun())
				{
					Int64 lastMsg;
					cube.TimeSinceLastMsgReceived(out lastMsg);
					Console.WriteLine("device not responded, lastMsg {0}", lastMsg);
					cube.Close();
					Console.ReadKey();
					return false;
				}
			}
		}

		/// <summary> Callback, called when a message is waiting. </summary>
		private static void MessageWaitingCallback()
		{
			// set message recieved event
			_event.Set();
		}
	}
}
