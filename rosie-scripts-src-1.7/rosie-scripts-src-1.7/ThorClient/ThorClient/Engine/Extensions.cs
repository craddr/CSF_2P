using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ThorClient.Engine
{
    internal static class Extensions
    {
        private static int c = 0;
        public static void AppendText(this System.Windows.Controls.RichTextBox box, string text, SolidColorBrush brush)
        {
            if ((c++) % 50 == 0)
            {
                var textRange = new TextRange(box.Document.ContentStart, box.Document.ContentEnd);

                if (textRange.Text.Length > 5000)
                    box.Document.Blocks.Clear();
            }

            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }
    }
}
