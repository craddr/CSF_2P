function RunThor(RemotePCHostName, FullSaveName)
conn = IpcConnection();

if(RemotePCHostName ~= "")
    conn.RemotePCHostName = RemotePCHostName;
else
    disp("Default remote host name is used: '.' ");
    conn.RemotePCHostName = ".";
end

if(FullSaveName ~= "")
    conn.FullSaveName = FullSaveName;
else
    disp("Default save name is used: C:\\temp\\exp01");
    conn.FullSaveName = "C:\\temp\\exp01";
end

conn.connectionServerID = "MATLABThorImagePipe";
conn.connectionClientID = "ThorImageMATLABPipe";

disp("Send to client");
conn.SendToClient(char(ThorPipeCommand.Establish), conn.getHostName());
%conn.StartNamedPipeClient();

disp("Init done");
cmd = "";

while 1
    if ~isempty(cmd)
        if (cmd == "s" || cmd == "S")
            conn.SendToClient(char(ThorPipeCommand.StartAcquiring), conn.FullSaveName);
            conn.StartNamedPipeClient();
        elseif (cmd == "x" || cmd == "X")
            conn.SendToClient(char(ThorPipeCommand.StopAcquiring), "0");
            conn.StartNamedPipeClient();
        elseif (cmd == "e" || cmd == "E")
            break;
        end
    end
    
    prompt = "s = start acquisition | x = stop acquisition | e = end application : ";
    cmd = input(prompt,"s");
end

conn.SendToClient(char(ThorPipeCommand.TearDown), "0");

disp("Bye, see you next time.");
pause(1);

end