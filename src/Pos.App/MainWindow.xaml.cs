using System.Windows;
using System.Windows.Controls;

namespace Pos.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        // نموذج أصناف مؤقت (لحين ربط الداتا)
        private string[] products = new string[]
        {
            "شاورما عربي", "صحن شاورما", "ساندوتش",
            "بطاطس", "مشروب غازي", "مياه معدنية"
        };

        private void LoadProducts()
        {
            foreach (var item in products)
            {
                var btn = new Button()
                {
                    Content = item,
                    Height = 80,
                    Margin = new Thickness(8),
                    Background = System.Windows.Media.Brushes.White,
                    FontSize = 18
                };

                btn.Click += (s, e) =>
                {
                    CartList.Items.Add(item);
                    UpdateTotal();
                };

                ProductsPanel.Children.Add(btn);
            }
        }

        private void UpdateTotal()
        {
            double total = CartList.Items.Count * 20; // سعر تجريبي للعرض فقط
            TotalText.Text = $"الإجمالي: {total:0.00}";
        }
    }
}
