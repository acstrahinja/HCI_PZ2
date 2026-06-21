using NetworkService.ViewModel;
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

            // Uzimamo DataContext koji je bezbedno kreiran unutar XAML-a
            context = this.DataContext as MainWindowViewModel;

            // Osluškujemo tastaturu za globalne prečice (Ctrl+E, Ctrl+D, Ctrl+G, Ctrl+H)
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (context == null) return;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (e.Key == Key.E)
                {
                    context.NavigationCommand.Execute("entities");
                }
                else if (e.Key == Key.D)
                {
                    context.NavigationCommand.Execute("display");
                }
                else if (e.Key == Key.G)
                {
                    context.NavigationCommand.Execute("graph");
                }
                else if (e.Key == Key.H)
                {
                    context.ToggleHelpCommand.Execute(null);
                }
            }
        }
    }
}