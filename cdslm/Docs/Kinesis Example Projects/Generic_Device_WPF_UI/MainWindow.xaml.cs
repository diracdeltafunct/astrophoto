using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.DeviceManagerUI;

namespace Generic_Device_WPF_UI
{
	/// <summary> Form for viewing the main. </summary>
	/// <seealso cref="T:System.Windows.Window"/>
	public partial class MainWindow : Window
	{
		private GenericDeviceHolder.GenericDevice _genericDevice;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			// register Device DLLs so they can ba accessed by the DeviceManager
			// These Devices are self referencing and do not need to be referenced by the project
			// This info could be supplied from a config file.
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			DeviceManager.RegisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.TCube.BrushlessMotorUI.DLL"), "TCubeBrushlessMotorUI");
			DeviceManager.RegisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.Benchtop.BrushlessMotorUI.DLL"), "BenchtopBrushlessMotorUI");
			DeviceManager.RegisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.TCube.DCServoUI.DLL"), "TCubeDCServoUI");

			// get list of devices
			DeviceManagerCLI.BuildDeviceList();
			List<string> devices = DeviceManagerCLI.GetDeviceList(new List<int> { 47, 67, 73, 83 });
			if (devices.Count == 0)
			{
				MessageBox.Show("No Devices");
				return;
			}
			
			// populate combo box
			_devices.ItemsSource = devices;
			_devices.SelectedIndex = 0;

			// get first serial number for example
			string serialNo = devices[0];
			// create device
			ConnectDevice(serialNo);
		}

		private void MainWindow_OnClosed(object sender, EventArgs e)
		{
			// disconnect any connected device
			DisconnectDevice();
			// unregister devices before exit
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			DeviceManager.UnregisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.TCube.BrushlessMotorUI.DLL"), "BrushlessMotorUI");
			DeviceManager.UnregisterLibrary(null, Path.Combine(path, "Thorlabs.MotionControl.Benchtop.BrushlessMotorUI.DLL"), "BenchtopBrushlessMotorUI");
			DeviceManager.UnregisterLibrary(null,Path.Combine(path,  "Thorlabs.MotionControl.TCube.DCServoUI.DLL"), "TCubeDCServoUI");
		}

		private void _devices_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			string serialNo = _devices.SelectedItem as string;
			if (_genericDevice != null && _genericDevice.CoreDevice.DeviceID == serialNo)
			{
				// already connected
				return;
			}

			// connect device
			ConnectDevice(serialNo);
		}

		/// <summary> Connects a device. </summary>
		/// <param name="serialNo"> The serial no. </param>
		private void ConnectDevice(string serialNo)
		{
			// unload device if not desired type
			if (_genericDevice != null)
			{
				if (_genericDevice.CoreDevice.DeviceID == serialNo)
				{
					return;
				}
				DisconnectDevice();
			}

			// create new device
			IGenericCoreDeviceCLI device = DeviceFactory.CreateDevice(serialNo);
			GenericDeviceHolder devices = new GenericDeviceHolder(device);
			_genericDevice = devices[1];
			if (_genericDevice == null)
			{
				MessageBox.Show("Unknown Device Type");
				return;
			}

			// connect device
			try
			{
				_genericDevice.CoreDevice.Connect(serialNo);

				// wait for settings to be initialized
				_genericDevice.Device.WaitForSettingsInitialized(5000);
			}
			catch (DeviceException ex)
			{
				MessageBox.Show(ex.Message);
				return;
			}

			// create view via factory
			// get factory
			IUIFactory factory = DeviceManager.GetUIFactory(_genericDevice.CoreDevice.DeviceID);

			// create and initialize view model for device
			IDeviceViewModel viewModel = factory.CreateViewModel(DisplayTypeEnum.Full, _genericDevice);
			viewModel.Initialize();

			// create view and attach to our display
			_contentControl.Content = factory.CreateLargeView(viewModel);
		}

		/// <summary> Disconnects the device. </summary>
		private void DisconnectDevice()
		{
			if ((_genericDevice != null) && _genericDevice.CoreDevice.IsConnected)
			{
				_genericDevice.CoreDevice.Disconnect(true);
				_genericDevice = null;
			}
		}
	}
}
