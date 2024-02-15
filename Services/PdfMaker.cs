using BookbindingPdfMaker.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;

namespace BookbindingPdfMaker.Services
{
    internal class PdfMaker : IPdfMaker
    {
        private MainWindowViewModel _mwvm;
        private PdfDocument? _pdfOutputDoc;
        private XPdfForm? _pdfInputForm;
        private string? _outputSignatureFolder;
        private int _defaultSignatureSize;

        public PdfMaker(MainWindowViewModel mwvm)
        {
            _mwvm = mwvm;
        }

        public void SetInputFileName(string fileName)
        {
            _mwvm.FileName = Path.GetFileName(fileName);
            _mwvm.InputPath = Path.GetDirectoryName(fileName) ?? "";
        }

        public void SetOutputPath(string selectedPath)
        {
            _mwvm.OutputPath = selectedPath;
        }

        public void Generate(string inputPdfPath, string outputSignatureFolder)
        {
            _outputSignatureFolder = outputSignatureFolder;
            _defaultSignatureSize = 8;

            using (_pdfInputForm = XPdfForm.FromFile(inputPdfPath))
            {
                var signatureSizeList = GetSignatureSizeList();

                var numberOfSignatures = signatureSizeList.Count();
                var signaturePageStart = 1;
                for (var signatureNumber = 0; signatureNumber < numberOfSignatures; signatureNumber++)
                {
                    var currentSignatureSize = signatureSizeList[signatureNumber];
                    using (_pdfOutputDoc = new PdfDocument())
                    {

                        for (var signaturePageNumber = 0; signaturePageNumber < (currentSignatureSize * 2); signaturePageNumber++)
                        {
                            var topPageNum = (signaturePageStart + (currentSignatureSize * 4)) - signaturePageNumber - 1;
                            var bottomPageNum = signaturePageStart + signaturePageNumber;

                            PageDirection direction = bottomPageNum % 2 == 1 ? PageDirection.TopToRight : PageDirection.TopToLeft;

                            var outputPage = AddPage();
                            ApplyPage(outputPage, topPageNum, OutputLocation.Top, direction);
                            ApplyPage(outputPage, bottomPageNum, OutputLocation.Bottom, direction);
                        }

                        _pdfOutputDoc.Save(Path.Combine(_outputSignatureFolder, $"Signature{signatureNumber + 1}.pdf"));
                        _pdfOutputDoc.Close();
                    }

                    signaturePageStart += currentSignatureSize * 4;
                }
            }
        }

        private List<int> GetSignatureSizeList()
        {
            var numberOfFullSignatures = _pdfInputForm!.PageCount / (_defaultSignatureSize * 4);
            var pagesInPartialSignature = _pdfInputForm!.PageCount % _defaultSignatureSize;

            List<int> signatureSizeList = Enumerable.Repeat(_defaultSignatureSize, numberOfFullSignatures).ToList();
            if (pagesInPartialSignature > 0)
            {
                signatureSizeList.Add(pagesInPartialSignature);
            }

            var lastUpdated = -1;

            var lastIndex = signatureSizeList.Count() - 1;
            if (signatureSizeList.Count() > 1)
            {
                while (signatureSizeList[lastIndex] < signatureSizeList[lastIndex - 1])
                {
                    if (lastUpdated < 1)
                    {
                        lastUpdated = lastIndex;
                    }

                    lastUpdated--;
                    signatureSizeList[lastUpdated]--;
                    signatureSizeList[lastIndex]++;
                }
            }

            return signatureSizeList;
        }

        private PdfPage AddPage()
        {
            var newPage = _pdfOutputDoc!.AddPage();
            newPage.Orientation = PageOrientation.Portrait;
            newPage.Width = _mwvm.SelectedPaperSize.Width;
            newPage.Height = _mwvm.SelectedPaperSize.Height;
            return newPage;
        }

        private void ApplyPage(PdfPage outputPage, int inputPageNum, OutputLocation outputLocation, PageDirection pageDirection)
        {
            if (inputPageNum > _pdfInputForm!.PageCount)
            {
                return;
            }

            using (var gfx = XGraphics.FromPdfPage(outputPage))
            {
                _pdfInputForm.PageNumber = inputPageNum;
                var width = outputPage.Width;
                var height = outputPage.Height;

                XRect box;

                if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                {
                    gfx.RotateAtTransform(90, new XPoint(0, 0));
                    box = new XRect(0, -width, height / 2, width);
                }
                else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                {
                    gfx.RotateAtTransform(-90, new XPoint(0, 0));
                    box = new XRect(-height / 2, 0, height / 2, width);
                }
                else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                {
                    if (_mwvm.AlternatePageRotation)
                    {
                        gfx.RotateAtTransform(90, new XPoint(0, 0));
                        box = new XRect(height / 2, -width, height / 2, width);
                    }
                    else
                    {
                        gfx.RotateAtTransform(90, new XPoint(0, 0));
                        box = new XRect(0, -width, height / 2, width);
                    }
                }
                else
                {
                    if (_mwvm.AlternatePageRotation)
                    {
                        gfx.RotateAtTransform(-90, new XPoint(0, 0));
                        box = new XRect(-height, 0, height / 2, width);
                    }
                    else
                    {
                        gfx.RotateAtTransform(-90, new XPoint(0, 0));
                        box = new XRect(-height / 2, 0, height / 2, width);
                    }
                }

                gfx.DrawImage(_pdfInputForm, box);
            }
        }
    }
}
