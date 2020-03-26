import os
import pyperclip
import string
import sys

file_path = sys.argv[1]
split_path = file_path.split('\\')
file_data = None
file_name = None
output_buffer = []
spaces_per_tab = 4

print("Opening " + file_path)

for part in split_path:
    if '.cs' in part:
        file_name = part.split('.')[0]

with open(file_path, 'r') as file:
    file_data = file.read().split('\n')

lines_to_skip = 0
trailing_braces = 0
de_indent = 0
expected_endbraces = 0

for line in file_data:
    if lines_to_skip > 0:
        lines_to_skip -= 1
        continue
    if 'using' in line:
        continue
    if ': MyGridProgram' in line:
        trailing_braces += 1
        lines_to_skip = 1
        de_indent += spaces_per_tab
        continue
    if 'namespace' in line:
        trailing_braces += 1
        lines_to_skip = 1
        de_indent += spaces_per_tab
        continue
    if 'public ' + file_name in line:
        line = line.replace(file_name, 'Program')
    if '{' in line:
        expected_endbraces += 1
    if '}' in line:
        if expected_endbraces > 0:
            expected_endbraces -= 1
        else:
            continue
    indent = len(line) - len(line.lstrip(' '))
    if indent >= de_indent:
        line = line[de_indent:]
    output_buffer.append(line)

separator = '\n'
output = separator.join(output_buffer)
pyperclip.copy(output)
