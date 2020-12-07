using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalRClientSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        public event PropertyChangedEventHandler PropertyChanged;
        private HubConnection connection;
        private bool isConnected = false;
        private bool isSending = false;

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        public bool IsSending
        {
            get => isSending;
            set
            {
                isSending = value;
                OnPropertyChanged(nameof(IsSending));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                await Connect();
            }
            else
            {
                await connection.DisposeAsync();
                WriteToLog("Client disconnected!");
                IsConnected = false;
            }

        }

        public async Task Connect()
        {
            connection = new HubConnectionBuilder()
              .WithUrl(txtUrl.Text)
              .Build();
            try
            {
                WriteToLog("Connecting ...");
                await connection.StartAsync();
                WriteToLog("Connected to hub");
                IsConnected = true;

                connection.Closed += (error) =>
                {
                    IsConnected = true;
                    return Task.CompletedTask;
                };
            }
            catch (Exception ex)
            {
                logList.Items.Add(ex.Message);
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!IsSending && IsConnected)
            {
                IsSending = true;
                int interval = int.Parse(txtInterval.Text);
                WriteToLog($"Client started sending messages, interval: {interval}, message {txtMessage.Text}");
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                var SpanTime = new TimeSpan(0, 0, interval);
                dispatcherTimer.Interval = SpanTime;
                dispatcherTimer.Start();

            }
            else
            {
                IsSending = false;
                dispatcherTimer.Tick -= new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Stop();
                WriteToLog($"Client stop sending messages");
            }
        }

        private void WriteToLog(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                logList.SelectedIndex = logList.Items.Count - 1;
                logList.ScrollIntoView(logList.SelectedItem);
                logList.Items.Add($"{DateTime.Now} >>> {msg}");
            });
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            connection.InvokeAsync("ReceiveMessage", txtMessage.Text);
            WriteToLog("Message sent");
        }
    }
}
