﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Shell;
using System.Threading;

namespace BetterExplorer
{
    /// <summary>
    /// Interaction logic for BreadcrumbBarControl.xaml
    /// </summary>
    public partial class BreadcrumbBarControl : UserControl
    {
        public BreadcrumbBarControl()
        {
            InitializeComponent();
        }

        private List<string> hl = new List<string>();


        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            ddGrid.Background = new SolidColorBrush(Colors.PowderBlue);
            //ddBorder.BorderBrush = new SolidColorBrush(Colors.LightBlue);
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            ddGrid.Background = new SolidColorBrush(Colors.White);
            //ddBorder.BorderBrush = new SolidColorBrush(Colors.White);
        }


        private ContextMenu CreateHistoryMenu()
        {
            ContextMenu hm = new ContextMenu();

            foreach (string item in hl)
            {
                MenuItem g = new MenuItem();
                g.Header = item;
                g.Width = grdMain.ActualWidth - 4;
                g.Click += new RoutedEventHandler(g_Click);
                hm.Items.Add(g);
            }

            return hm;
        }

        void g_Click(object sender, RoutedEventArgs e)
        {
            HistoryCombo.Text = (string)(sender as MenuItem).Header;
            //throw new NotImplementedException();
        }
        ContextMenu mnu;
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ddGrid.Background = new SolidColorBrush(Colors.SkyBlue);
            //ddBorder.BorderBrush = new SolidColorBrush(Colors.DeepSkyBlue);
            EnterEditMode();
            mnu = CreateHistoryMenu();
            mnu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            mnu.PlacementTarget = this;
            mnu.IsOpen = true;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ddGrid.Background = new SolidColorBrush(Colors.PowderBlue);
            //ddBorder.BorderBrush = new SolidColorBrush(Colors.LightBlue);
        }

        private BreadcrumbBarItem furthestrightitem;

        private bool writetohistory = true;

        public void ClearHistory()
        {
            hl.Clear();
        }

        public List<string> HistoryItems
        {
            get
            {
                List<string> hilist = new List<string>();

                foreach (string item in hl)
                {
                    hilist.Add(item);
                }

                return hilist;
            }
            set
            {
                foreach (string item in value)
                {
                    hl.Add(item);
                }
            }
        }

        private DragEventHandler de;
        private DragEventHandler dl;
        private DragEventHandler dm;
        private DragEventHandler dp;

        public delegate void PathEventHandler(object sender, PathEventArgs e);

        // An event that clients can use to be notified whenever the
        // elements of the list change:
        public event PathEventHandler NavigateRequested;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnNavigateRequested(PathEventArgs e)
        {
            if (NavigateRequested != null)
                NavigateRequested(this, e);
        }

        public void SetDragHandlers(DragEventHandler dragenter, DragEventHandler dragleave, DragEventHandler dragover, DragEventHandler drop)
        {
            de = dragenter;
            dl = dragleave;
            dm = dragover;
            dp = drop;
        }

        public bool RecordHistory
        {
            get
            {
                return writetohistory;
            }
            set
            {
                writetohistory = value;
            }
        }

        private List<ShellObject> GetPaths(ShellObject currloc)
        {
            List<ShellObject> res = new List<ShellObject>();
            ShellObject subject = currloc;

            bool apf = false;

            while (apf == false)
            {
                res.Add(subject);
                if (subject.Parent != null)
                {
                    subject = subject.Parent;
                }
                else
                {
                    apf = true;
                }
            }

            return res;
        }

        public void LoadDirectory(ShellObject currloc, bool loadDragEvents = true)
        {
            this.stackPanel1.Children.Clear();
            GetBreadCrumbItems(GetPaths(currloc));
            if (loadDragEvents == true)
            {
                foreach (BreadcrumbBarItem item in this.stackPanel1.Children)
                {
                    item.AllowDrop = true;
                    item.DragEnter += de;
                    item.DragLeave += dl;
                    item.DragOver += dm;
                    item.Drop += dp;
                }
            }
        }

        public ShellObject GetDirectoryAtPoint(Point pt)
        {
            try
            {
                return ((BreadcrumbBarItem)stackPanel1.InputHitTest(pt)).ShellObject;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void UpdateLastItem(int CurLocationCount)
        {
            BreadcrumbBarItem lastitem =
                stackPanel1.Children[stackPanel1.Children.Count - 1] as BreadcrumbBarItem;
            lastitem.SetChildren(CurLocationCount > 0);
        }

        private void GetBreadCrumbItems(List<ShellObject> items)
        {
            ShellObject lastmanstanding = items[0];
            items.Reverse();
            

            foreach (ShellObject thing in items)
            {
                bool isSearch = false;
                try 
	            {
                    isSearch = thing.IsSearchFolder;
	            }
	            catch 
	            {
		            isSearch = false;
	            }
                BreadcrumbBarItem duh = new BreadcrumbBarItem();
                if (!isSearch)
                {
                    duh.LoadDirectory(thing);
                    
                }
                else
                {
                    thing.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                    thing.Thumbnail.CurrentSize = new Size(16, 16);
                    duh.pathName.Text = thing.GetDisplayName(DisplayNameType.Default);
                    duh.PathImage.Source = thing.Thumbnail.BitmapSource;
                    duh.MenuBorder.Visibility = System.Windows.Visibility.Collapsed;
                    duh.grid1.Visibility = System.Windows.Visibility.Collapsed;
                    
                }

                
                
                duh.NavigateRequested += new BreadcrumbBarItem.PathEventHandler(duh_NavigateRequested);

                this.stackPanel1.Children.Add(duh);
                
                if (thing == lastmanstanding)
                {
                    furthestrightitem = duh;
                    duh.BringIntoView();
                }
               
            }
        }

        void duh_MouseDoubleClick(object sender, EventArgs e)
        {

        }

        private string FixShellPathsInEditMode(string LastPath)
        {
            string LLastPath = LastPath;
                   
            foreach (ShellObject item in KnownFolders.All)
            {
                LLastPath = LLastPath.Replace(item.ParsingName, item.GetDisplayName(DisplayNameType.Default));
            }

            return LLastPath.Replace(".library-ms","");
        }

        void duh_NavigateRequested(object sender, PathEventArgs e)
        {
            OnNavigateRequested(e);
            LastPath = e.ShellObject.ParsingName;
        }

        public string LastPath = "";

        private void HistoryCombo_GotFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            FocusManager.SetIsFocusScope(this, true);
            stackPanel1.Visibility = System.Windows.Visibility.Collapsed;
            HistoryCombo.Text = FixShellPathsInEditMode(LastPath);
        }

        private void HistoryCombo_LostFocus(object sender, RoutedEventArgs e)
        {

            e.Handled = true;
            ExitEditMode();
        }

        private void HistoryCombo_MouseLeave(object sender, MouseEventArgs e)
        {

        }

        private void HistoryCombo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            stackPanel1.Visibility = System.Windows.Visibility.Collapsed;
            if (LastPath != "")
            {
                HistoryCombo.Text = FixShellPathsInEditMode(LastPath);
            }
        }

        private void stackPanel1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        public bool IsInEditMode
        {
            get
            {
                return stackPanel1.Visibility != System.Windows.Visibility.Visible;
            }
        }

        public void ExitEditMode()
        {
            stackPanel1.Visibility = System.Windows.Visibility.Visible;
            //if (HistoryCombo.Text != "")
            //{
            //    LastPath = HistoryCombo.Text;
            //}

            HistoryCombo.Text = "";
            stackPanel1.Focusable = true;
            stackPanel1.IsHitTestVisible = false;
            stackPanel1.Focus();
            stackPanel1.Focusable = false;
            stackPanel1.IsHitTestVisible = true;
        }

        public void EnterEditMode()
        {
            stackPanel1.Visibility = System.Windows.Visibility.Collapsed;
            if (LastPath != "")
            {
                HistoryCombo.Text = FixShellPathsInEditMode(LastPath);
            }
            else
            {
                HistoryCombo.Text = FixShellPathsInEditMode(furthestrightitem.ShellObject.ParsingName);
            }
            HistoryCombo.Focus();
        }

        private void HistoryCombo_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (e.Key == Key.Enter)
            {
                try
                {
                    PathEventArgs ea = new PathEventArgs(ShellObject.FromParsingName(HistoryCombo.Text));
                    OnNavigateRequested(ea);
                    if (writetohistory == true)
                    {
                        if (hl.Contains(HistoryCombo.Text) == false)
                        {
                            hl.Add(HistoryCombo.Text);
                        }
                    }
                }
                catch (Exception)
                {
                    
                    // For now just handle the exception. later will be fixed to navigate correct path.
                }
            }
            if (e.Key == Key.Escape)
            {

                ExitEditMode();
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                furthestrightitem.BringIntoView();
            }
            catch (NullReferenceException)
            {
                
                //throw;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //MessageBox.Show(e.Delta.ToString(), "Mouse Wheel Change");
            //itemholder.ScrollToHorizontalOffset(0);
        }

        private void HistoryCombo_DropDownOpened(object sender, EventArgs e)
        {
            HistoryCombo.Focus();
            HistoryCombo.Text = FixShellPathsInEditMode(LastPath);
            stackPanel1.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HistoryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HistoryCombo.Focus();
            stackPanel1.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HistoryCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInEditMode)
            {
                TextChange[] tc = e.Changes.ToArray();
                if (tc[0].RemovedLength == 0 || tc[0].AddedLength > 0)
                {
                    foreach (string item in HistoryItems)
                    {
                        if (item.ToLowerInvariant().StartsWith(HistoryCombo.Text.ToLowerInvariant()))
                        {
                            int SelStart = HistoryCombo.Text.Length;
                            HistoryCombo.Text = item;
                            HistoryCombo.SelectionStart = SelStart;
                            HistoryCombo.SelectionLength = item.Length - SelStart;
                        }
                    } 
                }
            }
        }

        private void HistoryCombo_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (!mnu.IsOpen)
                {
                    ExitEditMode();
                }
            }
            catch
            {

            }
            
        }

        private void HistoryCombo_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
        }

    }
}
