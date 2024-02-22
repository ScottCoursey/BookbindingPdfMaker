using BookbindingPdfMaker.Models;
using BookbindingPdfMaker.Services;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BookbindingPdfMaker
{
    public partial class MainWindow : Window
    {
        private IPdfMaker _pdfMaker;
        private MainWindowViewModel _mwvm;

        public MainWindow()
        {
            InitializeComponent();
            _mwvm = new MainWindowViewModel();
            _pdfMaker = new PdfMaker(_mwvm);

            _mwvm.PaperSizes = LoadPapers();
            _mwvm.SelectedPaperSize = _mwvm.PaperSizes.First(paper => paper.Name!.ToUpper() == "LETTER");

            _mwvm.PrinterTypes = LoadPrinterTypes();
            _mwvm.SelectedPrinterType = _mwvm.PrinterTypes.First(printerType => printerType.IsDuplex == true);

            _mwvm.ScaleOfPages = LoadScaleOfPages();
            _mwvm.SelectedScaleOfPage = _mwvm.ScaleOfPages.First(scaleOfPage => scaleOfPage.ScaleOfPage == PageScaling.KeepProportionWidth);

            DataContext = _mwvm;
        }

        private IEnumerable<PaperDefinition> LoadPapers()
        {
            var section = App.Configuration.GetSection("Papers");
            var papersConfig = section.GetChildren();
            var papers = papersConfig.Select(config => new PaperDefinition(config));
            return papers.OrderBy(paper => paper.Name).ToList();
        }

        private IEnumerable<PrinterType> LoadPrinterTypes()
        {
            return new List<PrinterType>()
            {
                new PrinterType()
                {
                    Name = "Single Sided",
                    IsDuplex = false
                },
                new PrinterType()
                {
                    Name = "Double Sided / Duplex",
                    IsDuplex = true
                }
            };
        }

        private IEnumerable<PageScale> LoadScaleOfPages()
        {
            return new List<PageScale>()
            {
                new PageScale()
                {
                    Name = "Stretch To Fit",
                    ScaleOfPage = PageScaling.Stretch
                },
                new PageScale()
                {
                    Name = "Keep Proportion (by Width)",
                    ScaleOfPage = PageScaling.KeepProportionWidth
                },
                new PageScale()
                {
                    Name = "Keep Proportion (by Height)",
                    ScaleOfPage = PageScaling.KeepProportionHeight
                }
            };
        }

        private void MenuFileOpenInputPdf_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".pdf",
                Filter = "PDF|*.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                var ext = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(ext) || ext.ToUpper() != ".PDF")
                {
                    System.Windows.MessageBox.Show("This program only supports opening files that end in the '.pdf' extension.", "Invalid File Extension", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    _pdfMaker.SetInputFileName(fileName);
                    UpdateFileRelevantInfo();
                    CheckOutputPath();
                }
            }
        }

        private void UpdateFileRelevantInfo()
        {
            if (string.IsNullOrEmpty(_mwvm.InputFilePath))
            {
                return;
            }

            var signatureInfo = _pdfMaker.ReadSignatureInfo(_mwvm.InputFilePath);
            _mwvm.TotalPages = signatureInfo.FullPageCount;

            if (_mwvm.LayoutIsStacked)
            {
                var numSheets = 0;
                for (int signatureSet = 0; signatureSet < signatureInfo.SignatureSizeList.Count(); signatureSet += 2)
                {
                    var signatureSize1 = signatureInfo.SignatureSizeList[signatureSet];
                    var signatureSize2 = signatureInfo.SignatureSizeList[signatureSet + 1];
                    var signatureSetSize = Math.Max(signatureSize1, signatureSize2);
                    numSheets += signatureSetSize;
                }

                _mwvm.TotalSheets = numSheets;
            }
            else
            {
                _mwvm.TotalSheets = signatureInfo.SignatureSizeList.Sum(size => size);
            }

            _mwvm.NumberOfSignatures = signatureInfo.SignatureSizeList.Where(sig => sig > 0).Count();
            _mwvm.NumberOfPages = _pdfMaker.PdfInputForm!.PageCount.ToString();
        }

        private void MenuFileSetOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _pdfMaker.SetOutputPath(dialog.SelectedPath);
                    CheckOutputPath();
                }
            }
        }

        private void CheckOutputPath()
        {
            if (string.IsNullOrEmpty(_mwvm.FileName) || string.IsNullOrEmpty(_mwvm.OutputPath))
            {
                ButtonGenerateDocument.IsEnabled = false;
                return;
            }

            // Does the input file name exist?  It really should...
            if (!File.Exists(Path.Combine(_mwvm.InputPath, _mwvm.FileName)))
            {
                System.Windows.MessageBox.Show("The input file does not exist.", "Input File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // The final output folder must contain the input file name without the .pdf extension.
            if (Path.Exists(_mwvm.OutputPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(_mwvm.FileName);
                if (!_mwvm.OutputPath.EndsWith(fileName))
                {
                    _mwvm.OutputPath = Path.Combine(_mwvm.OutputPath, fileName);
                }
            }

            ButtonGenerateDocument.IsEnabled = true;
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void MenuHelpUsageInformation_Click(object sender, RoutedEventArgs e)
        {
            var processStartInfo = new ProcessStartInfo()
            {
                FileName = App.Configuration["HelpUrlRoot"],
                UseShellExecute = true
            };
            Process.Start(processStartInfo);
        }

        private void ButtonGenerateDocument_Click(object sender, RoutedEventArgs e)
        {
            // If the folder already exists, it might have a previous extraction.  Prompt for its removal.
            if (Directory.Exists(_mwvm.OutputPath))
            {
                if (System.Windows.MessageBox.Show("The selected output folder already exists, which means it may have information from a previous signature extration or some other data.  Are you sure you want to remove it?", "Remove Previous Output", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Directory.Delete(_mwvm.OutputPath, true);
                }
                else
                {
                    return;
                }
            }

            Directory.CreateDirectory(_mwvm.OutputPath);

            _pdfMaker.Generate(_mwvm.InputFilePath, _mwvm.OutputPath);
            System.Windows.MessageBox.Show("The PDF has completed its generation", "PDF Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PaperSelectionCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void PrinterTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void TextInputOnlyAllowNumeric(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            bool approvedDecimalPoint = false;

            if (e.Text == ".")
            {
                if (!((System.Windows.Controls.TextBox)sender).Text.Contains("."))
                {
                    approvedDecimalPoint = true;
                }
            }

            if (!(char.IsDigit(e.Text, e.Text.Length - 1) || approvedDecimalPoint))
            {
                e.Handled = true;
            }
        }

        private void FlyleafCheckbox_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }

        private void BookletRadioButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }

        private void PerfectBoundRadioButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }

        private void StangardSignatureRadioButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }

        private void CustomSignaturesRadioButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }

        private void CustomSignatureTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_mwvm == null)
            {
                return;
            }

            UpdateFileRelevantInfo();
        }

        private void DoNotStackLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }

        private void LayoutIsStackedButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateFileRelevantInfo();
        }
    }
}