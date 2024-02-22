using PdfSharp.Drawing;

namespace BookbindingPdfMaker.Services
{
    internal interface IPdfMaker
    {
        XPdfForm? PdfInputForm { get; }

        void Generate(string inputPdfPath, string outputSignatureFolder);
        void SetInputFileName(string fileName);
        void SetOutputPath(string selectedPath);
        SignatureInfo ReadSignatureInfo(string inputPdfPath);
    }
}
