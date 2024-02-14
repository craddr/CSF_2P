classdef StreamString
    
    properties
        ioStream;
        streamEncoding;
    end
    
    methods
        function obj = StreamString(stream)
            obj.ioStream = stream;
            obj.streamEncoding = System.Text.UnicodeEncoding();
        end
        
        function [obj,msg] = ReadString(obj)
            len = obj.ioStream.ReadByte() * 256;
            len = len + obj.ioStream.ReadByte();

            inBuffer = zeros(1,len);

            for x = 1:len
                inBuffer(x) = obj.ioStream.ReadByte();
            end

            msg = obj.streamEncoding.GetString(inBuffer);
            disp(strcat("[RECV] ", string(msg)));
        end

        function [obj, leng] = WriteString(obj, outString)
            outBuffer = obj.streamEncoding.GetBytes(outString);
            len = obj.streamEncoding.GetByteCount(outString);
            
            if (len > intmax("uint16"))
                len = intmax("uint16");
            end

            obj.ioStream.WriteByte(uint8(len / 256));
            obj.ioStream.WriteByte(uint8(bitand(len , 255)));
          
            obj.ioStream.Write(outBuffer, 0, len);
            obj.ioStream.Flush();

            disp(strcat("[SEND] " ,string(outString)));
            leng = len + 2;
        end
    end
end