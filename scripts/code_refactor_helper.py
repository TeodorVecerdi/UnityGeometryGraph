#!python {0}

import re, os, sys

search_for = r"Initialize\((.*?), (.*?)\);"
add_after = r"public class (.*?) \{"
remove_line = r"\s+Initialize\((.*?)\);"

def refactor(code: str) -> str:
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

def refactor_file(file_path: str) -> None:
    with open(file_path, 'r') as f:
        code = f.read()
    refactored_code = refactor(code)
    with open(file_path, 'w') as f:
        f.write(refactored_code)

def refactor_folder_recursive(folder_path: str) -> None:
    for file_name in os.listdir(folder_path):
        file_path = os.path.join(folder_path, file_name)
        if os.path.isdir(file_path):
            refactor_folder_recursive(file_path)
        elif file_name.endswith(".cs"):
            refactor_file(file_path)

def main() -> None:
    path = sys.argv[1]
    if len(sys.argv) > 2:
        mode = sys.argv[2]
    else:
        mode = '--file'

    if mode == '--file':
        refactor_file(path)
    elif mode == '--folder':
        refactor_folder_recursive(path)
    else:
        print(f'Invalid mode "{mode}"')


if __name__ == '__main__':
    main()