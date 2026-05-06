using Avalonia.Controls;
using Avalonia.Interactivity;
using MuseumApp.Domain;
using MuseumApp.Services;
using MuseumApp.Storage;
using System;
using System.Linq;

namespace MuseumApp
{
    public partial class MainWindow : Window
    {
        private MuseumManagerService _service;

        public MainWindow()
        {
            InitializeComponent();
            // Ініціалізація сервісу з локальним файлом JSON
            _service = new MuseumManagerService(new JsonMuseumRepository("museum_data.json"));
            
            // Заповнення списків категорій для інтерфейсу
            CategoryCombo.ItemsSource = Enum.GetValues(typeof(Category));
            FilterCategoryCombo.ItemsSource = Enum.GetValues(typeof(Category));
            
            LoadHalls();
        }

        private void LoadHalls()
        {
            HallsList.ItemsSource = null;
            HallsList.ItemsSource = _service.GetAllHalls();
        }

        private void HallsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateExhibitsDisplay();
        }

        private void UpdateExhibitsDisplay()
        {
            if (HallsList.SelectedItem is ExhibitionHall hall)
            {
                // Отримуємо відфільтровані експонати через сервіс
                var filtered = _service.FindExhibits(SearchInput.Text, (Category?)FilterCategoryCombo.SelectedItem);
                
                ExhibitsList.ItemsSource = null;
                // Відображаємо лише ті, що належать вибраному залу
                ExhibitsList.ItemsSource = filtered.Where(e => hall.Items.Any(i => i.InventoryNumber == e.InventoryNumber)).ToList();
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (HallsList.SelectedItem is ExhibitionHall hall && CategoryCombo.SelectedItem is Category cat) {
                    var exhibit = new Exhibit {
                        InventoryNumber = InvNumInput.Text ?? "",
                        Title = TitleInput.Text ?? "",
                        AuthorOrEra = AuthorInput.Text ?? "",
                        AcquisitionDate = DateTime.Now,
                        Category = cat,
                        Status = ConditionStatus.InStorage // Нові експонати за замовчуванням потрапляють у сховище
                    };
                    _service.AddExhibit(hall.Id, exhibit);
                    UpdateExhibitsDisplay();
                    
                    // Очищення полів
                    InvNumInput.Text = "";
                    TitleInput.Text = "";
                    AuthorInput.Text = "";
                }
                else {
                    TitleInput.Text = "Помилка: Виберіть зал та категорію!";
                }
            } catch (Exception ex) { TitleInput.Text = $"Помилка: {ex.Message}"; }
        }

        private void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (HallsList.SelectedItem is ExhibitionHall hall) {
                    _service.MoveExhibit(InvNumInput.Text ?? "", hall.Id);
                    LoadHalls();
                    InvNumInput.Text = "";
                }
            } catch (Exception ex) { TitleInput.Text = $"Помилка: {ex.Message}"; }
        }

        private void AddHallBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (!string.IsNullOrWhiteSpace(NewHallInput.Text)) {
                    _service.AddHall(NewHallInput.Text);
                    LoadHalls();
                    NewHallInput.Text = "";
                }
            } catch (Exception ex) { TitleInput.Text = $"Помилка: {ex.Message}"; }
        }

        private void DeleteHallBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (HallsList.SelectedItem is ExhibitionHall hall) {
                    _service.RemoveHall(hall.Id);
                    LoadHalls();
                }
            } catch (Exception ex) { TitleInput.Text = $"Помилка: {ex.Message}"; }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                _service.RemoveExhibit(InvNumInput.Text ?? "");
                LoadHalls();
            } catch (Exception ex) { TitleInput.Text = $"Помилка: {ex.Message}"; }
        }

        // Обробники подій для пошуку та фільтрів з правильною типізацією
        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => UpdateExhibitsDisplay();

        private void OnFilterSelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateExhibitsDisplay();
    }
}