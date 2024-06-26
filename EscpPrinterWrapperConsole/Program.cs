﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;
using System.Reflection;

namespace EscpPrinterWrapperConsole
{
    /// <summary>
    /// Program class for demonstrating the use of EscpPrinterWrapper library.
    /// </summary>
    /// <remarks>
    /// This class sets up logging and processes command line arguments to generate ESC/P commands for a QL-8XX series printer.
    /// </remarks>
    class Program
    {
        private static readonly Assembly assembly;
        private static readonly string title;
        private static readonly string version;

        static Program()
        {
            assembly = Assembly.GetExecutingAssembly();
            title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Unknown Application";
            version = assembly.GetName().Version?.ToString() ?? "Unknown Version";
        }

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

            Console.WriteLine($"Program class for demonstrating the use of EscpPrinterWrapper library.\n");

            string outputFile = args[0];
            List<byte[]> commands = new List<byte[]>();
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
                        if (parameters.Length < 8)
                        {
                            throw new ArgumentException("Insufficient parameters for text command.");
                        }
                        string text1 = parameters[0];
                        string text2 = parameters[1];
                        if (!int.TryParse(parameters[2], out int fontSize))
                        {
                            throw new FormatException($"Invalid fontSize: {parameters[2]}");
                        }
                        FontType fontType = Enum.Parse<FontType>(parameters[3], true);
                        Bold bold = bool.Parse(parameters[4]) ? Bold.On : Bold.Off;
                        Italic italic = bool.Parse(parameters[5]) ? Italic.On : Italic.Off;
                        Underline underline = Enum.Parse<Underline>(parameters[6], true);
                        Alignment alignment = Enum.Parse<Alignment>(parameters[7], true);
                        Spacing spacing = parameters.Length > 8 ? Enum.Parse<Spacing>(parameters[8], true) : Spacing.Normal;
                        bool includeCarriageReturn = parameters.Length > 9 && bool.Parse(parameters[9]);
                        commands.Add(printerWrapper.WrapText(text1, text2, fontSize, fontType, bold, italic, underline, alignment, spacing, includeCarriageReturn));
                    }
                    else if (args[i].StartsWith("barcode:"))
                    {
                        var parameters = SplitParameters(args[i].Substring(8), logger);
                        logger.LogInformation($"Barcode parameters: {string.Join(", ", parameters)}");
                        if (parameters.Length < 7)
                        {
                            throw new ArgumentException("Insufficient parameters for barcode command.");
                        }
                        string data = parameters[0];
                        BarcodeType barcodeType = Enum.Parse<BarcodeType>(parameters[1], true);
                        int height = int.TryParse(parameters[2], out int h) ? h : 100; // Default height
                        BarcodeWidth width = Enum.Parse<BarcodeWidth>(parameters[3], true); // Default width
                        BarcodeRatio ratio = Enum.Parse<BarcodeRatio>(parameters[4], true); // Default ratio
                        bool printCharsBelow = bool.Parse(parameters[5]);
                        Alignment alignment = Enum.Parse<Alignment>(parameters[6], true);
                        bool includeCarriageReturn = parameters.Length > 7 && bool.Parse(parameters[7]);
                        commands.Add(printerWrapper.WrapBarcode(data, barcodeType, height, width, ratio, printCharsBelow, alignment, includeCarriageReturn));
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
                byte[] printCommandBytes = printerWrapper.GeneratePrintCommand(
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

                // Convert byte array to string for output
                string printCommand = Encoding.ASCII.GetString(printCommandBytes);

                // Write result to output file
                File.WriteAllText(outputFile, printCommand);
                Console.WriteLine($"\n*******************************************");
                Console.WriteLine($"{title} - Version {version}\n");
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
            Console.WriteLine($"{title} - Version {version}");
            Console.WriteLine($"*******************************************\n");
            Console.WriteLine("Usage: dotnet run <output file> <commands> [options]");
            Console.WriteLine("Commands:");
            Console.WriteLine("  text:<text1>,<text2>,<fontSize>,<fontType>,<bold>,<italic>,<underline>,<alignment>,<spacing>,<includeCarriageReturn>");
            Console.WriteLine("  barcode:<data>,<barcodeType>,<height>,<width>,<ratio>,<printCharsBelow>,<alignment>,<includeCarriageReturn>");
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
                    parameters.Add(currentParameter.ToString().Trim('\'').Trim());
                    currentParameter.Clear();
                }
                else
                {
                    currentParameter.Append(c);
                }
            }

            if (currentParameter.Length > 0)
            {
                parameters.Add(currentParameter.ToString().Trim('\'').Trim());
            }

            logger.LogInformation($"SplitParameters: {string.Join(", ", parameters)}");
            return parameters.ToArray();
        }

        /// <summary>
        /// Retrieves the value of a specific argument from the command line arguments.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// The name of the argument to retrieve.
        /// The value of the argument, or null if not found.
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