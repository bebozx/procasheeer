using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pos.App
{
    public partial class MainWindow : Window
    {
        private record Product(string Name, string Category, double Price);

        private readonly List<Product> _allProducts = new()
        {
            new Product("شاورما عربي",  "SHAWARMA", 55),
            new Product("صحن شاورما",   "SHAWARMA", 75),
            new Product("ساندوتش",      "SHAWARMA", 45),
            new Product("بطاطس",        "FRIES",   25),
            new Product("بطاطس كبيرة",  "FRIES",   35),
            new Product("مياه معدنية",  "DRINKS",  10),
            new Product("مشروب غازي",   "DRINKS",  18),
            new Product("عصير",         "DRINKS",  22),
        };

        private readonly List<Product> _cart = new();
        private string _category = "ALL";
        private string _search = "";

        // إعدادات تجريبية (في الصفحة 2 هتيجي من لوحة الإدارة/SQLite)
        private const double VatRate = 0.14;          // ضريبة 14%
        private double ServiceRate = 0.10;            // خدمة 10% (للطاولات)
        private double DeliveryFee = 15;              // رسوم توصيل (للدليفري)

        public MainWindow()
        {
            InitializeComponent();
            RenderProducts();
            UpdateTotal();
        }

        // ================= UI Events =================

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is string tag)
            {
                _category = tag;
                RenderProducts();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _search = SearchBox.Text?.Trim() ?? "";
            RenderProducts();
        }

        private void OrderType_Checked(object sender, RoutedEventArgs e)
        {
            UpdateTotal();
        }

        private void RemoveLast_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0) return;
            _cart.RemoveAt(_cart.Count - 1);
            RefreshCartList();
            UpdateTotal();
        }

        private void ClearCart_Click(object sender, RoutedEventArgs e)
        {
            _cart.Clear();
            RefreshCartList();
            UpdateTotal();
        }

        private void FinishOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("السلة فارغة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // صفحة 2: هنا هنحفظ الفاتورة في SQLite + نطبع + نقاط العملاء
            MessageBox.Show("تمام ✅ (لسه تجريبي)\nفي الصفحة 2 هنضيف الحفظ والطباعة.", "إتمام الطلب", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("الإعدادات هتكون في صفحة مستقلة (لوحة الإدارة) في صفحات لاحقة.", "إعدادات", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ================= Rendering =================

        private void RenderProducts()
        {
            ProductsPanel.Children.Clear();

            var filtered = _allProducts
                .Where(p => _category == "ALL" || p.Category == _category)
                .Where(p => string.IsNullOrWhiteSpace(_search) || p.Name.Contains(_search, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var p in filtered)
                ProductsPanel.Children.Add(BuildProductButton(p));
        }

        private Button BuildProductButton(Product p)
        {
            var btn = new Button
            {
                Margin = new Thickness(8),
                Height = 96,
                Cursor = System.Windows.Input.Cursors.Hand,
                Background = (Brush)new BrushConverter().ConvertFromString("#111F36"),
                BorderBrush = (Brush)new BrushConverter().ConvertFromString("#203353"),
                BorderThickness = new Thickness(1),
                Foreground = (Brush)new BrushConverter().ConvertFromString("#E8EEF8"),
                FontSize = 16,
                Padding = new Thickness(12, 10, 12, 10),
                Tag = p
            };

            // Template بسيط بالـ code (عشان يطلع شيك بدون تعقيد)
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var name = new TextBlock
            {
                Text = p.Name,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            var price = new TextBlock
            {
                Text = $"{p.Price:0.00} ج",
                Foreground = (Brush)new BrushConverter().ConvertFromString("#9DB0CF"),
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };

            Grid.SetRow(name, 0);
            Grid.SetRow(price, 1);
            grid.Children.Add(name);
            grid.Children.Add(price);

            btn.Content = grid;

            btn.Click += (_, __) =>
            {
                _cart.Add(p);
                RefreshCartList();
                UpdateTotal();
            };

            // Hover effect بسيط
            btn.MouseEnter += (_, __) =>
            {
                btn.Background = (Brush)new BrushConverter().ConvertFromString("#142746");
                btn.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#2F4B78");
            };
            btn.MouseLeave += (_, __) =>
            {
                btn.Background = (Brush)new BrushConverter().ConvertFromString("#111F36");
                btn.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#203353");
            };

            return btn;
        }

        private void RefreshCartList()
        {
            CartList.Items.Clear();

            foreach (var item in _cart)
                CartList.Items.Add($"{item.Name}  —  {item.Price:0.00} ج");
        }

        private void UpdateTotal()
        {
            var subTotal = _cart.Sum(x => x.Price);

            // ضريبة على الإجمالي (تجريبي)
            var vat = subTotal * VatRate;

            // خدمة للطاولات فقط
            var service = (OrderDineIn.IsChecked == true) ? subTotal * ServiceRate : 0;

            // رسوم توصيل للدليفري فقط
            var delivery = (OrderDelivery.IsChecked == true) ? DeliveryFee : 0;

            var total = subTotal + vat + service + delivery;

            TotalText.Text = total.ToString("0.00");
        }
    }
}
