from enum import Enum

def convert_to_enum(item, enum, prefix='', to_upper=False):
    if isinstance(item, Enum):
        return item

    elif isinstance(item, (int, float)):
        try:
            return enum(item)
        except ValueError:
            raise ValueError('Cannon convert int/float to enum')
    elif isinstance(item, str):
        member = item.replace(' ', '_')
        if to_upper:
            member = member.upper()
        if prefix and not member.startswith(prefix):
            member = prefix + member
        try:
            return enum[member]
        except KeyError:
            raise KeyError('Invalid key in converting enum')
    else:
        raise ValueError('Invalid type for enum')


def check(result):
    if result != 0:
        raise ValueError('Function failed')
    else:
        return result