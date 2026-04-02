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

        [RelayCommand]
        private void SaveChanges()
        {
            AppDataService.Instance.SaveData(SelectedMushroom);
            AppDataService.Instance.SaveAll(); // 🔹 Garante persistência
            IsEditing = false;
        }

        [RelayCommand]
        private void DeleteMushroom(Mushroom? mushroom)
        {
            if (mushroom == null) return;

            Mushrooms.Remove(mushroom);

            if (SelectedMushroom == mushroom)
                SelectedMushroom = null;

            AppDataService.Instance.SaveAll(); // 🔹 Atualiza JSON após exclusão
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

                // 🔹 Caminho raiz do projeto (sobe 3 níveis a partir de /bin/Debug/net8.0/)
                var projectRoot = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..")
                );

                // 🔹 Caminho da pasta Assets/Images dentro do projeto
                var assetsDir = Path.Combine(projectRoot, "Assets", "Images");

                // 🔹 Garante que a pasta exista
                if (!Directory.Exists(assetsDir))
                    Directory.CreateDirectory(assetsDir);

                // 🔹 Caminho completo do destino
                var destPath = Path.Combine(assetsDir, file.Name);

                // 🔹 Copia o arquivo
                await using (var sourceStream = await file.OpenReadAsync())
                await using (var destStream = File.Create(destPath))
                {
                    await sourceStream.CopyToAsync(destStream);
                }

                // 🔹 Caminho relativo usado no app
                SelectedMushroom.ImagePath = $"Assets/Images/{file.Name}";

                // 🔹 Salva no JSON
                AppDataService.Instance.SaveData(SelectedMushroom);
                AppDataService.Instance.SaveAll();

                OnPropertyChanged(nameof(Mushrooms));
                LoadSelectedImage();
            }
        }
    }
}