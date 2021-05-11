import logging

from PyQt5 import QtGui, QtCore, QtWidgets

from CDSLM.GUI import mainwindow
from CDSLM.GUI.app_singleton import register_app_singleton
from CDSLM.models import SignalsModel

logger = logging.getLogger()


class App:

    def __init__(self, qapp, models):
        self.qapp = qapp

        self.model = models
        self.model.create_signals_model()
        self.window = mainwindow.CDSLMMainWindow(models)

        self.build_models()

    def start(self):
        register_app_singleton(self)
        self._load_splash()
        self.window.show()

    def append_signals(self):
        self.model.signals_model = SignalsModel()


    def _load_splash(self):
        # make a spash screen
        pass

    def build_models(self):
        pass

    def shutdown(self):
        try:
            if self.window:
                self.window.close()
                self.wiwndow.deleteLater()
                self.window = None
            self.qapp = None
          # self.model.shutdown()
        finally:
            register_app_singleton(None)