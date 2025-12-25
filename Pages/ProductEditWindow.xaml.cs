using System;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Pract15.Models;
using Pract15.Services;
using System.Windows.Input;

namespace Pract15.Windows
{
    public partial class ProductEditWindow : Window
    {
        private int _productId;
        private bool _isEditMode;
        private Product _productToEdit;

        // Событие для уведомления об обновлении
        public event Action<Product> ProductUpdated;
        public event Action ProductAdded;

        public ProductEditWindow(Product product = null)
        {
            InitializeComponent();

            if (product != null)
            {
                _productId = product.Id;
                _isEditMode = true;
                Title = "Редактирование товара";
                _productToEdit = product;

                // Сразу загружаем данные товара в поля
                NameTextBox.Text = product.Name;
                DescriptionTextBox.Text = product.Description;
                PriceTextBox.Text = product.Price.ToString("0.00");
                StockTextBox.Text = product.Stock.ToString("0");
                RatingTextBox.Text = product.Rating.ToString("0.0");
            }
            else
            {
                _isEditMode = false;
                Title = "Добавление товара";
            }

            LoadData();
            NameTextBox.Focus();
        }

        private void LoadData()
        {
            try
            {
                using var db = new Pract15Context();

                // Загружаем категории и бренды
                CategoryComboBox.ItemsSource = db.Categories.ToList();
                BrandComboBox.ItemsSource = db.Brands.ToList();

                // Загружаем теги
                var tags = db.Tags.ToList();
                TagsListBox.ItemsSource = tags.Select(t => new TagViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsSelected = false
                }).ToList();

                // Если режим редактирования - загружаем выбранные категорию, бренд и теги
                if (_isEditMode)
                {
                    LoadProductSelectionData(_productId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void LoadProductSelectionData(int productId)
        {
            try
            {
                using var db = new Pract15Context();

                // Загружаем товар с категорией, брендом и тегами (включая связанные теги)
                var product = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag) // ВАЖНО: загружаем связанные объекты Tag
                    .FirstOrDefault(p => p.Id == productId);

                if (product != null)
                {
                    // Устанавливаем выбранные категорию и бренд
                    var category = CategoryComboBox.ItemsSource.Cast<Category>()
                        .FirstOrDefault(c => c.Id == product.CategoryId);
                    if (category != null)
                        CategoryComboBox.SelectedItem = category;

                    var brand = BrandComboBox.ItemsSource.Cast<Brand>()
                        .FirstOrDefault(b => b.Id == product.BrandId);
                    if (brand != null)
                        BrandComboBox.SelectedItem = brand;

                    // Загружаем ВСЕ теги из базы (актуальные)
                    var allTags = db.Tags.ToList();
                    var tagViewModels = allTags.Select(t => new TagViewModel
                    {
                        Id = t.Id,
                        Name = t.Name,
                        IsSelected = false
                    }).ToList();

                    // Получаем ID выбранных тегов из товара
                    var selectedTagIds = product.ProductTags
                        .Select(pt => pt.TagId)
                        .ToList();

                    // Устанавливаем выбранные теги
                    foreach (var tagViewModel in tagViewModels)
                    {
                        tagViewModel.IsSelected = selectedTagIds.Contains(tagViewModel.Id);
                    }

                    // Обновляем ListBox с тегами
                    TagsListBox.ItemsSource = tagViewModels;
                    TagsListBox.Items.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных товара: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool Validate()
        {
            try
            {
                ErrorText.Visibility = Visibility.Collapsed;

                // Проверка названия
                if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    ShowError("Введите название товара");
                    NameTextBox.Focus();
                    return false;
                }

                if (NameTextBox.Text.Length > 100)
                {
                    ShowError("Название не должно превышать 100 символов");
                    NameTextBox.Focus();
                    return false;
                }

                // Проверка описания
                if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    ShowError("Введите описание товара");
                    DescriptionTextBox.Focus();
                    return false;
                }

                if (DescriptionTextBox.Text.Length > 500)
                {
                    ShowError("Описание не должно превышать 500 символов");
                    DescriptionTextBox.Focus();
                    return false;
                }

                // Проверка цены
                if (string.IsNullOrWhiteSpace(PriceTextBox.Text))
                {
                    ShowError("Введите цену товара");
                    PriceTextBox.Focus();
                    return false;
                }

                if (!double.TryParse(PriceTextBox.Text, out double price))
                {
                    ShowError("Введите корректную цену (число)");
                    PriceTextBox.Focus();
                    PriceTextBox.SelectAll();
                    return false;
                }

                if (price <= 0)
                {
                    ShowError("Цена должна быть положительной");
                    PriceTextBox.Focus();
                    PriceTextBox.SelectAll();
                    return false;
                }

                if (price > 100000)
                {
                    ShowError("Цена не может превышать 100 000");
                    PriceTextBox.Focus();
                    PriceTextBox.SelectAll();
                    return false;
                }

                // Проверка количества
                if (string.IsNullOrWhiteSpace(StockTextBox.Text))
                {
                    ShowError("Введите количество товара");
                    StockTextBox.Focus();
                    return false;
                }

                if (!int.TryParse(StockTextBox.Text, out int stock))
                {
                    ShowError("Введите корректное количество (целое число)");
                    StockTextBox.Focus();
                    StockTextBox.SelectAll();
                    return false;
                }

                if (stock < 0)
                {
                    ShowError("Количество не может быть отрицательным");
                    StockTextBox.Focus();
                    StockTextBox.SelectAll();
                    return false;
                }

                if (stock > 10000)
                {
                    ShowError("Количество не может превышать 10 000");
                    StockTextBox.Focus();
                    StockTextBox.SelectAll();
                    return false;
                }

                // Проверка рейтинга
                if (string.IsNullOrWhiteSpace(RatingTextBox.Text))
                {
                    ShowError("Введите рейтинг товара");
                    RatingTextBox.Focus();
                    return false;
                }

                if (!double.TryParse(RatingTextBox.Text, out double rating))
                {
                    ShowError("Введите корректный рейтинг (число)");
                    RatingTextBox.Focus();
                    RatingTextBox.SelectAll();
                    return false;
                }

                if (rating < 0 || rating > 5)
                {
                    ShowError("Рейтинг должен быть от 0 до 5");
                    RatingTextBox.Focus();
                    RatingTextBox.SelectAll();
                    return false;
                }

                // Проверка категории
                if (CategoryComboBox.SelectedItem == null)
                {
                    ShowError("Выберите категорию");
                    CategoryComboBox.Focus();
                    return false;
                }

                // Проверка бренда
                if (BrandComboBox.SelectedItem == null)
                {
                    ShowError("Выберите бренд");
                    BrandComboBox.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка валидации: {ex.Message}");
                return false;
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validate())
                    return;

                using var db = new Pract15Context();
                Product savedProduct = null;

                if (_isEditMode)
                {
                    // РЕДАКТИРОВАНИЕ существующего товара
                    var existingProduct = db.Products
                        .Include(p => p.ProductTags)
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .FirstOrDefault(p => p.Id == _productId);

                    if (existingProduct == null)
                    {
                        ShowError("Товар не найден в базе данных");
                        return;
                    }

                    // Обновляем свойства
                    existingProduct.Name = NameTextBox.Text.Trim();
                    existingProduct.Description = DescriptionTextBox.Text.Trim();
                    existingProduct.Price = (int)Math.Round(double.Parse(PriceTextBox.Text), 0);  // Convert to int
                    existingProduct.Stock = int.Parse(StockTextBox.Text);  // Already int
                    existingProduct.Rating = Math.Round(double.Parse(RatingTextBox.Text), 1);

                    // Обновляем категорию и бренд
                    var selectedCategory = CategoryComboBox.SelectedItem as Category;
                    var selectedBrand = BrandComboBox.SelectedItem as Brand;

                    existingProduct.CategoryId = selectedCategory.Id;
                    existingProduct.BrandId = selectedBrand.Id;

                    // Обновляем теги
                    var selectedTags = TagsListBox.ItemsSource.Cast<TagViewModel>()
                        .Where(t => t.IsSelected)
                        .Select(t => t.Id)
                        .ToList();

                    // Удаляем старые теги
                    var oldProductTags = db.ProductTags
                        .Where(pt => pt.ProductId == existingProduct.Id)
                        .ToList();
                    db.ProductTags.RemoveRange(oldProductTags);

                    // Добавляем новые теги
                    foreach (var tagId in selectedTags)
                    {
                        db.ProductTags.Add(new ProductTag
                        {
                            ProductId = existingProduct.Id,
                            TagId = (int)tagId  // Cast to int
                        });
                    }

                    db.SaveChanges();

                    // Загружаем полные данные для обновления
                    savedProduct = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductTags)
                            .ThenInclude(pt => pt.Tag)
                        .FirstOrDefault(p => p.Id == existingProduct.Id);
                }
                else
                {
                    // ДОБАВЛЕНИЕ нового товара
                    int maxId = 0;
                    if (db.Products.Any())
                    {
                        maxId = db.Products.Max(p => p.Id);
                    }
                    int newId = maxId + 1;

                    var selectedCategory = CategoryComboBox.SelectedItem as Category;
                    var selectedBrand = BrandComboBox.SelectedItem as Brand;

                    var newProduct = new Product
                    {
                        Id = newId,
                        Name = NameTextBox.Text.Trim(),
                        Description = DescriptionTextBox.Text.Trim(),
                        Price = (int)Math.Round(double.Parse(PriceTextBox.Text), 0),  // Convert to int
                        Stock = int.Parse(StockTextBox.Text),
                        Rating = Math.Round(double.Parse(RatingTextBox.Text), 1),
                        CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        CategoryId = selectedCategory.Id,
                        BrandId = selectedBrand.Id
                    };

                    db.Products.Add(newProduct);
                    db.SaveChanges();

                    // Добавляем теги
                    var selectedTags = TagsListBox.ItemsSource.Cast<TagViewModel>()
                        .Where(t => t.IsSelected)
                        .Select(t => t.Id)
                        .ToList();

                    foreach (var tagId in selectedTags)
                    {
                        db.ProductTags.Add(new ProductTag
                        {
                            ProductId = newProduct.Id,
                            TagId = (int)tagId  // Cast to int
                        });
                    }

                    db.SaveChanges();

                    // Загружаем полные данные
                    savedProduct = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductTags)
                            .ThenInclude(pt => pt.Tag)
                        .FirstOrDefault(p => p.Id == newId);
                }

                // Генерируем события для обновления UI
                if (_isEditMode && savedProduct != null)
                {
                    ProductUpdated?.Invoke(savedProduct);
                }
                else if (!_isEditMode)
                {
                    ProductAdded?.Invoke();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и точку
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }

            // Проверяем, что точка только одна
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null && e.Text == "." && textBox.Text.Contains('.'))
            {
                e.Handled = true;
            }
        }

        private void StockTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        // Обработчик нажатия Enter для сохранения
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Save_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                Cancel_Click(sender, e);
            }
        }
    }

    public class TagViewModel
    {
        public int Id { get; set; }  
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}