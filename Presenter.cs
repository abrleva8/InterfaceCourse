using NRU_LIFO_library;
using OxyPlot;
using OxyPlot.Series;
using ScottPlot;
using ScottPlot.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Windows;
using System.Windows.Markup;

namespace CourseProject {
    public class Presenter {
        private readonly IMainWindow _main;
        public IAlgorithm service;

        public Presenter(IMainWindow main) {
            this._main = main;
            this._main.OpenAbout += this.ShowInfo;
            this._main.Exit += this.CloseApp;
            this._main.ConnectionClick += this.ChangeConnectionLabel;
            this._main.ApplyClick += this.LaunchProgram;
        }

        public void CloseApp(object sender, EventArgs e) {
            Environment.Exit(0);
        }

        public void ShowInfo(object sender, EventArgs e) {
            new WindowAbout().ShowDialog();
        }

        public void ChangeConnectionLabel(object sender, EventArgs e) {
                string serviceAddress = _main.IdPort.Text;
                string serviceName = _main.ServiceName.Text; 

                //Uri tcpUri = new Uri($"net.tcp://{ConfigurationManager.AppSettings["serviceAddress"]}/{ConfigurationManager.AppSettings["serviceName"]}");
                Uri tcpUri = new Uri($"net.tcp://{serviceAddress}/{serviceName}");

                EndpointAddress address = new EndpointAddress(tcpUri);
                NetTcpBinding clientBinding = new NetTcpBinding();
            
                ChannelFactory<IAlgorithm> factory = new ChannelFactory<IAlgorithm>(clientBinding, address);

                service = factory.CreateChannel();
                try {
                    service.IsConnected();
                } catch (Exception) {
                    throw new ServerException();
                }
        }

        private string CheckingSize(string fieldName, string sizeText, bool zeroIsGood=true) {
            if (sizeText.Length == 0) {
                return $"{fieldName} is empty!";
            }

            bool result = int.TryParse(sizeText, out var number);

            if (!result) {
                return $"{fieldName} should be a number!";
            }

            if (zeroIsGood) {
                if (number < 0) {
                    return $"{fieldName} should be a positive number or zero!";
                }
            } else {
                if (number < 1) {
                    return $"{fieldName} should be a positive number!";
                }
            }
            return "OK";
        }

        private bool CheckingData(object sender, EventArgs e) {
            string bufferText = this._main.BufferSizeText.Text;
            string checkResultBufferText = this.CheckingSize("Size of buffer", bufferText, false);

            if (checkResultBufferText != "OK") {
                MessageBox.Show(checkResultBufferText, "Type Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string preBufferText = this._main.PreBufferSizeText.Text;
            string checkResultPreBufferText = this.CheckingSize("Size of prebuffer", preBufferText);

            if (checkResultPreBufferText != "OK") {
                MessageBox.Show(checkResultPreBufferText, "Type Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            int bufferSize = Convert.ToInt32(bufferText);
            int preBufferSize = Convert.ToInt32(preBufferText);

            if (preBufferSize > bufferSize) {
                MessageBox.Show("Buffer size less than a prebuffer size! Please, fix it!", "Size Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string strPages = this._main.Pages.Text;
            if (strPages.Length == 0) {
                MessageBox.Show("Pages is empty! Please, fix it!", "Type Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            string[] pages = strPages.Split();
            int index = Array.IndexOf(pages, "b");
            if (index != -1) {
                MessageBox.Show("Pages contains 'b' value. Plese, fix it!", "Type Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        public List<Page> getListOfPagesFromTextBox(string[] strArrPages) {
            List<Page> pages = new List<Page>();
            foreach (string strPage in strArrPages) {

                if (strPage == "") {
                   continue;
                }
                int lastIndex = strPage.Length;
                bool isModified = false;
                if (strPage[strPage.Length - 1] == 'b') {
                    isModified = true;
                    lastIndex--;
                }
                string pageString = strPage.Substring(0, lastIndex);
                Page page = new Page(pageString, isModified);
                pages.Add(page);
            }
            return pages;
        }

        List<String> stringsHistory(List<List<Page>> history) {
            List<String> res = new List<string>();

            foreach (List<Page> listOfPages in history) {
                string tmp = "";
                foreach(Page page in listOfPages) {
                    tmp += page.NameOfPage;
                }
                res.Add(tmp);
            }

            return res;
        }

        public double[] GetFaultsArray(string algName, double[] dataX, List<Page> pages, int maxBufferSize, int preBufferSize) {
            double[] dataY = new double[3];
            if (maxBufferSize == 1) {
                maxBufferSize++;
            }
            for (int bufferSize = maxBufferSize - 1; bufferSize <= maxBufferSize + 1; bufferSize++) {
                dataX[bufferSize - maxBufferSize + 1] = bufferSize;
                dataY[bufferSize - maxBufferSize + 1] = service.CountOfFault(algName, pages, new List<Page>(), bufferSize, preBufferSize); ;
            }

            return dataY;
        }

        public void ShowPlot(double[] dataX, double[] dataY, double[] dataZ) {
            this._main.BufferPlot.Reset();
            this._main.BufferPlot.Plot.AddScatter(dataX, dataY, label: "LIFO");
            this._main.BufferPlot.Plot.AddScatter(dataX, dataZ, label: "NRU");
            this._main.BufferPlot.Plot.Legend(location: Alignment.UpperRight);
            this._main.BufferPlot.Plot.XLabel("Buffer size");
            this._main.BufferPlot.Plot.YLabel("Number of faults");
            this._main.BufferPlot.Refresh();
        }

        public void LaunchProgram(object sender, EventArgs e) {
            try {
                service.IsConnected();
            } catch(Exception) {
                throw new ServerException();
            }
            bool isGoodData = CheckingData(sender, e);
            if (isGoodData) {
                string currentAlg = this._main.Alg.Text;
                int maxBufferSize = Convert.ToInt32(this._main.BufferSizeText.Text);
                int preBufferSize = Convert.ToInt32(this._main.PreBufferSizeText.Text);
                string[] strArrPages = this._main.Pages.Text.Split();
                List<Page> pages = getListOfPagesFromTextBox(strArrPages);
                int res = service.CountOfFault(currentAlg, pages, new List<Page>(), maxBufferSize, preBufferSize);
                List<String> history = service.GetStringBufferHistory();
                List<Boolean> faultsHistory = service.GetFaultsHistory();
                this._main.HistoryListBox.ItemsSource = history;
                double[] dataX = new double[3];
                double[] dataY = GetFaultsArray("LIFO", dataX, pages, maxBufferSize, preBufferSize);
                double[] dataZ = GetFaultsArray("NRU", dataX, pages, maxBufferSize, preBufferSize);
                ShowPlot(dataX, dataY, dataZ);
            }
        }
    }
}