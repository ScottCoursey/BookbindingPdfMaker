﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BookbindingPdfMaker.Models
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _fileName = "No File Selected";
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

                _fileName = value;
                OnPropertyChanged();
            }
        }

        private string _pageSize = "N/A";
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

        private string _numberOfPages = "N/A";
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

        private string _outputPath = "";
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

        private string _inputPath = "";
        public string InputPath
        {
            get
            {
                return _inputPath;
            }

            set
            {
                if (value == _inputPath)
                {
                    return;
                }

                _inputPath = value;
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
                OnPropertyChanged();
            }
        }

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
                OnPropertyChanged();
            }
        }

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

        public IEnumerable<PageScale> ScaleOfPages { get; set; }
        public PageScale SelectedScaleOfPage { get; set; }

        public IEnumerable<PrinterType> PrinterTypes { get; set; }
        public PrinterType SelectedPrinterType { get; set; }

        public IEnumerable<PaperDefinition> PaperSizes { get; set; }
        public PaperDefinition SelectedPaperSize { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
