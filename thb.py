import serial
import re
import time
from serial.tools import list_ports

print('start')
thb_ports = [];
ports = list_ports.comports(include_links=False)
for port in ports :    
    try:        
        print(port.device)
        portCom = serial.Serial(port.device, 115200, timeout = 1, write_timeout = 1)
        portCom.write(b'RMX.WHOIS\r\n') 
        data = portCom.readline()
        print(data)
        if re.search('HW_MODEL=THB_SENSOR', str(data, 'utf-8')):  
            print('Got correct reply')
            thb_ports.append(portCom)
            thb_port = port;
        else:
            portCom.close()       
    except:
       print('wrong port')

if len(thb_ports) != 0:
    print('devices found: ' + str(len(thb_ports)))
    while 1:        
        time.sleep(1)
        for thb in thb_ports :
            try:
                thb.write(b'THB.CURR\r\n')
                data = str(thb.readline(), 'utf-8')
                temperature = re.search('(?s)(?<=<T=).*?(?=>)', data).group()
                humidity = re.search('(?s)(?<=<H=).*?(?=>)', data).group()
                pressure = re.search('(?s)(?<=<P=).*?(?=>)', data).group()
                density = re.search('(?s)(?<=<D=).*?(?=>)', data).group()
                print('--------------------')
                print('Temperature: ' + temperature + ' °C')
                print('Humidity: ' + humidity + ' %')
                print('Pressure: ' + pressure + ' hPa')
                print('Density: ' + density + ' kg/m3')
            except:
                 print('error on thb')

else:
    print('No thb has been found')
print('done')
       
    
    