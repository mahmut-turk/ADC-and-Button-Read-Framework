# ADC chart project with colored threshold

## Description
This project reads ADC data from an Arduino and displays it in real-time on a Windows Forms Chart.  
- ADC values are drawn in **blue**.  
- Values **above 500** are displayed in **red**, and values between **400-500** are displayed in **yellow**.  
- The **numerical integral** of the ADC values can also be calculated and displayed on the chart.  

## Features
- Real-time data visualization  
- Color-coded threshold display  
- Scrolling X-axis (displays the last 100 samples)  
- Y-axis range: 0–1023  

## Requirements
- Visual Studio Code 2019 or later to program the Arduino Uno
- .NET Framework 4.7.2+  
- NuGet package: `RJCP.IO.Ports` (for serial communication)  
- Arduino connected via a COM port

## Setup & Usage
1. Open the project in Visual Studio.  
2. Install NuGet packages (RJCP.IO.Ports).  
3. Connect your Arduino and specify the correct COM port in the `OpenSerialPort` function:  
   ```csharp
   OpenSerialPort("COM7", 9600);
