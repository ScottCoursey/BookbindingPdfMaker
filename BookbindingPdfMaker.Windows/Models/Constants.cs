namespace BookbindingPdfMaker.Models
{
    internal static class Constants
    {
        public const string N_A = "N/A";
        public const string NoFileSelected = "No File Selected";
        public const string NoFolderSelected = "No Folder Selected";
        public const string PrinterTypeSingle = "Single Sided";
        public const string PrinterTypeDouble = "Double Sided / Duplex";
        public const string PageScaleKeepProportionWidth = "Keep Proportion (by Width)";
        public const string PageScaleKeepProportionHeight = "Keep Proportion (by Height)";
        public const string PageScaleStretchToFit = "Stretch To Fit";
        public const string SignatureFileNamePrefix = "Signature ";
        public const string WindowTitle = "Bookbinding PDF Maker";

        public static class ConfigValue
        {
            public static class BookSizes
            {
                public static class StandardPaperback
                {
                    public const string Width = "BookSizes:StandardPaperback:Width";
                    public const string Height = "BookSizes:StandardPaperback:Height";
                }

                public static class LargePaperback
                {
                    public const string Width = "BookSizes:LargePaperback:Width";
                    public const string Height = "BookSizes:LargePaperback:Height";
                }
            }
        }
    }
}
