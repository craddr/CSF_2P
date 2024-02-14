# CSF_2P
This includes codes required to the ThorClient app which listens for UDP input from the VS PC to start Rosie Craddock CSF experiment (Rosie Craddock 2024, thesis), and then controls THORIMAGE LS software for the acquisition of 2p data during presentation of visual stimuli. Timestamps are to be collected on the 2p PC using Thorsync software. 

PC setup/requirements: 
Setup Bscope or similar THORLABs hardware, using NI cards, and wiring as described by Janellia's scanimage r4.2 setup instructions (see Rosie Craddock Thesis Appendix).
Download NI MAX version 18.5 software and setup NI card as per appendix in Rosie Craddock 2024 thesis.
Obtain open-source Thorimage LS (version 4.1.2021.9131) and Thorsync (version 4.1.2020.1131) software from THORLABs (avaialable on request).
Install Microsoft visual studio 2022 (community) with the .NET PC developer package.
Save Code from this repos to your PC. 
Make a directory on a local drive of your PC for your 2P files to be saved in (I suggest on a fast drive- either C or D drive, needs 100+GB space. Use of antivirus software that may interfere with transfer of large files on the Drive used should be avoided.) 
Open ThorClient.sln project solution (found in rosie-scripts-src-1.7\ThorClient) in visual studio. Open the mainWindow.xaml.cs file and change the save locations on lines 235 and 61 correct to the directory you made for the 2p files to be saved in. Save your changes. 
Build the ThorClient.sln project solution using Visual Studio. 
Copy the ThorClient.exe from rosie-scripts-src-1.7\rosie-scripts-src-1.7\ThorClient\ThorClient\bin\Debug to a sensible location on the 2p PC.

Running the experiment: 
Start Thorimage software and focus your sample (for RC 2024 experiment, use x1 zoom, two-way scanning, 6-frame cumulative averaging). Set capture to finite, and ensure data will be saved in RAW format. 
Start Thorsync software 
Enter your animalID_1 (format AAAA123_1) and set the save location to where the 2p datas are to be saved.
Start recording on Thorsync. 
Start ThorClient Software, 
Press start 
Ensure that remote client checkbox is ticked in Thorimage software "Capture" screen 
PC will now wait for VS PC input (triggered by Master PC input).

Authorship and contributions: 
Codes were written by a freelancer under direction of Rosie Craddock 2024. Codes were later ammended by Rosie Craddock for bug fixing and optimisation
