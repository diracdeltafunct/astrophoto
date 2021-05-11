import logging

from PyQt5.QtWidgets import QApplication
from CDSLM import app as cdslm_app
from CDSLM.models import CDSLMModel
from multiprocessing import freeze_support

logger = logging.getLogger(__name__)

def main(argv, build=None):

    # config logs

    qapp = QApplication(argv)
    try:
        models = CDSLMModel()
        app = cdslm_app.App(qapp, models)
        app.start()
        result = qapp.exec_()

        return result
    except Exception:
        logger.exception('Exception in main handler')
        raise
    finally:
        logger.info('shutting down')
        logging.shutdown()





if __name__ == '__main__':
    freeze_support()
    import sys
    import warnings

    warnings.filterwarnings('ignore', r'divide by zero encountered in true_divide')
    main(sys.argv)
