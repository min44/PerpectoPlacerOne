using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimGen.PerpectoPlacerOne.Presentation.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow(Document doc)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(doc);
        }

        private new void PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = IsTextAllowed(e.Text);

        private bool IsTextAllowed(string text) => new Regex("[^0-9]+").IsMatch(text);

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (IsTextAllowed(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        private void TextBoxLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var source = sender as System.Windows.Controls.TextBox;
            source.Text = source.Text.Trim();
            var strval = source.Text;
            try
            {
                var parsed = float.Parse(strval);
                if (parsed < 200.0)
                {
                    source.Text = "200";
                    TaskDialog.Show("Value range error", "Value must be grater than 200");
                }
            }
            catch
            {
                source.Text = "200";
                TaskDialog.Show("Unexpected value", "Value must be a number");
            }
        }

        private void TextBoxGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var source = sender as System.Windows.Controls.TextBox;
            source.SelectAll();
        }
    }
}
