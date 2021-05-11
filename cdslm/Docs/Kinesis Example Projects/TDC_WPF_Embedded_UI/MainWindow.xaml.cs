using System;
using System.Collections.Generic;
using System.Windows;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.TCube.DCServoCLI;
using Thorlabs.MotionControl.TCube.DCServoUI;
using Thorlabs.MotionControl.TCube.DCServoUI.Views;

namespace TDC_WPF_Embedded_UI
{
	/// <summary> Form for viewing the main. </summary>
	/// <seealso cref="T:System.Windows.Window"/>
	public partial class MainWindow : Window
	{
		private TCubeDCServo _tCubeDCServo;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			// get list of devices
			DeviceManagerCLI.BuildDeviceList();
			List<string> devices = DeviceManagerCLI.GetDeviceList(83);
			if (devices.Count == 0)
			{
				MessageBox.Show("No Devices");
				return;
			}
			// get first serial number for example
			string serialNo = devices[0];
			// create device
			_tCubeDCServo = TCubeDCServo.CreateDevice(serialNo) as TCubeDCServo;
			if (_tCubeDCServo == null)
			{
				MessageBox.Show("Unknown Device Type");
				return;
			}

			// connect device
			try
			{
				_tCubeDCServo.Connect(serialNo);

				// wait for settings to be initialized
				_tCubeDCServo.WaitForSettingsInitialized(5000);
			}
			catch (DeviceException ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}
			// create view
			_contentControl.Content = TCubeDCServoUI.CreateLargeView(_tCubeDCServo);
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			// disconnect device before closing
			if ((_tCubeDCServo != null) && _tCubeDCServo.IsConnected)
			{
				_tCubeDCServo.Disconnect(true);
			}
		}
	}
}
