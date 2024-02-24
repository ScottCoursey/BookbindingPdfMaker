using BookbindingPdfMaker.Models;

namespace BookbindingPdfMaker.Services
{
    internal interface IStackedPageMatrixCalculator
    {
        IEnumerable<PageMatrixData> GetMatrix(IEnumerable<int> signatureList, int numPages);
    }
}