def walk_model(dict_values, model):
    for key, value in dict_values.items():
        if not isinstance(value, dict):
            assert model[key] == value
        else:
            walk_model(dict_values[key], model[key])


def test_settings_defaults(walk_model):
    from CDSLM.models import SettingsModel


    model = SettingsModel()

    values = {'cameras':{
                'camera_1': '2342352'}
    }
    walk_model(values, model)
