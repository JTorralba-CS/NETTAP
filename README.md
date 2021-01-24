![Preview](Preview.png?raw=true "Preview")

# TCPF
TCP logger and forwarder.

What does TCPF do? 

This utility is used as a troubleshooting tool to review and analyze what data is being passed between TCP systems.  It captures the data from source TCP system, stores the data to a log file (TCP_Raw.txt), then relays the data to the destination TCP system.

What does the "CCC" optional parameter do?

ESRI GeoEvent service was built for modern data communication.  It has no native support for legacy serial data communication over TCP.  With the "CCC" option, the utility has been modified to adopt to the inconsistent ANIALI data coming from the vendors.  Basically, it takes the raw ANIALI data from the vendors (CARRIERS/PROVIDERS, INTRADO, CENTRAL SQAURE, etc.) and makes it clean before the GeoEvent server can digest the ANIALI data for plotting on a map.

When "CCC" is enabled, the utility performs the following: 

1) Strips LINE-FEED (LF) ASCII control code in between the START-OF-TEXT (STX) and the END-OF-TEXT (ETX) ASCII control codes. LN in the data causes issues for GeoEvent service parsing.

2) Strips the BLOCK-CHECK-CHARACTER (BCC) that comes after the ETX. Sometimes the computed BCC value is another ASCII control code that causes issues for GeoEvent service parsing.

3) Converts "COMMA" to "SPACE". ("COMMA" as field separator used by GeoEvent service parsing.  Parsing process is setup to receive ANIALI as one long 512+ character field per record.  Required components are parsed from the long single field via fixed positioning.) 

4) Adds CARRAGIE-RETURN + LINE-FEED to minimize overlapping of received records causing extra processing for GeoEvent service.  GeoEvent service uses the ETX as record separator but sometimes receives short status messages like "STXE99ETX".

5) Stores the "cleansed" data in the TCP_CCC.txt file.

## Stack:

Windows 10 Professional\
Visual Studio 2019 CE\
C#\
.NET Core 3.1

## Usage:

1. Open solution.
2. Compile and publish project.
3. Excecute command in console/terminal.

## Syntax:

TCPF Local-IP Local-Port Destination-IP Destination-Port [CCC]




























