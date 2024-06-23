using System;
using Xunit;
using EscpPrinterWrapperLib;
using EscpPrinterWrapperLib.Enums;

namespace EscpPrinterWrapperLib.Tests
{
    public class EscpPrinterWrapperTests
    {
        private string EscapeNonPrintable(string input)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in input)
            {
                if (char.IsControl(c))
                {
                    sb.Append($"\\u{((int)c):x4}");
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        [Fact]
        public void WrapText_ShouldReturnCorrectEscpCommand()
        {
            var wrapper = new EscpPrinterWrapper(null);
            string result = wrapper.WrapText("Hello", "World", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide);
            string expected = EscapeNonPrintable("\u001bSOH\u001bX24\u001bk\u0003\u001bE\u001b5\u001b-1\u001ba1\u001b 1Hello\u0009World");
            Assert.Equal(expected, EscapeNonPrintable(result));
        }

        [Fact]
        public void WrapBarcode_ShouldReturnCorrectEscpCommand()
        {
            var wrapper = new EscpPrinterWrapper(null);
            string result = wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center);
            string expected = EscapeNonPrintable("\u001bia\u001br1\u001bh70\u001bw2\u001bz2\u001ba1B123456789\\\\");
            Assert.Equal(expected, EscapeNonPrintable(result));
        }

        [Fact]
        public void GeneratePrintCommand_ShouldReturnCorrectCommand()
        {
            var wrapper = new EscpPrinterWrapper(null);
            var commands = new System.Collections.Generic.List<string>
            {
                wrapper.WrapText("Test1", "Test2", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide),
                wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center)
            };
            string result = wrapper.GeneratePrintCommand(commands, cutPaper: true, landscapeOrientation: true);
            string expected = EscapeNonPrintable("\u001b@\u001biL\u001bSOH\u001bX24\u001bk\u0003\u001bE\u001b5\u001b-1\u001ba1\u001b 1Test1\u0009Test2\u001bia\u001br1\u001bh70\u001bw2\u001bz2\u001ba1B123456789\\\\\u001biC\u000c");
            Assert.Equal(expected, EscapeNonPrintable(result));
        }

        [Fact]
        public void GeneratePrintCommand_ShouldHandleMultipleTextCommands()
        {
            var wrapper = new EscpPrinterWrapper(null);
            var commands = new System.Collections.Generic.List<string>
            {
                wrapper.WrapText("Text1", "Part1", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide),
                wrapper.WrapText("Text2", "Part2", 24, FontType.Helsinki, Bold.On, Italic.Off, Underline.Single, Alignment.Center, Spacing.Wide),
                wrapper.WrapBarcode("123456789", BarcodeType.CODE128, 70, BarcodeWidth.Medium, BarcodeRatio.TwoToOne, true, Alignment.Center)
            };
            string result = wrapper.GeneratePrintCommand(commands, cutPaper: true, landscapeOrientation: true);
            string expected = EscapeNonPrintable("\u001b@\u001biL\u001bSOH\u001bX24\u001bk\u0003\u001bE\u001b5\u001b-1\u001ba1\u001b 1Text1\u0009Part1\u001bSOH\u001bX24\u001bk\u0003\u001bE\u001b5\u001b-1\u001ba1\u001b 1Text2\u0009Part2\u001bia\u001br1\u001bh70\u001bw2\u001bz2\u001ba1B123456789\\\\\u001biC\u000c");
            Assert.Equal(expected, EscapeNonPrintable(result));
        }
    }
}