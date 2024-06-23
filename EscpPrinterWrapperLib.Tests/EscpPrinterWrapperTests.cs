using System;
using System.Collections.Generic;
using Xunit;
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;

namespace EscpPrinterWrapperLib.Tests
{
    public class EscpPrinterWrapperTests
    {
        private string EscapeNonPrintable(byte[] input)
        {
            var sb = new System.Text.StringBuilder();
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

        [Fact]
        public void WrapText_ShouldReturnCorrectEscpCommand()
        {
            var wrapper = new EscpPrinterWrapper(null);
            byte[] result = wrapper.WrapText("Hello", "World", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide, true);
            byte[] expected = { 0x1B, (byte)'S', (byte)'O', (byte)'H', 0x1B, (byte)'X', 0x32, 0x34, 0x1B, (byte)'k', 0x03, 0x1B, (byte)'E', 0x1B, 0x35, 0x1B, 0x2D, 0x31, 0x1B, 0x61, 0x31, 0x1B, 0x20, 0x31, (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o', 0x09, (byte)'W', (byte)'o', (byte)'r', (byte)'l', (byte)'d', 0x0D };
            Assert.Equal(EscapeNonPrintable(expected), EscapeNonPrintable(result));
        }

        [Fact]
        public void WrapBarcode_ShouldReturnCorrectEscpCommand()
        {
            var wrapper = new EscpPrinterWrapper(null);
            byte[] result = wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center, true);
            byte[] expected = { 0x1B, (byte)'i', (byte)'a', 0x1B, (byte)'h', 0x37, 0x30, 0x1B, (byte)'w', 0x32, 0x1B, (byte)'z', 0x32, 0x1B, (byte)'r', 0x31, 0x1B, (byte)'a', 0x31, (byte)'B', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', 0x5C, 0x5C, 0x0D };
            Assert.Equal(EscapeNonPrintable(expected), EscapeNonPrintable(result));
        }

        [Fact]
        public void GeneratePrintCommand_ShouldReturnCorrectCommand()
        {
            var wrapper = new EscpPrinterWrapper(null);
            var commands = new List<byte[]>
            {
                wrapper.WrapText("Test1", "Test2", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide, true),
                wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center, true)
            };
            byte[] result = wrapper.GeneratePrintCommand(commands, cutPaper: true, landscapeOrientation: true);
            byte[] expected = { 0x1B, (byte)'@', 0x1B, 0x69, 0x4C, 0x1B, (byte)'S', (byte)'O', (byte)'H', 0x1B, (byte)'X', 0x32, 0x34, 0x1B, (byte)'k', 0x03, 0x1B, (byte)'E', 0x1B, 0x35, 0x1B, 0x2D, 0x31, 0x1B, 0x61, 0x31, 0x1B, 0x20, 0x31, (byte)'T', (byte)'e', (byte)'s', (byte)'t', (byte)'1', 0x09, (byte)'T', (byte)'e', (byte)'s', (byte)'t', (byte)'2', 0x0D, 0x1B, (byte)'i', (byte)'a', 0x1B, (byte)'h', 0x37, 0x30, 0x1B, (byte)'w', 0x32, 0x1B, (byte)'z', 0x32, 0x1B, (byte)'r', 0x31, 0x1B, (byte)'a', 0x31, (byte)'B', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', 0x5C, 0x5C, 0x0D, 0x1B, 0x69, 0x43, 0x0C };
            Assert.Equal(EscapeNonPrintable(expected), EscapeNonPrintable(result));
        }

        [Fact]
        public void GeneratePrintCommand_ShouldHandleMultipleTextCommands()
        {
            var wrapper = new EscpPrinterWrapper(null);
            var commands = new List<byte[]>
            {
                wrapper.WrapText("Text1", "Part1", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide, true),
                wrapper.WrapText("Text2", "Part2", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide, true),
                wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center, true)
            };
            byte[] result = wrapper.GeneratePrintCommand(commands, cutPaper: true, landscapeOrientation: true);
            byte[] expected = { 0x1B, (byte)'@', 0x1B, 0x69, 0x4C, 0x1B, (byte)'S', (byte)'O', (byte)'H', 0x1B, (byte)'X', 0x32, 0x34, 0x1B, (byte)'k', 0x03, 0x1B, (byte)'E', 0x1B, 0x35, 0x1B, 0x2D, 0x31, 0x1B, 0x61, 0x31, 0x1B, 0x20, 0x31, (byte)'T', (byte)'e', (byte)'x', (byte)'t', (byte)'1', 0x09, (byte)'P', (byte)'a', (byte)'r', (byte)'t', (byte)'1', 0x0D, 0x1B, (byte)'S', (byte)'O', (byte)'H', 0x1B, (byte)'X', 0x32, 0x34, 0x1B, (byte)'k', 0x03, 0x1B, (byte)'E', 0x1B, 0x35, 0x1B, 0x2D, 0x31, 0x1B, 0x61, 0x31, 0x1B, 0x20, 0x31, (byte)'T', (byte)'e', (byte)'x', (byte)'t', (byte)'2', 0x09, (byte)'P', (byte)'a', (byte)'r', (byte)'t', (byte)'2', 0x0D, 0x1B, (byte)'i', (byte)'a', 0x1B, (byte)'h', 0x37, 0x30, 0x1B, (byte)'w', 0x32, 0x1B, (byte)'z', 0x32, 0x1B, (byte)'r', 0x31, 0x1B, (byte)'a', 0x31, (byte)'B', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', 0x5C, 0x5C, 0x0D, 0x1B, 0x69, 0x43, 0x0C };
            Assert.Equal(EscapeNonPrintable(expected), EscapeNonPrintable(result));
        }
    }
}