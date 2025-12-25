using Microsoft.EntityFrameworkCore;
using Pract15.Models;
using Pract15.Services;
using Pract15.ViewModels;
using Pract15.Windows;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pract15.Pages
{
    public partial class MainPage : Page
    {
        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadProducts();
        }

        private void SortByNameAsc_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SortByNameAsc();
        }

        private void SortByNameDesc_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SortByNameDesc();
        }

        private void SortByPriceAsc_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SortByPriceAsc();
        }

        private void SortByPriceDesc_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SortByPriceDesc();
        }

        private void SortByStockAsc_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SortByStockAsc();
        }

        private void SortByStockDesc_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SortByStockDesc();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ResetFilters();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ProductEditWindow();
                dialog.Owner = Application.Current.MainWindow;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Подписываемся на событие добавления товара
                dialog.ProductAdded += Dialog_ProductAdded;

                if (dialog.ShowDialog() == true)
                {
                    MessageBox.Show("Товар успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Отписываемся от события
                dialog.ProductAdded -= Dialog_ProductAdded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна добавления товара: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Dialog_ProductAdded()
        {
            // Обновляем данные после добавления
            _viewModel.LoadProducts();
            _viewModel.RefreshFilter();
        }

        private void Manage_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new ManagementPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Logout();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new LoginPage());
        }

        private void ProductCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (AuthService.IsManagerMode)
                {
                    var border = sender as Border;
                    if (border?.DataContext is Pract15.Models.Product product)
                    {
                        try
                        {
                            var dialog = new ProductEditWindow(product);
                            dialog.Owner = Application.Current.MainWindow;
                            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                            // Подписываемся на событие обновления
                            dialog.ProductUpdated += Dialog_ProductUpdated;

                            if (dialog.ShowDialog() == true)
                            {
                                MessageBox.Show("Товар успешно обновлен!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }

                            // Отписываемся от события
                            dialog.ProductUpdated -= Dialog_ProductUpdated;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при редактировании товара: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void Dialog_ProductUpdated(Product updatedProduct)
        {
            // Находим и обновляем товар в коллекции
            var existingProduct = _viewModel.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);

            if (existingProduct != null)
            {
                // Загружаем полные данные из базы включая теги
                using var db = new Pract15Context();
                var freshProduct = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag) // ВАЖНО: загружаем теги
                    .FirstOrDefault(p => p.Id == updatedProduct.Id);

                if (freshProduct != null)
                {
                    // Находим индекс товара
                    int index = _viewModel.Products.IndexOf(existingProduct);

                    // Заменяем товар на полностью обновленный (с тегами)
                    _viewModel.Products.RemoveAt(index);
                    _viewModel.Products.Insert(index, freshProduct);

                    // Обновляем фильтр и статистику
                    _viewModel.RefreshFilter();
                }
            }
            else
            {
                // Если товар не найден, перезагружаем все
                _viewModel.LoadProducts();
            }
        }

        private void ProductCard_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Opacity = 0.9;
            }
        }

        private void ProductCard_MouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Opacity = 1.0;
            }
        }
    }
}