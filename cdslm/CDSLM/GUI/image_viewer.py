from PyQt5 import QtWidgets, QtCore, QtGui
from PIL import Image
from CDSLM.hardware import flir
import pyqtgraph as pg


import numpy as np


class PlotSelector(QtWidgets.QWidget):

    def __init__(self):
        super().__init__()
        layout = QtWidgets.QHBoxLayout()
        self.setLayout(layout)

        self.single_plot_radio = QtWidgets.QRadioButton('Single Camera')
        self.multi_plot_radio =  QtWidgets.QRadioButton('Multi Camera')

        layout.addWidget(self.single_plot_radio)
        layout.addWidget(self.multi_plot_radio)


class CameraSelector(QtWidgets.QWidget):

    pass



class MultiView(QtWidgets.QWidget):

    def __init__(self, graphs=3):
        pass

class ImageViewer(QtWidgets.QWidget):
    _image = None

    def __init__(self, model=None):
        super().__init__()
        self.model = model
        layout = QtWidgets.QVBoxLayout()
        self.setLayout(layout)

        self.plot_selector = PlotSelector()
        self.gv = pg.GraphicsView()

        layout.addWidget(self.gv)
        layout.addWidget(self.plot_selector)

        self.cameras = self.model.cameras

        if len(self.cameras) > 0:
            self.camera = self.cameras[self.cameras.keys()[0]]
        else:
            self.camera = None

        if self.camera:
            self.camera.begin_acquisition()

        self.glayout = pg.GraphicsLayout()
        self.vb = self.glayout.addViewBox()
        self.gv.setCentralItem(self.glayout)

        self.get_img()

        self.timer = QtCore.QTimer()
        self.timer.timeout.connect(self.get_img)
        self.timer.start(200)

    def get_img(self):
        #self.camera.force_trigger()
        if self.camera:
            img = self.camera.acquire_image()
            self.update_image(img)

    def update_image(self, data):
        if not isinstance(data, np.ndarray):
            data = data.array

        if self._image is None:
            self._image = pg.ImageItem(data)
            self.vb.addItem(self._image)
            print(self._image)
        else:
            self._image.setImage(data)

        self.vb.autoRange()






class ImageLabel(QtWidgets.QLabel):

    def __init__(self):
        super().__init__()
        self.resize(640, 640)

    def update(self, data):
        img = data
        arr = img.array
        img = QtGui.QImage(arr.data, arr.shape[1], arr.shape[0], QtGui.QImage.Format_Grayscale8)
        self.setPixmap(QtGui.QPixmap.fromImage(img))





if __name__ == '__main__':
    app = QtWidgets.QApplication([])
    w = ImageViewer()
    w.show()
    app.exec_()
