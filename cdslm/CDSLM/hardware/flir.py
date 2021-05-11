from collections import Mapping
from PIL import Image
import PySpin
import logging

logger = logging.getLogger(__name__)


class TriggerType:
    SOFTWARE = 1
    HARDWARE = 2


class CameraArray(Mapping):
    """Container to hold multiple camera instances
    lookup works like a dictionary by the camera serials
    location information is currently stored separately"""

    def __init__(self):
        self._system = PySpin.System.GetInstance()
        self._cam_list = self.get_cam_list()
        self._cams = {}
        for i, c in enumerate(self._cam_list):

            camera = Camera(c)
            self._cams[camera.serial_number] = camera

    def get_cam_list(self):
        """
        returns a list of all connected cameras
        :return:
        """
        return self._system.GetCameras()

    def __iter__(self):
        for cam in self._cams:
            yield cam

    def __repr__(self):
        return f'CameraArray({len(self)} cameras)'

    def __len__(self):
        return len(self._cams)

    def __getitem__(self, item):
        return self._cams[item]

    def __del__(self):
        """Important for cleanup else camera is not shut down propperly
        and throws an error in the console.  deletes all camears then the array then clears
        the camera connections in the api"""

        for serial, cam in self._cams.items():
            del cam.cam
            del cam
        del self._cams
        self._cam_list.Clear()

        # Release system instance
        self._system.ReleaseInstance()


class Camera:
    """Class to control camera
    mainly uses api class but rewritten in such a way to better facilitate the
    usecase for the CDSLM instrument"""
    _serial = None

    def __init__(self, cam):
        self._nodemap_tldevice = cam.GetTLDeviceNodeMap()
        self.cam = cam
        self.cam.Init()
        self._nodemap = cam.GetNodeMap()

    def __del__(self):
        """cleanup to return camera to default state"""
        try:
            # tries to reset camera to defaults, if it has already been done
            # just passes
            self.reset_trigger()
            self.cam.EndAcquisition()
        except Exception:
            pass

        self.cam.DeInit()
        del self._nodemap_tldevice
        del self._nodemap
        del self._serial

    @property
    def serial_number(self)-> str:
        """returns the serial number in string format for connected camera

        caches value for future requests because this value will not change"""
        if self._serial is None:
            node_device_serial_number = PySpin.CStringPtr(self._nodemap_tldevice.GetNode('DeviceSerialNumber'))
            if PySpin.IsAvailable(node_device_serial_number) and PySpin.IsReadable(node_device_serial_number):
                self._serial = node_device_serial_number.GetValue()
        return self._serial

    def _set_trigger_mode(self, state='Off'):
        """sets the camera to work with external trigger or turns off external trigger
        state must be strong on or off"""
        if state.lower() not in ['on', 'off']:
            raise ValueError(f'Unknown value {state} must be on or off')
        node_trigger_mode = PySpin.CEnumerationPtr(self._nodemap.GetNode('TriggerMode'))
        if not PySpin.IsAvailable(node_trigger_mode) or not PySpin.IsReadable(node_trigger_mode):
            logger.error('Unable to disable trigger mode (node retrieval). Aborting...')
            return False

        node_trigger_mode_off = node_trigger_mode.GetEntryByName(state.title())
        if not PySpin.IsAvailable(node_trigger_mode_off) or not PySpin.IsReadable(node_trigger_mode_off):
            logger.error('Unable to disable trigger mode (enum entry retrieval). Aborting...')
            return False
        node_trigger_mode.SetIntValue(node_trigger_mode_off.GetValue())

    def setup_trigger(self, trigger_type=TriggerType.HARDWARE):
        """does full setup of trigger.  Expects trigger type hardware or software"""

        self._set_trigger_mode('Off')  # turn off trigger before setting
        self._set_trigger_source(trigger_type)


    def _set_trigger_source(self, trigger_type):
        """sets trigger type to HARDWARE (external) or SOFTWARE (force trigger)
                defaults to Hardware trigger"""
        node_trigger_source = PySpin.CEnumerationPtr(self._nodemap.GetNode('TriggerSource'))
        if not PySpin.IsAvailable(node_trigger_source) or not PySpin.IsWritable(node_trigger_source):
            logger.error('Unable to get trigger source (node retrieval). Aborting...')
            return False

        if trigger_type == TriggerType.SOFTWARE:
            node_trigger_source_software = node_trigger_source.GetEntryByName('Software')
            if not PySpin.IsAvailable(node_trigger_source_software) or not PySpin.IsReadable(
                    node_trigger_source_software):
                logger.error('Unable to set trigger source (enum entry retrieval). Aborting...')
                return False
            node_trigger_source.SetIntValue(node_trigger_source_software.GetValue())

        elif trigger_type == TriggerType.HARDWARE:
            node_trigger_source_hardware = node_trigger_source.GetEntryByName('Line3')
            if not PySpin.IsAvailable(node_trigger_source_hardware) or not PySpin.IsReadable(
                    node_trigger_source_hardware):
                logger.error('Unable to set trigger source (enum entry retrieval). Aborting...')
                return False
            node_trigger_source.SetIntValue(node_trigger_source_hardware.GetValue())

        self.set_exposure()
        self._set_trigger_mode('On')

    def reset_trigger(self):
        """Resets trigger value to off and gives a basic exposure rate"""
        self.set_exposure(1)
        self._set_trigger_mode('Off')

    def force_trigger(self):
        """Activates software trigger"""
        node_softwaretrigger_cmd = PySpin.CCommandPtr(self._nodemap.GetNode('TriggerSoftware'))
        if not PySpin.IsAvailable(node_softwaretrigger_cmd) or not PySpin.IsWritable(node_softwaretrigger_cmd):
            logger.error('Unable to execute trigger. Aborting...')
            return False

        node_softwaretrigger_cmd.Execute()
        
    def begin_acquisition(self):
        """sets up the acquistion...must be called before any photo is pulled
        from device"""

        node_acquisition_mode = PySpin.CEnumerationPtr(self._nodemap.GetNode('AcquisitionMode'))
        if not PySpin.IsAvailable(node_acquisition_mode) or not PySpin.IsWritable(node_acquisition_mode):
            logger.error('Unable to set acquisition mode to continuous (enum retrieval). Aborting...')
            return False

        node_acquisition_mode_continuous = node_acquisition_mode.GetEntryByName('Continuous')
        if not PySpin.IsAvailable(node_acquisition_mode_continuous) or not PySpin.IsReadable(
                node_acquisition_mode_continuous):
            logger.error('Unable to set acquisition mode to continuous (entry retrieval). Aborting...')
            return False

        acquisition_mode_continuous = node_acquisition_mode_continuous.GetValue()

        node_acquisition_mode.SetIntValue(acquisition_mode_continuous)

        self.cam.BeginAcquisition()

    def set_exposure(self, value=None):
        """sets exposure value when using in autotrigger mode"""

        exp_mode = PySpin.CEnumerationPtr(self._nodemap.GetNode('ExposureMode'))
        if value is None:
            exp = exp_mode.GetEntryByName('TriggerWidth')
        else:
            exp = exp_mode.GetEntryByName('Timed')
        exp_mode.SetIntValue(exp.GetValue())
        # if value: TODO SET A VALUE
        #    time = exp_mode.GetEntryByName('ExposureTime')
        #    exp_mode.SetIntValue()

    @staticmethod
    def save_image(image, filename=None):
        """exports image directly to file"""
        image.Save(filename)

    def acquire_image(self):
        """Retrieve, convert, and save images
        returns image as image object below
        """

        try:
            image_result = self.cam.GetNextImage()
            current_image = ImageObject(image_result)
            image_result.Release()
            return current_image

        except PySpin.SpinnakerException as ex:
            logger.error('Error: %s' % ex)
            return False

    def end_acq(self):
        """closes acquisition on camera..important in cleanup"""

        self.cam.EndAcquisition()


class ImageObject:
    """
    Data Holder class for image types.  Accepts swig image from
    camera driver on input and allows output to various other datatypes
    with caching for performance at the cost of memory
    """
    _raw = None
    _converted = None
    _array = None
    _img = None

    def __init__(self, image):
        self._raw = image
        if image.IsIncomplete():
            raise IOError
        self.convert()

    def convert(self, cache=True):
        """
        Converts camera binary image into the correct pixel format and scale for analysis

        :param cache:  boolean, true stores the last version in memory for faster future access
        :return:
        """
        if not cache or self._converted is None:
            self._converted = self._raw.Convert(PySpin.PixelFormat_Mono8, PySpin.HQ_LINEAR)
        return self._converted

    @property
    def array(self):
        """
        returns a numpy arrray of the camera image, best for onscreen display in math libraries
        """
        if self._array is None:
            self._array = self._converted.GetNDArray()
        return self._array

    @property
    def image(self):
        """ returns a PIL image object from the image array.  best for viewing outside of
        software"""
        if self._img is None:
            self._img = Image.fromarray(self.array.astype('uint8'), 'L')
        return self._img

if __name__ == '__main__':
    from matplotlib import pyplot as plt
    cameras = CameraArray()
    camera = list(cameras.values())[0]
    print(camera)
    camera.set_exposure(100)

    camera.begin_acquisition()
    image = camera.acquire_image()
    print(image)
    camera.end_acq()