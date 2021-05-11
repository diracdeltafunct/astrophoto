from enum import IntEnum
from ctypes import *

'''
File Contains C types enums for passing structs
to device hardware
'''

class TravelDirectionEnum(IntEnum):
    MOT_TravelDirectionDisabled = 0x00
    MOT_Forwards = 0x01
    MOT_Reverse = 0x02

class JogModesEnum(IntEnum):
    MOT_JogModeUndefined = 0x00
    MOT_Continuous = 0x01
    MOT_SingleStep = 0x02


class StopModesEnum(IntEnum):
    MOT_StopModeUndefined = 0x00
    MOT_Immediate = 0x01
    MOT_Profiled = 0x02


class VelocityParameters(Structure):
    _fields_ = [
        ('minVelocity', c_int),
        ('acceleration', c_int),
        ('maxVelocity', c_int),
    ]

class JogParameters(Structure):
    _fields_ = [
        ('mode', c_short),
        ('stepSize', c_uint),
        ('velParams', VelocityParameters),
        ('stopMode', c_short),
    ]

class JogModesEnum(IntEnum):
    MOT_JogModeUndefined = 0x00
    MOT_Continuous = 0x01
    MOT_SingleStep = 0x02


class StopModesEnum(IntEnum):
    MOT_StopModeUndefined = 0x00
    MOT_Immediate = 0x01
    MOT_Profiled = 0x02

class UnitTypeEnum(IntEnum):
    DISTANCE = 0
    VELOCITY = 1
    ACCELERATION = 2