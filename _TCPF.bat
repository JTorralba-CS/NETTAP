REM Delete previous logs.
del *.log

REM Normal raw TCP forwarding.
TCPF Listen_IP Listen_Port Target_IP Target_Port > _Console.log

REM Strip LN control code before forwarding to GeoEvent server.
REM TCPF Listen_IP Listen_Port Target_IP Target_Port TACMAP > _Console.log
