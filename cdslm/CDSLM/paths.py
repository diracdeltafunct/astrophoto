import os
import sys
import CDSLM


class Paths:
    """
    class with listed class methods
    each method returns a path object to the directory of the common file in question

    usage:
    from CDSLM.paths import Paths
    target_path = Paths.<method for filepath>(*options)
    """
    @classmethod
    def settings_path(cls):
        return os.path.join(cls.appdata(), 'settings.ini')

    @classmethod
    def kinesis_dll_path(cls):
        return os.path.join('')

    @classmethod
    def kinesis_device(cls):
        return os.path.join('')

    @classmethod
    def get_package_dir(cls):
        package_path = CDSLM.__path__[0]
        return os.path.abspath(package_path)

    @classmethod
    def get_app_dir(cls):
        """
        Returns:
            str: the root path of the application.

            This is either the root of the repository in development mode,
            or the installed location of the application when frozen.
        """
        if getattr(sys, 'frozen', False):
            # The application is frozen
            return os.path.dirname(sys.executable)
        else:
            return os.path.normpath(os.path.join(cls.get_package_dir(), '..'))

    @classmethod
    def appdata(cls):
        return os.path.join(os.path.expanduser('~'), 'AppData', 'Local', 'CDSLM')


# on load make sure the app data folder exists before trying to do anything
if not os.path.isdir(Paths().appdata()):
    os.mkdir(Paths().appdata())
