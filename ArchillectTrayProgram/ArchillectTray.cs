using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArchillectTrayProgram
{
    public class ArchillectTray : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new ArchillectTray());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        public ArchillectTray()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Archillect";
            trayIcon.Icon = new Icon("archillectIcon.ico", 40, 40);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            System.Timers.Timer timer = new System.Timers.Timer { Interval = 120000 }; // 120 seconds  
            timer.Elapsed += Fetch.OnTimer;
            timer.Start();
            Fetch.TaskToDo();

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
