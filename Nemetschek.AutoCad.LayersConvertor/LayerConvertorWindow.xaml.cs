using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;


namespace Nemetschek.AutoCad.LayersConvertor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LayerConvertorWindow : Window
    {
        public LayerConvertorWindow()
        {
            InitializeComponent();
            _layerUtil = new LayerUtility();
            _isClear = false;
        }

        private readonly LayerUtility _layerUtil;

        private bool _isClear;

        private void OpenFiles_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Drawing files (*.dwg)|*.dwg|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (openFileDialog.ShowDialog() == true)
                {
                    _isClear = lbFiles.Items.Count > 0;
                    if (_isClear)
                    {
                        lbFiles.Items.Clear();
                        _isClear = false;
                    }

                    foreach (string filename in openFileDialog.FileNames)
                        lbFiles.Items.Add(Path.GetFullPath(filename));
                }
                var totalCount = lbFiles.Items.Count;
                lblInfo.Foreground = new SolidColorBrush(Colors.DarkGray);
                lblInfoAll.Foreground = new SolidColorBrush(Colors.DarkGray);
                if (totalCount > 0)
                {
                    lbFiles.SelectedItem = lbFiles.Items[0];
                    lblInfoAll.Text = $" {totalCount} files";
                }
                else
                    lblInfo.Text = "No selected files";
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lbFiles.SelectionMode = SelectionMode.Single;
            lblInfo.Foreground = new SolidColorBrush(Colors.DarkGray);
            lblInfo.Text = "No selected files";
            btnProcess.IsEnabled = false;
        }

        private void ProcessFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                string fromLayer = cmbFromLayer.SelectedValue.ToString()!;
                string toLayer = cmbToLayer.SelectedValue.ToString()!;
                int i = 1, totalCount = lbFiles.SelectedItems.Count;
                lblInfo.Foreground = new SolidColorBrush(Colors.DarkBlue);
                foreach (string itm in lbFiles.SelectedItems)
                {
                    lblInfo.Text = $"Processing ( {i} of {totalCount} files) - {itm}";
                    var msg = _layerUtil.ProcessLayer(itm, fromLayer, toLayer);
                    lblInfo.Text = msg;
                    prgBar.Dispatcher.Invoke(() => prgBar.Value = (i / totalCount) * 100, DispatcherPriority.Background);
                    i++;
                }
            }
            finally
            {
                prgBar.Value = 0;
                Mouse.OverrideCursor = null;
            }
        }

        private void lbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isClear)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                LoadComboLayers();
            }
            Mouse.OverrideCursor = null;
        }

        private void LoadComboLayers()
        {
            var totalCount = lbFiles.Items.Count;
            var propCount = 0;
            if (totalCount > 0)
            {
                var itm = lbFiles.SelectedItems[0];
                var path = itm!.ToString();
                var ds = _layerUtil.GetlayerList(path!);
                cmbFromLayer.ItemsSource = ds;
                cmbToLayer.ItemsSource = new List<string>(ds); // --clone it
                propCount = ds.Count;
                if (propCount > 0)
                {
                    cmbFromLayer.SelectedIndex = 0;
                    cmbToLayer.SelectedIndex = 0;
                    lblInfo.Text = path;
                }
            }
            btnProcess.IsEnabled = totalCount > 0 && propCount > 0;
        }
    }
}