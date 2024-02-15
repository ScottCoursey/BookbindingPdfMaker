namespace BookbindingPdfMaker.Services
{
    internal interface IPdfMaker
    {
        void Generate(string inputPdfPath, string outputSignatureFolder);
        void SetInputFileName(string fileName);
        void SetOutputPath(string selectedPath);
    }
}
