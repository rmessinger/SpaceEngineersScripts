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
de_indent = 0
expected_endbraces = 0

for line in file_data:
    if lines_to_skip > 0:
        lines_to_skip -= 1
        continue
    # using lines can be safely skipped without any later considerations
    if 'using' in line:
        continue
    # namespace enclosures should be skipped
    if 'namespace' in line:
        # so should their subsequent opening brace
        lines_to_skip = 1
        # removing this level of scope requires de-indenting
        de_indent += spaces_per_tab
        continue
    # extension of MyGridProgram signifies this begins the class definition
    if ': MyGridProgram' in line:
        # class definitions should also trim the following opening brace
        lines_to_skip = 1
        # add this to the indent butcher's bill
        de_indent += spaces_per_tab
        continue
    # SE programmable blocks are all of class "Program"
    # replace the class-specific constructor with one the game compiler won't hate
    if 'public ' + file_name in line:
        line = line.replace(file_name, 'Program')
    # If we've gotten this far, this brace begins a valid scope within the in-game class
    # increment the number of expected closing braces so that they don't get skipped
    if '{' in line:
        expected_endbraces += 1
    # Closing brace. Check if we've been expecting one of those
    # this should only be false for the last few lines of the file
    if '}' in line:
        if expected_endbraces > 0:
            expected_endbraces -= 1
        else:
            continue
    # Indents that don't match the surrounding style are gross
    # De-indent all lines by the amount subtracted by removing class, namespace, etc
    # But only remove whitespace
    indent = len(line) - len(line.lstrip(' '))
    if indent >= de_indent:
        line = line[de_indent:]
    output_buffer.append(line)

# Join all these line entries into a single string
separator = '\n'
output = separator.join(output_buffer)
# Shoot it into the clipboard
pyperclip.copy(output)
