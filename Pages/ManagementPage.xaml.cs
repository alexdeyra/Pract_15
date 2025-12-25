using Microsoft.EntityFrameworkCore;
using Pract15.Models;
using Pract15.Services;
using Pract15.Windows;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Pract15.Pages
{
    public partial class ManagementPage : Page
    {
        private Pract15Context _db;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Brand> _brands;
        private ObservableCollection<Tag> _tags;

        // Представления для фильтрации
        private ICollectionView _productsView;
        private ICollectionView _categoriesView;
        private ICollectionView _brandsView;
        private ICollectionView _tagsView;

        public ManagementPage()
        {
            InitializeComponent();
            _db = DbService.Instance;

            // Инициализируем коллекции
            _products = new ObservableCollection<Product>();
            _categories = new ObservableCollection<Category>();
            _brands = new ObservableCollection<Brand>();
            _tags = new ObservableCollection<Tag>();

            // Создаем представления для фильтрации
            _productsView = CollectionViewSource.GetDefaultView(_products);
            _categoriesView = CollectionViewSource.GetDefaultView(_categories);
            _brandsView = CollectionViewSource.GetDefaultView(_brands);
            _tagsView = CollectionViewSource.GetDefaultView(_tags);

            // Устанавливаем фильтры
            _productsView.Filter = FilterProduct;
            _categoriesView.Filter = FilterCategory;
            _brandsView.Filter = FilterBrand;
            _tagsView.Filter = FilterTag;

            // Устанавливаем источники данных для DataGrid
            ProductsGrid.ItemsSource = _productsView;
            CategoriesGrid.ItemsSource = _categoriesView;
            BrandsGrid.ItemsSource = _brandsView;
            TagsGrid.ItemsSource = _tagsView;

            LoadData();
        }

        private void LoadData()
        {
            LoadProducts();
            LoadCategories();
            LoadBrands();
            LoadTags();
        }

        #region Загрузка данных

        private void LoadProducts()
        {
            try
            {
                _products.Clear();

                var products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTags)
                    .ToList();

                foreach (var product in products)
                {
                    _products.Add(product);
                }

                UpdateProductsStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategories()
        {
            try
            {
                _categories.Clear();

                var categories = _db.Categories.ToList();
                foreach (var category in categories)
                {
                    _categories.Add(category);
                }

                UpdateCategoriesStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBrands()
        {
            try
            {
                _brands.Clear();

                var brands = _db.Brands.ToList();
                foreach (var brand in brands)
                {
                    _brands.Add(brand);
                }

                UpdateBrandsStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки брендов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTags()
        {
            try
            {
                _tags.Clear();

                var tags = _db.Tags.ToList();
                foreach (var tag in tags)
                {
                    _tags.Add(tag);
                }

                UpdateTagsStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Методы фильтрации

        private bool FilterProduct(object obj)
        {
            if (obj is not Product product)
                return false;

            var searchText = ProductSearchTextBox.Text?.ToLower()?.Trim() ?? "";

            // Если поиск пустой - показываем все товары
            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            // Ищем во всех полях товара
            return (product.Name?.ToLower().Contains(searchText) == true) ||
                   (product.Description?.ToLower().Contains(searchText) == true) ||
                   (product.Category?.Name?.ToLower().Contains(searchText) == true) ||
                   (product.Brand?.Name?.ToLower().Contains(searchText) == true);
        }

        private bool FilterCategory(object obj)
        {
            if (obj is not Category category)
                return false;

            var searchText = CategorySearchTextBox.Text?.ToLower()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            return category.Name?.ToLower().Contains(searchText) == true;
        }

        private bool FilterBrand(object obj)
        {
            if (obj is not Brand brand)
                return false;

            var searchText = BrandSearchTextBox.Text?.ToLower()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            return brand.Name?.ToLower().Contains(searchText) == true;
        }

        private bool FilterTag(object obj)
        {
            if (obj is not Tag tag)
                return false;

            var searchText = TagSearchTextBox.Text?.ToLower()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            return tag.Name?.ToLower().Contains(searchText) == true;
        }

        #endregion

        #region Обновление статистики

        private void UpdateProductsStats()
        {
            var searchText = ProductSearchTextBox.Text?.ToLower()?.Trim() ?? "";
            var totalCount = _products.Count;
            var filteredCount = _productsView.Cast<object>().Count();

            ProductsStatsText.Text = string.IsNullOrWhiteSpace(searchText)
                ? $"Всего товаров: {totalCount}"
                : $"Найдено товаров: {filteredCount} из {totalCount}";
        }

        private void UpdateCategoriesStats()
        {
            var searchText = CategorySearchTextBox.Text?.ToLower()?.Trim() ?? "";
            var totalCount = _categories.Count;
            var filteredCount = _categoriesView.Cast<object>().Count();

            CategoriesStatsText.Text = string.IsNullOrWhiteSpace(searchText)
                ? $"Всего категорий: {totalCount}"
                : $"Найдено категорий: {filteredCount} из {totalCount}";
        }

        private void UpdateBrandsStats()
        {
            var searchText = BrandSearchTextBox.Text?.ToLower()?.Trim() ?? "";
            var totalCount = _brands.Count;
            var filteredCount = _brandsView.Cast<object>().Count();

            BrandsStatsText.Text = string.IsNullOrWhiteSpace(searchText)
                ? $"Всего брендов: {totalCount}"
                : $"Найдено брендов: {filteredCount} из {totalCount}";
        }

        private void UpdateTagsStats()
        {
            var searchText = TagSearchTextBox.Text?.ToLower()?.Trim() ?? "";
            var totalCount = _tags.Count;
            var filteredCount = _tagsView.Cast<object>().Count();

            TagsStatsText.Text = string.IsNullOrWhiteSpace(searchText)
                ? $"Всего тегов: {totalCount}"
                : $"Найдено тегов: {filteredCount} из {totalCount}";
        }

        #endregion

        #region Обработчики событий

        // Обработчик для поиска товаров
        private void ProductSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _productsView.Refresh();
            UpdateProductsStats();
        }

        // Обработчик для поиска категорий
        private void CategorySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _categoriesView.Refresh();
            UpdateCategoriesStats();
        }

        // Обработчик для поиска брендов
        private void BrandSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _brandsView.Refresh();
            UpdateBrandsStats();
        }

        // Обработчик для поиска тегов
        private void TagSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tagsView.Refresh();
            UpdateTagsStats();
        }

        // Обработчики для предотвращения автогенерации столбцов
        private void ProductsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Cancel = true;
        }

        private void CategoriesGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Cancel = true;
        }

        private void BrandsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Cancel = true;
        }

        private void TagsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Cancel = true;
        }

        #endregion

        #region Кнопки обновления

        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void RefreshCategories_Click(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        private void RefreshBrands_Click(object sender, RoutedEventArgs e)
        {
            LoadBrands();
        }

        private void RefreshTags_Click(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        #endregion

        #region CRUD операции для товаров

        private void AddProduct_Click(object sender, RoutedEventArgs e)
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

        private void Dialog_ProductAdded()
        {
            // Обновляем список товаров после добавления
            LoadProducts();
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
            {
                try
                {
                    using var db = new Pract15Context();
                    var product = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductTags)
                        .FirstOrDefault(p => Math.Abs(p.Id - id) < 0.0001);

                    if (product != null)
                    {
                        var dialog = new ProductEditWindow(product);
                        dialog.Owner = Application.Current.MainWindow;
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        // Подписываемся на событие обновления товара
                        dialog.ProductUpdated += Dialog_ProductUpdated;

                        if (dialog.ShowDialog() == true)
                        {
                            MessageBox.Show("Товар успешно обновлен!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        // Отписываемся от события
                        dialog.ProductUpdated -= Dialog_ProductUpdated;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании товара: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Dialog_ProductUpdated(Product updatedProduct)
        {
            // Находим и обновляем товар в коллекции
            var existingProduct = _products.FirstOrDefault(p =>
                Math.Abs(p.Id - updatedProduct.Id) < 0.0001);

            if (existingProduct != null)
            {
                // Загружаем полные данные из базы
                using var db = new Pract15Context();
                var freshProduct = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTags)
                    .FirstOrDefault(p => Math.Abs(p.Id - updatedProduct.Id) < 0.0001);

                if (freshProduct != null)
                {
                    // Находим индекс товара
                    int index = _products.IndexOf(existingProduct);

                    // Заменяем товар на обновленный
                    _products.RemoveAt(index);
                    _products.Insert(index, freshProduct);

                    // Обновляем фильтры
                    _productsView.Refresh();
                    UpdateProductsStats();
                }
            }
            else
            {
                // Если товар не найден, перезагружаем все
                LoadProducts();
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить этот товар?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
                {
                    try
                    {
                        using var db = new Pract15Context();
                        var product = db.Products
                            .FirstOrDefault(p => Math.Abs(p.Id - id) < 0.0001);

                        if (product != null)
                        {
                            // Удаляем связанные теги
                            var productTags = db.ProductTags
                                .Where(pt => Math.Abs(pt.ProductId - id) < 0.0001)
                                .ToList();
                            db.ProductTags.RemoveRange(productTags);

                            // Удаляем товар
                            db.Products.Remove(product);
                            db.SaveChanges();

                            // Удаляем из коллекции
                            var productToRemove = _products.FirstOrDefault(p => Math.Abs(p.Id - id) < 0.0001);
                            if (productToRemove != null)
                            {
                                _products.Remove(productToRemove);
                                _productsView.Refresh();
                                UpdateProductsStats();
                            }

                            MessageBox.Show("Товар успешно удален!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion

        #region CRUD операции для категорий

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleEditWindow("Добавить категорию", "Название категории");
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Value))
            {
                try
                {
                    using var db = new Pract15Context();

                    // Получаем максимальный ID
                    double maxId = 0;
                    if (db.Categories.Any())
                    {
                        maxId = db.Categories.Max(c => c.Id);
                    }
                    double newId = maxId + 1;

                    var category = new Category
                    {
                        Id = newId,
                        Name = dialog.Value.Trim()
                    };
                    db.Categories.Add(category);
                    db.SaveChanges();

                    // Добавляем в коллекцию и обновляем UI
                    _categories.Add(category);
                    _categoriesView.Refresh();
                    UpdateCategoriesStats();

                    MessageBox.Show("Категория успешно добавлена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления категории: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
            {
                try
                {
                    var category = _categories.FirstOrDefault(c => Math.Abs(c.Id - id) < 0.0001);

                    if (category != null)
                    {
                        var dialog = new SimpleEditWindow("Редактировать категорию", "Название категории", category.Name);
                        dialog.Owner = Application.Current.MainWindow;
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Value))
                        {
                            using var db = new Pract15Context();
                            var dbCategory = db.Categories
                                .FirstOrDefault(c => Math.Abs(c.Id - id) < 0.0001);

                            if (dbCategory != null)
                            {
                                dbCategory.Name = dialog.Value.Trim();
                                db.SaveChanges();

                                // Обновляем в коллекции и UI
                                category.Name = dialog.Value.Trim();
                                _categoriesView.Refresh();
                                UpdateCategoriesStats();

                                // Также нужно обновить товары, которые ссылаются на эту категорию
                                LoadProducts();

                                MessageBox.Show("Категория успешно обновлена!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования категории: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить эту категорию? Все товары этой категории станут без категории.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
                {
                    try
                    {
                        using var db = new Pract15Context();
                        var category = db.Categories
                            .FirstOrDefault(c => Math.Abs(c.Id - id) < 0.0001);

                        if (category != null)
                        {
                            // Находим товары этой категории и удаляем их (или устанавливаем default категорию)
                            var products = db.Products
                                .Where(p => Math.Abs(p.CategoryId - id) < 0.0001)
                                .ToList();

                            // Можно либо удалить товары, либо установить им другую категорию
                            // В данном случае удаляем товары
                            foreach (var product in products)
                            {
                                // Удаляем связанные теги товара
                                var productTags = db.ProductTags
                                    .Where(pt => Math.Abs(pt.ProductId - product.Id) < 0.0001)
                                    .ToList();
                                db.ProductTags.RemoveRange(productTags);

                                // Удаляем товар
                                db.Products.Remove(product);
                            }

                            db.Categories.Remove(category);
                            db.SaveChanges();

                            // Удаляем из коллекции и обновляем UI
                            var categoryToRemove = _categories.FirstOrDefault(c => Math.Abs(c.Id - id) < 0.0001);
                            if (categoryToRemove != null)
                            {
                                _categories.Remove(categoryToRemove);
                                _categoriesView.Refresh();
                                UpdateCategoriesStats();
                            }

                            // Обновляем товары
                            LoadProducts();

                            MessageBox.Show("Категория успешно удалена!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления категории: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion

        #region CRUD операции для брендов

        private void AddBrand_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleEditWindow("Добавить бренд", "Название бренда");
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Value))
            {
                try
                {
                    using var db = new Pract15Context();

                    // Получаем максимальный ID
                    double maxId = 0;
                    if (db.Brands.Any())
                    {
                        maxId = db.Brands.Max(b => b.Id);
                    }
                    double newId = maxId + 1;

                    var brand = new Brand
                    {
                        Id = newId,
                        Name = dialog.Value.Trim()
                    };
                    db.Brands.Add(brand);
                    db.SaveChanges();

                    // Добавляем в коллекцию и обновляем UI
                    _brands.Add(brand);
                    _brandsView.Refresh();
                    UpdateBrandsStats();

                    MessageBox.Show("Бренд успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления бренда: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditBrand_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
            {
                try
                {
                    var brand = _brands.FirstOrDefault(b => Math.Abs(b.Id - id) < 0.0001);

                    if (brand != null)
                    {
                        var dialog = new SimpleEditWindow("Редактировать бренд", "Название бренда", brand.Name);
                        dialog.Owner = Application.Current.MainWindow;
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Value))
                        {
                            using var db = new Pract15Context();
                            var dbBrand = db.Brands
                                .FirstOrDefault(b => Math.Abs(b.Id - id) < 0.0001);

                            if (dbBrand != null)
                            {
                                dbBrand.Name = dialog.Value.Trim();
                                db.SaveChanges();

                                // Обновляем в коллекции и UI
                                brand.Name = dialog.Value.Trim();
                                _brandsView.Refresh();
                                UpdateBrandsStats();

                                // Обновляем товары
                                LoadProducts();

                                MessageBox.Show("Бренд успешно обновлен!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования бренда: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteBrand_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить этот бренд? Все товары этого бренда станут без бренда.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
                {
                    try
                    {
                        using var db = new Pract15Context();
                        var brand = db.Brands
                            .FirstOrDefault(b => Math.Abs(b.Id - id) < 0.0001);

                        if (brand != null)
                        {
                            // Находим товары этого бренда и удаляем их (или устанавливаем default бренд)
                            var products = db.Products
                                .Where(p => Math.Abs(p.BrandId - id) < 0.0001)
                                .ToList();

                            // Удаляем товары этого бренда
                            foreach (var product in products)
                            {
                                // Удаляем связанные теги товара
                                var productTags = db.ProductTags
                                    .Where(pt => Math.Abs(pt.ProductId - product.Id) < 0.0001)
                                    .ToList();
                                db.ProductTags.RemoveRange(productTags);

                                // Удаляем товар
                                db.Products.Remove(product);
                            }

                            db.Brands.Remove(brand);
                            db.SaveChanges();

                            // Удаляем из коллекции и обновляем UI
                            var brandToRemove = _brands.FirstOrDefault(b => Math.Abs(b.Id - id) < 0.0001);
                            if (brandToRemove != null)
                            {
                                _brands.Remove(brandToRemove);
                                _brandsView.Refresh();
                                UpdateBrandsStats();
                            }

                            // Обновляем товары
                            LoadProducts();

                            MessageBox.Show("Бренд успешно удален!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления бренда: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion

        #region CRUD операции для тегов

        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SimpleEditWindow("Добавить тег", "Название тега");
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Value))
            {
                try
                {
                    using var db = new Pract15Context();

                    // Получаем максимальный ID
                    double maxId = 0;
                    if (db.Tags.Any())
                    {
                        maxId = db.Tags.Max(t => t.Id);
                    }
                    double newId = maxId + 1;

                    var tag = new Tag
                    {
                        Id = newId,
                        Name = dialog.Value.Trim()
                    };
                    db.Tags.Add(tag);
                    db.SaveChanges();

                    // Добавляем в коллекцию и обновляем UI
                    _tags.Add(tag);
                    _tagsView.Refresh();
                    UpdateTagsStats();

                    MessageBox.Show("Тег успешно добавлен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления тега: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
            {
                try
                {
                    var tag = _tags.FirstOrDefault(t => Math.Abs(t.Id - id) < 0.0001);

                    if (tag != null)
                    {
                        var dialog = new SimpleEditWindow("Редактировать тег", "Название тега", tag.Name);
                        dialog.Owner = Application.Current.MainWindow;
                        dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Value))
                        {
                            using var db = new Pract15Context();
                            var dbTag = db.Tags
                                .FirstOrDefault(t => Math.Abs(t.Id - id) < 0.0001);

                            if (dbTag != null)
                            {
                                dbTag.Name = dialog.Value.Trim();
                                db.SaveChanges();

                                // Обновляем в коллекции и UI
                                tag.Name = dialog.Value.Trim();
                                _tagsView.Refresh();
                                UpdateTagsStats();

                                MessageBox.Show("Тег успешно обновлен!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка редактирования тега: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить этот тег?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (sender is Button button && button.Tag != null && double.TryParse(button.Tag.ToString(), out double id))
                {
                    try
                    {
                        using var db = new Pract15Context();
                        var tag = db.Tags
                            .FirstOrDefault(t => Math.Abs(t.Id - id) < 0.0001);

                        if (tag != null)
                        {
                            // Удаляем связи с товарами
                            var productTags = db.ProductTags
                                .Where(pt => Math.Abs(pt.TagId - id) < 0.0001)
                                .ToList();
                            db.ProductTags.RemoveRange(productTags);

                            db.Tags.Remove(tag);
                            db.SaveChanges();

                            // Удаляем из коллекции и обновляем UI
                            var tagToRemove = _tags.FirstOrDefault(t => Math.Abs(t.Id - id) < 0.0001);
                            if (tagToRemove != null)
                            {
                                _tags.Remove(tagToRemove);
                                _tagsView.Refresh();
                                UpdateTagsStats();
                            }

                            MessageBox.Show("Тег успешно удален!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления тега: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #endregion

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new MainPage());
        }
    }
}