# LogitechDpiShifter

This small program will hook into a running instance of Logitech Gaming Software and temporarily enable DPI Shift
while right click is being held. Tested with Logitech Gaming Software v9.02.65 and V9.04.49_x64. It may or may not work 
with other versions.

## Usage

1. Start Logitech Gaming Software
2. Run LogitechDpiShifter.exe 
3. A console window will appear. If it says "Successfully attached", then everything should work

DPI Shifter will only work as long as you keep the console window open. If at any point you close it,
it will stop working!

You can edit `InputHandler.cs` file in the source code to change the binding to anything else. 
In order to use keyboard bindings, you can use `WindowsInput.Capture.Global.KeyboardAsync()`
