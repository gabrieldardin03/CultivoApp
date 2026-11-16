using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CultivoApp.Models;
using CultivoApp.Services;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;

namespace CultivoApp.ViewModels
{
    public partial class CogumelosViewModel : ObservableObject
    {
        public ObservableCollection<Mushroom> Mushrooms => AppDataService.Instance.Mushrooms;

        [ObservableProperty]
        private Mushroom? selectedMushroom;

        [ObservableProperty]
        private bool isEditing;

        public bool IsReadOnly => !IsEditing;

        partial void OnIsEditingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsReadOnly));
        }

        // 🔹 Imagem exibida no painel de edição
        [ObservableProperty]
        private Bitmap? selectedImageBitmap;

        partial void OnSelectedMushroomChanged(Mushroom? value)
        {
            LoadSelectedImage();
            // Removido o disparo automático daqui para evitar envio de dados incompletos ao apenas clicar na lista.
        }

        private void LoadSelectedImage()
        {
            if (SelectedMushroom == null || string.IsNullOrEmpty(SelectedMushroom.ImagePath))
            {
                SelectedImageBitmap = null;
                return;
            }

            try
            {
                string path = SelectedMushroom.ImagePath
                    .Replace("file:///", "")
                    .Replace("file://", "")
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    SelectedImageBitmap = new Bitmap(stream);
                }
                else
                {
                    SelectedImageBitmap = null;
                }
            }
            catch
            {
                SelectedImageBitmap = null;
            }
        }

        [RelayCommand]
        private void AddMushroom()
        {
            var novo = new Mushroom
            {
                Name = "Novo Cogumelo",
                Temperature = "20–25 °C",
                Humidity = "80–90%",
                TempMin = 20.0, // Já inicia com valores padrão seguros
                TempMax = 25.0,
                UmidMin = 80.0,
                UmidMax = 90.0,
                ImagePath = "avares://CultivoApp/Assets/default.png"
            };

            Mushrooms.Add(novo);
            SelectedMushroom = novo;
            IsEditing = true;
        }

        [RelayCommand]
        private void EditMushroom(Mushroom? mushroom)
        {
            if (mushroom == null) return;
            SelectedMushroom = mushroom;
            IsEditing = true;
        }

        // 🔹 ATUALIZADO: Salva localmente E envia os dados reais para a placa ESP32 ao salvar
        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            if (SelectedMushroom != null)
            {
                // Salva no banco de dados local do aplicativo
                AppDataService.Instance.SaveData(SelectedMushroom);
                AppDataService.Instance.SaveAll();

                // Dispara os valores numéricos reais para o IP da placa
                await ApiService.UpdateConfigAsync(
                    SelectedMushroom.Name,
                    SelectedMushroom.TempMin,
                    SelectedMushroom.TempMax,
                    SelectedMushroom.UmidMin,
                    SelectedMushroom.UmidMax
                );
            }

            IsEditing = false;
        }

        [RelayCommand]
        private void DeleteMushroom(Mushroom? mushroom)
        {
            if (mushroom == null) return;

            Mushrooms.Remove(mushroom);

            if (SelectedMushroom == mushroom)
                SelectedMushroom = null;

            AppDataService.Instance.SaveAll();
        }

        [RelayCommand]
        public async Task AdicionarImagem()
        {
            if (SelectedMushroom == null) return;

            var topLevel = TopLevel.GetTopLevel(
                (App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow
            );

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Escolha uma imagem",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Imagens")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];

                var projectRoot = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
                );

                var assetsDir = Path.Combine(projectRoot, "Assets", "Images");

                if (!Directory.Exists(assetsDir))
                    Directory.CreateDirectory(assetsDir);

                var destPath = Path.Combine(assetsDir, file.Name);

                await using (var sourceStream = await file.OpenReadAsync())
                await using (var destStream = File.Create(destPath))
                {
                    await sourceStream.CopyToAsync(destStream);
                }

                SelectedMushroom.ImagePath = $"Assets/Images/{file.Name}";

                AppDataService.Instance.SaveData(SelectedMushroom);
                AppDataService.Instance.SaveAll();

                OnPropertyChanged(nameof(Mushrooms));
                LoadSelectedImage();
            }
        }
    }
}