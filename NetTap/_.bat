REM Delete previous logs.
del *.log
del *.txt

REM Strip LN control code before forwarding to GeoEvent server.
REM CCC argument is optional - used for stripping END-OF-LINE control code between the STX & ETX control codes. It also strips BCC checksum.

NetTap 192.168.0.119 9110 9110
