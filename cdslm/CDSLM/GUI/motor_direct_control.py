from PyQt5 import QtGui, QtWidgets, QtCore

from CDSLM.models import HardwareModel

import os
os.chdir(r'C:\Program Files\Thorlabs\Kinesis')

class MotorWindow(QtWidgets.QWidget):

    def __init__(self, model=None, parent=None):
        super().__init__(parent=parent)
        layout = QtWidgets.QVBoxLayout()
        self.setLayout(layout)
        self.model = model

        motors = []
        for i, motor_name in enumerate(model.motors.names):
            print(motor_name)
            motor = getattr(model.motors, motor_name)
            motors.append(MotorWidget(motor, motor_name))
            layout.addWidget(motors[i])




class MotorWidget(QtWidgets.QWidget):
    _precision = 9

    def __init__(self, motor=None, motor_name=None, parent=None):

        super().__init__(parent=parent)
        self.motor = motor
        layout = QtWidgets.QHBoxLayout()
        self.setLayout(layout)

        self.name = QtWidgets.QLabel(motor_name.replace('_', ' ').title())
        self.current_position = QtWidgets.QDoubleSpinBox(decimals=self._precision,
                                                         minimum=self.motor.min,
                                                         maximum=self.motor.max
                                                         )
        self.current_position.setEnabled(False)
        self.target_position = QtWidgets.QDoubleSpinBox(decimals=self._precision,
                                                        minimum=self.motor.min,
                                                        maximum=self.motor.max
                                                        )
        self.move_btn = QtWidgets.QPushButton('Move')
        self.home_btn = QtWidgets.QPushButton('Home')

        layout.addWidget(self.name)
        layout.addWidget(self.current_position)
        layout.addWidget(self.target_position)
        layout.addWidget(self.move_btn)
        layout.addWidget(self.move_btn)

        self.move_btn.pressed.connect(self._move_motor)
        self.home_btn.pressed.connect(self._move_home)
        self.poll = QtCore.QTimer()
        self.poll.timeout.connect(self.update_position)
        self.poll.start(1000)

    def _move_motor(self):
        target = self.target_position.value()
        # probably want to thread this
        self.motor.move_distance(float(target))

    def _move_home(self):
        self.motor.go_home()

    def update_position(self):
        pos = self.motor.get_distance()
        self.current_position.setValue(float(pos))



if __name__ == '__main__':
    app = QtWidgets.QApplication([])
    w = MotorWindow(HardwareModel())
    w.show()
    app.exec_()


