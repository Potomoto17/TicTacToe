   M S B A M L   `   `   `  ����  Vaja 3 - Tic Tac Toe%    Vaja_3___Tic_Tac_Toe.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Križec-Krožec��5   	   $	��300��6   $	��400��6,    � 	Resources 5         BoolToVisibilityConverter��%   &	        �� 5   
   6'   5      .��5      �� $��Center=�6   $��Center=�6,   $��20q�6I   .+�5	   
   �� $��Izberi način:��6   $
��Bold"�6)   $��0,0,0,10q�6;   	� 5   
   -   5        $��rbSinglePlayer��5   '    	� 	GroupName$
 Mode��6   $(��"Single-Player (proti računalniku)��6=    d� 	IsChecked$
 TrueY�6i   $��0,0,0,10q�6z   	� 5   
   -   5        $��rbMultiPlayer��5   '   $
 Mode��6   $��Multi-Player (mreža)��6<   $��0,0,0,20q�6[   �� 5   
    �� Orientation$ 
Horizontal=�6   y�6.   ��  �� ElementName$ rbMultiPlayer�� �� Path$ 	IsChecked� �� 	Converter# [  .+�5      �� -   5        $��Create Game��5      $	��100��6+   $��0,0,10,0q�67   6I   �� 5      -   5        $��txtServerIP��5      $	��100��6)   $��Center=�65   $��Center=�6R   $��0,0,10,0q�6m   $��	127.0.0.1��6   �� 5      -   5        $��	Join Game��5      $	��100��6)   65   5      �� 5   
   -   5        $��Start��5      $	��100��6!   $��0,20,0,0q�6-   6?   5      5      