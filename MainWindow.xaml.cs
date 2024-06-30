using BoxHitsBOIII_WPF.MemoryUtils;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BoxHitsBOIII_WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool _gameIsOpen = true;
        private bool _gameIsPaused = true;
        private bool _boxHitted = false;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private int _Hits = 0;
        public int Hits
        {
            get => _Hits;
            set
            {
                _Hits = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ContextMenu = (ContextMenu)Resources["contextMenu"];

            await MonitoringGameAsync();
        }

        #region BasicWindowConfig
        private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ChangeBackground_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("change background, init?");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            BoxHitsBOIII.Close();
        }


        #endregion BasicWindowConfig

        private async Task TrackBoxHitsAsync()
        {
            while(!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Process? gameProcess = GameMemory.GetGameFromMemory();
                bool isOpen = GameIsOpen(gameProcess);

                if(!isOpen)
                {
                    _gameIsOpen = false;
                    break;
                }

                bool? connectToGame = ConnectToGame(gameProcess);

                if(connectToGame == true && connectToGame != null)
                {
                    if(GameIsPaused(gameProcess!))
                    {
                        break;
                    }

                    // tracking starts here
                    bool boxOpened = BoxIsOpened(gameProcess!);

                    if(_boxHitted && !boxOpened)
                    {
                        _boxHitted = false;
                    }
                    else if(!_boxHitted && boxOpened)
                    {
                        _boxHitted = true;
                        Hits++;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task MonitoringGameAsync()
        {
            while(!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Process? gameProcess = GameMemory.GetGameFromMemory();
                bool isOpen = GameIsOpen(gameProcess);

                if(isOpen)
                {
                    _gameIsOpen = true;
                    UpdateHitsUI(Hits.ToString());
                    await TrackBoxHitsAsync();
                }
                else
                {
                    _gameIsOpen = false;
                    UpdateHitsUI("Game is closed.", true);
                }

                await Task.Delay(TimeSpan.FromSeconds(3), _cancellationTokenSource.Token);
            }
        }

        private void UpdateHitsUI(string hits, bool isError = false)
        {
            Dispatcher.Invoke(() =>
            {
                if(isError)
                {
                    tbHits.Text = hits;
                    tbHits.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    tbHits.Text = hits;
                    tbHits.Foreground = new SolidColorBrush(Colors.White);
                }
            });
        }

        private static bool GameIsOpen(Process? game)
        {
            return game != null;
        }

        /// <summary>
        /// Connects to the game and set all the base addresses
        /// </summary>
        /// <param name="game"></param>
        /// <returns>true if the game is open and the tracker is connected to the memory</returns>
        private bool? ConnectToGame(Process? game)
        {
            bool isOpen = GameIsOpen(game);
            if(game == null || !isOpen) // invalid game or game closed
            {
                return false;
            }

            IntPtr gameAddr = GameMemory.GetGameModuleBaseMemoryAddress(game);

            if(gameAddr == IntPtr.Zero)
            {
                return false;
            }

            Addresses.BaseGameAddr = gameAddr;
            Addresses.BoxCoverAddr = Addresses.BaseGameAddr + 0x47CB2E4;
            Addresses.GamePausedAddr = Addresses.BaseGameAddr + 0x347EE08;
            Addresses.RoundNumber = Addresses.BaseGameAddr + 0xA55BDEC;
            Addresses.BoxGunAddr = Addresses.BaseGameAddr + 0x1647D207;
            Addresses.MapNameAddr = Addresses.BaseGameAddr + 0x940C5E8;
            Addresses.PointsAddr = Addresses.BaseGameAddr + 0x4D1DD1C;

            return true;
        }

        private bool GameIsPaused(Process game)
        {
            long pauseValue = GameMemory.ReadIntFromMemory(game, Addresses.GamePausedAddr);

            switch(pauseValue)
            {
                case 1:
                    _gameIsPaused = true;
                    return true;
                case 0:
                    _gameIsPaused = false;
                    return false;
                default:
                    _gameIsPaused = false;
                    return false;
            }
        }

        private bool BoxIsOpened(Process game)
        {
            long boxState = GameMemory.ReadIntFromMemory(game, Addresses.BoxCoverAddr);

            return boxState switch
            {
                1 => true,
                2 => false,
                _ => false,
            };
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}