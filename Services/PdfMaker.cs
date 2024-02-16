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
                            var highPageNum = (signaturePageStart + (currentSignatureSize * 4)) - signaturePageNumber - 1;
                            var lowPageNum = signaturePageStart + signaturePageNumber;

                            var outputPage = AddPage();

                            PageDirection direction = PageDirection.TopToRight;
                            var highLocation = OutputLocation.Top;
                            var lowLocation = OutputLocation.Bottom;
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
                    _outputBookWidth = XUnit.FromInch(_mwvm.SelectedPaperSize.Width).Point;
                    _outputBookHeight = XUnit.FromInch(_mwvm.SelectedPaperSize.Height).Point;
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
            newPage.Width = XUnit.FromInch(_mwvm.SelectedPaperSize.Width);
            newPage.Height = XUnit.FromInch(_mwvm.SelectedPaperSize.Height);
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
                var width = _outputBookWidth;
                var height = _outputBookHeight;
                var paperWidth = outputPage.Width;
                var paperHeight = outputPage.Height;

                XRect box;

                if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                {
                    gfx.RotateAtTransform(90, new XPoint(0, 0));
                    box = new XRect(0, -(paperWidth / 2) - (height / 2), width, height);
                }
                else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                {
                    gfx.RotateAtTransform(-90, new XPoint(0, 0));
                    box = new XRect(-paperHeight / 2, (paperWidth / 2) - (height / 2), width, height);
                }
                else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                {
                    gfx.RotateAtTransform(90, new XPoint(0, 0));
                    box = new XRect(paperHeight / 2, -(paperWidth / 2) - (height / 2), width, height);
                }
                else
                {
                    gfx.RotateAtTransform(-90, new XPoint(0, 0));
                    box = new XRect(-paperHeight, (paperWidth / 2) - (height / 2), width, height);
                }

                gfx.DrawImage(_pdfInputForm, box);
                gfx.DrawEllipse(XBrushes.Red, new XRect(box.Left - 2.5, box.Top - 2.5, 5, 5));
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
