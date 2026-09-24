using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AnimeUpscaler.Core;
using Microsoft.Win32;
using System.Reflection;

namespace AnimeUpscaler;

public partial class MainWindow : Window
{
    private readonly string _engineDirectory = Path.Combine(AppContext.BaseDirectory, "Assets", "Engine");
    private readonly DispatcherTimer _elapsedTimer;
    private readonly Stopwatch _stopwatch = new();
    private string? _inputPath;
    private string? _resultPath;
    private CancellationTokenSource? _cancellation;
    private double _zoomFactor = 1;

    public MainWindow()
    {
        InitializeComponent();

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "stella_upscaling_icon.png");
        if (File.Exists(iconPath))
        {
            var iconBitmap = new BitmapImage();
            iconBitmap.BeginInit();
            iconBitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
            iconBitmap.EndInit();
            this.Icon = iconBitmap;
        }

        ModelCombo.ItemsSource = ModelCatalog.All;
        ModelCombo.SelectedItem = ModelCatalog.AnimeV3;
        FormatCombo.ItemsSource = Enum.GetValues<OutputFormat>();
        FormatCombo.SelectedItem = OutputFormat.Png;

        _elapsedTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _elapsedTimer.Tick += (_, _) => ElapsedText.Text = $"Waktu: {_stopwatch.Elapsed:mm\\:ss}";
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        const double margin = 24;
        MaxWidth = SystemParameters.WorkArea.Width;
        MaxHeight = SystemParameters.WorkArea.Height;
        Width = Math.Min(1180, Math.Max(MinWidth, SystemParameters.WorkArea.Width - margin));
        Height = Math.Min(720, Math.Max(MinHeight, SystemParameters.WorkArea.Height - margin));
    }

    private void ModelCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ModelCombo.SelectedItem is not ModelVariant model)
        {
            return;
        }

        ScaleCombo.ItemsSource = model.SupportedScales.Select(scale => $"{scale}x");
        ScaleCombo.SelectedIndex = model.SupportedScales.Count - 1;
        ModelHintText.Text = model.Key switch
        {
            "v1" => "Detail ilustrasi kuat. Tersedia pada 4x.",
            "v2" => "Lebih ringan untuk laptop. Tersedia pada 2x dan 4x.",
            _ => "Model paling fleksibel. Tersedia pada 2x, 3x, dan 4x."
        };
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Pilih gambar",
            Filter = "Gambar|*.png;*.jpg;*.jpeg;*.webp|Semua file|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) == true)
        {
            LoadInput(dialog.FileName);
        }
    }

    private void Stage_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Stage_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } paths)
        {
            LoadInput(paths[0]);
        }
    }

    private void LoadInput(string path)
    {
        if (!ImageFilePolicy.IsSupportedInput(path))
        {
            MessageBox.Show(this, "Format gambar tidak didukung. Gunakan PNG, JPG, JPEG, atau WebP.", "Format tidak didukung", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            OriginalImage.Source = LoadBitmap(path);
        }
        catch (Exception exception) when (exception is IOException or NotSupportedException)
        {
            MessageBox.Show(this, "Gambar tidak dapat dibuka atau file rusak.", "Gagal membuka gambar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _inputPath = path;
        InputNameText.Text = Path.GetFileName(path);
        var source = (BitmapSource)OriginalImage.Source;
        ImageInfoText.Text = $"{source.PixelWidth:N0} × {source.PixelHeight:N0} px  •  {Path.GetExtension(path).TrimStart('.').ToUpperInvariant()}";
        EmptyState.Visibility = Visibility.Collapsed;
        BeforeBadge.Visibility = Visibility.Visible;
        ClearResult();
        StatusText.Text = "Siap memproses";
    }

    private async void UpscaleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_inputPath is null || ModelCombo.SelectedItem is not ModelVariant model
            || FormatCombo.SelectedItem is not OutputFormat format || ScaleCombo.SelectedIndex < 0)
        {
            MessageBox.Show(this, "Pilih gambar, model, skala, dan format terlebih dahulu.", "Data belum lengkap", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var scale = model.SupportedScales[ScaleCombo.SelectedIndex];
        ClearResult();
        _resultPath = ImageFilePolicy.CreateTemporaryOutputPath(format);
        _cancellation = new CancellationTokenSource();
        SetBusy(true);
        _stopwatch.Restart();
        _elapsedTimer.Start();

        try
        {
            var progress = new Progress<double>(value =>
            {
                ProgressBar.Value = value;
                ProgressText.Text = $"{value:0}%";
            });
            var request = new UpscaleRequest(_inputPath, _resultPath, model, scale, format);
            await new UpscaleEngine(_engineDirectory).RunAsync(request, progress, _cancellation.Token);

            ResultImage.Source = LoadBitmap(_resultPath);
            ResultImage.Visibility = Visibility.Visible;
            AfterBadge.Visibility = Visibility.Visible;
            ComparisonDivider.Visibility = Visibility.Visible;
            ComparisonControls.Visibility = Visibility.Visible;
            ComparisonSlider.Value = 50;
            SetZoom(1);
            SaveButton.IsEnabled = true;
            StatusText.Text = "Upscaling selesai";
            ProgressText.Text = "100%";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = "Proses dibatalkan";
            ProgressText.Text = string.Empty;
            DeleteResultFile();
        }
        catch (Exception exception) when (exception is UpscaleEngineException or FileNotFoundException or ArgumentException or IOException)
        {
            StatusText.Text = "Upscaling gagal";
            ProgressText.Text = string.Empty;
            DeleteResultFile();
            MessageBox.Show(this, exception.Message, "Upscaling gagal", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _stopwatch.Stop();
            _elapsedTimer.Stop();
            ElapsedText.Text = $"Waktu: {_stopwatch.Elapsed:mm\\:ss}";
            SetBusy(false);
            _cancellation.Dispose();
            _cancellation = null;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => _cancellation?.Cancel();

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_resultPath is null || !File.Exists(_resultPath) || FormatCombo.SelectedItem is not OutputFormat format)
        {
            return;
        }

        var extension = format.Extension();
        var inputName = Path.GetFileNameWithoutExtension(_inputPath) ?? "anime";
        var dialog = new SaveFileDialog
        {
            Title = "Simpan hasil upscale",
            FileName = $"{inputName}-upscaled.{extension}",
            DefaultExt = extension,
            AddExtension = true,
            Filter = format switch
            {
                OutputFormat.Png => "PNG lossless|*.png",
                OutputFormat.Jpeg => "JPEG|*.jpg",
                _ => "WebP|*.webp"
            }
        };

        if (dialog.ShowDialog(this) == true)
        {
            try
            {
                File.Copy(_resultPath, dialog.FileName, overwrite: true);
                StatusText.Text = $"Tersimpan: {Path.GetFileName(dialog.FileName)}";
                ProgressText.Text = string.Empty;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, "Hasil tidak dapat ditulis ke lokasi tersebut.", "Gagal menyimpan", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void ComparisonSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateComparisonReveal();

    private void ComparisonStage_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateComparisonReveal();

    private void ImageScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateStageSize();

    private void ImageScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control || ResultImage.Source is null)
        {
            return;
        }

        SetZoom(_zoomFactor + (e.Delta > 0 ? 0.25 : -0.25));
        e.Handled = true;
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoomFactor + 0.25);

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => SetZoom(_zoomFactor - 0.25);

    private void ZoomResetButton_Click(object sender, RoutedEventArgs e) => SetZoom(1);

    private void SetZoom(double zoom)
    {
        _zoomFactor = ComparisonLayout.ClampZoom(zoom);
        ZoomResetButton.Content = $"{_zoomFactor * 100:0}%";
        UpdateStageSize();
    }

    private void UpdateStageSize()
    {
        if (ImageScrollViewer is null || ComparisonStage is null)
        {
            return;
        }

        var viewportWidth = ImageScrollViewer.ViewportWidth > 0 ? ImageScrollViewer.ViewportWidth : ImageScrollViewer.ActualWidth;
        var viewportHeight = ImageScrollViewer.ViewportHeight > 0 ? ImageScrollViewer.ViewportHeight : ImageScrollViewer.ActualHeight;
        if (viewportWidth <= 0 || viewportHeight <= 0)
        {
            return;
        }

        ComparisonStage.Width = viewportWidth * _zoomFactor;
        ComparisonStage.Height = viewportHeight * _zoomFactor;
        UpdateComparisonReveal();
    }

    private void UpdateComparisonReveal()
    {
        if (ComparisonStage is null || BeforeReveal is null || ComparisonDivider is null || OriginalImage is null || ResultImage is null || ComparisonSlider is null)
        {
            return;
        }

        OriginalImage.Width = ComparisonStage.ActualWidth;
        OriginalImage.Height = ComparisonStage.ActualHeight;
        ResultImage.Width = ComparisonStage.ActualWidth;
        ResultImage.Height = ComparisonStage.ActualHeight;
        var revealPercent = ResultImage.Visibility == Visibility.Visible ? ComparisonSlider.Value : 100;
        var revealWidth = ComparisonLayout.RevealWidth(ComparisonStage.ActualWidth, revealPercent);
        BeforeReveal.Width = revealWidth;
        BeforeReveal.Height = ComparisonStage.ActualHeight;
        ComparisonDivider.Margin = new Thickness(ComparisonLayout.DividerOffset(ComparisonStage.ActualWidth, revealPercent, ComparisonDivider.Width), 0, 0, 0);
    }

    private void SetBusy(bool busy)
    {
        OpenButton.IsEnabled = !busy;
        ModelCombo.IsEnabled = !busy;
        ScaleCombo.IsEnabled = !busy;
        FormatCombo.IsEnabled = !busy;
        UpscaleButton.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        SaveButton.IsEnabled = !busy && _resultPath is not null && File.Exists(_resultPath);
        StatusText.Text = busy ? "Memproses dengan GPU…" : StatusText.Text;
        ProgressBar.Value = busy ? 0 : ProgressBar.Value;
        ProgressText.Text = busy ? "0%" : ProgressText.Text;
    }

    private void ClearResult()
    {
        ResultImage.Source = null;
        ResultImage.Visibility = Visibility.Collapsed;
        AfterBadge.Visibility = Visibility.Collapsed;
        ComparisonDivider.Visibility = Visibility.Collapsed;
        ComparisonControls.Visibility = Visibility.Collapsed;
        SaveButton.IsEnabled = false;
        DeleteResultFile();
        ProgressBar.Value = 0;
        ProgressText.Text = string.Empty;
        ElapsedText.Text = string.Empty;
        SetZoom(1);
    }

    private void DeleteResultFile()
    {
        if (_resultPath is not null)
        {
            try
            {
                File.Delete(_resultPath);
            }
            catch (IOException)
            {
            }
        }

        _resultPath = null;
    }

    private static BitmapImage LoadBitmap(string path)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _cancellation?.Cancel();
        DeleteResultFile();
    }
}
