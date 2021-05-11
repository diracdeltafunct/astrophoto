class BorgSingleton:
    _instances = {}
    def __new__(cls, *args, **kwargs):
        if cls._instances.get(cls, None) is None:
            cls._instances[cls] = super().__new__(cls)

        return BorgSingleton._instances[cls]


def setup_logging(filename=None):
    """Configure python logging using "logging.ini".

    See: https://docs.python.org/3/library/logging.config.html#configuration-file-format
    """
    import logging.config
    import os
    from CDSLM.paths import Paths
    if not filename:
        filename = os.path.join(Paths.get_app_dir(), 'logging.ini')
    logging.config.fileConfig(filename)
