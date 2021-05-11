from PyQt5 import QtWidgets, QtCore


cameras = {'12345': 'asdf',
'22345': 'asdf',
'32345': 'asdf',
'42345': 'asdf',
'52345': 'asdf',
           }

class TabBook(QtWidgets.QTabWidget):  #placeholder
    _tabs = ('Start', 'Setup', 'Review')

    def __init__(self, model):
        super().__init__()
        self.tabs = []

        for i, t in enumerate(self._tabs):
            print(t)
            print(globals()[f'{t.capitalize()}Tab'])
            tab = globals()[f'{t.capitalize()}Tab'](model)
            self.tabs.append(tab)
            self.insertTab(i, tab, t)


class StartTab(QtWidgets.QWidget):
    def __init__(self, model):
        super().__init__()


class SetupTab(QtWidgets.QWidget):
    def __init__(self, model=None):
        super().__init__()
        _layout = QtWidgets.QHBoxLayout()
        self.setLayout(_layout)

        self.camera_selector = CameraSelector(cameras) # pass ncams from model here
        self.motor_settings = MotorExperimentGroup(model.hardware.motors)

        _layout.addWidget(self.camera_selector)
        _layout.addWidget(self.motor_settings)


class ReviewTab(QtWidgets.QWidget):
    def __init__(self, model):
        super().__init__()


class CameraSelector(QtWidgets.QGroupBox):

    def __init__(self, cameras):
        super().__init__()
        self.setTitle('Select Cameras to Use')
        _layout = QtWidgets.QVBoxLayout()
        self.setLayout(_layout)
        self.camera_radios = {}

        for cam_serial, camera in cameras.items():
            self.camera_radios[cam_serial] = QtWidgets.QCheckBox(cam_serial, checked=True)
            _layout.addWidget(self.camera_radios[cam_serial])





class ExperimentButtons(QtWidgets.QWidget):


    def __init__(self, signals):
        super().__init__()
        self.signals = signals
        _layout = QtWidgets.QVBoxLayout()
        self.setLayout(_layout)

        self.run_btn = QtWidgets.QPushButton('Run')
        self.cancel_btn = QtWidgets.QPushButton('Stop')

    def enable(self):
        self.run_btn.setEnabled(True)
        self.cancel_btn.setEnabled(False)
        self.run_btn.setText('Run')

    def disable(self):
        self.run_btn.setEnabled(False)
        self.cancel_btn.setEnabled(True)
        self.run_btn.setText('Running')

    def run(self):
        self.signals.run_signal.emit()
        self.disable()

    def stop(self):
        self.signals.stop_signal.emit()
        self.enable()


class MotorExperimentGroup(QtWidgets.QGroupBox):
    header_formatter = '<b>{}</b>'

    def __init__(self, motor_model):
        self.motor_model = motor_model
        super().__init__()
        _layout = QtWidgets.QVBoxLayout()
        self.setLayout(_layout)
        _layout.addLayout(self.header)
        self._motor_settings = {}
        for motor_name, motor in motor_model.items():
            name = motor_name.replace('_', ' ').title()
            motor_setting = MotorExperimentSettings(0, 10000,  # TODO USE REAL SETTINGS
                                                                 name,
                                                                 start=0,
                                                                 stop=10)
            self._motor_settings[name] = motor_setting


            _layout.addWidget(motor_setting)


    @property
    def header(self):
        _layout = QtWidgets.QHBoxLayout()

        widgets = (QtWidgets.QLabel(''),
                QtWidgets.QLabel(self.header_formatter.format('Start')),
                QtWidgets.QLabel(self.header_formatter.format('Stop'))
                )
        for widget in widgets:
            _layout.addWidget(widget)

        return _layout



class MotorExperimentSettings(QtWidgets.QWidget):

    def __init__(self, min, max, label=None, start=0, stop=0):
        super().__init__()
        _layout = QtWidgets.QHBoxLayout()
        self.setLayout(_layout)

        if label is None:
            label = 'Motor Name'
        self.label = QtWidgets.QLabel(label)
        self.position_start = QtWidgets.QDoubleSpinBox(minimum=min,
                                                       maximum=max,
                                                       value=start)
        self.position_stop = QtWidgets.QDoubleSpinBox(minimum=min,
                                                       maximum=max,
                                                       value=stop)
        _layout.addWidget(self.label)
        _layout.addWidget(self.position_start)
        _layout.addWidget(self.position_stop)
