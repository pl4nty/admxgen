# admxgen

[![Build Status](https://ci.appveyor.com/api/projects/status/git/lordjeb/win32cpp?svg=true)](https://ci.appveyor.com/project/lordjeb/admxgen)

admxgen is a simple tools designed to help in generation of group policy template files. These files can be difficult to edit by hand, particularly when they become large. admxgen takes a simple csv file that can be edited in Microsoft Excel (or any text editor) and generates a properly formatted and cross-linked .admx and .adml file.

Usage:

`admxgen.exe [input_file] [output_filename]`
