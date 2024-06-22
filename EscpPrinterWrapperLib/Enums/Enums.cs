namespace EscpPrinterWrapperLib.Enums
{
    /// <summary>
    /// Enumeration of barcode types.
    /// </summary>
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

    /// <summary>
    /// Enumeration of text alignments.
    /// </summary>
    public enum Alignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    /// <summary>
    /// Enumeration of font types.
    /// </summary>
    public enum FontType
    {
        FontA = '0',
        FontB = '1'
    }

    /// <summary>
    /// Enumeration of bold settings.
    /// </summary>
    public enum Bold
    {
        Off = 'F',
        On = 'E'
    }

    /// <summary>
    /// Enumeration of italic settings.
    /// </summary>
    public enum Italic
    {
        Off = '5',
        On = '4'
    }

    /// <summary>
    /// Enumeration of underline settings.
    /// </summary>
    public enum Underline
    {
        None = '0',
        Single = '1',
        Double = '2'
    }

    /// <summary>
    /// Enumeration of character spacings.
    /// </summary>
    public enum Spacing
    {
        Normal = '0',
        Wide = '1'
    }

    /// <summary>
    /// Enumeration of barcode widths.
    /// </summary>
    public enum BarcodeWidth
    {
        ExtraSmall = '0',
        Small = '1',
        Medium = '2',
        Large = '3'
    }

    /// <summary>
    /// Enumeration of barcode ratios.
    /// </summary>
    public enum BarcodeRatio
    {
        ThreeToOne = '0',
        TwoPointFiveToOne = '1',
        TwoToOne = '2'
    }
}