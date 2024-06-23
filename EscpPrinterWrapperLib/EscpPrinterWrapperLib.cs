﻿using System;
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
        private const string CarriageReturn = "\r";  // CR (0D)
        private const string HorizontalTab = "\u0009";  // HT (09)

        /// <summary>
        /// Inserts horizontal tabs between words in the given text.
        /// </summary>
        /// <param name="text1">The first part of the text.</param>
        /// <param name="text2">The second part of the text.</param>
        /// <returns>The text with tabs inserted between words.</returns>
        public string InsertTabsBetweenWords(string text1, string text2)
        {
            return $"{text1}{HorizontalTab}{text2}";
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
        /// <returns>The wrapped text command.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is null or empty.</exception>
        /// <example>
        /// <code>
        /// var wrapper = new EscpPrinterWrapper(null);
        /// string command = wrapper.WrapText("Hello", "World", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide);
        /// </code>
        /// </example>
        public string WrapText(string text1, string text2, int fontSize, FontType fontType = FontType.Brougham, Bold bold = Bold.Off, Italic italic = Italic.Off, Underline underline = Underline.None, Alignment alignment = Alignment.Left, Spacing spacing = Spacing.Normal)
        {
            _logger.LogInformation($"Wrapping text: {text1} {text2}, FontSize: {fontSize}, FontType: {fontType}, Bold: {bold}, Italic: {italic}, Underline: {underline}, Alignment: {alignment}, Spacing: {spacing}");

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text1) + " or " + nameof(text2));
            }

            string combinedText = InsertTabsBetweenWords(text1, text2);
            string setFontSize = $"{Esc}X{fontSize}";  // Set font size (for bitmap fonts min: 24, for outline fonts min: 33)
            string setFontType = $"{Esc}k{(char)fontType}";  // Set font type
            string setBold = $"{Esc}{(char)bold}";  // Set bold
            string setItalic = $"{Esc}{(char)italic}";  // Set italic
            string setUnderline = $"{Esc}-{(char)underline}";  // Set underline
            string setAlignment = $"{Esc}a{(int)alignment}";  // Set alignment
            string setSpacing = $"{Esc} {(char)spacing}";  // Set spacing

            string result = Esc + "SOH" + setFontSize + setFontType + setBold + setItalic + setUnderline + setAlignment + setSpacing + combinedText + CarriageReturn;
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
        /// <example>
        /// <code>
        /// var wrapper = new EscpPrinterWrapper(null);
        /// string command = wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center);
        /// </code>
        /// </example>
        public string WrapBarcode(string data, BarcodeType barcodeType, int height = 100, BarcodeWidth width = BarcodeWidth.Medium, BarcodeRatio ratio = BarcodeRatio.TwoToOne, bool printCharsBelow = true, Alignment alignment = Alignment.Left)
        {
            _logger.LogInformation($"Wrapping barcode: {data}, BarcodeType: {barcodeType}, Height: {height}, Width: {width}, Ratio: {ratio}, PrintCharsBelow: {printCharsBelow}, Alignment: {alignment}");

            string setType = $"{Esc}i{(char)barcodeType}";  // Barcode type
            string setHeight = $"{Esc}h{height:D2}";  // Height
            string setWidth = $"{Esc}w{(char)width}";  // Width
            string setRatio = $"{Esc}z{(char)ratio}";  // Ratio
            string setCharsBelow = $"{Esc}r{(printCharsBelow ? '1' : '0')}";  // Characters below barcode
            string setAlignment = $"{Esc}a{(int)alignment}";  // Alignment

            // Determine the end of barcode command based on barcode type
            string endOfBarcode = barcodeType == BarcodeType.CODE128 ? "\\\\" : "\\";

            string command = $"{setType}{setCharsBelow}{setHeight}{setWidth}{setRatio}{setAlignment}B{data}{endOfBarcode}";
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
                sb.Append(CarriageReturn);  // Add carriage return after each command
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
                    sb.Append($"\\u{((int)c):x4}");
                }
                else
                {
                    sb.Append(c);
                    sb.Append(CarriageReturn);
                }
            }
            return sb.ToString();
        }
    }
}