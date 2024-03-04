using Newtonsoft.Json;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BookbindingPdfMaker.Models
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _inputFilePath = "";
        public string InputFilePath
        {
            get
            {
                return _inputFilePath;
            }

            set
            {
                if (value == _inputFilePath)
                {
                    return;
                }

                _inputFilePath = value;
                FileName = value;
                OnPropertyChanged();
            }
        }

        private bool _isDirty = false;

        [JsonIgnore]
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }

            set
            {
                if (value == _isDirty)
                {
                    return;
                }

                _isDirty = value;
            }
        }

        private string _projectFilePath;
        public string ProjectFilePath
        {
            get
            {
                return _projectFilePath;
            }

            set
            {
                if (value == _projectFilePath)
                {
                    return;
                }

                _projectFilePath = value;
                OnPropertyChanged();
            }
        }

        private string _fileName = Constants.NoFileSelected;
        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                if (value == _fileName)
                {
                    return;
                }

                _fileName = string.IsNullOrEmpty(value) ? Constants.NoFileSelected : Path.GetFileName(value);
                OnPropertyChanged();
            }
        }

        private string _pageSize = Constants.N_A;
        public string PageSize
        {
            get
            {
                return _pageSize;
            }

            set
            {
                if (value == _pageSize)
                {
                    return;
                }

                _pageSize = value;
                OnPropertyChanged();
            }
        }

        private string _numberOfPages = Constants.N_A;
        public string NumberOfPages
        {
            get
            {
                return _numberOfPages;
            }

            set
            {
                if (value == _numberOfPages)
                {
                    return;
                }

                _numberOfPages = value;
                OnPropertyChanged();
            }
        }

        private string _outputPath = Constants.NoFolderSelected;
        public string OutputPath
        {
            get
            {
                return _outputPath;
            }

            set
            {
                if (value == _outputPath)
                {
                    return;
                }

                _outputPath = value;
                OnPropertyChanged();
            }
        }

        private BookSize _sizeOfBook = BookSize.FullPaperSize;
        public BookSize SizeOfBook
        {
            get
            {
                return _sizeOfBook;
            }

            set
            {
                if (value == _sizeOfBook)
                {
                    return;
                }

                _sizeOfBook = value;
                IsCustomBookSize = _sizeOfBook == BookSize.Custom;
                OnPropertyChanged();
            }
        }

        public bool IsCustomBookSize
        {
            get
            {
                return _sizeOfBook == BookSize.Custom;
            }

            set
            {
                OnPropertyChanged();
            }
        }

        private SignatureFormat _formatOfSignature = SignatureFormat.StandardSignatures;
        public SignatureFormat FormatOfSignature
        {
            get
            {
                return _formatOfSignature;
            }

            set
            {
                if (value == _formatOfSignature)
                {
                    return;
                }

                _formatOfSignature = value;
                IsCustomSignatureFormat = _formatOfSignature == SignatureFormat.CustomSignatures;
                OnPropertyChanged();
            }
        }

        public bool IsCustomSignatureFormat
        {
            get
            {
                return _formatOfSignature == SignatureFormat.CustomSignatures;
            }

            set
            {
                OnPropertyChanged();
            }
        }

        private bool _addFlyleaf = false;
        public bool AddFlyleaf
        {
            get
            {
                return _addFlyleaf;
            }

            set
            {
                if (value == _addFlyleaf)
                {
                    return;
                }

                _addFlyleaf = value;
                OnPropertyChanged();
            }
        }

        private bool _outputTestOverlay = false;
        public bool OutputTestOverlay
        {
            get
            {
                return _outputTestOverlay;
            }

            set
            {
                if (value == _outputTestOverlay)
                {
                    return;
                }

                _outputTestOverlay = value;
                OnPropertyChanged();
            }
        }

        private bool _layoutIsStacked = false;
        public bool LayoutIsStacked
        {
            get
            {
                return _layoutIsStacked;
            }

            set
            {
                if (value == _layoutIsStacked)
                {
                    return;
                }

                _layoutIsStacked = value;
                OnPropertyChanged();
            }
        }

        private bool _alternatePageRotation = true;
        public bool AlternatePageRotation
        {
            get
            {
                return _alternatePageRotation;
            }

            set
            {
                if (value == _alternatePageRotation)
                {
                    return;
                }

                _alternatePageRotation = value;
                OnPropertyChanged();
            }
        }

        private string _customBookSizeWidth = "";
        public string CustomBookSizeWidth
        {
            get
            {
                return _customBookSizeWidth;
            }

            set
            {
                if (value == _customBookSizeWidth)
                {
                    return;
                }

                _customBookSizeWidth = value;
                if (float.TryParse(value, out var f))
                {
                    CustomBookSizeWidthF = f;
                }
                OnPropertyChanged();
            }
        }

        public float CustomBookSizeWidthF { get; private set; }

        private string _customBookSizeHeight = "";
        public string CustomBookSizeHeight
        {
            get
            {
                return _customBookSizeHeight;
            }

            set
            {
                if (value == _customBookSizeHeight)
                {
                    return;
                }

                _customBookSizeHeight = value;
                if (float.TryParse(value, out var f))
                {
                    CustomBookSizeHeightF = f;
                }
                OnPropertyChanged();
            }
        }

        public float CustomBookSizeHeightF { get; private set; }

        private string _customSignatures = "";
        public string CustomSignatures
        {
            get
            {
                return _customSignatures;
            }

            set
            {
                if (value == _customSignatures)
                {
                    return;
                }

                _customSignatures = value;
                OnPropertyChanged();
            }
        }

        private PageUnit _pageUnit = PageUnit.Inches;
        public PageUnit PageUnit
        {
            get
            {
                return _pageUnit;
            }

            set
            {
                if (value == _pageUnit)
                {
                    return;
                }

                _pageUnit = value;
                OnPropertyChanged();
            }
        }

        private SourcePageAlignment _sourcePageAlignment = SourcePageAlignment.Centered;
        public SourcePageAlignment SourcePageAlignment
        {
            get
            {
                return _sourcePageAlignment;
            }

            set
            {
                if (value == _sourcePageAlignment)
                {
                    return;
                }

                _sourcePageAlignment = value;
                IsOffsetFromSpine = value == SourcePageAlignment.OffsetFromSpine;
                OnPropertyChanged();
            }
        }

        private bool _isOffsetFromSpine = false;
        public bool IsOffsetFromSpine
        {
            get
            {
                return _isOffsetFromSpine;
            }

            set
            {
                if (value == _isOffsetFromSpine)
                {
                    return;
                }

                _isOffsetFromSpine = value;
                OnPropertyChanged();
            }
        }

        private float _offsetFromSpine = 0.5f;
        public float OffsetFromSpine
        {
            get
            {
                return _offsetFromSpine;
            }

            set
            {
                if (value == _offsetFromSpine)
                {
                    return;
                }

                _offsetFromSpine = value;
                OnPropertyChanged();
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get
            {
                return _totalPages;
            }

            set
            {
                if (value == _totalPages)
                {
                    return;
                }

                _totalPages = value;
                OnPropertyChanged();
            }
        }

        private int _totalSheets;
        public int TotalSheets
        {
            get
            {
                return _totalSheets;
            }

            set
            {
                if (value == _totalSheets)
                {
                    return;
                }

                _totalSheets = value;
                OnPropertyChanged();
            }
        }

        private int _numberOfSignatures;
        public int NumberOfSignatures
        {
            get
            {
                return _numberOfSignatures;
            }

            set
            {
                if (value == _numberOfSignatures)
                {
                    return;
                }

                _numberOfSignatures = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public IEnumerable<PageScale> ScaleOfPages { get; set; } = [];

        public PageScale? SelectedScaleOfPage { get; set; }

        [JsonIgnore]
        public IEnumerable<PrinterType> PrinterTypes { get; set; } = [];

        public PrinterType? SelectedPrinterType { get; set; }

        [JsonIgnore]
        public IEnumerable<PaperDefinition> PaperSizes { get; set; } = [];

        public PaperDefinition? SelectedPaperSize { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            _isDirty = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void ApplyModel(MainWindowViewModel mwvm)
        {
            SelectedPaperSize = mwvm.SelectedPaperSize;
            SelectedPrinterType = mwvm.SelectedPrinterType;
            SelectedScaleOfPage = mwvm.SelectedScaleOfPage;
            NumberOfSignatures = mwvm.NumberOfSignatures;
            TotalSheets = mwvm.TotalSheets;
            TotalPages = mwvm.TotalPages;
            OffsetFromSpine = mwvm.OffsetFromSpine;
            IsOffsetFromSpine = mwvm.IsOffsetFromSpine;
            PageUnit = mwvm.PageUnit;
            SourcePageAlignment = mwvm.SourcePageAlignment;
            CustomSignatures = mwvm.CustomSignatures;
            CustomBookSizeHeightF = mwvm.CustomBookSizeHeightF;
            CustomBookSizeHeight = mwvm.CustomBookSizeHeight;
            CustomBookSizeWidthF = mwvm.CustomBookSizeWidthF;
            CustomBookSizeWidth = mwvm.CustomBookSizeWidth;
            AlternatePageRotation = mwvm.AlternatePageRotation;
            LayoutIsStacked = mwvm.LayoutIsStacked;
            OutputTestOverlay = mwvm.OutputTestOverlay;
            AddFlyleaf = mwvm.AddFlyleaf;
            FormatOfSignature = mwvm.FormatOfSignature;
            IsCustomSignatureFormat = mwvm.IsCustomSignatureFormat;
            IsCustomBookSize = mwvm.IsCustomBookSize;
            SizeOfBook = mwvm.SizeOfBook;
            InputFilePath = mwvm.InputFilePath;
            OutputPath = mwvm.OutputPath;
            NumberOfPages = mwvm.NumberOfPages;
            PageSize = mwvm.PageSize;
            InputFilePath = mwvm.InputFilePath;
            IsDirty = false;
        }

        public void CreateNewProject(MainWindowViewModel originalProject)
        {
            SelectedPaperSize = originalProject.PaperSizes.First(paper => paper.Name!.ToUpper() == "LETTER");
            SelectedPrinterType = originalProject.PrinterTypes.First(printerType => printerType.IsDuplex == true);
            SelectedScaleOfPage = originalProject.ScaleOfPages.First(scaleOfPage => scaleOfPage.ScaleOfPage == PageScaling.KeepProportionWidth);
        }
    }
}
