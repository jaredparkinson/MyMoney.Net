﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Walkabout.Utilities;
using Walkabout.Help;

namespace Walkabout.Controls
{
    /// <summary>
    /// Interaction logic for QuickFilterControl.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for QuickFilterControl.xaml
    /// </summary>
    public partial class QuickFilterControl : UserControl
    {
        public delegate void QuickFilterValueChanged(object sender, string filter);

        public event QuickFilterValueChanged FilterValueChanged;

        public QuickFilterControl()
        {
            this.InitializeComponent();
        }

        public string FilterText
        {
            get
            {
                return this.InputFilterText.Text;
            }
            set
            {
                this.InputFilterText.Text = value;
            }
        }

        private void OnTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tv = sender as TextBox;
                if (tv != null)
                {
                    FiterEventTextChanged(tv.Text);
                }
            }

        }

        private void FiterEventTextChanged(string filter)
        {
            if (FilterValueChanged != null)
            {
                UiDispatcher.BeginInvoke(FilterValueChanged, this, filter);
            }
        }

        private void OnInputFilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb != null && string.IsNullOrWhiteSpace(tb.Text) == false)
            {
                ClearFilter.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                ClearFilter.Visibility = System.Windows.Visibility.Collapsed;
            }
        }


        private void OnClearFilterButton_Closed(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            this.InputFilterText.Text = string.Empty;
            FiterEventTextChanged(this.InputFilterText.Text);
        }

        internal void FocusTextBox()
        {
            InputFilterText.Focus();
            InputFilterText.SelectAll();
        }
    }

}
