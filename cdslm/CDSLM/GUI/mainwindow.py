from PyQt5 import QtWidgets
from CDSLM import __version__
from CDSLM.GUI import image_viewer
from CDSLM.GUI.tabbook import TabBook


class CDSLMMainWindow(QtWidgets.QMainWindow):

    def __init__(self, model=None):
        self.model = model
        super().__init__()

        self.setWindowTitle(f'CDSLM - v{__version__.__version__}')

        central_widget = QtWidgets.QWidget()
        layout = QtWidgets.QGridLayout()
        central_widget.setLayout(layout)
        self.setCentralWidget(central_widget)

        widgets = [('single_image', image_viewer.ImageViewer(self.model.hardware)),
                   ]
        self.image_area = ImageArea(widgets)
        self.tab_widget = TabBook(model)

        layout.addWidget(self.image_area)
        layout.addWidget(self.tab_widget)


class ImageArea(QtWidgets.QStackedWidget):

    def __init__(self, widgets):
        super().__init__()
        for i, (name, widget) in enumerate(widgets):
            setattr(self, name, widget)
            self.addWidget(getattr(self, name))
            getattr(self, name).index = i



if __name__ == '__main__':
    from CDSLM.models import CDSLMModel
    app = QtWidgets.QApplication([])
    w = CDSLMMainWindow(model=CDSLMModel())
    w.show()
    app.exec_()