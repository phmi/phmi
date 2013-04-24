using PHmiResources.Loc;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;

namespace PHmiRunner
{
    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper(Window window, WindowState showState)
        {
            InitializeComponent();
            _window = window;
            notifyIcon.Text = _window.Title;
            _showState = showState;

            toolStripMenuItemShow.Text = Res.Show;
            toolStripMenuItemShow.Click += ToolStripMenuItemShowClick;

            toolStripMenuItemClose.Text = Res.Close;
            toolStripMenuItemClose.Click += ToolStripMenuItemCloseClick;
        }

        private readonly Window _window;
        private readonly WindowState _showState;
        
        public void SetGreenIcon()
        {
            SetImageIndex(0);
        }

        public void SetGrayIcon()
        {
            SetImageIndex(1);
        }

        public void SetRedIcon()
        {
            SetImageIndex(2);
        }

        private int _lastImageIndex = -1;

        private void SetImageIndex(int index)
        {
            if (index != _lastImageIndex)
            {
                notifyIcon.Icon = Icon.FromHandle(((Bitmap)imageList.Images[index]).GetHicon());
                _lastImageIndex = index;
            }
        }

        private void ToolStripMenuItemCloseClick(object sender, EventArgs e)
        {
            _window.WindowState = _showState;
            _window.Close();
        }

        private void ToolStripMenuItemShowClick(object sender, EventArgs e)
        {
            _window.WindowState = _showState;
        }

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
