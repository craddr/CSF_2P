disp("~~~~~~~~~~~~~~~~~~");
disp("ThorClient - v1.5");
disp("~~~~~~~~~~~~~~~~~~");

%RunThor("","");

udp = UDPn('192.168.2.3', 9988);
udp.writeMsg("StartAcquiring");
%udp.writeMsg("StopAcquiring");

prompt = "e = end application : ";
cmd = input(prompt,"s");

udp.disconnect();