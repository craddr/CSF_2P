classdef UDPn < handle    
    properties
        udpObject;
        remoteHost;
        remotePort;
    end
    
    methods
        function obj = UDPn(remotehost,remoteport)
            obj.udpObject = udpport("IPV4");
            
            configureTerminator(obj.udpObject,"CR/LF");
            configureCallback(obj.udpObject,"terminator", @obj.DatagramReceivedFcn);
            obj.remoteHost = remotehost;
            obj.remotePort = remoteport;
        end

        function writeMsg(obj, msg)
            try
                writeline(obj.udpObject,msg,obj.remoteHost, obj.remotePort);
            catch ex
                disp(ex.message);
            end
        end

        function DatagramReceivedFcn(obj,~,~)
            try
                data = readline(obj.udpObject);
                disp(data);
            catch ex
                disp(ex.message);
            end
        end

        function disconnect(obj)
            flush(obj.udpObject,"output");
            clear('obj.udpObject');
        end
    end
end
