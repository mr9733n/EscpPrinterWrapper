# EscpPrinterWrapperLib

EscpPrinterWrapperLib is a .NET library for generating ESC/P commands for text and barcode printing. This library helps in wrapping text and barcode data into ESC/P commands which can be sent to ESC/P compatible printers.

## Features

- Wrap text into ESC/P commands
- Wrap barcode data into ESC/P commands
- Generate full print commands with various options like cutting paper, landscape orientation, and more

## Installation

To install EscpPrinterWrapperLib, you can clone the repository and build the project using .NET CLI.

```sh
git clone https://github.com/mr9733n/EscpPrinterWrapperLib.git
cd EscpPrinterWrapperLib
dotnet build
```

## Usage

### Wrapping Text

```csharp
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;

var printerWrapper = new EscpPrinterWrapper();
string textCommand = printerWrapper.WrapText("Hello World", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide);
Console.WriteLine(textCommand);
```

### Wrapping Barcode

```csharp
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;

var printerWrapper = new EscpPrinterWrapper();
string barcodeCommand = printerWrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center);
Console.WriteLine(barcodeCommand);
```

### Generating Full Print Command

```csharp
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;
using System.Collections.Generic;

var printerWrapper = new EscpPrinterWrapper();
var commands = new List<string>
{
    printerWrapper.WrapText("Test", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide),
    printerWrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center)
};
string printCommand = printerWrapper.GeneratePrintCommand(commands, cutPaper: true, landscapeOrientation: true);
Console.WriteLine(printCommand);
```

## Running Tests

To run the tests, you can use the .NET CLI:

```sh
 dotnet test --runtime win-x64
```

## Console Application Usage

The console application allows you to generate ESC/P commands and write them to an output file.

Running the Application

### To run the application, use the following command:
```sh
.\EscpPrinterWrapperConsole.exe output.txt "text:'Hello','World',24,Helsinki,true,false,Single,Center,Wide" "barcode:123456789,CODE128,70,Medium,TwoToOne,true,Center" --cutPaper --landscape
```

## Options

	•	--cutPaper: Adds a cut paper command to the end of the print command.
	•	--landscape: Sets the orientation to landscape.
	•	--pageFormatWidth <value>: Specifies the page format width.
	•	--pageFormatHeight <value>: Specifies the page format height.
	•	--pageLength <value>: Specifies the page length.
	•	--leftMargin <value>: Specifies the left margin.
	•	--rightMargin <value>: Specifies the right margin.
	•	--horizontalPosition <value>: Specifies the horizontal position.
	•	--verticalPosition <value>: Specifies the vertical position.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for more details.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.