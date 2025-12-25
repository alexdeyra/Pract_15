using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using Pract15.Models;
using Pract15.Services;

namespace Pract15.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly Pract15Context _db;
        private string _searchQuery = "";
        private int _selectedCategoryId = 0;
        private int _selectedBrandId = 0;
        private string _priceFrom = "";
        private string _priceTo = "";
        private ICollectionView _productsView;
        private ObservableCollection<Product> _products;

        public MainViewModel()
        {
            _db = DbService.Instance;
            _products = new ObservableCollection<Product>();

            LoadCategories();
            LoadBrands();
            LoadTags();

            _productsView = CollectionViewSource.GetDefaultView(_products);
            _productsView.Filter = FilterProduct;

            UserRole = AuthService.IsManagerMode ? "Менеджер" : "Посетитель";
            IsManagerMode = AuthService.IsManagerMode;
        }

        // Коллекции
        public ObservableCollection<Product> Products => _products;
        public ICollectionView ProductsView => _productsView;
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Brand> Brands { get; } = new();
        public ObservableCollection<Tag> Tags { get; } = new();

        // Свойства для фильтрации
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    RefreshFilter();
                }
            }
        }

        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                if (_selectedCategoryId != value)
                {
                    _selectedCategoryId = value;
                    OnPropertyChanged();
                    RefreshFilter();
                }
            }
        }

        public int SelectedBrandId
        {
            get => _selectedBrandId;
            set
            {
                if (_selectedBrandId != value)
                {
                    _selectedBrandId = value;
                    OnPropertyChanged();
                    RefreshFilter();
                }
            }
        }

        public string PriceFrom
        {
            get => _priceFrom;
            set
            {
                if (_priceFrom != value)
                {
                    _priceFrom = value;
                    OnPropertyChanged();
                    RefreshFilter();
                }
            }
        }

        public string PriceTo
        {
            get => _priceTo;
            set
            {
                if (_priceTo != value)
                {
                    _priceTo = value;
                    OnPropertyChanged();
                    RefreshFilter();
                }
            }
        }

        // Статистика
        public int TotalProductsCount => _products.Count;
        public int DisplayedProductsCount => _productsView?.Cast<object>().Count() ?? 0;

        // Режим пользователя
        public string UserRole { get; }
        public bool IsManagerMode { get; }

        // КОМАНДЫ СОРТИРОВКИ - ДОБАВЬТЕ ЭТИ МЕТОДЫ
        public void SortByNameAsc()
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            _productsView.Refresh();
        }

        public void SortByNameDesc()
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
            _productsView.Refresh();
        }

        public void SortByPriceAsc()
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
            _productsView.Refresh();
        }

        public void SortByPriceDesc()
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
            _productsView.Refresh();
        }

        public void SortByStockAsc()
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Ascending));
            _productsView.Refresh();
        }

        public void SortByStockDesc()
        {
            _productsView.SortDescriptions.Clear();
            _productsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Descending));
            _productsView.Refresh();
        }

        public void ResetFilters()
        {
            SearchQuery = "";
            SelectedCategoryId = 0;
            SelectedBrandId = 0;
            PriceFrom = "";
            PriceTo = "";
            _productsView.SortDescriptions.Clear();
            _productsView.Refresh();
            OnPropertyChanged(nameof(TotalProductsCount));
            OnPropertyChanged(nameof(DisplayedProductsCount));
        }

        public void LoadProducts()
        {
            _products.Clear();

            try
            {
                var products = _db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .ToList();

                foreach (var product in products)
                {
                    _products.Add(product);
                }

                RefreshFilter();

                OnPropertyChanged(nameof(TotalProductsCount));
                OnPropertyChanged(nameof(DisplayedProductsCount));

                System.Diagnostics.Debug.WriteLine($"Загружено {_products.Count} товаров");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки продуктов: {ex.Message}");
            }
        }




        private void LoadCategories()
        {
            Categories.Clear();
            try
            {
                var categories = _db.Categories.ToList();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadBrands()
        {
            Brands.Clear();
            try
            {
                var brands = _db.Brands.ToList();
                foreach (var brand in brands)
                {
                    Brands.Add(brand);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки брендов: {ex.Message}");
            }
        }

        private void LoadTags()
        {
            Tags.Clear();
            try
            {
                var tags = _db.Tags.ToList();
                foreach (var tag in tags)
                {
                    Tags.Add(tag);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки тегов: {ex.Message}");
            }
        }

        private bool FilterProduct(object obj)
        {
            if (obj is not Product product)
                return false;

            // Поиск по названию
            if (!string.IsNullOrEmpty(SearchQuery) &&
                !product.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
                return false;

            // Фильтр по категории (0 означает "все категории")
            if (SelectedCategoryId > 0 && product.CategoryId != SelectedCategoryId)
                return false;

            // Фильтр по бренду (0 означает "все бренды")
            if (SelectedBrandId > 0 && product.BrandId != SelectedBrandId)
                return false;

            // Фильтр по цене
            if (!string.IsNullOrEmpty(PriceFrom) && int.TryParse(PriceFrom, out int minPrice))
            {
                if (product.Price < minPrice)
                    return false;
            }

            if (!string.IsNullOrEmpty(PriceTo) && int.TryParse(PriceTo, out int maxPrice))
            {
                if (product.Price > maxPrice)
                    return false;
            }

            return true;
        }

        public void RefreshFilter()
        {
            _productsView?.Refresh();
            OnPropertyChanged(nameof(DisplayedProductsCount));
            OnPropertyChanged(nameof(TotalProductsCount));
        }

        // Метод для обновления товара в коллекции
        // Метод для обновления товара в коллекции
        public void UpdateProductInCollection(Product updatedProduct)
        {
            var existingProduct = Products.FirstOrDefault(p => p.Id == updatedProduct.Id);

            if (existingProduct != null)
            {
                var index = Products.IndexOf(existingProduct);
                Products.RemoveAt(index);

                try
                {
                    var db = DbService.Instance;
                    // Загружаем полные данные ВКЛЮЧАЯ ТЕГИ
                    var freshProduct = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductTags)
                            .ThenInclude(pt => pt.Tag) // ВАЖНО: загружаем связанные теги
                        .FirstOrDefault(p => p.Id == updatedProduct.Id);

                    if (freshProduct != null)
                    {
                        Products.Insert(index, freshProduct);
                    }
                    else
                    {
                        // Если не удалось загрузить из базы, используем обновленный
                        Products.Insert(index, updatedProduct);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка обновления товара: {ex.Message}");
                    Products.Insert(index, updatedProduct);
                }

                OnPropertyChanged(nameof(TotalProductsCount));
                OnPropertyChanged(nameof(DisplayedProductsCount));
            }
            else
            {
                Products.Add(updatedProduct);
                OnPropertyChanged(nameof(TotalProductsCount));
                OnPropertyChanged(nameof(DisplayedProductsCount));
            }

            RefreshFilter();
        }


        // Метод для добавления нового товара
        public void AddNewProduct()
        {
            LoadProducts();
            RefreshFilter();
        }

        // Метод для обновления категорий
        public void RefreshCategories()
        {
            LoadCategories();
        }

        // Метод для обновления брендов
        public void RefreshBrands()
        {
            LoadBrands();
        }

        // Метод для обновления тегов
        public void RefreshTags()
        {
            LoadTags();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}