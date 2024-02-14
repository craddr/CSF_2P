classdef IpcConnection
    properties
       threadCompleted = true;
       connectionServerID = "";
       connectionClientID = "";
       DataLength = 4;
       RemotePCHostName = "";
       FullSaveName = "";
       receiveIPCCommandActive = false;
       sendBuffer;
       thorImageLSConnectionStats = false;
       pipeServer;
    end
    
    methods
        function obj = SendToClient(obj, command, data)
            if obj.threadCompleted == true
                %if obj.serverThread ~= null
                 %   obj.serverThread = null;
                %end
                
                obj.sendBuffer = {char(ThorPipeSrc.Remote), char(ThorPipeDst.Local), char(command), char(data)};
                %_serverThread = new Thread(ServerThread);
                %_serverThread.Start();
                obj.ServerThread();
            end
        end

        function obj = ServerThread(obj)
            obj.threadCompleted = false;
            obj.StreamOutNamedPipe();
            if obj.thorImageLSConnectionStats == true
                cmd = ThorPipeCommand.(obj.sendBuffer(2));
                switch cmd
                    case ThorPipeCommand.Establish
                        obj.thorImageLSConnectionStats = true;
                        pause(50/1000);
                        configurationInformation = ["true", "0"];
                        obj.sendBuffer = [char(ThorPipeSrc.Remote),  char(ThorPipeDst.Local), char(ThorPipeCommand.UpdataInformation), strjoin(configurationInformation,"/")];
                        StreamOutNamedPipe();
                    case ThorPipeCommand.TearDown
                        obj.thorImageLSConnectionStats = false;
                    otherwise
                end
            end
            obj.threadCompleted = true;
        end
        
        function obj = StreamOutNamedPipe(obj)
            try
                NET.addAssembly('System.Core');
                obj.pipeServer  = System.IO.Pipes.NamedPipeServerStream(obj.connectionServerID, System.IO.Pipes.PipeDirection.InOut, 4, System.IO.Pipes.PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous);
                
                obj.pipeServer.WaitForConnection();
            catch exception
                disp(exception);
                obj.thorImageLSConnectionStats = false;

                if (obj.sendBuffer(2) ~= char(ThorPipeCommand.TearDown))
                    disp("The ThorImage is Disconnected. --Connection Error");
                end

                obj.threadCompleted = true;
                return;
            end

            % Send Message
            try
                % Read the request from the client. Once the client has
                % written to the pipe its security token will be available.
                obj.thorImageLSConnectionStats = true;
                ss = StreamString(obj.pipeServer);
                % Verify our identity to the connected client using a
                % string that the client anticipates.
                ss.WriteString(strjoin(obj.sendBuffer, "~"));
                [~, msgRecv] = ss.ReadString();
                obj.ReceiveIPCMessageACK(string(msgRecv));
            catch exception
                disp(exception);
                disp("Error while reading message.");
            end
            
            obj.pipeServer.Close();
        end
        
        function obj = ReceiveIPCMessageACK(obj, msg)
            if (contains(msg,"~"))
                msgRecv = split(msg, '~');
                if (length(msgRecv) == 4)
                    if (length(msgRecv) == obj.DataLength && msgRecv(1) == char(ThorPipeSrc.Remote) && msgRecv(2) == char(ThorPipeDst.Local))
                        if (msgRecv(3) == obj.sendBuffer(3) && msgRecv(4) == "1")
                            % do nothing.
                        else                   
                            switch ThorPipeStatus(str2double(msgRecv(4)))
                                case ThorPipeStatus.ThorPipeStsNoError
                                    disp("Error : ThorPipeStsNoError");
                                case ThorPipeStatus.ThorPipeStsBusy
                                    disp("Error : ThorPipeStsBusy");
                                case ThorPipeStatus.ThorPipeStsBlankCommandError
                                    disp("Error : ThorPipeStsBlankCommandError");
                                case ThorPipeStatus.ThorPipeStreamNotSupportedError
                                    disp("Error : ThorPipeStreamNotSupportedError");
                                case ThorPipeStatus.ThorPipeFormatError
                                    disp("Error : ThorPipeFormatError");
                                case ThorPipeStatus.ThorPipeFormatRoutingError
                                    disp("Error : ThorPipeFormatRoutingError");
                                case ThorPipeStatus.ThorpipeIOError
                                    disp("Error : ThorpipeIOError");
                                case ThorPipeStatus.ThorPipeError
                                    disp("Error : ThorPipeError");
                                otherwise
                            end
                        end
                    end 
                end
            end
        end

        function obj = StartNamedPipeClient(obj)
            obj.ClientThread();
        end

        function obj = ClientThread(obj)
            NET.addAssembly('System.Core');
            disp(strcat("Waiting for response on ", obj.connectionClientID));
            while 1
                % New Server NamedPipeClientStream Instance
                % namedPipeClient;
                if (obj.RemotePCHostName == obj.getHostName())
                    namedPipeClient = System.IO.Pipes.NamedPipeClientStream(".", obj.connectionClientID, System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation);
                else
                    namedPipeClient = System.IO.Pipes.NamedPipeClientStream(obj.RemotePCHostName, obj.connectionClientID, System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.Impersonation);
                end

                try
                    if namedPipeClient.IsConnected ~= true
                        namedPipeClient.Connect(5000);
                    end
                    
                    % Read the request from the Server. Once the Server has
                    % written to the pipe its security token will be available
                    ss = StreamString(namedPipeClient);
                    [~, msg] = ss.ReadString();
                    obj.ReceiveIPCCommand(msg, ss);
                catch exception
                    %disp(exception);
                    disp("~."); % a.k.a connection error.
                    pause(1);
                    continue;
                end
                
                namedPipeClient.Close();
                %continue;
                break;
            end
            disp("done.");
        end

        function obj = ReceiveIPCCommand(obj, thorImagePipeRecv, ss)
            if (thorImagePipeRecv.Contains("~"))
                msgRecv = thorImagePipeRecv.Split('~');
                if (msgRecv.Length == obj.DataLength && msgRecv(1) == char(ThorPipeSrc.Remote) && msgRecv(2) == char(ThorPipeDst.Local))
                    [~, execute] = obj.ExcuteNamedPipeData(msgRecv);
                    if (execute == true)
                        ss.WriteString(strjoin({char(ThorPipeSrc.Remote), char(ThorPipeDst.Local), char(msgRecv(3)), char("1")}, "~"));
                    else                    
                        ss.WriteString(strjoin({char(ThorPipeSrc.Remote), char(ThorPipeDst.Local), char(ThorPipeCommand.Error), char("2")}, "~"));
                    end
                else
                    ss.WriteString(strjoin({char(ThorPipeSrc.Remote), char(ThorPipeDst.Local), char(ThorPipeCommand.Error), char("11")}, "~"));
                end
            else 
                ss.WriteString(strjoin({char(ThorPipeSrc.Remote), char(ThorPipeDst.Local), char(ThorPipeCommand.Error), char("3")}, "~"));
            end
        end

        function [obj, execute] = ExcuteNamedPipeData(obj, msg)
            obj.receiveIPCCommandActive = true;
            if (msg.Length == 4)
                switch (ThorPipeCommand(string(msg(3))))
                    case ThorPipeCommand.Establish
                    case ThorPipeCommand.TearDown
                    case ThorPipeCommand.StartAcquiring
                    case ThorPipeCommand.StopAcquiring
                    case ThorPipeCommand.AcquireInformation
                    otherwise
                        obj.receiveIPCCommandActive = false;
                        execute = false;
                        return;
                end
            
                obj.receiveIPCCommandActive = false;
                execute = true;
            else
                obj.receiveIPCCommandActive = false;
                execute = false;
            end
        end
    end

    methods(Static)
        function name = getHostName()
            name = getenv('COMPUTERNAME');
        end
    end
end

