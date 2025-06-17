using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Vaja_3___Tic_Tac_Toe
{
    public partial class GameWindow : Window
    {
        // 3x3 polje gumbov za križec-krožec
        private Button[,] fields = new Button[3, 3];

        // Trenutni igralec (X ali O)
        private char currentPlayer = 'X'; // X začne

        // Ali je igra enojna (proti računalniku) ali ne
        private bool isSinglePlayer;

        // Ali smo strežnik ali odjemalec
        private bool isServer;

        // Omrežni elementi
        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;
        private Thread networkThread;

        // Za naključno izbiro poteze računalnika
        private Random rnd = new Random();

        // Kontrola poteze - če smo mi na vrsti ali ne
        private bool isMyTurn = true; // Strežnik začne prvi
        private bool gameOver = false; // Označuje, ali je igra zaključena

        public GameWindow(bool isSinglePlayer, bool isServer = false, string ipAddress = null)
        {
            InitializeComponent();
            this.isSinglePlayer = isSinglePlayer;
            this.isServer = isServer;

            // Ustvari gumbe 3x3 in jih dodaj v GameBoard
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var btn = new Button { FontSize = 32 };
                    btn.Click += Cell_Click;
                    fields[r, c] = btn;
                    GameBoard.Children.Add(btn);
                }
            }

            // Če ni single-player, vzpostavi povezavo (strežnik ali odjemalec)
            if (!isSinglePlayer)
            {
                if (isServer)
                {
                    // Zaženi strežnik in čakaj na nasprotnika
                    StartServer();
                    StatusText.Text = "Čakam na nasprotnika... (Strežnik)";
                    isMyTurn = true;
                }
                else
                {
                    // Poveži se na strežnik
                    ConnectToServer(ipAddress);
                    StatusText.Text = "Povezujem se na strežnik...";
                    isMyTurn = false;
                }
            }
            else
            {
                // Single-player mod: X začne, računalnik bo O
                StatusText.Text = "Na potezi: X (Single-player)";
            }
        }

        // Klik na celico na plošči
        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (gameOver) return; // Če je igra končana, ne naredi nič
            var btn = (Button)sender;

            // Če je polje prazno in smo mi na potezi (ali je single-player)
            if (btn.Content == null && (isSinglePlayer || (isMyTurn && !gameOver)))
            {
                // Postavi znak trenutnega igralca v izbrano polje
                btn.Content = currentPlayer.ToString();

                // Preveri ali je kdo zmagal ali je neodločeno
                CheckWinOrDraw();

                if (!gameOver)
                {
                    // Zamenjaj igralca
                    SwitchPlayer();

                    // Če smo v multi-player načinu, pošlji novo stanje igre nasprotniku
                    if (!isSinglePlayer && stream != null)
                    {
                        isMyTurn = false;
                        StatusText.Text = "Na potezi je nasprotnik.";
                        SendBoardState();
                    }
                    else if (isSinglePlayer && currentPlayer == 'O')
                    {
                        // V single-player načinu, če je zdaj računalnik na vrsti, naredi njegovo potezo
                        ComputerMove();
                    }
                }
            }
        }

        // Zamenja trenutnega igralca (X -> O ali O -> X)
        private void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == 'X') ? 'O' : 'X';
            if (isSinglePlayer && !gameOver)
            {
                StatusText.Text = "Na potezi: " + currentPlayer;
            }
        }

        // Poteza računalnika: izbere naključno prazno polje in vanj postavi svoj znak
        private void ComputerMove()
        {
            if (gameOver) return;
            var emptyCells = GameBoard.Children.OfType<Button>().Where(b => b.Content == null).ToList();
            if (emptyCells.Any())
            {
                var cell = emptyCells[rnd.Next(emptyCells.Count)];
                cell.Content = currentPlayer.ToString();
                CheckWinOrDraw();
                if (!gameOver) SwitchPlayer();
            }
        }

        // Preveri ali je kdo zmagal ali neodločeno stanje
        private void CheckWinOrDraw()
        {
            char? winner = GetWinner();
            if (winner.HasValue)
            {
                gameOver = true;
                MessageBox.Show("Zmagal je: " + winner.Value);
                CloseGame();
            }
            else if (GameBoard.Children.OfType<Button>().All(b => b.Content != null))
            {
                // Če so vsa polja zapolnjena in ni zmagovalca, je neodločeno
                gameOver = true;
                MessageBox.Show("Neodločeno!");
                CloseGame();
            }
        }

        // Zapri igro in počisti vire
        private void CloseGame()
        {
            try
            {
                stream?.Close();
                client?.Close();
                server?.Stop();
            }
            catch { }
            this.Close();
        }

        // Preveri vrstice, stolpce in diagonale, da najde zmagovalca
        private char? GetWinner()
        {
            // Preveri vrstice
            for (int i = 0; i < 3; i++)
            {
                // Vsebino gumbov v vrsti pretvorimo v string in jih primerjamo
                string c1 = fields[i, 0].Content as string;
                string c2 = fields[i, 1].Content as string;
                string c3 = fields[i, 2].Content as string;

                // Če so vsi trije enaki in niso null/prazni, imamo zmagovalca
                if (!string.IsNullOrEmpty(c1) && c1 == c2 && c2 == c3)
                    return c1[0]; // vrne 'X' ali 'O'
            }

            // Preveri stolpce
            for (int i = 0; i < 3; i++)
            {
                string c1 = fields[0, i].Content as string;
                string c2 = fields[1, i].Content as string;
                string c3 = fields[2, i].Content as string;

                if (!string.IsNullOrEmpty(c1) && c1 == c2 && c2 == c3)
                    return c1[0];
            }

            // Preveri diagonali
            {
                string c1 = fields[0, 0].Content as string;
                string c2 = fields[1, 1].Content as string;
                string c3 = fields[2, 2].Content as string;

                if (!string.IsNullOrEmpty(c1) && c1 == c2 && c2 == c3)
                    return c1[0];
            }

            {
                string c1 = fields[0, 2].Content as string;
                string c2 = fields[1, 1].Content as string;
                string c3 = fields[2, 0].Content as string;

                if (!string.IsNullOrEmpty(c1) && c1 == c2 && c2 == c3)
                    return c1[0];
            }

            // Če ne najdemo zmagovalca, vrnemo null
            return null;
        }

        #region Networking - omrežna komunikacija

        // Zaženi strežnik in počakaj na nasprotnika
        private void StartServer()
        {
            networkThread = new Thread(() =>
            {
                try
                {
                    server = new TcpListener(IPAddress.Any, 5000);
                    server.Start();
                    client = server.AcceptTcpClient();
                    stream = client.GetStream();
                    Dispatcher.Invoke(() =>
                    {
                        StatusText.Text = "Nasprotnik se je povezal. Začenja strežnik (X).";
                        isMyTurn = true;
                    });

                    ListenForBoardState();
                }
                catch
                {
                    if (!gameOver)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("Napaka pri povezavi ali nasprotnik je zapustil igro.");
                            CloseGame();
                        }));
                    }
                }
            });
            networkThread.IsBackground = true;
            networkThread.Start();
        }

        // Poveži se na obstoječi strežnik (igro)
        private void ConnectToServer(string ipAddress)
        {
            networkThread = new Thread(() =>
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(ipAddress, 5000);
                    stream = client.GetStream();
                    Dispatcher.Invoke(() =>
                    {
                        StatusText.Text = "Povezano! Na potezi je nasprotnik (X).";
                        isMyTurn = false;
                    });
                    ListenForBoardState();
                }
                catch
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("Ne morem se povezati na strežnik ali strežnik je zapustil igro.");
                        CloseGame();
                    }));
                }
            });
            networkThread.IsBackground = true;
            networkThread.Start();
        }

        // Neprekinjeno poslušaj stanje plošče, ki nam ga pošilja nasprotnik
        private void ListenForBoardState()
        {
            while (!gameOver)
            {
                try
                {
                    // Format podatkov:
                    // [0]: currentPlayer (X ali O)
                    // [1..9]: 9 znakov za polja (X, O ali presledek za prazno)
                    // [10]: 1, če je zdaj naša poteza, 0 če ni
                    byte[] buffer = new byte[11];
                    int bytesRead = 0;
                    while (bytesRead < 11 && !gameOver)
                    {
                        int r = stream.Read(buffer, bytesRead, 11 - bytesRead);
                        if (r == 0) throw new Exception("Povezava prekinjena");
                        bytesRead += r;
                    }

                    char receivedCurrentPlayer = (char)buffer[0];
                    char[] boardChars = new char[9];
                    for (int i = 0; i < 9; i++)
                        boardChars[i] = (char)buffer[i + 1];

                    bool turnFlag = buffer[10] == 1;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Posodobi GUI glede na prejete podatke
                        currentPlayer = receivedCurrentPlayer;
                        UpdateBoardFromChars(boardChars);
                        isMyTurn = turnFlag;
                        

                        // Posodobi status glede na to, kdo je na vrsti
                        if (!gameOver)
                        {
                            if (isMyTurn)
                                StatusText.Text = "Na potezi si ti (" + currentPlayer + ")";
                            else
                                StatusText.Text = "Na potezi je nasprotnik.";
                        }
                        CheckWinOrDraw();
                    }));
                }
                catch
                {
                    // Če pride do napake (nasprotnik je zapustil, povezava prekinjena ...)
                    if (!gameOver)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("Nasprotnik je zapustil igro ali je prišlo do napake.");
                            CloseGame();
                        }));
                    }
                    break;
                }
            }
        }

        // Pošlji trenutno stanje plošče nasprotniku
        private void SendBoardState()
        {
            if (stream == null || gameOver) return;

            char[] boardChars = GetBoardAsChars();

            // Format:
            // [0]: trenutni igralec (X/O)
            // [1..9]: stanje polj
            // [10]: 1 če je zdaj nasprotnik na vrsti, 0 če sem jaz
            byte[] data = new byte[11];
            data[0] = (byte)currentPlayer;
            for (int i = 0; i < 9; i++)
            {
                data[i + 1] = (byte)boardChars[i];
            }
            // Jaz sem zdaj odigral potezo, torej nasprotnik je na vrsti
            data[10] = (byte)(!isMyTurn ? 1 : 0);

            try
            {
                stream.Write(data, 0, data.Length);
            }
            catch
            {
                if (!gameOver)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("Napaka pri pošiljanju podatkov. Nasprotnik je morda zapustil igro.");
                        CloseGame();
                    }));
                }
            }
        }

        // Vrne trenutno stanje plošče kot char polje (X, O ali ' ')
        private char[] GetBoardAsChars()
        {
            char[] arr = new char[9];
            var buttons = GameBoard.Children.OfType<Button>().ToArray();
            for (int i = 0; i < 9; i++)
            {
                arr[i] = buttons[i].Content == null ? ' ' : ((string)buttons[i].Content)[0];
            }
            return arr;
        }

        // Posodobi ploščo iz char[] stanja, ki smo ga prejeli od nasprotnika
        private void UpdateBoardFromChars(char[] boardChars)
        {
            var buttons = GameBoard.Children.OfType<Button>().ToArray();
            for (int i = 0; i < 9; i++)
            {
                buttons[i].Content = boardChars[i] == ' ' ? null : boardChars[i].ToString();
            }
        }

        #endregion
    }
}
