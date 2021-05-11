from PyQt5 import QtCore

from CDSLM.hardware.thor_motor import KST101
from CDSLM.hardware import flir
from CDSLM.paths import Paths
import configparser

import os
from collections import Mapping
from collections import OrderedDict


class CDSLMModel:
    signals_model = None

    def __init__(self):
        self.settings = SettingsModel(self)
        self.hardware = HardwareModel(self)

    def create_signals_model(self):
        # app needs to be created before making model
        # so this method needs to be called after app is created
        self.signals_model = SignalsModel()


class SettingsModel:
    _path = Paths.settings_path()

    def __init__(self, parent=None):
        self._check_file()
        self._parser = configparser.ConfigParser()
        self.defaults() # set defaults
        self.read_file()

    def __getitem__(self, item):
        return self._parser[item]

    def read_file(self):
        self._parser.read(self._path)

    def write_to_file(self):
        with open(self._path, 'w') as configfile:  # save
            self._parser.write(configfile)

    def _check_file(self):
        if not os.path.exists(self._path):
            with open(self._path, 'w') as f:
                pass # make empty file

    def defaults(self):
        self._parser.add_section('general')
        self._parser.add_section('cameras')
        self['cameras']['camera_1'] = '2342312'
        self['cameras']['camera_2'] = '2342352'
        self['cameras']['camera_3'] = '2342352'
        self['cameras']['camera_4'] = '2342352'
        self['cameras']['camera_5'] = '2342352'
        self._parser.add_section('motor_serial')
        self['motor_serial']['x'] = '26000767'
        self['motor_serial']['y'] = '26000884'
        self['motor_serial']['z'] = '26001318'
        self._parser.add_section('motor_type')
        self['motor_type']['x'] = ''
        self['motor_type']['y'] = ''
        self['motor_type']['z'] = ''

    @property
    def camera_values(self):
        return {c: v for c, v in self['cameras'].items()}

    @property
    def motor_values(self):
        return {m:{'serial':v, 'type':self['motor_type'][m]} for m, v in self['motor_serial'].items()}




class HardwareModel:
    motors = None

    def __init__(self, parent_model=None):
        # temp fake addresses
        #self._parent_model = parent_model
        settings = parent_model.settings
        self.setup_motors(settings.motor_values)
        self.setup_cameras()

    def setup_motors(self, addresses):

        self.motors = MotorModel(addresses)

    def setup_cameras(self):
        self.cameras = flir.CameraArray()



class MotorModel(Mapping):


    names = ['x_motor', 'y_motor', 'z_motor']

    def __init__(self, addresses):
        for key, value in addresses.items():
            # TODO make motor type selectable
            setattr(self, f'{key}_motor', KST101(value))

    @property
    def motors(self):
        for name in self.names:
            yield getattr(self, name)

    def __getitem__(self, item):
        return getattr(self, item)

    def __iter__(self):
        for name in self.names:
            yield name

    def __len__(self):
        return len(self.names)

class StateSignals(QtCore.QObject):
    run_signal = QtCore.pyqtSignal()
    stop_signal = QtCore.pyqtSignal()

class SignalsModel:
    state_signals = StateSignals()





if __name__ == '__main__':
    #HardwareModel()

    x = SettingsModel()
    print(x['cameras'])
    print(x['cameras']['camera_1'])
    x.write_to_file()

    print(x.camera_values)
    print(x.motor_values)


