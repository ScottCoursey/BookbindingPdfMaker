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
        private double _outputBookWidth;
        private double _outputBookHeight;

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

            CalculateBookSize();

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
                            var outputPage = AddPage();

                            PageDirection direction = PageDirection.TopToRight;
                            var highLocation = OutputLocation.Top;
                            var lowLocation = OutputLocation.Bottom;

                            var highPageNum = (signaturePageStart + (currentSignatureSize * 4)) - signaturePageNumber - 1;
                            var lowPageNum = signaturePageStart + signaturePageNumber;

                            if (lowPageNum % 2 == 0)
                            {
                                if (_mwvm.AlternatePageRotation)
                                {
                                    direction = PageDirection.TopToLeft;
                                }
                            }
                            else
                            {
                                if (!_mwvm.AlternatePageRotation)
                                {
                                    highLocation = OutputLocation.Bottom;
                                    lowLocation = OutputLocation.Top;
                                }
                            }

                            ApplyPage(outputPage, highPageNum, highLocation, direction);
                            ApplyPage(outputPage, lowPageNum, lowLocation, direction);
                        }

                        _pdfOutputDoc.Save(Path.Combine(_outputSignatureFolder, $"Signature{signatureNumber + 1}.pdf"));
                        _pdfOutputDoc.Close();
                    }

                    signaturePageStart += currentSignatureSize * 4;
                }
            }
        }

        private void CalculateBookSize()
        {
            float f;
            switch (_mwvm.SizeOfBook)
            {
                case BookSize.StandardPaperback:
                    float.TryParse(App.Configuration["BookSizes:StandardPaperback:Width"]!.ToString(), out f);
                    _outputBookWidth = XUnit.FromInch(f).Point;

                    float.TryParse(App.Configuration["BookSizes:StandardPaperback:Height"]!.ToString(), out f);
                    _outputBookHeight = XUnit.FromInch(f).Point;
                    break;

                case BookSize.LargeFormatPaperback:
                    float.TryParse(App.Configuration["BookSizes:LargePaperback:Width"]!.ToString(), out f);
                    _outputBookWidth = XUnit.FromInch(f).Point;

                    float.TryParse(App.Configuration["BookSizes:LargePaperback:Height"]!.ToString(), out f);
                    _outputBookHeight = XUnit.FromInch(f).Point;
                    break;

                case BookSize.FullPaperSize:
                    _outputBookWidth = XUnit.FromInch(_mwvm.SelectedPaperSize.Height / 2).Point;
                    _outputBookHeight = XUnit.FromInch(_mwvm.SelectedPaperSize.Width).Point;
                    break;

                default:

                    switch (_mwvm.PageUnit)
                    {
                        case PageUnit.Inches:
                            _outputBookWidth = XUnit.FromInch(_mwvm.CustomBookSizeWidthF).Point;
                            _outputBookHeight = XUnit.FromInch(_mwvm.CustomBookSizeHeightF).Point;
                            break;

                        case PageUnit.Millimeters:
                            _outputBookWidth = XUnit.FromMillimeter(_mwvm.CustomBookSizeWidthF).Point;
                            _outputBookHeight = XUnit.FromMillimeter(_mwvm.CustomBookSizeHeightF).Point;
                            break;

                        case PageUnit.Points:
                            _outputBookWidth = _mwvm.CustomBookSizeWidthF;
                            _outputBookHeight = _mwvm.CustomBookSizeHeightF;
                            break;
                    }

                    break;
            }

        }

        private List<int> GetSignatureSizeList()
        {
            var pageCount = _pdfInputForm!.PageCount + (_mwvm.AddFlyleaf ? 4 : 0);

            var numberOfFullSignatures =  pageCount / (_defaultSignatureSize * 4);
            var pagesInPartialSignature = pageCount % (_defaultSignatureSize * 4);

            List<int> signatureSizeList = Enumerable.Repeat(_defaultSignatureSize, numberOfFullSignatures).ToList();
            if (pagesInPartialSignature > 0)
            {
                var fullPageCount = pagesInPartialSignature / 4 + (pagesInPartialSignature % 4 > 0 ? 1 : 0);
                signatureSizeList.Add(fullPageCount);
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
            newPage.Width = XUnit.FromInch(_mwvm.SelectedPaperSize.Width);
            newPage.Height = XUnit.FromInch(_mwvm.SelectedPaperSize.Height);
            return newPage;
        }

        private void ApplyBlankPage(PdfPage outputPage)
        {
            var paperWidth = outputPage.Width;
            var paperHeight = outputPage.Height;

            using (var gfx = XGraphics.FromPdfPage(outputPage))
            {
                if (_mwvm.OutputTestOverlay)
                {
                    gfx.DrawLines(XPens.LightPink, new[]
                    {
                        new XPoint(0, paperHeight / 2),
                        new XPoint(paperWidth, paperHeight / 2)
                    });
                }
            }
        }

        private void ApplyPage(PdfPage outputPage, int inputPageNum, OutputLocation outputLocation, PageDirection pageDirection)
        {
            if (inputPageNum > _pdfInputForm!.PageCount)
            {
                return;
            }

            var outputPageImage = true;
            if (_mwvm.AddFlyleaf)
            {
                if (inputPageNum < 3)
                {
                    outputPageImage = false;
                }
                else
                {
                    inputPageNum -= 2;
                }
            }

            using (var gfx = XGraphics.FromPdfPage(outputPage))
            {
                _pdfInputForm.PageNumber = inputPageNum;
                var width = _outputBookWidth;
                var height = _outputBookHeight;
                var paperWidth = outputPage.Width;
                var paperHeight = outputPage.Height;

                XRect box;

                if (_mwvm.OutputTestOverlay)
                {
                    gfx.DrawLines(XPens.LightPink, new[]
                    {
                        new XPoint(0, paperHeight / 2),
                        new XPoint(paperWidth, paperHeight / 2)
                    });
                }

                if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                {
                    gfx.RotateAtTransform(90, new XPoint(0, 0));
                    box = new XRect((paperHeight / 4) - (width / 2), -(paperWidth / 2) - (height / 2), width, height);
                }
                else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                {
                    gfx.RotateAtTransform(-90, new XPoint(0, 0));
                    box = new XRect(-(paperHeight / 4) - (width / 2), (paperWidth / 2) - (height / 2), width, height);
                }
                else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                {
                    gfx.RotateAtTransform(90, new XPoint(0, 0));
                    box = new XRect((paperHeight * 3 / 4) - (width / 2), -(paperWidth / 2) - (height / 2), width, height);
                }
                else
                {
                    gfx.RotateAtTransform(-90, new XPoint(0, 0));
                    box = new XRect(-(paperHeight * 3 / 4) - (width / 2), (paperWidth / 2) - (height / 2), width, height);
                }

                if (outputPageImage)
                {
                    gfx.DrawImage(_pdfInputForm, box);
                }

                if (_mwvm.OutputTestOverlay)
                {
                    gfx.DrawLines(XPens.LightBlue, new[]
                    {
                        new XPoint(box.Left, box.Top),
                        new XPoint(box.Left, box.Bottom),
                        new XPoint(box.Right, box.Bottom),
                        new XPoint(box.Right, box.Top),
                        new XPoint(box.Left, box.Top)
                    });
                }
            }
        }
    }
}
