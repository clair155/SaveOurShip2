import re
import os

filename = "./Source/1.5/ShipInteriorMod2.cs"
version_pattern = re.compile(r'public const string SOS2version = "GithubV2\.\d+\.\d+";')

with open(filename, 'r', encoding='utf_8_sig') as f:
    lines = f.readlines()

new_lines = []
updated = False

for line in lines:
    match = version_pattern.search(line)
    if match:
        # Extract the current version string
        version_str = match.group(0)
        # Extract the numbers
        numbers = re.findall(r'\d+', version_str)
        if len(numbers) == 4:
            constant, major, minor, patch = map(int, numbers)
        else:
            new_lines.append(line)
            continue
        # Increment patch
        patch += 1
        new_version = f'GithubV{major}.{minor}.{patch}'
        new_line = re.sub(r'GithubV2\.\d+\.\d+', new_version, line)
        new_lines.append(new_line)
        updated = True
        print(f"Updated version: {new_version}")
    else:
        new_lines.append(line)

if updated:
    with open(filename, 'w', encoding='utf_8_sig') as f:
        f.writelines(new_lines)
else:
    print("Version line not found or not updated.")