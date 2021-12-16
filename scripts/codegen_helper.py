#!python {0}

from enum import Enum, IntEnum
from typing import List, NewType, Optional, Tuple
import sys


class PropertyType(IntEnum):
    IN = 0
    SETTING = 1
    OUT = 2

    @staticmethod
    def from_string(string: str) -> 'PropertyType':
        if string == 'i':
            return PropertyType.IN
        elif string == 'o':
            return PropertyType.OUT
        else:
            return PropertyType.SETTING

    @staticmethod
    def to_string(property_type: 'PropertyType') -> str:
        if property_type == PropertyType.IN:
            return '[In]'
        elif property_type == PropertyType.OUT:
            return '[Out]'
        else:
            return '[Setting]'

class Mode(Enum):
    EDITOR = 0
    RUNTIME = 1
    BOTH = 2


Property = NewType('Property', Tuple[PropertyType, str, str, Optional[str]])
editor_fields = {
    'float': 'FloatField',
    'int': 'IntegerField',
    'bool': 'Toggle',
    'string': 'TextField',
    'float3': 'Vector3Field',
}
editor_value_blacklist = ['GeometryData', 'CurveData']

def run(mode: Mode) -> None:
    properties: List[Property] = []
    usings: List[str] = []
    while True:
        entry = input()
        if entry == '': break
        vars = entry.split(' ')

        if vars[0] == 'u':
            usings.append(f'using {vars[1]} = {vars[2]};')
        elif vars[0] in ['i', 'o', 's']:
            properties.append(Property((PropertyType.from_string(vars[0]), vars[1], vars[2], vars[3] if len(vars) > 3 else None)))


    properties.sort(key=lambda tup: tup[0])

    if len(usings) > 0:
        print('\n'.join(usings))

    if len(properties) <= 0:
        return

    if mode is Mode.EDITOR or mode is Mode.BOTH:
        print_props_editor(properties)
    if mode is Mode.RUNTIME or mode is Mode.BOTH:
        print_props_runtime(properties)

def print_props_editor(props: List[Property]) -> None:
    ports: str = ''
    for prop in props:
        if prop[0] == PropertyType.SETTING:
            continue
        ports += f'private GraphFrameworkPort {camel_case(prop[2])}Port;\n'

    fields: str = ''
    values: str = ''
    for prop in props:
        if prop[1] not in editor_value_blacklist and prop[0] != PropertyType.OUT:
            if prop[1] in editor_fields:
                fields += f'private {editor_fields[prop[1]]} {camel_case(prop[2])}Field;\n'
            else:
                fields += f'private FieldTypeFor<{prop[1]}> {camel_case(prop[2])}Field;\n'
            values += f'private {prop[1]} {camel_case(prop[2])}{(f" = {prop[3]}" if prop[3] is not None else "")};\n'
    print(f'''{ports.strip()}\n\n{fields.strip()}\n\n{values.strip()}'''.strip())


def print_props_runtime(props: List[Property]) -> None:
    prop_str = '';
    for prop in props:
        prop_str += f'{PropertyType.to_string(prop[0])} public {prop[1]} {prop[2]} {{ get; private set; }}{(f" = {prop[3]};" if prop[3] is not None else "")}\n'
    print(prop_str.strip())

def camel_case(string: str) -> str:
    return string[0].lower() + string[1:]

if __name__ == '__main__':
    # Get first argument
    mode: Mode = Mode.BOTH
    if len(sys.argv) > 1:
        if sys.argv[1] == '--editor' or sys.argv[1] == '-e':
            mode = Mode.EDITOR
        elif sys.argv[1] == '--runtime' or sys.argv[1] == '-r':
            mode = Mode.RUNTIME
        elif sys.argv[1] == '--both' or sys.argv[1] == '-b':
            mode = Mode.BOTH

    run(mode)
