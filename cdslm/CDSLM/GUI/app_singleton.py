"""
Single access point to the current global application object.
"""

_singleton = None


def register_app_singleton(app):
    """
    Application instances must be registered before calling **start()**, and
    unregister after shutdown().
    """
    from CDSLM.app import App

    global _singleton
    if app is None:
        _singleton = None
    else:
        if not isinstance(app, App):
            raise TypeError(f'{app} is not an instance of App')
        if _singleton is not None:
            raise TypeError(f'a singleton is already registered: {app}')
        _singleton = app


def get_app_singleton():
    """
    Get the current singleton application.
    """
    return _singleton
