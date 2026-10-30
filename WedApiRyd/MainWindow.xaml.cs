using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using WedApiRyd.Models;

namespace WedApiRyd
{
    public partial class MainWindow : Window
    {
        HttpClient client = new HttpClient();

        public ObservableCollection<FSubscription> Subscriptions { get; set; } = new();
        public FSubscription? CurrentSubscription { get; set; } = new();

        public string? filtActive { get; set; } = null;
        public decimal? filtMaxPrice { get; set; } = null;

        FSubscription C = new();

        public MainWindow()
        {
            client.BaseAddress = new Uri("https://localhost:7018/");

            InitializeComponent();

            Current.DataContext = C;
            DataContext = this;

            GetAll();
        }

        private async void PatchChanges(object sender, RoutedEventArgs e)
        {
            if (C.Id == null || CurrentSubscription == null)
            {
                MessageBox.Show("Выберите запись из таблицы для изменения!");
                return;
            }

            try
            {
                Current.UpdateLayout();

                var patchData = new Dictionary<string, object>
                {
                    ["Id"] = C.Id.Value,
                    ["Name"] = C.Name ?? "",
                    ["Description"] = C.Description ?? "",
                    ["IsActive"] = C.IsActive ?? false
                };
                if (C.Price != null) patchData["Price"] = C.Price.Value;
                if (C.DurationDays != null) patchData["DurationDays"] = C.DurationDays.Value;

                HttpResponseMessage res = await client.PatchAsJsonAsync(
                    $"api/v1/Subscriptions/{C.Id}",
                    patchData
                );

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Успешно изменено");
                    GetAll();
                }
                else
                {
                    string errorDetails = await res.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {res.StatusCode}\n\nСервер говорит:\n{errorDetails}", "Детали ошибки");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async void GetAll()
        {
            try
            {
                string url = "api/v1/Subscriptions";
                var query = new List<string>();

                if (filtActive == "True") query.Add("active=true");
                else if (filtActive == "False") query.Add("active=false");

                if (filtMaxPrice.HasValue) query.Add($"maxPrice={filtMaxPrice.Value}");

                if (query.Count > 0) url += "?" + string.Join("&", query);

                var response = await client.GetFromJsonAsync<List<FSubscription>>(url);

                Subscriptions.Clear();
                if (response != null)
                {
                    foreach (var item in response) Subscriptions.Add(item);
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("Связь с API сервером не установлена. Проверьте, запущен ли сервер.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Change(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentSubscription != null)
            {
                C = new FSubscription();
                C.Id = CurrentSubscription.Id;
                C.Name = CurrentSubscription.Name;
                C.Price = CurrentSubscription.Price;
                C.DurationDays = CurrentSubscription.DurationDays;
                C.Description = CurrentSubscription.Description;
                C.IsActive = CurrentSubscription.IsActive;
            }
            else
            {
                C = new FSubscription();
            }
            Current.DataContext = C;
        }

      

        private void Filter2(object sender, RoutedEventArgs e)
        {
            BindingExpression beActive = BindingOperations.GetBindingExpression(ComboBoxActive, ComboBox.SelectedValueProperty);
            beActive?.UpdateSource();

            BindingExpression bePrice = BindingOperations.GetBindingExpression(TextBoxPrice, TextBox.TextProperty);
            bePrice?.UpdateSource();

            GetAll();
        }


        private async void SaveChanges(object sender, RoutedEventArgs e)
        {
            if (C.Id == null)
            {
                MessageBox.Show("Выберите запись из таблицы для изменения");
                return;
            }

            try
            {
                Current.UpdateLayout();

                HttpResponseMessage res = await client.PutAsJsonAsync(
                    $"api/v1/Subscriptions/{C.Id}",
                    new
                    {
                        Id = C.Id,
                        Name = C.Name,
                        Price = C.Price,
                        DurationDays = C.DurationDays,
                        Description = C.Description,
                        IsActive = C.IsActive ?? false
                    });

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Успешно изменено");
                    GetAll();
                }
                else
                {
                    MessageBox.Show($"Ошибка сервера: {res.StatusCode}. Неверные данные.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }


        private async void Create(object sender, RoutedEventArgs e)
        {
            try
            {
                Current.UpdateLayout();

                if (string.IsNullOrWhiteSpace(C.Name) || string.IsNullOrWhiteSpace(C.Description) || C.Price == null || C.Price == null || C.DurationDays == null)
                {
                    MessageBox.Show("Заполните данные");
                    return;
                }

                HttpResponseMessage res = await client.PostAsJsonAsync(
                    "api/v1/Subscriptions",
                    new
                    {
                        Name = C.Name,
                        Price = C.Price,
                        DurationDays = C.DurationDays,
                        Description = C.Description,
                        IsActive = C.IsActive ?? true 
                    });

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Успешно добавлено");
                    GetAll();
                }
                else
                {
                    MessageBox.Show($"Ошибка сервера: {res.StatusCode}. Неверные данные (проверьте поля).");
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("Связь с API сервером не установлена");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            if (C.Id != null)
            {
                HttpResponseMessage res = await client.DeleteAsync($"api/v1/Subscriptions/{C.Id}");

                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show("Успешное удаление");
                    GetAll();
                }
                else
                {
                    MessageBox.Show("Ошибка удаления");
                }
            }
            else
            {
                MessageBox.Show("Выберите элемент!");
            }
        }
    }
}