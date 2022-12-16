using System;
using System.Collections.Generic;
using System.Windows;
using System.Configuration;
using System.ServiceModel;
using NRU_LIFO_library;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Data;
using ScottPlot;

namespace CourseProject {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow {
        TextBox IMainWindow.BufferSizeText {get => this.BufferSizeTextBox; }
        TextBox IMainWindow.PreBufferSizeText { get => this.PreBufferSizeTextBox; }

        TextBox IMainWindow.Pages { get => this.PageTextBox; }


        ComboBox IMainWindow.Alg { get => this.AlgorithmComboBox;  }

        ListBox IMainWindow.HistoryListBox { get => this.HistoryListBox;}

        WpfPlot IMainWindow.BufferPlot { get => this.BufferWpfPlot; }

        TextBox IMainWindow.IdPort { get => this.IpPortTextBox; }

        TextBox IMainWindow.ServiceName { get => this.ServiceNameTextBox; }

        public event EventHandler<EventArgs> OpenAbout;
        public event EventHandler<EventArgs> Exit;
        public event EventHandler<EventArgs> ConnectionClick;
        public event EventHandler<EventArgs> ApplyClick;

        public MainWindow() {
            this.InitializeComponent();
            var presenter = new Presenter(this);
            this.ConnectionStatus.Style = FindResource("RedConnection") as System.Windows.Style;
        }

        private void OpenAbout_Click(object sender, RoutedEventArgs e) {
            this.OpenAbout.Invoke(this, EventArgs.Empty);
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            this.Exit.Invoke(this, EventArgs.Empty);
        }

        private void ConnectButtonClick(object sender, RoutedEventArgs e) {
            try {
                this.ConnectionClick.Invoke(this, EventArgs.Empty);
            } catch (ServerException) {
                MessageBox.Show("Server does not work! Please launch it!", "Server Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                this.ConnectionStatus.Style = FindResource("RedConnection") as System.Windows.Style;
                return;
            }
            this.ConnectionStatus.Style = FindResource("GreenConnection") as System.Windows.Style;
        }

        private void ApplyButtonClick(object sender, RoutedEventArgs e) {
            try {
                this.ApplyClick.Invoke(this, EventArgs.Empty);
            } catch (ServerException) {
                MessageBox.Show("Server does not work! Please launch it!", "Server Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                this.ConnectionStatus.Style = FindResource("RedConnection") as System.Windows.Style;
            }
        }
    }
}