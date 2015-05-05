using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PHmiClient.Controls.Input;
using PHmiClient.Loc;
using PHmiClient.Utils;

namespace PHmiClient.Controls
{
    public class ImageInput : Control
    {
        static ImageInput()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageInput), new FrameworkPropertyMetadata(typeof(ImageInput)));
        }

        #region BinarySource
        
        public IEnumerable<byte> BinarySource
        {
            get { return (IEnumerable<byte>)GetValue(BinarySourceProperty); }
            set { SetValue(BinarySourceProperty, value); }
        }

        public static readonly DependencyProperty BinarySourceProperty =
            DependencyProperty.Register("BinarySource", typeof(IEnumerable<byte>), typeof(ImageInput),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var imageInput = (ImageInput)dependencyObject;
            imageInput._saveCommand.RaiseCanExecuteChanged();
            imageInput._deleteCommand.RaiseCanExecuteChanged();
        }

        #endregion

        public ImageInput()
        {
            MaxFileSize = 204800;
            _openCommand = new DelegateCommand(OpenCommandExecuted);
            _saveCommand = new DelegateCommand(SaveCommandExecuted, SaveCommandCanExecute);
            _deleteCommand = new DelegateCommand(DeleteCommandExecuted, DeleteCommandCanExecute);
        }

        public long MaxFileSize { get; set; }

        #region Format

        private const string Formats =
            "PNG (*.png)|*.png|JPEG (*.jpg, *.jpeg)|*.jpg; *.jpeg|BMP (*.bmp)|*.bmp|GIF (*.gif)|*.gif|TIFF (*.tiff)|*.tiff";

        private static ImageFormat IndexToFormat(int index)
        {
            ImageFormat format;
            switch (index)
            {
                case 1:
                    format = ImageFormat.Png;
                    break;
                case 2:
                    format = ImageFormat.Jpeg;
                    break;
                case 3:
                    format = ImageFormat.Bmp;
                    break;
                case 4:
                    format = ImageFormat.Gif;
                    break;
                case 5:
                    format = ImageFormat.Tiff;
                    break;
                default:
                    format = ImageFormat.Png;
                    break;
            }
            return format;
        }

        #endregion

        #region OpenCommand

        private readonly DelegateCommand _openCommand;

        public ICommand OpenCommand { get { return _openCommand; } }

        private OpenFileDialog _openDialog;

        private void OpenCommandExecuted(object obj)
        {
            if (_openDialog == null)
            {
                _openDialog = new OpenFileDialog
                    {
                        Filter = Formats,
                        Multiselect = false
                    };
            }
            _openDialog.FileName = null;

            if (!_openDialog.ShowDialog(Window.GetWindow(this)).GetValueOrDefault(false))
                return;
            if (File.Exists(_openDialog.FileName))
            {
                var fi = new FileInfo(_openDialog.FileName);
                if (fi.Length > MaxFileSize)
                {
                    var message = string.Format(Res.TooBigFileMessage, MaxFileSize.ToString("N0"));
                    ShowMessage(message);
                    return;
                }
            }
            var format = IndexToFormat(_openDialog.FilterIndex);
            try
            {
                BinarySource = new BitmapImage(new Uri(_openDialog.FileName)).ToBytes(format);
            }
            catch (Exception exception)
            {
                ShowMessage(exception.Message);
            }
        }

        #endregion

        private void ShowMessage(string message)
        {
            var owner = Window.GetWindow(this);
            if (owner != null)
                MessageBox.Show(owner, message);
            else
                MessageBox.Show(message);
        }

        #region SaveCommand

        private readonly DelegateCommand _saveCommand;

        public ICommand SaveCommand { get { return _saveCommand; } }

        private bool SaveCommandCanExecute(object obj)
        {
            return BinarySource != null;
        }

        private SaveFileDialog _saveDialog;

        private void SaveCommandExecuted(object obj)
        {
            if (BinarySource == null)
                return;
            if (_saveDialog == null)
            {
                _saveDialog = new SaveFileDialog
                    {
                        Filter = Formats,
                        OverwritePrompt = true
                    };
            }
            _saveDialog.FileName = null;
            if (!_saveDialog.ShowDialog(Window.GetWindow(this)).GetValueOrDefault(false))
                return;

            using (var stream = _saveDialog.OpenFile())
            {
                var imageSource = (BitmapImage) ImageHelper.ToImage(BinarySource.ToArray());
                var bytes = imageSource.ToBytes(IndexToFormat(_saveDialog.FilterIndex));
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        #endregion

        #region DeleteCommand

        private readonly DelegateCommand _deleteCommand;

        public ICommand DeleteCommand { get { return _deleteCommand; } }

        private bool DeleteCommandCanExecute(object obj)
        {
            return BinarySource != null;
        }

        private void DeleteCommandExecuted(object obj)
        {
            BinarySource = null;
        }

        #endregion
    }
}
