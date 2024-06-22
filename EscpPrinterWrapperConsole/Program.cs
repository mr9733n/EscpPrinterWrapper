﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;

namespace EscpPrinterWrapperConsole
{
    /// <summary>
    /// Program class for demonstrating the use of EscpPrinterWrapper library.
    /// </summary>
    /// <remarks>
    /// This class sets up logging and processes command line arguments to generate ESC/P commands for a QL-8XX series printer.
    /// </remarks>
    /// <author>Dima Green</author>
    /// <version>1.0.1</version>
    class Program
    {
        static void Main(string[] args)
        {
            // Setup logging
            using var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Program>();

            if (logger == null)
            {
                throw new InvalidOperationException("Logger not configured properly.");
            }

            // Validate arguments
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string outputFile = args[0];
            List<string> commands = new List<string>();

            var printerWrapper = new EscpPrinterWrapper(serviceProvider.GetService<ILogger<EscpPrinterWrapper>>());

            try
            {
                for (int i = 1; i < args.Length; i++)
                {
                    logger.LogInformation($"Processing argument: {args[i]}");
                    if (args[i].StartsWith("text:"))
                    {
                        var parameters = SplitParameters(args[i].Substring(5), logger);
                        logger.LogInformation($"Text parameters: {string.Join(", ", parameters)}");
                        if (parameters.Length < 2)
                        {
                            throw new ArgumentException("Insufficient parameters for text command.");
                        }
                        string text = parameters[0];
                        if (!int.TryParse(parameters[1], out int fontSize))
                        {
                            throw new FormatException($"Invalid fontSize: {parameters[1]}");
                        }
                        FontType fontType = parameters.Length > 2 ? Enum.Parse<FontType>(parameters[2], true) : FontType.Helsinki;
                        Bold bold = parameters.Length > 3 && bool.Parse(parameters[3]) ? Bold.On : Bold.Off;
                        Italic italic = parameters.Length > 4 && bool.Parse(parameters[4]) ? Italic.On : Italic.Off;
                        Underline underline = parameters.Length > 5 ? Enum.Parse<Underline>(parameters[5], true) : Underline.None;
                        Alignment alignment = parameters.Length > 6 ? Enum.Parse<Alignment>(parameters[6], true) : Alignment.Left;
                        Spacing spacing = parameters.Length > 7 ? Enum.Parse<Spacing>(parameters[7], true) : Spacing.Normal;
                        commands.Add(printerWrapper.WrapText(text, fontSize, fontType, bold, italic, underline, alignment, spacing));
                    }
                    else if (args[i].StartsWith("barcode:"))
                    {
                        var parameters = SplitParameters(args[i].Substring(8), logger);
                        logger.LogInformation($"Barcode parameters: {string.Join(", ", parameters)}");
                        if (parameters.Length < 6)
                        {
                            throw new ArgumentException("Insufficient parameters for barcode command.");
                        }
                        string data = parameters[0];
                        BarcodeType barcodeType = Enum.Parse<BarcodeType>(parameters[1], true);
                        int height = parameters.Length > 2 && int.TryParse(parameters[2], out int h) ? h : 100; // Default height
                        BarcodeWidth width = parameters.Length > 3 ? Enum.Parse<BarcodeWidth>(parameters[3], true) : BarcodeWidth.Medium; // Default width
                        BarcodeRatio ratio = parameters.Length > 4 ? Enum.Parse<BarcodeRatio>(parameters[4], true) : BarcodeRatio.TwoToOne; // Default ratio
                        bool printCharsBelow = parameters.Length > 5 && bool.Parse(parameters[5]);
                        Alignment alignment = parameters.Length > 6 ? Enum.Parse<Alignment>(parameters[6], true) : Alignment.Left;
                        commands.Add(printerWrapper.WrapBarcode(data, barcodeType, height, width, ratio, printCharsBelow, alignment));
                    }
                }

                // Read additional options from command line
                bool cutPaper = Array.Exists(args, arg => arg == "--cutPaper");
                bool landscapeOrientation = Array.Exists(args, arg => arg == "--landscape");
                int? pageFormatWidth = GetArgumentValue(args, "--pageFormatWidth");
                int? pageFormatHeight = GetArgumentValue(args, "--pageFormatHeight");
                int? pageLength = GetArgumentValue(args, "--pageLength");
                int? leftMargin = GetArgumentValue(args, "--leftMargin");
                int? rightMargin = GetArgumentValue(args, "--rightMargin");
                int? horizontalPosition = GetArgumentValue(args, "--horizontalPosition");
                int? verticalPosition = GetArgumentValue(args, "--verticalPosition");

                // Generate full print command with options
                string printCommand = printerWrapper.GeneratePrintCommand(
                    commands,
                    cutPaper: cutPaper,
                    landscapeOrientation: landscapeOrientation,
                    pageFormatWidth: pageFormatWidth,
                    pageFormatHeight: pageFormatHeight,
                    pageLength: pageLength,
                    leftMargin: leftMargin,
                    rightMargin: rightMargin,
                    horizontalPosition: horizontalPosition,
                    verticalPosition: verticalPosition);

                // Write result to output file
                File.WriteAllText(outputFile, printCommand);
                logger.LogInformation($"Print command written to {outputFile}");
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid argument provided.");
                PrintUsage();
            }
            catch (FormatException ex)
            {
                logger.LogError(ex, "Error parsing arguments.");
                PrintUsage();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing the commands.");
            }
        }

        /// <summary>
        /// Prints the usage instructions for the program.
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run <output file> <commands> [options]");
            Console.WriteLine("Commands:");
            Console.WriteLine("  text:<text>,<fontSize>,<fontType>,<bold>,<italic>,<underline>,<alignment>,<spacing>");
            Console.WriteLine("  barcode:<data>,<barcodeType>,<height>,<width>,<ratio>,<printCharsBelow>,<alignment>");
            Console.WriteLine("Options:");
            Console.WriteLine("  --cutPaper          Cut the paper after printing.");
            Console.WriteLine("  --landscape         Print in landscape orientation.");
            Console.WriteLine("  --pageFormatWidth   Set the page format width.");
            Console.WriteLine("  --pageFormatHeight  Set the page format height.");
            Console.WriteLine("  --pageLength        Set the page length.");
            Console.WriteLine("  --leftMargin        Set the left margin.");
            Console.WriteLine("  --rightMargin       Set the right margin.");
            Console.WriteLine("  --horizontalPosition Set the horizontal position.");
            Console.WriteLine("  --verticalPosition  Set the vertical position.");
        }

        /// <summary>
        /// Splits a parameter string into individual parameters.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="logger">The logger instance.</param>
        /// <returns>An array of split parameters.</returns>
        private static string[] SplitParameters(string input, ILogger logger)
        {
            var parameters = new List<string>();
            var currentParameter = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    parameters.Add(currentParameter.ToString().Trim());
                    currentParameter.Clear();
                }
                else
                {
                    currentParameter.Append(c);
                }
            }

            if (currentParameter.Length > 0)
            {
                parameters.Add(currentParameter.ToString().Trim());
            }

            logger.LogInformation($"SplitParameters: {string.Join(", ", parameters)}");
            return parameters.ToArray();
        }

        /// <summary>
        /// Retrieves the value of a specific argument from the command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="argumentName">The name of the argument to retrieve.</param>
        /// <returns>The value of the argument, or null if not found.</returns>
        private static int? GetArgumentValue(string[] args, string argumentName)
        {
            int index = Array.FindIndex(args, arg => arg == argumentName);
            if (index >= 0 && index < args.Length - 1 && int.TryParse(args[index + 1], out int value))
            {
                return value;
            }
            return null;
        }
    }
}