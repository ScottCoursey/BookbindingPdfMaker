using BookbindingPdfMaker.Models;

namespace BookbindingPdfMaker.Services
{
    internal class StackedPageMatrixCalculator : IStackedPageMatrixCalculator
    {
        public IEnumerable<PageMatrixData> GetMatrix(IEnumerable<int> signatureList, int numPages)
        {
            var firstPageNum = 1;
            var signatureStartSheet = 1;

            var pages = new List<PageMatrixData>();

            for (var signatureSetStart = 0; signatureSetStart < signatureList.Count(); signatureSetStart += 2)
            {
                var upperSignatureSize = signatureList.ElementAt(signatureSetStart);
                var lowerSignatureSize = signatureList.ElementAt(signatureSetStart + 1);

                var largerSignatureSize = Math.Max(upperSignatureSize, lowerSignatureSize);
                var lesserSignatureSize = Math.Min(upperSignatureSize, lowerSignatureSize);

                for (var pageNum = 0; pageNum < largerSignatureSize * 2; pageNum++)
                {
                    var page = new PageMatrixData()
                    {
                        SheetNum = signatureStartSheet + (pageNum / 2),
                        FrontOrBack = pageNum % 2 == 1 ? PageFrontOrBack.Back : PageFrontOrBack.Front
                    };
                    pages.Add(page);
                }

                var upperPageRangeStart = firstPageNum;
                var upperPageRangeEnd = firstPageNum + upperSignatureSize * 4 - 1;
                var lowerPageRangeStart = upperPageRangeEnd + 1;
                var lowerPageRangeEnd = lowerPageRangeStart + lowerSignatureSize * 4 - 1;

                var pageNumFrontTopLeft = upperPageRangeStart + (upperPageRangeEnd - upperPageRangeStart) / 2;
                var pageNumFrontTopRight = pageNumFrontTopLeft + 1;
                var pageNumBackTopLeft = pageNumFrontTopLeft + 2;
                var pageNumBackTopRight = pageNumFrontTopLeft - 1;

                var pageNumFrontBottomLeft = lowerPageRangeStart + (lowerPageRangeEnd - lowerPageRangeStart) / 2;
                var pageNumFrontBottomRight = pageNumFrontBottomLeft + 1;
                var pageNumBackBottomLeft = pageNumFrontBottomLeft + 2;
                var pageNumBackBottomRight = pageNumFrontBottomLeft - 1;

                // These help track if a page in the signature needs to be marked as "DISCARD" by allowing the page
                // numbers to remain 0.
                var upperSheetsRemaining = upperSignatureSize;
                var lowerSheetsRemaining = lowerSignatureSize;

                for (var topSheetNum = signatureStartSheet + largerSignatureSize - 1; topSheetNum >= signatureStartSheet; topSheetNum--)
                {
                    var frontPage = pages.First(p => p.SheetNum == topSheetNum && p.FrontOrBack == PageFrontOrBack.Front);
                    var backPage = pages.First(p => p.SheetNum == topSheetNum && p.FrontOrBack == PageFrontOrBack.Back);

                    if (upperSheetsRemaining > 0)
                    {
                        frontPage.PageNumTopLeft = pageNumFrontTopLeft;
                        pageNumFrontTopLeft -= 2;

                        frontPage.PageNumTopRight = pageNumFrontTopRight;
                        pageNumFrontTopRight += 2;

                        backPage.PageNumTopLeft = pageNumBackTopLeft;
                        pageNumBackTopLeft += 2;

                        backPage.PageNumTopRight = pageNumBackTopRight;
                        pageNumBackTopRight -= 2;
                    }

                    if (lowerSheetsRemaining > 0)
                    {
                        frontPage.PageNumBottomLeft = pageNumFrontBottomLeft;
                        pageNumFrontBottomLeft -= 2;

                        frontPage.PageNumBottomRight = pageNumFrontBottomRight;
                        pageNumFrontBottomRight += 2;

                        backPage.PageNumBottomLeft = pageNumBackBottomLeft;
                        pageNumBackBottomLeft += 2;

                        backPage.PageNumBottomRight = pageNumBackBottomRight;
                        pageNumBackBottomRight -= 2;
                    }

                    upperSheetsRemaining--;
                    lowerSheetsRemaining--;
                }

                signatureStartSheet += upperSignatureSize;

                firstPageNum += largerSignatureSize * 4;
                firstPageNum += lesserSignatureSize * 4;
            }

            foreach (var page in pages)
            {
                page.ClearPageNumIfOver(numPages);
            }

            return pages;
        }
    }
}
