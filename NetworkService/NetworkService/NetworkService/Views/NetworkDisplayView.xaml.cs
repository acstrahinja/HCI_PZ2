using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class NetworkDisplayView : UserControl
    {
        private Point startPoint;

        public NetworkDisplayView()
        {
            InitializeComponent();
        }

        // Kada korisnik klikne mišem na stavku u TreeView-u
        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }

        // Kada pomeri miš, proveravamo da li je pokrenut Drag
        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                TreeView treeView = sender as TreeView;
                if (treeView?.SelectedItem is RoadEntity selectedRoad)
                {
                    // Privremeno zapamtimo u ViewModel-u šta nosimo
                    var viewModel = this.DataContext as NetworkDisplayViewModel;
                    if (viewModel != null)
                    {
                        viewModel.SelectedEntity = selectedRoad;

                        // Pokrećemo zvanični WPF Drag Drop mehanizam
                        DataObject dragData = new DataObject("RoadFormat", selectedRoad);
                        DragDrop.DoDragDrop(treeView, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        // Kada se pusti entitet iznad nekog polja na mapi
        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("RoadFormat"))
            {
                Border border = sender as Border;
                if (border != null)
                {
                    var viewModel = this.DataContext as NetworkDisplayViewModel;
                    if (viewModel != null && viewModel.SelectedEntity != null)
                    {
                        // Uzimamo indeks polja iz Tag svojstva i javljamo ViewModelu da odradi Drop
                        object targetIndex = border.Tag;
                        if (viewModel.DropCommand.CanExecute(targetIndex))
                        {
                            viewModel.DropCommand.Execute(targetIndex);
                        }
                    }
                }
            }
        }

        // Dozvoljavamo mapi da prihvati Drop
        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("RoadFormat"))
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
    }
}