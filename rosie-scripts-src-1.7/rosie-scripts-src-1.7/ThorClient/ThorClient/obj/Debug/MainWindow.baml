   M S B A M L   `   `   `  ����E  AThorClient, Version=1.0.7.0, Culture=neutral, PublicKeyToken=null'clr-namespace:ThorClient
ThorClient  5   	       ThorClient.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   *localclr-namespace:ThorClient     5   	   -   5         
  WinAll#��[  5   	     � Title$  
ThorClient��5	   	   ���#EEF1FF$	��600��61   $
��1000��6>    � WindowStartupLocation$ CenterScreen=�6K   .��5
      � .+�5   
   � -   5         �Name$
 Main��5       � RowDefinitions 5      �� 5      $
��AUTO �6    �� 5      $��* �6    �� 5      $��50 �6    .+�5      � $��0��6   $��0 5 5 10q�6     � ColumnDefinitions 5      �� 5      $��* �6'   �� 5      $
��0.9* �6'   .+�5      ��  �� Orientation$ Vertical=�6   $��0��64   .+�5      � $	��0 1q�6   .+�5      �� $��App Name��6    $
��Bold"�63   �� 5      -   5         ��Name$ 
txtAppName��5   "   $��
ThorClient��66   $	��150��6H   $��Center=�5   "   $��Right=�6D   5      � 5      $	��0 1q�6   .+�5      �� $(��"Remote PC Host Name (.default : .)��6    �� 5      -   5        $ txtRemotePCHostName��5   "   $��.��6?   $	��150��6H   $��Center=�5   "   $��Right=�6D   5       � 5!      $	��0 1q�6   .+�5"      �� $/��)Full Save Name (.default : C:\temp\exp01)��6    �� 5#      -   5        $ txtFullSaveName��5#   "   $��C:\temp\exp01��6;   $	��150��6P   $��Center=�6_   $��Right=�5$   "   5%      5&      �� 5'      $ Vertical=�6   $��1��64   .+�5(      � $	��0 1q�6   .+�5)      �� $��UDP Listener Port��6    $
��Bold"�6<   �� 5*      -   5        $ 
txtUDPPort��5*   "   $
��9988��66   $	��150��6B   $��Center=�5+   "   $��Right=�6D   5,      5-      5.      � 5/      $��1��6   .+�50      �� -   5         ��Name$ txtURL��50   \   ���#FFFFFF���#1C6758$��0q�6H   $��14(�6l   $(��"/ThorClient;component/#Ubuntu Mono)�51   "   " �� VerticalScrollBarVisibility$
 Auto=�52   "   	 �� 
IsReadOnly	 . $��5q�54      56      � 57      $��2��6    58      �� 59      $
��AUTO �6'   �� 5:      $��* �6'   �� 5;      $
��AUTO �6'   .+�5>      �� -   5        
 ��Name$
 btnStartStop��5>      $��Start��6/   $��75��6Z   $��5q�5?      $
��Bold"�6$   $��0��66   �� 5A      -	   5         ��Name$ lblShortMsg��5A   (   $��1��6   ���#1C6758$
��--.~��5B   2   $��Center=�5C      $��Center=�6?   �� 5E      $ 
Horizontal=�6   $��2��66   .+�5F      �� -
   5        $
 btnStartStopAcq��5F      $��Start Acquiring��66   $��75��6n   $��5q�5G      $
��Bold"�6$   $y�	Collapsed=�66   5H      5I      5J      5K      5L      