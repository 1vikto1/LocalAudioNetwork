using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.Net.Sockets;
//using NAudio.Wave;
//using NAudio.CoreAudioApi;
using VisioForge.Libs.NAudio.Wave;
using System.IO;

namespace AudioServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Перетаскивается ли объект
        bool MouseDown = false;
        //Перенесена ли иконка в пределы зоны удаления
        bool MouseDownDelete = false;

        //перетаскиваемый объект
        Image GlobalSenderImage;
        TextBlock GlobalSenderTextBlock;

        public MainWindow()
        {
            InitializeComponent();

            //цикл вывода изображений из базы данных
            foreach(Connect.ComputerTable ComputerItem in App.DataBase.ComputerTable.ToList())
            {
                //Задаем свойства для иконки
                Image Image = new Image();
                Image.Name = "Computer" + ComputerItem.ComputerNumber.ToString();
                Image.Tag = ComputerItem.ComputerNumber;
                Uri uri = new Uri("file:///" + Directory.GetCurrentDirectory().Replace(@"\bin\Debug", "") + "/Images/ImagePC.png");
                BitmapImage bitmap = new BitmapImage(uri);
                Image.Source = bitmap;
                Image.Width = 150;
                Image.MouseDown += Image_MouseDown;
                Canvas.SetTop(Image, Convert.ToDouble(ComputerItem.VerticalCoordinates));
                Canvas.SetLeft(Image, Convert.ToDouble(ComputerItem.HorizontalCoordinates));

                //задаем свойства для ТекстБлока
                TextBlock TextBlock = new TextBlock();
                TextBlock.Text = ComputerItem.ComputerNumber.ToString();
                TextBlock.Name = "TextBlock" + ComputerItem.ComputerNumber.ToString();
                TextBlock.Tag = ComputerItem.ComputerNumber;
                TextBlock.MouseDown += Image_MouseDown;
                Canvas.SetTop(TextBlock, Convert.ToDouble(ComputerItem.VerticalCoordinates) + 10);
                Canvas.SetLeft(TextBlock, Convert.ToDouble(ComputerItem.HorizontalCoordinates) + 10);

                CanvasBox.Children.Add(Image);
                CanvasBox.Children.Add(TextBlock);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseDown)
            {
                try
                {
                    
                    //установка координат изображения
                    Canvas.SetTop(GlobalSenderImage, e.GetPosition(CanvasBox).Y);
                    Canvas.SetLeft(GlobalSenderImage, e.GetPosition(CanvasBox).X);

                    //установка координат текста
                    Canvas.SetTop(GlobalSenderTextBlock, e.GetPosition(CanvasBox).Y + 10);
                    Canvas.SetLeft(GlobalSenderTextBlock, e.GetPosition(CanvasBox).X + 10);
                }
                catch
                {
                    return;
                }
            }
            else
                return;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(sender.GetType().ToString() == "System.Windows.Controls.Image")
            {
                //Если нажали на иконку глобализируется она и найденнай текст
                Image SelectedImage = sender as Image;
                GlobalSenderImage = SelectedImage;

                string SelectedTBlockName = "TextBlock" + SelectedImage.Name.Replace("Computer", "");
                GlobalSenderTextBlock = (TextBlock) LogicalTreeHelper.FindLogicalNode(CanvasBox, SelectedTBlockName);
            }
            else
            {
                //Если нажали на текст глобализируется он и найденная иконка компьютера
                TextBlock SelectedTBlock = sender as TextBlock;
                GlobalSenderTextBlock = SelectedTBlock;

                string SelectedImageName = "Computer" + SelectedTBlock.Name.Replace("TextBlock", "");
                GlobalSenderImage = (Image)LogicalTreeHelper.FindLogicalNode(CanvasBox, SelectedImageName);
            }
            //начало работы перетаскивания и передача данных в ТекстБокс'ы
            MouseDown = true;
            TBoxIP.Text = App.DataBase.ComputerTable.FirstOrDefault
                (p => p.ComputerNumber == (int)GlobalSenderImage.Tag).ComputerIP;
            TBoxNumberPC.Text = GlobalSenderImage.Tag.ToString();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Проверка на наличие переносимых иконок
            if (GlobalSenderImage != null)
            {
                //Сохранение координат в базу данных
                App.DataBase.ComputerTable.FirstOrDefault
                        (p => p.ComputerNumber == (int)GlobalSenderImage.Tag).
                        VerticalCoordinates = (int) Canvas.GetTop(GlobalSenderImage);
                App.DataBase.ComputerTable.FirstOrDefault
                        (p => p.ComputerNumber == (int)GlobalSenderImage.Tag).
                        HorizontalCoordinates = (int)Canvas.GetLeft(GlobalSenderImage);
                App.DataBase.SaveChanges();

                //Условие на нахождение картинки в зоне удаления
                if ((int)Canvas.GetTop(GlobalSenderImage) > 592 && (int)Canvas.GetLeft(GlobalSenderImage) > 950)
                {
                    //Удаляем запись из базы данных
                    Connect.ComputerTable DeleteComputer = new Connect.ComputerTable();
                    DeleteComputer = App.DataBase.ComputerTable.FirstOrDefault(p => p.ComputerNumber == (int)GlobalSenderImage.Tag);
                    App.DataBase.ComputerTable.Remove(DeleteComputer);
                    App.DataBase.SaveChanges();

                    //удаление иконки и текста
                    CanvasBox.Children.Remove(GlobalSenderImage);
                    CanvasBox.Children.Remove(GlobalSenderTextBlock);

                    //чистка глобальных переменных
                    GlobalSenderImage = null;
                    GlobalSenderTextBlock = null;
                }
            }
            //отмена перетаскивания
            MouseDown = false;
        }

        private void AddZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int Number = App.DataBase.ComputerTable.Count() + 1;

            //Добавляем запись компьютера в базу данных
            Connect.ComputerTable NewComputer = new Connect.ComputerTable()
            {
                ComputerNumber = Number,
                ComputerIP = "000.000.0.00",
                VerticalCoordinates = 420,
                HorizontalCoordinates = 1000
            };
            App.DataBase.ComputerTable.Add(NewComputer);
            App.DataBase.SaveChanges();

            //Задаем свойства для иконки
            Image Image = new Image();
            Image.Name = "Computer" + Number.ToString();
            Image.Tag = Number;
            Uri uri = new Uri("file:///" + Directory.GetCurrentDirectory().Replace(@"\bin\Debug", "") + "/Images/ImagePC.png");
            BitmapImage bitmap = new BitmapImage(uri);
            Image.Source = bitmap;
            Image.Width = 150;
            Image.MouseDown += Image_MouseDown;
            Canvas.SetTop(Image, 420);
            Canvas.SetLeft(Image, 1000);

            //задаем свойства для ТекстБлока
            TextBlock TextBlock = new TextBlock();
            TextBlock.Text = Number.ToString();
            TextBlock.Name = "TextBlock" + Number.ToString();
            TextBlock.Tag = Number;
            TextBlock.MouseDown += Image_MouseDown;
            Canvas.SetTop(TextBlock, 430);
            Canvas.SetLeft(TextBlock, 1010);

            //добавление элементов в канвас
            CanvasBox.Children.Add(Image);
            CanvasBox.Children.Add(TextBlock);
        }

        private void TBoxIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            //обновление данных в базе данных
            App.DataBase.ComputerTable.FirstOrDefault
                (p => p.ComputerNumber == (int)GlobalSenderImage.Tag).ComputerIP = TBoxIP.Text;
            App.DataBase.SaveChanges();
        }

        private void TBoxNumberPC_TextChanged(object sender, TextChangedEventArgs e)
        {
            //условие исключает ошибки при удалении всех символов в текстбокс'е
            if(TBoxNumberPC.Text != "")
            {
                int NewNumberPC = Convert.ToInt16(TBoxNumberPC.Text);

                //обновление данных в базе данных
                App.DataBase.ComputerTable.FirstOrDefault
                    (p => p.ComputerNumber == (int)GlobalSenderImage.Tag).ComputerNumber = NewNumberPC;
                App.DataBase.SaveChanges();

                //обновление иконки и текста
                GlobalSenderImage.Tag = NewNumberPC;
                GlobalSenderImage.Name = "Computer" + TBoxNumberPC.Text;
                GlobalSenderTextBlock.Tag = NewNumberPC;
                GlobalSenderTextBlock.Name = "TextBlock" + TBoxNumberPC.Text;
                GlobalSenderTextBlock.Text = TBoxNumberPC.Text;
            }
        }
    }
}
