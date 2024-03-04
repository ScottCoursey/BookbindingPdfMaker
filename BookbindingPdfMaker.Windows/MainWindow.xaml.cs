using BookbindingPdfMaker.Models;
using BookbindingPdfMaker.Services;
using Newtonsoft.Json;
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
            _pdfMaker = new PdfMaker(_mwvm, new StackedPageMatrixCalculator());

            _mwvm.PaperSizes = LoadPapers();
            _mwvm.PrinterTypes = LoadPrinterTypes();
            _mwvm.ScaleOfPages = LoadScaleOfPages();

            CreateNewProject();

            DataContext = _mwvm;
            UpdateWindowTitle();
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
                    Name = Constants.PrinterTypeSingle,
                    IsDuplex = false
                },
                new PrinterType()
                {
                    Name = Constants.PrinterTypeDouble,
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
                    Name = Constants.PageScaleStretchToFit,
                    ScaleOfPage = PageScaling.Stretch
                },
                new PageScale()
                {
                    Name = Constants.PageScaleKeepProportionWidth,
                    ScaleOfPage = PageScaling.KeepProportionWidth
                },
                new PageScale()
                {
                    Name = Constants.PageScaleKeepProportionHeight,
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
                    if (!_pdfMaker.SetInputFileName(fileName))
                    {
                        System.Windows.MessageBox.Show("There was an error opening the PDF.  Many times, this occurs because something else has the file open or it hasn't finished the creation process.  Please try this again; it may take a minute or so for the creation to complete if that's the error.", "Unable to open PDF", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        UpdateFileRelevantInfo();
                        CheckOutputPath();
                    }
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
            if (signatureInfo == null)
            {
                // There was an issue in getting the signature info.  This is most likely
                // due to the going missing.
                _mwvm.ProjectFilePath = "";
                _mwvm.InputFilePath = "";
                UpdateWindowTitle();
                return;
            }

            _mwvm.TotalPages = signatureInfo.FullPageCount;

            if (_mwvm.LayoutIsStacked && signatureInfo.SignatureSizeList.Count() % 2 == 1)
            {
                signatureInfo.SignatureSizeList.Add(0);
            }

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
            if (!File.Exists(_mwvm.InputFilePath))
            {
                System.Windows.MessageBox.Show("The input file does not exist.", "Input File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // The final output folder must contain the input file name without the .pdf extension.
            if (_mwvm.OutputPath == Constants.NoFolderSelected)
            {
                _mwvm.OutputPath = Path.GetDirectoryName(_mwvm.InputFilePath);
            }

            //if (Path.Exists(_mwvm.OutputPath))
            //{
            //    var fileName = Path.GetFileNameWithoutExtension(_mwvm.FileName);
            //    if (!_mwvm.OutputPath.EndsWith(fileName))
            //    {
            //        _mwvm.OutputPath = Path.Combine(_mwvm.OutputPath, fileName);
            //    }
            //}

            ButtonGenerateDocument.IsEnabled = true;
        }

        private void MenuFileExit_Click(object sender, RoutedEventArgs e)
        {
            InvokeWithSave(() => System.Windows.Application.Current.Shutdown());
        }

        private void InvokeWithSave(Action invokedIfSafe)
        {
            if (!_mwvm.IsDirty)
            {
                invokedIfSafe.Invoke();
                return;
            }

            switch (System.Windows.MessageBox.Show("Do you want to save the project first?", "Project Has Changed", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    SaveProject();
                    break;

                case MessageBoxResult.No:
                    // Don't need to save.
                    break;

                default:
                    // Don't do anything and just return to the caller.
                    return;
            }

            // We've handled the save or opted out of it, so go ahead and invoke.
            invokedIfSafe.Invoke();
        }

        private void SaveProject()
        {
            if (string.IsNullOrEmpty(_mwvm.ProjectFilePath))
            {
                SaveProjectAs();
                return;
            }

            var json = JsonConvert.SerializeObject(_mwvm);
            File.WriteAllText(_mwvm.ProjectFilePath, json);
            _mwvm.IsDirty = false;
            UpdateWindowTitle();
        }

        private void SaveProjectAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog()
            {
                FileName = string.IsNullOrEmpty(_mwvm.InputFilePath) ? Constants.NewProjectFileName : Path.GetFileNameWithoutExtension(_mwvm.InputFilePath) + ".bpmp",
                DefaultExt = ".bpmp",
                Filter = "Bookbinding PDF Maker Project|*.bpmp"
            };

            if (dialog.ShowDialog() == true)
            {
                _mwvm.ProjectFilePath = dialog.FileName;
                SaveProject();
            }
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
            if (Directory.Exists(_mwvm.OutputPath))
            {
                if (System.Windows.MessageBox.Show("The selected output folder already exists, which means it may have information from a previous signature extration or some other data.  Are you sure you want to overwrite it?", "Overwrite Previous Output", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            Directory.CreateDirectory(_mwvm.OutputPath);

            _pdfMaker.Generate(_mwvm.InputFilePath, _mwvm.OutputPath);

            System.Windows.MessageBox.Show("The PDF has completed its generation", "PDF Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TextInputOnlyAllowNumeric(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            bool approvedDecimalPoint = false;

            if (e.Text == ".")
            {
                if (!((System.Windows.Controls.TextBox)sender).Text.Contains('.'))
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

        private void MenuFileOpenProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".bpmp",
                Filter = "Bookbinding PDF Maker Project|*.bpmp",
                FileName = string.IsNullOrEmpty(_mwvm.ProjectFilePath) ? "New Project.bpmp" : Path.GetFileName(_mwvm.ProjectFilePath)
            };

            if (dialog.ShowDialog() == true)
            {
                var fileName = dialog.FileName;
                try
                {
                    var newMwvm = JsonConvert.DeserializeObject<MainWindowViewModel>(File.ReadAllText(fileName)) ?? new MainWindowViewModel();
                    if (string.IsNullOrEmpty(newMwvm?.InputFilePath) || !File.Exists(newMwvm.InputFilePath))
                    {
                        newMwvm!.InputFilePath = "";
                        System.Windows.MessageBox.Show("Unable to open the input PDF as defined in the project, so be sure to select one before generating an output.", "Missing Input File", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    _mwvm.ProjectFilePath = fileName;
                    _mwvm.ApplyModel(newMwvm!);
                    UpdateWindowTitle();
                }
                catch
                {
                    System.Windows.MessageBox.Show("Unable to open that project file.  Please try another one.", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuFileSaveProject_Click(object sender, RoutedEventArgs e)
        {
            SaveProject();
        }

        private void MenuFileSaveAsProject_Click(object sender, RoutedEventArgs e)
        {
            SaveProjectAs();
        }

        private void MenuFileNewProject_Click(object sender, RoutedEventArgs e)
        {
            CreateNewProject();
        }

        private void CreateNewProject()
        {
            var newProject = new MainWindowViewModel();
            newProject.CreateNewProject(_mwvm);
            _mwvm.ApplyModel(newProject);
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            var suffix = "";
            if (!string.IsNullOrEmpty(_mwvm.ProjectFilePath) && _mwvm.ProjectFilePath != Constants.NoFileSelected)
            {
                suffix = $" : {_mwvm.ProjectFilePath}";
            }
            Title = $"{Constants.WindowTitle}{suffix}";
        }
    }
}