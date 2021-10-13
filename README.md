# CbusLogger
Small lightweight console application to log CBUS serial messages.

Logging can be done at a number of levels:
* the serial port level: byte data is logged as it arrives;
* the GridConnect level: messages are logged as GridConnect strings; 
* the CBUS level: messages are logged as the CBUS Op-Code followed by the data bytes.

## References
Makes use of CbusDefsEnums.cs from https://github.com/MERG-DEV/cbusdefs to provide the list of CBUS Op-Codes.

Logging is makes use of NLog.
Two log files are supported:
* One for standard application logging;
* The other for the incoming messages.
The configuration can be changed by updating the NLog.config file in the usual way; for documentation see https://nlog-project.org/
