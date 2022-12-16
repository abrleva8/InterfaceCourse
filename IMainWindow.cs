using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using OxyPlot;
using ScottPlot;

namespace CourseProject {
    public interface IMainWindow {
        event EventHandler<EventArgs> OpenAbout;
        event EventHandler<EventArgs> Exit;
        event EventHandler<EventArgs> ConnectionClick;
        event EventHandler<EventArgs> ApplyClick;
        TextBox BufferSizeText { get; }
        TextBox PreBufferSizeText { get; }

        TextBox Pages { get; }

        TextBox IdPort { get; }

        TextBox ServiceName { get; }

        ComboBox Alg { get; }

        ListBox HistoryListBox { get;}

        WpfPlot BufferPlot { get; }

    }
}
