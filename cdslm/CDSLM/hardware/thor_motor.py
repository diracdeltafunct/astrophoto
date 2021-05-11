from ctypes import *
import logging
import time
from CDSLM.paths import Paths
from CDSLM.hardware.utils import convert_to_enum, check
from CDSLM.hardware.enums import (TravelDirectionEnum,
                                  JogModesEnum,
                                  StopModesEnum,
                                  JogParameters,
                                  UnitTypeEnum
                                  )
from CDSLM.utils import BorgSingleton


# just for testing import logger
import os
os.chdir(r'C:\Program Files\Thorlabs\Kinesis')
from CDSLM.utils import setup_logging
setup_logging()
# TODO DOCUMENTATION STRINGS!!!!!!!!!

# logger
logger = logging.getLogger('hardware')
logger.info('Loaded motor module')



class BaseSDK(BorgSingleton):
    # NOTE Library has to be loaded as a singleton because multiple motor instances
    # will reset override each others DLL and cause issues
    _sdk = None

    def __init__(self, library):
        self.library = library
        # this is kind of a dirty trick to make the singleton load...
        logger.info(f'Loaded {self.sdk}')

    @property
    def sdk(self):
        if self._sdk is None:
            self._sdk = cdll.LoadLibrary(self.library)

        return self._sdk

    def __getattr__(self, item):
        """
        Override the get atter to make class act as a passthrough to the SDK,
        """
        return getattr(self._sdk, item)


class KCubeStepperSDK(BaseSDK):
    pass


class DeviceManager(BaseSDK):
    """ New SDK singleton for Device manager """
    pass

Stepper =  cdll.LoadLibrary('Thorlabs.MotionControl.KCube.StepperMotor.dll')

class KinesisJogMixin:

    def jog(self, direction):
        direction = convert_to_enum(direction, TravelDirectionEnum, prefix='MOT_')
        self.sdk.SCC_MoveJog(self._serial, direction)

    def get_jog_params(self):
        self.sdk.SCC_RequestJogParams(self._serial)

    def set_jog_mode(self, mode, stop_mode):
        mode_ = self.convert_to_enum(mode, JogModesEnum, prefix='MOT_')
        stop_mode_ = self.convert_to_enum(stop_mode, StopModesEnum, prefix='MOT_')
        self.sdk.SCC_SetJogMode(self._serial, mode_, stop_mode_)

    def set_jog_step_size(self, step_size):
        self.sdk.SCC_SetJogStepSize(self._serial, step_size)

    def set_jog_vel_params(self, max_velocity, acceleration):
        self.sdk.SCC_SetJogVelParams(self._serial, acceleration, max_velocity)

    def get_jog_mode(self):

        mode = c_short()
        stop_mode = c_short()
        self.sdk.SCC_GetJogMode(self._serial, byref(mode), byref(stop_mode))
        return JogModesEnum(mode.value), StopModesEnum(stop_mode.value)

    def get_jog_params_block(self):

        params = JogParameters()
        self.sdk.SCC_GetJogParamsBlock(self._serial, byref(params))
        return params

    def get_jog_step_size(self):

        return self.sdk.SCC_GetJogStepSize(self._serial)

    def get_jog_vel_params(self):
        acceleration = c_int()
        max_velocity = c_int()
        self.sdk.SCC_GetJogVelParams(self._serial, byref(acceleration), byref(max_velocity))
        return max_velocity.value, acceleration.value


class KinesisBase:
    """
    Provides the base connection and operation parameters
    common to all of the kinesis type devices

    if a motor is defined in the init it overides the device
    unit conversion parameters with preprogrammed position settings

    :serial: <str> or <int>
    :polling: <int> default=200
    :motor: <Motor> (optional)
    """
    _motor_library = 'Thorlabs.MotionControl.KCube.StepperMotor.dll'
    _device_manager = None
    _sdk = None
    _serial = (9*c_char_p)
    _id = None

    def __init__(self, serial, *, polling=200, motor=None):
        logger.debug(f'Loaded {self.__class__.__qualname__}')
        self._polling = polling
        if motor:
            self.motor = motor()
        else:
            self.motor = None
        self.build_device_list()

        self._serial = str(serial).encode()
        self._open()

        # check response for open here
        self.start_polling(self._polling)
        self.default_velocity, self.default_acceleration = self.get_vel_params()

    def get_vel_params(self):
        acceleration = c_int()
        max_velocity = c_int()
        self.sdk.SCC_GetVelParams(self._serial, byref(acceleration), byref(max_velocity))
        logger.debug(f'recieved velocity params {max_velocity.value}, {acceleration.value}')
        return max_velocity.value, acceleration.value

    @property
    def sdk(self):
        """
        Loads the DLL for the specified device loaded with ctypes

        :return: <ctypes instance>
        """
        return Stepper#KCubeStepperSDK(self._motor_library)

    def _open(self, serial=None):
        """
        opens a connection to a device,

        Connection MUST be closed before a second conneciton to the same device
        can be made
        """
        resp = self.sdk.SCC_Open(self._serial)
        #if int(resp) != 0:
        #    raise ConnectionError('Could Not Connect to Device')
        logger.info(f'Opened Motor connection: {self._serial}')

    @property
    def device_manager(self):
        logger.debug('Accessing device manager')
        if self._device_manager is None:
            self._device_manager = DeviceManager('Thorlabs.MotionControl.DeviceManager.dll')
        print('DM', self._device_manager)
        return self._device_manager


    def build_device_list(self):
        dm = self.device_manager
        logger.info('Building Device list')
        return check(dm.TLI_BuildDeviceList())

    def get_device_list_size(self):
        """
        returns an the number of devices connected to the unit
        :return: <int>
        """
        size = self.device_manager.TLI_GetDeviceListSize()
        logger.debug(f'Device list size: {size}')
        return size

    def close(self):
        """
        closes the connection to the device, allows new connections to start
        again without restarting the software
        :return:  None
        """
        self.sdk.SCC_Close(self._serial)
        logger.info(f'Closed conection to: {self}')

    @property
    def id(self):
        if self._id is None:
            self._id = self.sdk.SCC_Identify(self._serial)
        logger.debug(f'ID: {self._id}')
        return self._id


    def start_polling(self, milliseconds):
        """
        starts the talking between the SDK and the device
        allows for updating of common parameters
        :param milliseconds: <int>
        :return: None
        """
        self.sdk.SCC_StartPolling(self._serial, milliseconds)
        logger.debug('Started Polling')

    def __repr__(self):
        return f'{self.__class__.__qualname__}({self._serial})'

    def __str__(self):
        return f'{self.__class__.__qualname__}({self._serial})'


class KST101(KinesisBase):
    max = 25
    min = 0

    def __init__(self, *args, **kwargs):
        """
        runs existing init and tells load settings specific to KST101
        """
        super().__init__(*args, **kwargs)
        logger.info(f'Created: {self}')
        self.sdk.SCC_LoadSettings(self._serial)

    def can_home(self):
        """
        Checks to see if the device is capable of finding home

        :return: INT: 1=can home 0=cannot home
        """
        resp = self.sdk.SCC_CanHome(self._serial)
        logging.debug(f'Can home: {resp}')
        return resp

    def can_move_without_homing_first(self):
        """
        Checks to see if the device is allowed to move without finding home
        ideally should be run periodically

        :return: bool
        """
        resp = self.sdk.SCC_CanMoveWithoutHomingFirst(self._serial)
        logging.debug(f'Can move without home: {resp}')
        return resp

    def check_connection(self):
        resp = self.sdk.SCC_CheckConnection(self._serial)
        logging.debug(f'Check Connection: {resp}')

    def disable_channel(self):
        self.sdk.SCC_DisableChannel(self._serial)
        logging.debug('Disabled Channel')

    def enable_channel(self):
        self.sdk.SCC_EnableChannel(self._serial)
        logging.debug('Enabled Channel')

    def get_distance(self):
        device_value = self.get_position()
        real_unit = c_double()
        unit_type = UnitTypeEnum.DISTANCE
        unit = convert_to_enum(unit_type, UnitTypeEnum)
        self.sdk.SCC_GetRealValueFromDeviceUnit(self._serial, device_value, byref(real_unit), unit)
        logging.debug(f'Current Distance: {real_unit.value}')
        return real_unit.value

    def move_distance(self, value, unit_type='distance'):
        if unit_type == 'distance':
            unit_type = UnitTypeEnum.DISTANCE
            value = c_double(value)
        device_unit = c_int()
        unit = convert_to_enum(unit_type, UnitTypeEnum)
        self.sdk.SCC_GetDeviceUnitFromRealValue(self._serial, value, byref(device_unit), unit)
        self.move_to_position(device_unit.value)
        logging.debug(f'Moving to distance: {value}, position: {device_unit.value}')

    def move_relative(self, distance):
        self.sdk.SCC_MoveRelativeDistance(self._serial)
        raise NotImplementedError


    def needs_homing(self):
        resp =  self.can_move_without_homing_first()
        logging.debug(f'Needs Homing: {resp}')

    def go_home(self, wait_until_home=False):
        """
        Moves the motor to the preset home position

        wait_until_home: boolean [default: false]

        returns: None
        """
        self.sdk.SCC_Home(self._serial)
        logging.debug('Sending to home')
        if wait_until_home:
            self._wait_until_home()


    def get_position(self):
        """
        Returns device unit position
        to get actual distance use ... TODO

        :return: int
        """
        resp = self.sdk.SCC_GetPosition(self._serial)
        logging.debug(f'Current Position: {resp}')
        return resp

    def move_to_position(self, position):
        """
        Moves the motors to device unit location

        :param position:  <int> in device units
        :return:  None
        """
        logging.debug(f'Moving to position: {position}')
        self.sdk.SCC_MoveToPosition(self._serial, position)

    def _wait_until_home(self, timeout=10):
        """
        loops, blocking functions until position value reaches 0

        :return: None
        """
        logging.debug('Waiting until home')
        start_time = time.time()
        while True:
            position = self.get_position()
            if int(position) == 0: break
            if (time.time() - start_time) > timeout:
                logging.error('Motor Could not find home')
                raise TimeoutError('Motor could not find home, timeout')

if __name__ == '__main__':
    import sys, os # having some issues with extending path so will just CD for now
    from ctypes import *
    root_path = r'C:\Program Files\Thorlabs\Kinesis'
    #sys.path.extend()
    os.chdir(r'C:\Program Files\Thorlabs\Kinesis') # doing this until i can see why append fails
    #sys.path.insert(1, root_path)
    p = r'Thorlabs.MotionControl.KCube.StepperMotor.dll'
    device_man_dll = r'Thorlabs.MotionControl.DeviceManager.dll'


    man = cdll.LoadLibrary(device_man_dll)
    #print(man.__dict__)
    #man.SimulationManager.InitializeSimulations()
    #man.SimlationManager.UninitializeSimulations()



    kin = KST101(26000009)

    print(kin.get_device_list_size())