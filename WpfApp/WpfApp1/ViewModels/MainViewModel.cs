using ExcelDataReader;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp1.Commands;

namespace WpfApp1.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        public DelegateCommand ImportFileCommand { get; set; }
        public DelegateCommand UpdateRectangleLocationCommand { get; set; }
        public DelegateCommand UpdatingDataCommand { get; set; }
        public ObservableCollection<ObjectModel> Objects { get; set; }
        public ObservableCollection<Rectangle> Rectangles { get; set; }

        private OpenFileDialog _openFileDialog;
        
        private ObjectModel _selectedObject; 
        public ObjectModel SelectedObject 
        {
            get => _selectedObject;
            set 
            {
                _selectedObject = value;
                OnPropertyChanged(nameof(TextInfo));
                OnPropertyChanged(nameof(SelectedObject));
                updateRectangles();
            }
        }

        public string TextInfo 
        {
            get => SelectedObject is null ? "Выберите строку в таблице." : getTextInfo();
        }

        private string getTextInfo() 
        {
            return $"Название: {SelectedObject.Name}\n" +
                   $"Гор. координата: {SelectedObject.X}\n" +
                   $"Верт. координата: {SelectedObject.Y}\n" +
                   $"Гор. размер: {SelectedObject.Width}\n" +
                   $"Верт. размер: {SelectedObject.Height}\n" +
                   $"Дефект: {(SelectedObject.IsDefect ? "Да" : "Нет")}";
        }

        private double _actualWidth;
        public double ActualWidth 
        {
            get => _actualWidth;
            set 
            {
                _actualWidth = value;
                OnPropertyChanged(nameof(ActualWidth));
            }
        }

        private double _actualHeight;
        public double ActualHeight
        {
            get => _actualHeight;
            set
            {
                _actualHeight = value;
                OnPropertyChanged(nameof(ActualHeight));
            }
        }

        public MainViewModel() 
        {
            ImportFileCommand = new DelegateCommand(obj => importFile());
            UpdateRectangleLocationCommand = new DelegateCommand((obj) => updateRectangleLocation());
            UpdatingDataCommand = new DelegateCommand((obj) => updatingData());
            Objects = new ObservableCollection<ObjectModel>();
            Rectangles = new ObservableCollection<Rectangle>();
            _openFileDialog = new OpenFileDialog()
            {
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx|CSV Files (*.csv)|*.csv"
            };
        }

        private void updatingData() 
        {
            OnPropertyChanged(nameof(TextInfo));
            updateRectangles();
        }

        private void updateRectangleLocation()
        {
            if(Rectangles.Count > 0) updateRectangles();
        }

        private void importFile()
        {
            if (_openFileDialog.ShowDialog() is true)
            {
                string filePath = _openFileDialog.FileName;
                if (Objects.Count > 0) 
                {
                    Objects.Clear();
                    SelectedObject = null;
                } 
                if (System.IO.Path.GetExtension(filePath).ToLower() == ".csv")
                {
                    ImportCsv(filePath);
                }
                else
                {
                    ImportExcel(filePath);
                }
            }
        }

        private void ImportCsv(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    var line = reader.ReadLine();
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(';');
                        if (values.Length == 6)
                        {
                            var obj = new ObjectModel
                            {
                                Name = values[0],
                                X = double.Parse(values[1]),
                                Y = double.Parse(values[2]),
                                Width = double.Parse(values[3]),
                                Height = double.Parse(values[4]),
                                IsDefect = values[5].Trim().ToLower() == "yes"
                            };
                            Objects.Add(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте CSV: {ex.Message}");
            }
        }

        private void ImportExcel(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet();
                        var table = result.Tables[0];

                        for (int i = 1; i < table.Rows.Count; i++) 
                        {
                            var row = table.Rows[i];
                            if (row.ItemArray.Length == 6)
                            {
                                var obj = new ObjectModel
                                {
                                    Name = row[0].ToString(),
                                    X = double.Parse(row[1].ToString()),
                                    Y = double.Parse(row[2].ToString()),
                                    Width = double.Parse(row[3].ToString()),
                                    Height = double.Parse(row[4].ToString()),
                                    IsDefect = row[5].ToString().Trim().ToLower() == "yes"
                                };
                                Objects.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте Excel: {ex.Message}");
            }
        }

        private Rectangle createRectangle() 
        {
            return new Rectangle
            {
                Width = SelectedObject.Width * (_actualWidth / 20),
                Height = SelectedObject.Height * (_actualHeight / 12),
                Stroke = SelectedObject.IsDefect ? Brushes.Red : Brushes.Green,
                StrokeThickness = 2
            };
        }

        private void updateRectangles()
        {
            if (SelectedObject != null) 
            {
                Rectangles.Clear();
                var rectangle = createRectangle();
                Canvas.SetLeft(rectangle, SelectedObject.X * (_actualWidth / 20));
                Canvas.SetTop(rectangle, SelectedObject.Y * (_actualHeight / 12));
                Rectangles.Add(rectangle);
            }
        }
    }
}
