using PSI_IUIS___PZ2___početni_projekat;
using System;
using System.Windows;
using System.Windows.Input;

namespace NetworkService
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel context;

        public MainWindow()
        {
            InitializeComponent();

            // Povezujemo pozadinsku logiku sa prozorom
            context = new MainWindowViewModel();
            this.DataContext = context;

            // Osluškujemo tastaturu za prečice (Ctrl+E, Ctrl+D, Ctrl+G, Ctrl+H)
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.E)
                {
                    context.NavigateCommand.Execute("entities");
                }
                else if (e.Key == Key.D)
                {
                    context.NavigateCommand.Execute("display");
                }
                else if (e.Key == Key.G)
                {
                    context.NavigateCommand.Execute("graph");
                }
                else if (e.Key == Key.H)
                {
                    context.ToggleHelpCommand.Execute(null);
                }
            }
        }
    }
}