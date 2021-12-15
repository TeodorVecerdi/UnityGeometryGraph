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


Property = NewType('Property', Tuple[PropertyType, str, str, Optional[str]])
editor_fields = {
    'float': 'FloatField',
    'int': 'IntegerField',
    'bool': 'Toggle',
    'string': 'TextField',
    'float3': 'Vector3Field',
}
editor_value_blacklist = ['GeometryData', 'CurveData']

def main(editor_mode: bool) -> None:
    properties: List[Property] = []
    while True:
        i = input()
        if i == '':
            break
        split = i.split(' ')

        properties.append(Property((PropertyType.from_string(split[0]), split[1], split[2], split[3] if len(split) > 3 else None)))
    properties.sort(key=lambda tup: tup[0])
    if editor_mode:
        print_props_editor(properties)
    else:
        for prop in properties:
            print_prop(prop)

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

    print(f'''{ports.strip()}

{fields.strip()}

{values.strip()}'''.strip())


def print_prop(prop: Property) -> None:
    prop_str = '';
    if prop[0] == PropertyType.IN:
        prop_str += '[In] '
    elif prop[0] == PropertyType.OUT:
        prop_str += '[Out] '
    elif prop[0] == PropertyType.SETTING:
        prop_str += '[Setting] '
    prop_str += f'public {prop[1]} {prop[2]} {{ get; private set; }}'
    if prop[3] is not None:
        prop_str += f' = {prop[3]};'
    print(prop_str)

def camel_case(string: str) -> str:
    return string[0].lower() + string[1:]

if __name__ == '__main__':
    # Get first argument
    editor_mode: bool = False
    if len(sys.argv) > 1:
        editor_mode = sys.argv[1] == '--edit' or sys.argv[1] == '-e'

    main(editor_mode)
