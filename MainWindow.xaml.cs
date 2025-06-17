using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Vaja_3___Tic_Tac_Toe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // Zaženi novo okno z igro
            if (rbSinglePlayer.IsChecked == true)
            {
                // Single-player
                GameWindow gw = new GameWindow(isSinglePlayer: true);
                gw.Show();
            }
            else
            {
                // Multi-player brez ustvarjanja/ joinanja
                MessageBox.Show("Prosimo izberite Create Game ali Join Game.");
            }
        }

        private void BtnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            // Ustvarjamo strežnik
            GameWindow gw = new GameWindow(isSinglePlayer: false, isServer: true, ipAddress: "0.0.0.0");
            gw.Show();
        }

        private void BtnJoinGame_Click(object sender, RoutedEventArgs e)
        {
            // Pridružujemo se obstoječi igri
            GameWindow gw = new GameWindow(isSinglePlayer: false, isServer: false, ipAddress: txtServerIP.Text);
            gw.Show();
        }
    }
}