using BookbindingPdfMaker.Models;
using BookbindingPdfMaker.Services;
using PdfSharp.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BookbindingPdfMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            _mwvm.SelectedScaleOfPage = _mwvm.ScaleOfPages.First(scaleOfPage => scaleOfPage.ScaleOfPage == PageScaling.KeepProportion);

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
                    Name = "Keep Proportion",
                    ScaleOfPage = PageScaling.KeepProportion
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
                    UpdateFileRelevantInfo(fileName);
                    CheckOutputPath();
                }
            }
        }

        private void UpdateFileRelevantInfo(string fileName)
        {
            using (var pdfForm = XPdfForm.FromFile(fileName))
            {
                _mwvm.NumberOfPages = pdfForm.PageCount.ToString();
            }
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
            //if (string.IsNullOrEmpty(_mwvm.FileName) || string.IsNullOrEmpty(_mwvm.OutputPath))
            //{
            //    ButtonGenerateDocument.IsEnabled = false;
            //    return;
            //}

            //// Does the input file name exist?  It really should...
            //if (!File.Exists(Path.Combine(_mwvm.InputPath, _mwvm.FileName)))
            //{
            //    System.Windows.MessageBox.Show("The input file does not exist.", "Input File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //// The final output folder must contain the input file name without the .pdf extension.
            //var fileName = Path.GetFileNameWithoutExtension(_mwvm.FileName);
            //if (!_mwvm.OutputPath.EndsWith(fileName))
            //{
            //    _mwvm.OutputPath = Path.Combine(_mwvm.OutputPath, fileName);
            //}

            //// If the folder already exists, it might have a previous extraction.  Prompt for its removal.
            //if (Directory.Exists(_mwvm.OutputPath))
            //{
            //    if (System.Windows.MessageBox.Show("The selected output folder already exists, which means it may have information from a previous signature extration or some other data.  Are you sure you want to remove it?", "Remove Previous Output", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //    {
            //        Directory.Delete(_mwvm.OutputPath);
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}

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
            _pdfMaker.Generate(@"c:\temp\dummy.pdf", @"c:\temp\dummy");
        }

        private void PaperSelectionCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void PrinterTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}