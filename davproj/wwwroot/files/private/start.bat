xcopy rdpc.reg C:\temp\protocols.reg /y /i /s /w
xcopy rdp_control.cmd "%ProgramFiles%\TightVNC\web\rdp_control.cmd" /y /i /s
xcopy rdp_shadow.cmd "%ProgramFiles%\TightVNC\web\rdp_shadow.cmd" /y /i /s
xcopy vnc.cmd "%ProgramFiles%\TightVNC\web\vnc.cmd" /y /i /s
regedit /s C:\temp\protocols.reg