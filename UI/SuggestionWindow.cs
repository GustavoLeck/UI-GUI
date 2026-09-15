using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TeamsAccessibilityPoc.UI;

/// <summary>
/// Minimal always-on-top panel that mirrors what the watcher is capturing right now.
/// Stands in for where a real suggestion UI would eventually render (out of scope for this POC).
/// </summary>
public sealed class SuggestionWindow : Window
{
    private readonly TextBlock _boxLabel;
    private readonly TextBlock _text;
    private readonly TextBlock _updatedAt;
    private readonly TextBlock _apiStatus;
    private readonly TextBlock _apiResult;
    private readonly Image _sprite;

    public SuggestionWindow()
    {
        Title = "Compose Box Monitor (POC)";
        Width = 380;
        Height = 360;
        Topmost = true;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        WindowStyle = WindowStyle.None;
        Background = new SolidColorBrush(Color.FromRgb(24, 24, 27));

        Left = Math.Max(0, SystemParameters.WorkArea.Right - Width - 24);
        Top = Math.Max(0, SystemParameters.WorkArea.Bottom - Height - 24);

        var root = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(14)
        };

        var stack = new StackPanel();

        var header = new TextBlock
        {
            Text = "MONITOR DE COMPOSICAO (POC)",
            Foreground = new SolidColorBrush(Color.FromRgb(161, 161, 170)),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };

        _boxLabel = new TextBlock
        {
            Text = "Aguardando foco em uma caixa de texto...",
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        };

        _text = new TextBlock
        {
            Text = string.Empty,
            Foreground = new SolidColorBrush(Color.FromRgb(74, 222, 128)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 80
        };

        _updatedAt = new TextBlock
        {
            Text = string.Empty,
            Foreground = new SolidColorBrush(Color.FromRgb(113, 113, 122)),
            FontSize = 10,
            Margin = new Thickness(0, 10, 0, 0)
        };

        var separator = new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.FromRgb(63, 63, 70)),
            Margin = new Thickness(0, 12, 0, 12)
        };

        var apiHeader = new TextBlock
        {
            Text = "POKEAPI (debounce 500ms)",
            Foreground = new SolidColorBrush(Color.FromRgb(161, 161, 170)),
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };

        _apiStatus = new TextBlock
        {
            Text = string.Empty,
            Foreground = new SolidColorBrush(Color.FromRgb(96, 165, 250)),
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 6)
        };

        var apiRow = new StackPanel { Orientation = Orientation.Horizontal };

        _sprite = new Image
        {
            Width = 56,
            Height = 56,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Top
        };

        _apiResult = new TextBlock
        {
            Text = string.Empty,
            Foreground = Brushes.White,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Width = 280
        };

        apiRow.Children.Add(_sprite);
        apiRow.Children.Add(_apiResult);

        stack.Children.Add(header);
        stack.Children.Add(_boxLabel);
        stack.Children.Add(_text);
        stack.Children.Add(_updatedAt);
        stack.Children.Add(separator);
        stack.Children.Add(apiHeader);
        stack.Children.Add(_apiStatus);
        stack.Children.Add(apiRow);
        root.Child = stack;
        Content = root;

        MouseLeftButtonDown += (_, _) => DragMove();
    }

    public void SetBoxLabel(string label) => _boxLabel.Text = label;

    public void SetText(string text)
    {
        _text.Text = string.IsNullOrEmpty(text) ? "(vazio)" : text;
        _updatedAt.Text = $"Atualizado as {DateTime.Now:HH:mm:ss.fff}";
    }

    public void SetApiStatus(string status) => _apiStatus.Text = status;

    public void SetApiResult(string summary, string? spriteUrl)
    {
        _apiResult.Text = summary;

        if (string.IsNullOrEmpty(spriteUrl))
        {
            _sprite.Source = null;
            return;
        }

        try
        {
            _sprite.Source = new BitmapImage(new Uri(spriteUrl));
        }
        catch
        {
            _sprite.Source = null;
        }
    }
}
