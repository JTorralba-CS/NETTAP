REM Delete previous logs.
del *.log

REM Strip LN control code before forwarding to GeoEvent server.
REM CCC argument is optional - used for stripping END-OF-LINE control code between the STX & ETX control codes. It also strips BCC checksum.

TCPF Listen_IP Listen_Port Target_IP Target_Port CCC
