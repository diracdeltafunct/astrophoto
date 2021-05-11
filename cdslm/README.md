# CDSLM

The following is intended to control XX for usage in ... imaging


## Installation

### Developer Mode
To install run `python setup.py develop`

(creates simlink in python site-packages)

### User mode
To install run `python setup.py install`

(copies files to python site-packages)

## Launching Software

### From the Console

- Navigate to the cdslm root directory (*/cdslm-control-software)
- type `python CDSLM`

### From a Python Interpreter

```python
import CDSLM
CDSLM.__main__.main()
```

## First Launch and Settings

After the first lauch a settings file will be created in `%localappdata%\CDSLM`
This will contain the basic formatting for the hardware locations of all of the hardware devices
After launching open and update this settings file to reflect your hardware serial numbers.  A
GUI window to do this task is forthcoming.