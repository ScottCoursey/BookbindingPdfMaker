using BookbindingPdfMaker.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.IO;

namespace BookbindingPdfMaker.Services
{
    internal class PdfMaker : IPdfMaker
    {
        private readonly MainWindowViewModel _mwvm;
        private readonly IStackedPageMatrixCalculator _stackedPageMatrixCalculator;
        private PdfDocument? _pdfOutputDoc;
        public XPdfForm? PdfInputForm { get; private set; }
        private string? _outputSignatureFolder;
        private int _defaultSignatureSize;
        private double _outputBookWidth;
        private double _outputBookHeight;

        public PdfMaker(MainWindowViewModel mwvm, IStackedPageMatrixCalculator stackedPageMatrixCalculator)
        {
            _mwvm = mwvm;
            _stackedPageMatrixCalculator = stackedPageMatrixCalculator;
        }

        public void SetInputFileName(string fileName)
        {
            _mwvm.FileName = Path.GetFileName(fileName);
            _mwvm.InputPath = Path.GetDirectoryName(fileName) ?? "";
            _mwvm.InputFilePath = fileName;

            PdfInputForm = XPdfForm.FromFile(fileName);
        }

        public void SetOutputPath(string selectedPath)
        {
            _mwvm.OutputPath = selectedPath;
        }

        public SignatureInfo ReadSignatureInfo(string inputPdfPath)
        {
            using (PdfInputForm = XPdfForm.FromFile(inputPdfPath))
            {
                return GetSignatureInfo();
            }
        }

        public void Generate(string inputPdfPath, string outputSignatureFolder)
        {
            _outputSignatureFolder = outputSignatureFolder;

            CalculateBookSize();

            using (PdfInputForm = XPdfForm.FromFile(inputPdfPath))
            {
                var signatureInfo = GetSignatureInfo();
                int.TryParse(_mwvm.NumberOfPages, out var numberOfPages);
                var pageMatrix = _stackedPageMatrixCalculator.GetMatrix(signatureInfo.SignatureSizeList, numberOfPages);
                var numberOfSignatures = signatureInfo.SignatureSizeList.Count();
                var signaturePageStart = 1;
                var signatureIncrement = _mwvm.LayoutIsStacked ? 2 : 1;
                var signatureNumberForFile = 0;
                for (var signatureNumber = 0; signatureNumber < numberOfSignatures; signatureNumber += signatureIncrement)
                {
                    int currentSignatureSetSize = _mwvm.LayoutIsStacked
                        ? Math.Max(signatureInfo.SignatureSizeList[signatureNumber], signatureInfo.SignatureSizeList[signatureNumber + 1])
                        : signatureInfo.SignatureSizeList[signatureNumber];

                    using (_pdfOutputDoc = new PdfDocument())
                    {
                        var signaturePageNumberMax = currentSignatureSetSize * 2;
                        for (var signaturePageNumber = 0; signaturePageNumber < signaturePageNumberMax; signaturePageNumber++)
                        {
                            var outputPage = AddPage();

                            if (!_mwvm.LayoutIsStacked)
                            {
                                var direction = PageDirection.TopToRight;
                                var highLocation = OutputLocation.Top;
                                var lowLocation = OutputLocation.Bottom;

                                var highPageNum = (signaturePageStart + (currentSignatureSetSize * 4)) - signaturePageNumber - 1;
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
                            else
                            {
                                var signatureFrontMin = signaturePageStart;
                                var signatureFrontMax = signatureFrontMin + (signatureInfo.SignatureSizeList[signatureNumber] * 4) - 1;

                                var signatureBackMin = signatureFrontMax + 1;
                                var signatureBackMax = signatureBackMin + (signatureInfo.SignatureSizeList[signatureNumber + 1] * 4) - 1;

                                int outputPageA;
                                int outputPageB;
                                int outputPageC;
                                int outputPageD;

                                var outputPageNumber = signaturePageStart + signaturePageNumber;

                                if (signaturePageNumber % 2 == 0)
                                {
                                    outputPageA = signaturePageStart - 1 + ((signatureFrontMax - signatureFrontMin) / 2) + signatureFrontMin - outputPageNumber;
                                    outputPageB = signaturePageStart - 1 + ((signatureFrontMax - signatureFrontMin) / 2) + signatureFrontMin + outputPageNumber + 1;
                                    outputPageC = signaturePageStart - 1 + ((signatureBackMax - signatureBackMin) / 2) + signatureBackMin - outputPageNumber;
                                    outputPageD = signaturePageStart - 1 + ((signatureBackMax - signatureBackMin) / 2) + signatureBackMin + outputPageNumber + 1;
                                }
                                else
                                {
                                    outputPageA = signaturePageStart - 1 + ((signatureFrontMax - signatureFrontMin) / 2) + signatureFrontMin + (((outputPageNumber / 2) - 1) * 2) + 2;
                                    outputPageB = signaturePageStart - 1 + ((signatureFrontMax - signatureFrontMin) / 2) + signatureFrontMin - (((outputPageNumber / 2) - 1) * 2) - 1;
                                    outputPageC = signaturePageStart - 1 + ((signatureBackMax - signatureBackMin) / 2) + signatureBackMin + (((outputPageNumber / 2) - 1) * 2) + 2;
                                    outputPageD = signaturePageStart - 1 + ((signatureBackMax - signatureBackMin) / 2) + signatureBackMin - (((outputPageNumber / 2) - 1) * 2) - 1;
                                }

                                Trace.WriteLine($"{outputPageA} {outputPageB} {outputPageC} {outputPageD}");

                                ApplyPage(outputPage, outputPageA, OutputLocation.TopLeft, PageDirection.TopToTop);
                                ApplyPage(outputPage, outputPageB, OutputLocation.TopRight, PageDirection.TopToTop);
                                ApplyPage(outputPage, outputPageC, OutputLocation.BottomLeft, PageDirection.TopToTop);
                                ApplyPage(outputPage, outputPageD, OutputLocation.BottomRight, PageDirection.TopToTop);
                            }
                        }

                        _pdfOutputDoc.Save(Path.Combine(_outputSignatureFolder, $"Signature{signatureNumberForFile + 1}.pdf"));
                        signatureNumberForFile++;
                        _pdfOutputDoc.Close();
                    }

                    signaturePageStart += currentSignatureSetSize * (_mwvm.LayoutIsStacked ? 8 : 4);
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
                    _outputBookWidth = XUnit.FromInch(_mwvm.SelectedPaperSize!.Height / 2).Point;
                    _outputBookHeight = XUnit.FromInch(_mwvm.SelectedPaperSize!.Width).Point;
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

        private SignatureInfo GetSignatureInfo()
        {
            var result = new SignatureInfo();
            var pageCount = PdfInputForm!.PageCount + (_mwvm.AddFlyleaf ? 4 : 0);

            switch (_mwvm.FormatOfSignature)
            {
                case SignatureFormat.Booklet:
                    _defaultSignatureSize = (pageCount / 4) + (pageCount % 4 > 0 ? 1 : 0);
                    break;

                case SignatureFormat.PerfectBound:
                    _defaultSignatureSize = 1;
                    break;

                case SignatureFormat.StandardSignatures:
                    _defaultSignatureSize = 8;
                    break;

                case SignatureFormat.CustomSignatures:
                    var split = _mwvm.CustomSignatures.Split(" ");
                    result.SignatureSizeList = split.Select(str => { int.TryParse(str, out var val); return val; }).ToList();
                    result.FullPageCount = result.SignatureSizeList.Sum(pc => pc) * 4;
                    return result;
            }

            var numberOfFullSignatures = pageCount / (_defaultSignatureSize * 4);
            var pagesInPartialSignature = pageCount % (_defaultSignatureSize * 4);

            List<int> signatureSizeList = Enumerable.Repeat(_defaultSignatureSize, numberOfFullSignatures).ToList();
            if (pagesInPartialSignature > 0)
            {
                var extraPageCount = (pagesInPartialSignature / 4) + (pagesInPartialSignature % 4 > 0 ? 1 : 0);
                signatureSizeList.Add(extraPageCount);
            }
            result.FullPageCount = pageCount + (pagesInPartialSignature % 4 > 0 ? 1 : 0);

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

            if (_mwvm.LayoutIsStacked && signatureSizeList.Count() % 2 == 1)
            {
                signatureSizeList.Add(0);
            }

            result.SignatureSizeList = signatureSizeList;

            return result;
        }

        private PdfPage AddPage()
        {
            var newPage = _pdfOutputDoc!.AddPage();
            newPage.Orientation = PageOrientation.Portrait;
            newPage.Width = XUnit.FromInch(_mwvm.SelectedPaperSize!.Width);
            newPage.Height = XUnit.FromInch(_mwvm.SelectedPaperSize!.Height);
            return newPage;
        }

        private void ApplyPage(PdfPage outputPage, int inputPageNum, OutputLocation outputLocation, PageDirection pageDirection)
        {
            if (inputPageNum > PdfInputForm!.PageCount)
            {
                return;
            }

            var offsetFromSpineAmount = _mwvm.PageUnit == PageUnit.Inches
                ? XUnit.FromInch(_mwvm.OffsetFromSpine)
                : _mwvm.PageUnit == PageUnit.Millimeters
                    ? XUnit.FromMillimeter(_mwvm.OffsetFromSpine)
                    : _mwvm.OffsetFromSpine;

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
                PdfInputForm.PageNumber = inputPageNum;
                var width = _outputBookWidth;
                var height = _outputBookHeight;

                if (_mwvm.SelectedScaleOfPage!.ScaleOfPage != PageScaling.Stretch)
                {
                    var aspectRatio = PdfInputForm.PointWidth / PdfInputForm.PointHeight;
                    if (_mwvm.SelectedScaleOfPage.ScaleOfPage == PageScaling.KeepProportionWidth)
                    {
                        height = width / aspectRatio;
                    }
                    else
                    {
                        width = height * aspectRatio;
                    }
                }

                var paperWidth = outputPage.Width;
                var paperHeight = outputPage.Height;

                XRect box;

                double rotation = 0;
                double top = 0;
                double left = 0;

                if (_mwvm.SourcePageAlignment == SourcePageAlignment.Centered)
                {
                    if (pageDirection == PageDirection.TopToTop)
                    {
                        switch (outputLocation)
                        {
                            case OutputLocation.TopLeft:
                                left = (paperWidth / 4) - (width / 2);
                                top = (paperHeight / 4) - (height / 2);
                                break;

                            case OutputLocation.TopRight:
                                left = (paperWidth * 3 / 4) - (width / 2);
                                top = (paperHeight / 4) - (height / 2);
                                break;

                            case OutputLocation.BottomLeft:
                                left = (paperWidth / 4) - (width / 2);
                                top = (paperHeight * 3 / 4) - (height / 2);
                                break;

                            case OutputLocation.BottomRight:
                                left = (paperWidth * 3 / 4) - (width / 2);
                                top = (paperHeight * 3 / 4) - (height / 2);
                                break;
                        }
                    }
                    else
                    {
                        if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight / 4) - (width / 2);
                            top = -(paperWidth / 2) - (height / 2);
                        }
                        else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                        {
                            rotation = -90;
                            left = -(paperHeight / 4) - (width / 2);
                            top = (paperWidth / 2) - (height / 2);
                        }
                        else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight * 3 / 4) - (width / 2);
                            top = -(paperWidth / 2) - (height / 2);
                        }
                        else
                        {
                            rotation = -90;
                            left = -(paperHeight * 3 / 4) - (width / 2);
                            top = (paperWidth / 2) - (height / 2);
                        }
                    }
                }
                else
                {
                    if (pageDirection == PageDirection.TopToTop)
                    {
                        switch (outputLocation)
                        {
                            case OutputLocation.TopLeft:
                                left = (paperWidth / 2) - width - offsetFromSpineAmount;
                                top = (paperHeight / 4) - (height / 2);
                                break;

                            case OutputLocation.TopRight:
                                left = (paperWidth / 2) + offsetFromSpineAmount;
                                top = (paperHeight / 4) - (height / 2);
                                break;

                            case OutputLocation.BottomLeft:
                                left = (paperWidth / 2) - width - offsetFromSpineAmount;
                                top = (paperHeight * 3 / 4) - (height / 2);
                                break;

                            case OutputLocation.BottomRight:
                                left = (paperWidth / 2) + offsetFromSpineAmount;
                                top = (paperHeight * 3 / 4) - (height / 2);
                                break;
                        }
                    }
                    else
                    {
                        if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight / 2) - width - offsetFromSpineAmount;
                            top = -(paperWidth / 2) - (height / 2);
                        }
                        else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                        {
                            rotation = -90;
                            left = -(paperHeight / 2) + offsetFromSpineAmount;
                            top = (paperWidth / 2) - (height / 2);
                        }
                        else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight / 2) + offsetFromSpineAmount;
                            top = -(paperWidth / 2) - (height / 2);
                        }
                        else
                        {
                            rotation = -90;
                            left = -(paperHeight / 2) - width - offsetFromSpineAmount;
                            top = (paperWidth / 2) - (height / 2);
                        }
                    }
                }

                if (rotation != 0)
                {
                    gfx.RotateAtTransform(rotation, new XPoint(0, 0));
                }

                box = new XRect(left, top, width, height);

                if (outputPageImage)
                {
                    gfx.DrawImage(PdfInputForm, box);
                }

                if (_mwvm.OutputTestOverlay)
                {
                    gfx.DrawLines(_mwvm.LayoutIsStacked ? XPens.Red : XPens.LightPink,
                    [
                        new XPoint(0, paperHeight / 2),
                        new XPoint(paperWidth, paperHeight / 2)
                    ]);

                    if (_mwvm.LayoutIsStacked)
                    {
                        gfx.DrawLines(XPens.LightPink,
                        [
                            new XPoint(paperWidth / 2, 0),
                            new XPoint(paperWidth / 2, paperHeight)
                        ]);
                    }

                    gfx.DrawLines(XPens.LightBlue,
                    [
                        new XPoint(box.Left, box.Top),
                        new XPoint(box.Left, box.Bottom),
                        new XPoint(box.Right, box.Bottom),
                        new XPoint(box.Right, box.Top),
                        new XPoint(box.Left, box.Top)
                    ]);
                }
            }
        }
    }
}
