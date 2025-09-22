# WPFDialControl
WPF Dial widget for wpf applications
Works with NET 8.0 and above
Three modes: Modern, Flat and Vintage
Set custom colors for ticks, dial and pointer. 
Responds to mouseup, mousedown, mousewheel and mouseclick events.

# Sep 20, 2025
Implemented Mousedown handler.

# Sep 18, 2025
Customize colors of the ticks, dial and pointer

<img width="886" height="335" alt="Screenshot 2025-09-18 184514" src="https://github.com/user-attachments/assets/fc460237-53ab-4166-8795-9c5adca8d3f5" />

# June 7, 2025
Improved the Flat Look :


![DialControl](https://github.com/user-attachments/assets/802b8dd2-c8e0-46c9-9822-a6e8f0c39c1d)

[![WPFDialControl Demo](https://i9.ytimg.com/vi/hommN9eepbg/mqdefault.jpg?sqp=COTNuLQG-oaymwEmCMACELQB8quKqQMa8AEB-AHUBoACwgOKAgwIABABGGQgZChkMA8=&rs=AOn4CLD2xv6VvvBFRmFoI_NIvql2vHpdNA)](https://www.youtube.com/watch?v=hommN9eepbg "WPFDialControl demo")


# How To Use It In Your Project

In the repo, there is a sample project called DialControl which shows how to use the WPFDial control. The basic steps are as follows:

1.Add a reference to the WpfDial project  
2.In your window xaml, use  *xmlns:app="clr-namespace:WpfDial;assembly=WpfDial"* to refer to the project  
3.Use the *<app:UserControl1* tag to insert the dial(s).   

The following design-time attributes are available:

  Required Attributes:
  
     Mode - MODERN/FLAT/VINTAGE - set the look of the dial  
     Angle - ANGLE_15/ANGLE_40/ANGLE_45/ANGLE_60/ANGLE_90 - set the number of ticks for the dial. Eg.ANGLE_90 will show 4 ticks spaced at 90 degrees  
  
  Optional Attributes:
  
     By default all ticks and dial colors are gray or black. You can override the default colors with the attributes below:  
     FillStartColor - Color - start gradient color of dial color   
     FillStopColor - Color - stop gradient colot of dial color - if stop and start are the same color then you get a solid color fill  
     TickColor - Color - color of the ticks around the dial  
     PointerColor - Color - color of the pointer marker  

  Events:
  
    dialClick - called when there is a mouse event on the dial. This event sends a DialClickRoutedEventArgs object. This object returns a .SelectedPos property which 
                denotes the current marker position of the dial. Marker positions start from zero. So if there are 4 ticks then the marker positions will be 0 to 3.  
                
    
  
An overview explanation video is here:
https://www.youtube.com/watch?v=VQsPnhnT8ac
