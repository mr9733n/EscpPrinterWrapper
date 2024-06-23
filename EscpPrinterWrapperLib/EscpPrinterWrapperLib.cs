using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EscpPrinterWrapperLib.Enums;

namespace EscpPrinterWrapperLib
{
    /// <summary>
    /// A wrapper class for ESC/P printer commands for controlling the QL-8XX series printers.
    /// </summary>
    /// <remarks>
    /// This class provides methods to generate ESC/P commands for printing text and barcodes.
    /// </remarks>
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
        private static readonly byte[] Esc = { 0x1B };
        private static readonly byte[] Init = { 0x1B, 0x40 };
        private static readonly byte[] LineFeed = { 0x0A };
        private static readonly byte[] EndOfBarcode = { 0x5C };
        private static readonly byte[] FormFeed = { 0x0C };
        private static readonly byte[] Cut = { 0x1B, 0x69, 0x43 };
        private static readonly byte[] SelectLandscapeOrientation = { 0x1B, 0x69, 0x4C };
        private static readonly byte[] SpecifyPageFormat = { 0x1B, 0x28, 0x63 };
        private static readonly byte[] SpecifyPageLength = { 0x1B, 0x28, 0x43 };
        private static readonly byte[] SpecifyLeftMargin = { 0x1B, 0x6C };
        private static readonly byte[] SpecifyRightMargin = { 0x1B, 0x51 };
        private static readonly byte[] SpecifyHorizontalPosition = { 0x1B, 0x24 };
        private static readonly byte[] SpecifyVerticalPosition = { 0x1B, 0x28, 0x56 };
        private static readonly byte[] SpecifyAlignment = { 0x1B, 0x61 };
        private static readonly byte[] CarriageReturn = { 0x0D };
        private static readonly byte[] HorizontalTab = { 0x09 };


        /// <summary>
        /// Inserts horizontal tabs between words in the given text.
        /// </summary>
        /// <param name="text1">The first part of the text.</param>
        /// <param name="text2">The second part of the text.</param>
        /// <returns>The text with tabs inserted between words.</returns>
        public byte[] InsertTabsBetweenWords(string text1, string text2)
        {
            var result = new List<byte>();
            result.AddRange(Encoding.ASCII.GetBytes(text1));
            result.AddRange(HorizontalTab);
            result.AddRange(Encoding.ASCII.GetBytes(text2));
            return result.ToArray();
        }

        /// <summary>
        /// Wraps text with ESC/P commands for the specified styles.
        /// </summary>
        /// <param name="text1">The first part of the text to wrap.</param>
        /// <param name="text2">The second part of the text to wrap.</param>
        /// <param name="fontSize">The font size.</param>
        /// <param name="fontType">The font type.</param>
        /// <param name="bold">Bold setting.</param>
        /// <param name="italic">Italic setting.</param>
        /// <param name="underline">Underline setting.</param>
        /// <param name="alignment">Text alignment.</param>
        /// <param name="spacing">Character spacing.</param>
        /// <param name="includeCarriageReturn">Flag to include carriage return after text.</param>
        /// <returns>The wrapped text command.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        /// <example>
        /// <code>
        /// var wrapper = new EscpPrinterWrapper(null);
        /// string command = wrapper.WrapText("Hello", "World", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide, true);
        /// </code>
        /// </example>
        public byte[] WrapText(string text1, string text2, int fontSize, FontType fontType = FontType.Brougham, Bold bold = Bold.Off, Italic italic = Italic.Off, Underline underline = Underline.None, Alignment alignment = Alignment.Left, Spacing spacing = Spacing.Normal, bool includeCarriageReturn = false)
        {
            _logger.LogInformation($"Wrapping text: {text1} {text2}, FontSize: {fontSize}, FontType: {fontType}, Bold: {bold}, Italic: {italic}, Underline: {underline}, Alignment: {alignment}, Spacing: {spacing}, CarriageReturn: {includeCarriageReturn}");

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text1) + " or " + nameof(text2));
            }

            var result = new List<byte>();
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes("SOH"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"X{fontSize}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"k{(char)fontType}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"{(char)bold}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"{(char)italic}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"-{(char)underline}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"a{(int)alignment}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($" {(char)spacing}"));
            result.AddRange(InsertTabsBetweenWords(text1, text2));

            if (includeCarriageReturn)
            {
                result.AddRange(CarriageReturn);
            }

            _logger.LogInformation($"Resulting command: {EscapeNonPrintable(result.ToArray())}");
            return result.ToArray();
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
        /// <param name="includeCarriageReturn">Flag to include carriage return after text.</param>
        /// <returns>The wrapped barcode command.</returns>
        /// <example>
        /// <code>
        /// var wrapper = new EscpPrinterWrapper(null);
        /// string command = wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center, true);
        /// </code>
        /// </example>
        public byte[] WrapBarcode(string data, BarcodeType barcodeType, int height = 100, BarcodeWidth width = BarcodeWidth.Medium, BarcodeRatio ratio = BarcodeRatio.TwoToOne, bool printCharsBelow = true, Alignment alignment = Alignment.Left, bool includeCarriageReturn = false)
        {
            _logger.LogInformation($"Wrapping barcode: {data}, BarcodeType: {barcodeType}, Height: {height}, Width: {width}, Ratio: {ratio}, PrintCharsBelow: {printCharsBelow}, Alignment: {alignment}, CarriageReturn: {includeCarriageReturn}");

            var result = new List<byte>();
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"i{(char)barcodeType}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"h{height:D2}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"w{(char)width}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"z{(char)ratio}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"r{(printCharsBelow ? '1' : '0')}"));
            result.AddRange(Esc);
            result.AddRange(Encoding.ASCII.GetBytes($"a{(int)alignment}"));

            string endOfBarcode = barcodeType == BarcodeType.CODE128 ? "\\\\" : "\\";
            result.AddRange(Encoding.ASCII.GetBytes($"B{data}{endOfBarcode}"));

            if (includeCarriageReturn)
            {
                result.AddRange(CarriageReturn);
            }

            _logger.LogInformation($"Resulting command: {EscapeNonPrintable(result.ToArray())}");
            return result.ToArray();
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
        /// <example>
        /// <code>
        /// var wrapper = new EscpPrinterWrapper(null);
        /// var commands = new List<string>
        /// {
        ///     wrapper.WrapText("Test", 24, FontType.FontA, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide),
        ///     wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center)
        /// };
        /// string printCommand = wrapper.GeneratePrintCommand(commands, cutPaper: true, landscapeOrientation: true);
        /// </code>
        /// </example>
        public byte[] GeneratePrintCommand(List<byte[]> wrappedCommands, bool cutPaper = false, bool landscapeOrientation = false, int? pageFormatWidth = null, int? pageFormatHeight = null, int? pageLength = null, int? leftMargin = null, int? rightMargin = null, int? horizontalPosition = null, int? verticalPosition = null)
        {
            _logger.LogInformation("Generating full print command with options: " +
                $"cutPaper: {cutPaper}, landscapeOrientation: {landscapeOrientation}, " +
                $"pageFormatWidth: {pageFormatWidth?.ToString() ?? "null"}, " +
                $"pageFormatHeight: {pageFormatHeight?.ToString() ?? "null"}, " +
                $"pageLength: {pageLength?.ToString() ?? "null"}, " +
                $"leftMargin: {leftMargin?.ToString() ?? "null"}, " +
                $"rightMargin: {rightMargin?.ToString() ?? "null"}, " +
                $"horizontalPosition: {horizontalPosition?.ToString() ?? "null"}, " +
                $"verticalPosition: {verticalPosition?.ToString() ?? "null"}");

            var result = new List<byte>();
            result.AddRange(Init);

            if (landscapeOrientation) result.AddRange(SelectLandscapeOrientation);
            if (pageFormatWidth.HasValue && pageFormatHeight.HasValue)
            {
                result.AddRange(SpecifyPageFormat);
                result.AddRange(new byte[] { (byte)pageFormatWidth.Value, (byte)pageFormatHeight.Value });
            }
            if (pageLength.HasValue)
            {
                result.AddRange(SpecifyPageLength);
                result.AddRange(new byte[] { (byte)pageLength.Value });
            }
            if (leftMargin.HasValue)
            {
                result.AddRange(SpecifyLeftMargin);
                result.AddRange(new byte[] { (byte)leftMargin.Value });
            }
            if (rightMargin.HasValue)
            {
                result.AddRange(SpecifyRightMargin);
                result.AddRange(new byte[] { (byte)rightMargin.Value });
            }
            if (horizontalPosition.HasValue)
            {
                result.AddRange(SpecifyHorizontalPosition);
                result.AddRange(new byte[] { (byte)horizontalPosition.Value });
            }
            if (verticalPosition.HasValue)
            {
                result.AddRange(SpecifyVerticalPosition);
                result.AddRange(new byte[] { (byte)verticalPosition.Value });
            }

            foreach (var command in wrappedCommands)
            {
                result.AddRange(command);
            }

            if (cutPaper) result.AddRange(Cut);
            result.AddRange(FormFeed);

            _logger.LogInformation($"Final print command: {EscapeNonPrintable(result.ToArray())}");
            return result.ToArray();
        }

        /// <summary>
        /// Escapes non-printable characters in a string for logging.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The escaped string.</returns>
        private string EscapeNonPrintable(byte[] input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                if (c < 32 || c > 126)
                {
                    sb.Append($"\\x{c:x2}");
                }
                else
                {
                    sb.Append((char)c);
                }
            }
            return sb.ToString();
        }
        public byte[] GenerateInitCommand()
        {
            return new byte[] { 0x1B, 0x40 }; // ESC @
        }

        public byte[] GenerateLandscapeModeCommand()
        {
            return new byte[] { 0x1B, 0x69, 0x4C, 0x01 }; // ESC i L 01
        }

        public byte[] GenerateMarginAndLineFeedCommand()
        {
            return new byte[] { 0x1B, 0x55, 0x02, 0x1B, 0x33, 0x04 }; // ESC U 02, ESC 3 04
        }

        public byte[] GenerateFontAndSizeCommand(int fontSize)
        {
            return new byte[] { 0x1B, 0x6B, 3, 0x1B, 0x58, 0x00, (byte)fontSize, 0x00 }; // ESC k 3, ESC X 00, fontSize
        }

        public byte[] GenerateNewLineCommand()
        {
            return new byte[] { 0x0A, 0x0A }; // Line feed
        }

        public byte[] GenerateFormFeedCommand()
        {
            return new byte[] { 0x0C }; // Form feed
        }

    }
}