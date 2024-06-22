using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;

namespace EscpPrinterWrapperLib
{
    /// <summary>
    /// A wrapper class for ESC/P printer commands for controlling the QL-8XX series printers.
    /// </summary>
    public class EscpPrinterWrapper
    {
        private readonly ILogger<EscpPrinterWrapper> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EscpPrinterWrapper"/> class.
        /// </summary>
        /// <param name="logger">The logger instance. If null, a null logger is used.</param>
        public EscpPrinterWrapper(ILogger<EscpPrinterWrapper>? logger = null)
        {
            _logger = logger ?? NullLogger<EscpPrinterWrapper>.Instance;
        }

        // ESC/P control codes
        private const string Esc = "\u001B";
        private const string Init = Esc + "@";
        private const string LineFeed = "\n";
        private const string EndOfBarcode = "\\";
        private const string FormFeed = "\u000C";
        private const string Cut = Esc + "iC";
        private const string SelectLandscapeOrientation = Esc + "iL";
        private const string SpecifyPageFormat = Esc + "(c";
        private const string SpecifyPageLength = Esc + "(C";
        private const string SpecifyLeftMargin = Esc + "l";
        private const string SpecifyRightMargin = Esc + "Q";
        private const string SpecifyHorizontalPosition = Esc + "$";
        private const string SpecifyVerticalPosition = Esc + "(V";
        private const string SpecifyAlignment = Esc + "a";

        // Enums for various printer settings
        public enum BarcodeType
        {
            CODE39 = '0',
            ITF = '1',
            EAN_8 = '5',
            EAN_13 = '5',
            UPC_A = '5',
            UPC_E = '6',
            CODABAR = '9',
            CODE128 = 'a',
            GS1_128 = 'b',
            RSS = 'c',
            CODE93 = 'd',
            POSTNET = 'e',
            UPC_E_EXTENSION = 'f',
            MSI = 'g'
        }

        public enum Alignment
        {
            Left = 0,
            Center = 1,
            Right = 2
        }

        public enum FontType
        {
            FontA = '0',
            FontB = '1'
        }

        public enum Bold
        {
            Off = 'F',
            On = 'E'
        }

        public enum Italic
        {
            Off = '5',
            On = '4'
        }

        public enum Underline
        {
            None = '0',
            Single = '1',
            Double = '2'
        }

        public enum Spacing
        {
            Normal = '0',
            Wide = '1'
        }

        public enum BarcodeWidth
        {
            ExtraSmall = '0',
            Small = '1',
            Medium = '2',
            Large = '3'
        }

        public enum BarcodeRatio
        {
            ThreeToOne = '0',
            TwoPointFiveToOne = '1',
            TwoToOne = '2'
        }

        /// <summary>
        /// Wraps text with ESC/P commands for the specified styles.
        /// </summary>
        /// <param name="text">The text to wrap.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="fontType">The font type.</param>
        /// <param name="bold">Bold setting.</param>
        /// <param name="italic">Italic setting.</param>
        /// <param name="underline">Underline setting.</param>
        /// <param name="alignment">Text alignment.</param>
        /// <param name="spacing">Character spacing.</param>
        /// <returns>The wrapped text command.</returns>
        public string WrapText(string text, int fontSize, FontType fontType = FontType.FontA, Bold bold = Bold.Off, Italic italic = Italic.Off, Underline underline = Underline.None, Alignment alignment = Alignment.Left, Spacing spacing = Spacing.Normal)
        {
            _logger.LogInformation($"Wrapping text: {text}, FontSize: {fontSize}, FontType: {fontType}, Bold: {bold}, Italic: {italic}, Underline: {underline}, Alignment: {alignment}, Spacing: {spacing}");

            string setFontSize = $"{Esc}X{fontSize}";  // Set font size (for bitmap fonts min: 24, for outline fonts min: 33)
            string setFontType = $"{Esc}k{(char)fontType}";  // Set font type
            string setBold = $"{Esc}{(char)bold}";  // Set bold
            string setItalic = $"{Esc}{(char)italic}";  // Set italic
            string setUnderline = $"{Esc}-{(char)underline}";  // Set underline
            string setAlignment = $"{Esc}a{(int)alignment}";  // Set alignment
            string setSpacing = $"{Esc} {(char)spacing}";  // Set spacing

            string result = Esc + "SOH" + setFontSize + setFontType + setBold + setItalic + setUnderline + setAlignment + setSpacing + text + LineFeed;
            _logger.LogInformation($"Resulting command: {EscapeNonPrintable(result)}");
            return result;
        }

        /// <summary>
        /// Wraps barcode data with ESC/P commands for the specified settings.
        /// </summary>
        /// <param name="data">The barcode data.</param>
        /// <param name="barcodeType">The barcode type.</param>
        /// <param name="height">The height of the barcode.</param>
        /// <param name="width">The width of the barcode.</param>
        /// <param name="ratio">The ratio of the barcode.</param>
        /// <param name="printCharsBelow">Whether to print characters below the barcode.</param>
        /// <param name="alignment">Barcode alignment.</param>
        /// <returns>The wrapped barcode command.</returns>
        public string WrapBarcode(string data, BarcodeType barcodeType, int height = 100, BarcodeWidth width = BarcodeWidth.Medium, BarcodeRatio ratio = BarcodeRatio.TwoToOne, bool printCharsBelow = true, Alignment alignment = Alignment.Left)
        {
            _logger.LogInformation($"Wrapping barcode: {data}, BarcodeType: {barcodeType}, Height: {height}, Width: {width}, Ratio: {ratio}, PrintCharsBelow: {printCharsBelow}, Alignment: {alignment}");

            string setType = $"{Esc}i{(char)barcodeType}";  // Barcode type
            string setHeight = $"h{height:D2}";  // Height
            string setWidth = $"w{(char)width}";  // Width
            string setRatio = $"z{(char)ratio}";  // Ratio
            string setCharsBelow = printCharsBelow ? "r1" : "r0";  // Characters below barcode
            string setAlignment = $"{Esc}a{(int)alignment}";  // Alignment

            string command = $"{setType}{setCharsBelow}{setHeight}{setWidth}{setRatio}{setAlignment}B{data}{EndOfBarcode}";
            _logger.LogInformation($"Resulting command: {EscapeNonPrintable(command)}");

            return command;
        }

        /// <summary>
        /// Generates the full print command from a list of wrapped commands.
        /// </summary>
        /// <param name="wrappedCommands">The list of wrapped commands.</param>
        /// <param name="cutPaper">Whether to cut the paper after printing.</param>
        /// <param name="landscapeOrientation">Whether to use landscape orientation.</param>
        /// <param name="pageFormatWidth">The width of the page format.</param>
        /// <param name="pageFormatHeight">The height of the page format.</param>
        /// <param name="pageLength">The page length.</param>
        /// <param name="leftMargin">The left margin.</param>
        /// <param name="rightMargin">The right margin.</param>
        /// <param name="horizontalPosition">The horizontal position.</param>
        /// <param name="verticalPosition">The vertical position.</param>
        /// <returns>The full print command.</returns>
        public string GeneratePrintCommand(
            List<string> wrappedCommands,
            bool cutPaper = false,
            bool landscapeOrientation = false,
            int? pageFormatWidth = null,
            int? pageFormatHeight = null,
            int? pageLength = null,
            int? leftMargin = null,
            int? rightMargin = null,
            int? horizontalPosition = null,
            int? verticalPosition = null)
        {
            _logger.LogInformation("Generating full print command with options: " +
                $"cutPaper: {cutPaper}, landscapeOrientation: {landscapeOrientation}, pageFormatWidth: {pageFormatWidth}, pageFormatHeight: {pageFormatHeight}, " +
                $"pageLength: {pageLength}, leftMargin: {leftMargin}, rightMargin: {rightMargin}, horizontalPosition: {horizontalPosition}, verticalPosition: {verticalPosition}");

            var sb = new StringBuilder();
            sb.Append(Init);

            if (landscapeOrientation) sb.Append(SelectLandscapeOrientation);
            if (pageFormatWidth.HasValue && pageFormatHeight.HasValue) sb.Append($"{SpecifyPageFormat}{(char)pageFormatWidth}{(char)pageFormatHeight}");
            if (pageLength.HasValue) sb.Append($"{SpecifyPageLength}{(char)pageLength}");
            if (leftMargin.HasValue) sb.Append($"{SpecifyLeftMargin}{(char)leftMargin}");
            if (rightMargin.HasValue) sb.Append($"{SpecifyRightMargin}{(char)rightMargin}");
            if (horizontalPosition.HasValue) sb.Append($"{SpecifyHorizontalPosition}{(char)horizontalPosition}");
            if (verticalPosition.HasValue) sb.Append($"{SpecifyVerticalPosition}{(char)verticalPosition}");

            foreach (var command in wrappedCommands)
            {
                sb.Append(command);
            }

            if (cutPaper) sb.Append(Cut);
            sb.Append(FormFeed);

            string result = sb.ToString();
            _logger.LogInformation($"Final print command: {EscapeNonPrintable(result)}");
            return result;
        }

        /// <summary>
        /// Escapes non-printable characters in a string for logging.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The escaped string.</returns>
        private string EscapeNonPrintable(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (char.IsControl(c))
                {
                    sb.Append($"\\u{(int)c:x4}");
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}