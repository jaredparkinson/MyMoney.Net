﻿using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Walkabout.Data;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Imaging;
using Walkabout.Configuration;
using Walkabout.Charts;

namespace Walkabout.WpfConverters
{
    // Extracts the first letter of the Category name.
    public class CategoryTypeLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is CategoryType)
            {
                CategoryType type = (CategoryType)value;
                return type.ToString()[0].ToString();
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool param = parameter == null ? true : bool.Parse(parameter as string);
            bool val = (bool)value;
            return val == param ?
            Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                bool rc = (bool)value;
                if (rc && parameter != null)
                {
                    try
                    {
                        Color c = (Color)ColorConverter.ConvertFromString(parameter.ToString());
                        return new SolidColorBrush(c);
                    }
                    catch
                    {
                    }
                }
            }
            return DependencyProperty.UnsetValue; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    public class MonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int month = (int)value;
            DateTime date = new DateTime(1900, month, 1);
            return date.ToString("MMMM");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }


    public class NullableValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value is INullable && value.ToString() == "Null"))
            {
                return string.Empty;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (string.IsNullOrEmpty(value.ToString()))
            //    return null;
            return value;
        }

    }


    public class AttachmentIconConverter : IValueConverter
    {
        ImageSource cachedIcon;

        public AttachmentIconConverter()
        {
            
        }

        private ImageSource Icon
        {
            get
            {
                if (cachedIcon == null)
                {
                    try
                    {
                        BitmapDecoder decoder = BitmapDecoder.Create(new Uri("pack://application:,,,/MyMoney;component/Dialogs/Icons/Attachment.png"), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None);
                        cachedIcon = decoder.Frames[0];
                    }
                    catch
                    {
                        MessageBox.Show("Cannot find Attachment.png icon", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                return cachedIcon;
            }
        }

        public string AttachmentPath
        {
            get
            {
                return Settings.TheSettings.AttachmentDirectory;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            if (value is bool && (bool)value)
            {
                return this.Icon;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }



    public class RoutingLines : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string valueAsString = (string)value;
            if (valueAsString == null)
            {
                return value;
            }

            Grid g = new Grid();
            g.Opacity = 0.66;

            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

            g.RowDefinitions.Add(new RowDefinition());
            g.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);

            int column = 0;
            TrackStyle ts = TrackStyle.Empty;
            foreach (char c in valueAsString)
            {
                switch (c)
                {
                    case 'B':
                        ts = TrackStyle.Buy;
                        break;

                    case 'A':
                        ts = TrackStyle.Add;
                        break;

                    case '|':
                        ts = TrackStyle.Full;
                        break;

                    case 'S':
                        ts = TrackStyle.Sell;
                        break;

                    case 'L':
                        ts = TrackStyle.CloseLinked;
                        break;

                    case 'C':
                        ts = TrackStyle.ClosePositive;
                        break;

                    case 'c':
                        ts = TrackStyle.CloseNegative;
                        break;


                }

                AddTrack(g, column, ts);
                column++;
            }

            return g;
        }

        enum TrackStyle
        {
            Empty,
            Full,
            Buy,
            Add,
            Sell,
            ClosePositive,
            CloseNegative,
            CloseLinked
        }


        static Brush lineColorBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x7F, 0xFF)); // Light semi-transparent blue
        static CornerRadius shapeAdd = new CornerRadius(0,5,5,0);
        static CornerRadius shapeSubtract = new CornerRadius(5,0,0,5);
        static CornerRadius shapeStart = new CornerRadius(5);
        static CornerRadius shapeStop = new CornerRadius(5);

        /// <summary>
        /// 
        ///       FULL    BUY    ADD     SELL    CLOSE LINKED
        ///   0    │             │       │        │  
        ///        │             │       │        │
        ///   1    │      O      O       O        └─ 
        ///        │      │      |
        ///   2    │      │      |
        /// </summary>
        /// <param name="g"></param>
        /// <param name="columnIndex"></param>
        /// <param name="trackStyle"></param>
        private static void AddTrack(Grid g, int columnIndex, TrackStyle trackStyle)
        {
            //-----------------------------------------------------------------
            // Add a new column
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = new GridLength(18);
            g.ColumnDefinitions.Add(cd);


            //-----------------------------------------------------------------
            // Add a the vertical bar
            // 
            Border b = new Border();
            b.Background = lineColorBrush;
            b.Width = 2;
            b.HorizontalAlignment = HorizontalAlignment.Center;
            b.VerticalAlignment = VerticalAlignment.Stretch;

            if (trackStyle == TrackStyle.ClosePositive || trackStyle == TrackStyle.CloseNegative || trackStyle == TrackStyle.CloseLinked)
            {
                // From top to center
                Grid.SetRow(b, 0);
                Grid.SetRowSpan(b, 2);
            }
            else if (trackStyle == TrackStyle.Buy)
            {
                // From center to bottom
                Grid.SetRow(b, 1);
                Grid.SetRowSpan(b, 2);
            }
            else
            {
                // Full vertical length
                Grid.SetRow(b, 0);
                Grid.SetRowSpan(b, 3);
            }

            Grid.SetColumn(b, columnIndex);
            g.Children.Add(b);


            //-----------------------------------------------------------------
            // Add the Circle
            //
            if (trackStyle != TrackStyle.Full)
            {
                Border spot = new Border();

                spot.HorizontalAlignment = HorizontalAlignment.Center;

                switch (trackStyle)
                {
                    case TrackStyle.Add:
                        spot.BorderBrush = lineColorBrush;
                        spot.Background = Brushes.AliceBlue;
                        spot.CornerRadius = shapeAdd;
                        spot.HorizontalAlignment = HorizontalAlignment.Right;
                        break;

                    case TrackStyle.Sell:
                        spot.BorderBrush = lineColorBrush;
                        spot.Background = lineColorBrush;
                        spot.CornerRadius = shapeSubtract;
                        spot.HorizontalAlignment = HorizontalAlignment.Left;
                        break;

                    case TrackStyle.Buy:
                        spot.BorderBrush = lineColorBrush;
                        spot.Background = Brushes.AliceBlue;
                        spot.CornerRadius = shapeStart;
                        break;

                    case TrackStyle.CloseNegative:
                        spot.BorderBrush = Brushes.Red;
                        spot.Background = Brushes.Red;
                        spot.CornerRadius = shapeStop;
                        break;

                    case TrackStyle.ClosePositive:
                        spot.BorderBrush = Brushes.Green;
                        spot.Background = Brushes.Green;
                        spot.CornerRadius = shapeStop;
                        break;
                }

                spot.BorderThickness = new Thickness(2);

                spot.VerticalAlignment = VerticalAlignment.Center;
                
                spot.Height = 10;
                spot.Width = 10;
                Grid.SetRow(spot, 1); // Always in the center row
                Grid.SetColumn(spot, columnIndex);
                g.Children.Add(spot);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }


    public class SqlDecimalToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SqlDecimal)
            {
                SqlDecimal sqlDecimal = (SqlDecimal)value;
                if (sqlDecimal.IsNull)
                {
                    return string.Empty;
                }
                return sqlDecimal.Value;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                string s = (string)value;
                if (string.IsNullOrWhiteSpace(s))
                {
                    return SqlDecimal.Null;
                }
                return new SqlDecimal((decimal)System.Convert.ToDecimal(value));
            }

            if (value is decimal)
            {
                return new SqlDecimal((decimal)value);
            }
            return new SqlDecimal(0);
        }
    }



    public class DecimalToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value is INullable && value.ToString() == "Null"))
            {
                return string.Empty;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is string)
            {
                string stringVal = value as string;

                decimal d = 0;
                if (!string.IsNullOrWhiteSpace(stringVal) && 
                    false == Decimal.TryParse(stringVal, NumberStyles.Currency, CultureInfo.CurrentCulture, out d))
                {
                    d = System.Convert.ToDecimal(stringVal, CultureInfo.GetCultureInfo("en-US"));
                }

                return d;
            }
            return value;
        }
    }

    public class TrueToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue || value == null || ((bool)value) == false)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not a valid method to call
            return 0;
        }
    }

    public class FalseToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || ((bool)value) == false)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not a valid method to call
            return 0;
        }
    }

    public class NullOrEmptyStringToVisible : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not a valid method to call
            return 0;
        }
    }

    public class TrueToVisibleWhenSelected : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || ((bool)value) == false)
            {
                return DataGridRowDetailsVisibilityMode.Collapsed;
            }

            return DataGridRowDetailsVisibilityMode.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not a valid method to call
            return 0;
        }
    }


    public class MoneyColorConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is decimal)
            {
                decimal c = (decimal)value;
                if (c < 0)
                {
                    return Brushes.Red;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    public class NonzeroToFontBoldConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int flags = (int)value;
            if (flags != 0)
            {
                return FontWeights.Bold;
            }
            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
