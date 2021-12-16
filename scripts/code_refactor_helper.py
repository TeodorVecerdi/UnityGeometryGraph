#!python {0}

import re, os, sys
from typing import Callable

def refactor_0(code: str) -> str:
    '''
    Replaces the 'Initialize(title, category)' call from CreateNode with properties.
    '''
    search_for = r"Initialize\((.*?), (.*?)\);"
    add_after = r"public class (.*?) \{"
    remove_line = r"\s+Initialize\((.*?)\);"

    # Find title and category
    search_result = re.search(search_for, code)
    if search_result is None:
        return code
    title = search_result.group(1)
    category = search_result.group(2)

    added_code = f'''
        protected override string Title => {title};
        protected override NodeCategory Category => {category};
        '''
    add_after_result = re.search(add_after, code)
    add_after_position = add_after_result.end()

    # Remove Initialize()
    remove_line_result = re.search(remove_line, code)
    remove_line_start = remove_line_result.start()
    remove_line_end = remove_line_result.end()

    code_without_removed_line_start = code[:remove_line_start].strip()
    code_without_removed_line_end = code[remove_line_end:].strip()
    code_without_removed_line = code_without_removed_line_start + '\n            ' + code_without_removed_line_end
    return code_without_removed_line[:add_after_position].strip() + added_code.rstrip() + '\n\n        ' + code_without_removed_line[add_after_position:].strip()

def refactor_1(code: str) -> str:
    '''
    Changes the access modifiers of CreateNode and BindPorts to protected
    '''
    create_re = r'public override void CreateNode'
    bind_re = r'public override void BindPorts'
    replaced_create = re.sub(create_re, 'protected override void CreateNode', code)
    replaced_bind = re.sub(bind_re, 'protected override void BindPorts', replaced_create)
    return replaced_bind

def refactor_2(code: str) -> str:
    '''
    Changes the access modifiers of GetNodeData and SetNodeData to protected internal
    '''
    create_re = r'public override JObject GetNodeData'
    bind_re = r'public override void SetNodeData'
    replaced_create = re.sub(create_re, 'protected internal override JObject GetNodeData', code)
    replaced_bind = re.sub(bind_re, 'protected internal override void SetNodeData', replaced_create)
    return replaced_bind

def refactor_file(refactor_func: Callable[[str], str], file_path: str) -> None:
    with open(file_path, 'r') as f:
        code = f.read()
    refactored_code = refactor_func(code)
    with open(file_path, 'w') as f:
        f.write(refactored_code)

def refactor_folder_recursive(refactor_func: Callable[[str], str], folder_path: str) -> None:
    for file_name in os.listdir(folder_path):
        file_path = os.path.join(folder_path, file_name)
        if os.path.isdir(file_path):
            refactor_folder_recursive(refactor_func, file_path)
        elif file_name.endswith(".cs"):
            refactor_file(refactor_func, file_path)

refactor_versions = {
    0: refactor_0,
    1: refactor_1,
    2: refactor_2,
}

def main() -> None:
    version = sys.argv[1]
    refactor_function = refactor_versions[int(version)]
    path = sys.argv[2]
    if len(sys.argv) > 3:
        mode = sys.argv[3]
    else:
        mode = '--file'

    if mode == '--file':
        refactor_file(refactor_function, path)
    elif mode == '--folder':
        refactor_folder_recursive(refactor_function, path)
    else:
        print(f'Invalid mode "{mode}"')


if __name__ == '__main__':
    main()