using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NetworkService.Model;
using NetworkService.ViewModel;

namespace NetworkService.Views
{
    public partial class NetworkDisplayView : UserControl
    {
        private Point startPoint;
        private bool isDraggingFromCanvas = false;

        public NetworkDisplayView()
        {
            InitializeComponent();
        }

        private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            isDraggingFromCanvas = false;
        }

        public void CanvasSlot_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
            isDraggingFromCanvas = true;
        }

        private void TreeView_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                var viewModel = this.DataContext as NetworkDisplayViewModel;
                if (viewModel == null) return;

                if (!isDraggingFromCanvas)
                {
                    TreeView treeView = sender as TreeView;
                    if (treeView?.SelectedItem is RoadEntity selectedRoad)
                    {
                        viewModel.SelectedEntity = selectedRoad;
                        viewModel.FirstSelectedSlot = -1;

                        DataObject dragData = new DataObject("RoadFormat", selectedRoad);
                        DragDrop.DoDragDrop(treeView, dragData, DragDropEffects.Move);
                    }
                }
                else
                {
                    Grid gridPanel = sender as Grid;
                    Border border = FindAncestor<Border>(gridPanel);

                    if (border != null && border.Tag != null)
                    {
                        int sourceIndex = Convert.ToInt32(border.Tag);

                        if (viewModel.CanvasSlots[sourceIndex].IsOccupied)
                        {
                            RoadEntity roadToMove = viewModel.CanvasSlots[sourceIndex].Road;

                            viewModel.SelectedEntity = roadToMove;
                            viewModel.FirstSelectedSlot = sourceIndex;

                            DataObject dragData = new DataObject("RoadFormat", roadToMove);
                            DragDrop.DoDragDrop(border, dragData, DragDropEffects.Move);
                        }
                    }
                }
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("RoadFormat"))
            {
                Border border = sender as Border;
                if (border == null)
                {
                    border = FindAncestor<Border>(sender as DependencyObject);
                }

                if (border != null && border.Tag != null)
                {
                    var viewModel = this.DataContext as NetworkDisplayViewModel;
                    if (viewModel != null && viewModel.SelectedEntity != null)
                    {
                        object targetIndex = border.Tag;
                        if (viewModel.DropCommand.CanExecute(targetIndex))
                        {
                            viewModel.DropCommand.Execute(targetIndex);

                            // Sinhronizacija sa nitima renderovanja slike
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    int index = Convert.ToInt32(targetIndex);
                                    if (index >= 0 && index < viewModel.CanvasSlots.Count)
                                    {
                                        viewModel.RefreshTreeView();
                                    }
                                }
                                catch (Exception) { }
                            }), System.Windows.Threading.DispatcherPriority.Background);
                        }
                    }
                }
            }
            isDraggingFromCanvas = false;
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("RoadFormat"))
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T ancestor) return ancestor;
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }
    }
}