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
        private double _aspectRatio;
        private double _width;
        private double _height;

        public PdfMaker(MainWindowViewModel mwvm, IStackedPageMatrixCalculator stackedPageMatrixCalculator)
        {
            _mwvm = mwvm;
            _stackedPageMatrixCalculator = stackedPageMatrixCalculator;
        }

        public bool SetInputFileName(string fileName)
        {
            _mwvm.InputFilePath = fileName;

            try
            {
                PdfInputForm = XPdfForm.FromFile(fileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void SetOutputPath(string selectedPath)
        {
            _mwvm.OutputPath = selectedPath;
        }

        public SignatureInfo? ReadSignatureInfo(string inputPdfPath)
        {
            if (!File.Exists(inputPdfPath))
            {
                return null;
            }

            using (PdfInputForm = XPdfForm.FromFile(inputPdfPath))
            {
                return GetSignatureInfo();
            }
        }

        public void Generate(string inputPdfPath, string outputSignatureFolder)
        {
            try
            {
                _aspectRatio = -1;
                _outputSignatureFolder = outputSignatureFolder;

                CalculateBookSize();
                _width = _outputBookWidth;
                _height = _outputBookHeight;

                using (PdfInputForm = XPdfForm.FromFile(inputPdfPath))
                {
                    var signatureInfo = GetSignatureInfo();
                    int.TryParse(_mwvm.NumberOfPages, out var numberOfPages);
                    numberOfPages += _mwvm.AddFlyleaf ? 4 : 0;
                    var pageMatrixCollection = _stackedPageMatrixCalculator.GetMatrix(signatureInfo.SignatureSizeList, numberOfPages);
                    var numberOfSignatures = signatureInfo.SignatureSizeList.Count();
                    var signaturePageStart = 1;
                    var signatureSheetStart = 1;
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
                                    var outputSignaturePageNumber = signaturePageNumberMax - signaturePageNumber;
                                    var outputSheetNumber = signatureSheetStart + ((outputSignaturePageNumber + 1) / 2) - 1;

                                    var isFront = signaturePageNumber % 2 == 0;
                                    var pageMatrix = pageMatrixCollection.FirstOrDefault(pm =>
                                        pm.SheetNum == outputSheetNumber &&
                                        pm.FrontOrBack == (
                                            isFront
                                                ? PageFrontOrBack.Front
                                                : PageFrontOrBack.Back
                                        )
                                    );

                                    if (pageMatrix != null)
                                    {
                                        ApplyPage(outputPage, pageMatrix.PageNumTopLeft, OutputLocation.TopLeft, PageDirection.TopToTop);
                                        ApplyPage(outputPage, pageMatrix.PageNumTopRight, OutputLocation.TopRight, PageDirection.TopToTop);
                                        ApplyPage(outputPage, pageMatrix.PageNumBottomLeft, OutputLocation.BottomLeft, PageDirection.TopToTop);
                                        ApplyPage(outputPage, pageMatrix.PageNumBottomRight, OutputLocation.BottomRight, PageDirection.TopToTop);
                                    }
                                }
                            }

                            _pdfOutputDoc.Save(Path.Combine(_outputSignatureFolder, $"{Constants.SignatureFileNamePrefix}{signatureNumberForFile + 1}.pdf"));
                            signatureNumberForFile++;
                            _pdfOutputDoc.Close();
                        }

                        signaturePageStart += currentSignatureSetSize * (_mwvm.LayoutIsStacked ? 8 : 4);
                        signatureSheetStart += currentSignatureSetSize;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void CalculateBookSize()
        {
            float tempFloat;
            switch (_mwvm.SizeOfBook)
            {
                case BookSize.StandardPaperback:
                    float.TryParse(App.Configuration[Constants.ConfigValue.BookSizes.StandardPaperback.Width]!.ToString(), out tempFloat);
                    _outputBookWidth = XUnit.FromInch(tempFloat).Point;

                    float.TryParse(App.Configuration[Constants.ConfigValue.BookSizes.StandardPaperback.Height]!.ToString(), out tempFloat);
                    _outputBookHeight = XUnit.FromInch(tempFloat).Point;
                    break;

                case BookSize.LargeFormatPaperback:
                    float.TryParse(App.Configuration[Constants.ConfigValue.BookSizes.LargePaperback.Width]!.ToString(), out tempFloat);
                    _outputBookWidth = XUnit.FromInch(tempFloat).Point;

                    float.TryParse(App.Configuration[Constants.ConfigValue.BookSizes.LargePaperback.Height]!.ToString(), out tempFloat);
                    _outputBookHeight = XUnit.FromInch(tempFloat).Point;
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
            Trace.WriteLine($"InputPageNum: {inputPageNum}");
            if (inputPageNum > PdfInputForm!.PageCount + (_mwvm.AddFlyleaf ? 4 : 0))
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
                if (inputPageNum < 3 || inputPageNum > PdfInputForm!.PageCount + (_mwvm.AddFlyleaf ? 2 : 0))
                {
                    outputPageImage = false;
                }
                else
                {
                    inputPageNum -= 2;
                }
            }

            var paperWidth = outputPage.Width;
            var paperHeight = outputPage.Height;

            using (var gfx = XGraphics.FromPdfPage(outputPage))
            {
                if (inputPageNum > 0)
                {
                    PdfInputForm.PageNumber = inputPageNum;
                }

                if (_mwvm.SelectedScaleOfPage!.ScaleOfPage != PageScaling.Stretch)
                {
                    if (_aspectRatio == -1)
                    {
                        _aspectRatio = PdfInputForm.PointWidth / PdfInputForm.PointHeight;
                        if (_mwvm.SelectedScaleOfPage.ScaleOfPage == PageScaling.KeepProportionWidth)
                        {
                            _height = _width / _aspectRatio;
                        }
                        else
                        {
                            _width = _height * _aspectRatio;
                        }
                    }
                }

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
                                left = (paperWidth / 4) - (_width / 2);
                                top = (paperHeight / 4) - (_height / 2);
                                break;

                            case OutputLocation.TopRight:
                                left = (paperWidth * 3 / 4) - (_width / 2);
                                top = (paperHeight / 4) - (_height / 2);
                                break;

                            case OutputLocation.BottomLeft:
                                left = (paperWidth / 4) - (_width / 2);
                                top = (paperHeight * 3 / 4) - (_height / 2);
                                break;

                            case OutputLocation.BottomRight:
                                left = (paperWidth * 3 / 4) - (_width / 2);
                                top = (paperHeight * 3 / 4) - (_height / 2);
                                break;
                        }
                    }
                    else
                    {
                        if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight / 4) - (_width / 2);
                            top = -(paperWidth / 2) - (_height / 2);
                        }
                        else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                        {
                            rotation = -90;
                            left = -(paperHeight / 4) - (_width / 2);
                            top = (paperWidth / 2) - (_height / 2);
                        }
                        else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight * 3 / 4) - (_width / 2);
                            top = -(paperWidth / 2) - (_height / 2);
                        }
                        else
                        {
                            rotation = -90;
                            left = -(paperHeight * 3 / 4) - (_width / 2);
                            top = (paperWidth / 2) - (_height / 2);
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
                                left = (paperWidth / 2) - _width - offsetFromSpineAmount;
                                top = (paperHeight / 4) - (_height / 2);
                                break;

                            case OutputLocation.TopRight:
                                left = (paperWidth / 2) + offsetFromSpineAmount;
                                top = (paperHeight / 4) - (_height / 2);
                                break;

                            case OutputLocation.BottomLeft:
                                left = (paperWidth / 2) - _width - offsetFromSpineAmount;
                                top = (paperHeight * 3 / 4) - (_height / 2);
                                break;

                            case OutputLocation.BottomRight:
                                left = (paperWidth / 2) + offsetFromSpineAmount;
                                top = (paperHeight * 3 / 4) - (_height / 2);
                                break;
                        }
                    }
                    else
                    {
                        if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight / 2) - _width - offsetFromSpineAmount;
                            top = -(paperWidth / 2) - (_height / 2);
                        }
                        else if (outputLocation == OutputLocation.Top && pageDirection == PageDirection.TopToLeft)
                        {
                            rotation = -90;
                            left = -(paperHeight / 2) + offsetFromSpineAmount;
                            top = (paperWidth / 2) - (_height / 2);
                        }
                        else if (outputLocation == OutputLocation.Bottom && pageDirection == PageDirection.TopToRight)
                        {
                            rotation = 90;
                            left = (paperHeight / 2) + offsetFromSpineAmount;
                            top = -(paperWidth / 2) - (_height / 2);
                        }
                        else
                        {
                            rotation = -90;
                            left = -(paperHeight / 2) - _width - offsetFromSpineAmount;
                            top = (paperWidth / 2) - (_height / 2);
                        }
                    }
                }

                if (rotation != 0)
                {
                    gfx.RotateAtTransform(rotation, new XPoint(0, 0));
                }

                box = new XRect(left, top, _width, _height);

                if (inputPageNum == -1)
                {
                    gfx.DrawLines(XPens.Black,
                    [
                        new XPoint(box.Left, box.Top),
                        new XPoint(box.Left + box.Width, box.Top + box.Height)
                    ]);

                    gfx.DrawLines(XPens.Black,
                    [
                        new XPoint(box.Left + box.Width, box.Top),
                        new XPoint(box.Left, box.Top + box.Height)
                    ]);
                }
                else
                {
                    if (outputPageImage)
                    {
                        gfx.DrawImage(PdfInputForm, box);
                    }
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
