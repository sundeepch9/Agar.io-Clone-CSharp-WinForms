using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net.Security;
using System.Net;
using System.Threading;
using System.Drawing.Text;
using System.Collections;

namespace GameTest
{
    public partial class Form1 : Form
    {
        bool goLeft, goRight, goUp, goDown;
        int playerScore = 0;
        int playerSpeed = 10;
        List<PictureBox> playerList = new List<PictureBox>();
        List<string> playersActive = new List<string>();
        string clientPlayerInfo;
        string clientPlayerName = "";
        bool respawn = true;

        List<PictureBox> balls_orbs = new List<PictureBox>();
        List <string> orbsStrList = new List<string>();

        //Establishing connection
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        private static EndPoint Remote = (EndPoint)sender;

        public void connectToServer()
        {
            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint tmpRemote = (EndPoint)sender;
            try
            {
                server.Connect(ipep);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Unable to connect to server.");
                Console.WriteLine(e.ToString());

            }
        }
        public Form1()
        {
            InitializeComponent();
            connectToServer();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkServermessage()
        {
            try
            {
                string message = messageReceived();
                if (message != "")
                {
                    string[] dataString = message.Split(',');
                    if (dataString[0] == "[SERVER balls]")//To create cells/orbs
                    {
                        makeOrbs(message);
                    }
                    else if (dataString[0] == "[OrbRemove]") //To delete cells/orbs
                    {
                        removeOrbs(message);
                        Console.WriteLine("Remove orbs function being called");
                    }
                    else if (dataString[0] == "[SERVERsendDataR]") //To delete player
                    {
                        if ((respawn == true))
                        {
                            //Thread.Sleep(1000);
                            deletePlayer(message);
                            Console.WriteLine("delete player");
                            respawn = false;
                        }
                    }
                    else if (dataString[0] == "[SERVER sendData]") // To create and update players
                    {
                        if(clientPlayerName == dataString[7])
                        {
                            updateClientPlayer(message);
                        }
                        if (!playersActive.Contains(dataString[7]))
                        {
                            createOtherPlayers(message);
                        }
                        else
                        {
                            moveOtherPLayers(message);
                        }
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("No messages recieved");
            }
        }

        //Function to update player size and score after collisions with cells and players
        private void updateClientPlayer(string message)
        {
            PictureBox clientPlayer;
            string[] dataString = message.Split(',');
            clientPlayer = new PictureBox();
            int index = playersActive.FindIndex(a => a.Contains(dataString[7]));
            clientPlayer = playerList[index];
            clientPlayer.Top = int.Parse(dataString[1]);
            clientPlayer.Left = int.Parse(dataString[2]);
            clientPlayer.Height = int.Parse(dataString[3]);
            clientPlayer.Width = int.Parse(dataString[4]);
            //string[] colorStr = dataString[5].Split(';');
            //clientPlayer.BackColor = Color.FromArgb(int.Parse(colorStr[0]), int.Parse(colorStr[1]), int.Parse(colorStr[2]));
            playerScore = int.Parse(dataString[6]);
            clientPlayerInfo = message;
            //clientPlayer.BringToFront();
        }

        //Function to delete player in the Game
        private void deletePlayer(string message)
        {
            Random rand = new Random();
            PictureBox clientPlayer;
            string[] dataString = message.Split(',');
            clientPlayer = new PictureBox();
            int index = playersActive.FindIndex(a => a.Contains(dataString[7]));
            clientPlayer = playerList[index];
            this.Controls.Remove(clientPlayer);
            
            //if statement to print you lost if client lost
            if(clientPlayerName == dataString[7])
            {
                Label gameOver = new Label();
                gameOver.Text = "You lost";
                gameOver.Location = new Point(350, 350);
                gameOver.Size = new Size(200, 200);
                gameOver.Font = new Font("Arial", 24, FontStyle.Regular);
                this.Controls.Add(gameOver);
                gameOver.BringToFront();
                //Thread.Sleep(1000000);
            }
        }

        //Function to create players in the Game
        private void createOtherPlayers(string message)
        {   
            PictureBox newPlayer;
            string[] dataString = message.Split(',');
            newPlayer = new PictureBox();
            //Label playerName = new Label();
            newPlayer.Top = int.Parse(dataString[1]);
            newPlayer.Left = int.Parse(dataString[2]);
            newPlayer.Height = int.Parse(dataString[3]);
            newPlayer.Width = int.Parse(dataString[4]);
            string[] colorStr = dataString[5].Split(';');
            newPlayer.BackColor = Color.FromArgb(int.Parse(colorStr[0]), int.Parse(colorStr[1]), int.Parse(colorStr[2]));
            playerScore = int.Parse(dataString[6]);
            newPlayer.Name = dataString[7];
            //playerName.Text = dataString[7];
            //playerName.Location = new Point(newPlayer.Top + 5, newPlayer.Left + 5);
            if (clientPlayerName == "")
            {
                playerScore = int.Parse(dataString[6]);
                clientPlayerName = dataString[7];
                clientPlayerInfo = message;
                //To add image to picture box
                //newPlayer.ImageLocation = @"C:\source\repos\GameTest\GameTest\bin\Image\player1.PNG"; //You will need to change the path
                //newPlayer.SizeMode = PictureBoxSizeMode.StretchImage;
                //Thread.Sleep(1000);
            }
            if (!playersActive.Contains(dataString[7]))
            {
                playerList.Add(newPlayer);
                playersActive.Add(dataString[7]);
                this.Controls.Add(newPlayer);
                //this.Controls.Add(playerName);
                newPlayer.BringToFront();
            }
            
        }

        //Function to update other players. It is called by checkServerMessage
        private void moveOtherPLayers(string message)
        {
            PictureBox otherPlayer;
            string[] dataString = message.Split(',');
            otherPlayer = new PictureBox();
            int index = playersActive.FindIndex(a => a.Contains(dataString[7]));
            otherPlayer = playerList[index];
            otherPlayer.Top = int.Parse(dataString[1]);
            otherPlayer.Left = int.Parse(dataString[2]);
            otherPlayer.Height = int.Parse(dataString[3]);
            otherPlayer.Width = int.Parse(dataString[4]);
            //string[] colorStr = dataString[5].Split(';');
            //otherPlayer.BackColor = Color.FromArgb(int.Parse(colorStr[0]), int.Parse(colorStr[1]), int.Parse(colorStr[2]));
            //otherPlayer.BringToFront();
        }

        //Function to render game window
        private void MainTimerEvent(object sender, EventArgs e)
        {
            checkServermessage();
            if (playerList.Count != 0)
            {
                PictureBox player = playerList[0];
                txtScore.Text = "Score: " + playerScore;
                movePlayer(player);
                sendPlayerData(player.Top, player.Left);
            }

        }

        //Function to move player
        private void movePlayer(PictureBox player)
        {
            if (goLeft && playerList[0].Left > 0)
            {
                playerList[0].Left -= playerSpeed;
                // Move the player LEFT
            }
            if (goRight && playerList[0].Left + playerList[0].Width < this.ClientSize.Width)
            {
                playerList[0].Left += playerSpeed;
                // Move the player RIGHT
            }
            if (goUp && playerList[0].Top > 0)
            {
                playerList[0].Top -= playerSpeed;
                // Move the player UP
            }
            if (goDown && playerList[0].Top + playerList[0].Height < this.ClientSize.Height)
            {
                playerList[0].Top += playerSpeed;
                //Move the player DOWN
            }
        }

        //Checking if key is pressed down
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Left)
            {
                goLeft = true;
            }
            if (e.KeyCode == Keys.Right)
            {
                goRight = true;
            }
            if (e.KeyCode == Keys.Up)
            {
                goUp = true;
            }
            if (e.KeyCode == Keys.Down)
            {
                goDown = true;
            }

        }

        //Checking if key is released
        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                goLeft = false;
            }
            if (e.KeyCode == Keys.Right)
            {
                goRight = false;
            }
            if (e.KeyCode == Keys.Up)
            {
                goUp = false;
            }
            if (e.KeyCode == Keys.Down)
            {
                goDown = false;
            }
        }

        //Function to make orbs
        private void makeOrbs(string message)
        {
            PictureBox newOrb;
            string[] dataString = message.Split(',');
            newOrb = new PictureBox();
            newOrb.Top = int.Parse(dataString[1]);
            newOrb.Left = int.Parse(dataString[2]);
            newOrb.Height = int.Parse(dataString[3]);
            newOrb.Width = int.Parse(dataString[4]);
            Console.WriteLine(dataString[5]);
            string[] colorStr = dataString[5].Split(';');
            newOrb.BackColor = Color.FromArgb(int.Parse(colorStr[0]), int.Parse(colorStr[1]), int.Parse(colorStr[2]));

            newOrb.Name = dataString[6];
            this.Controls.Add(newOrb);
            balls_orbs.Add(newOrb);
            orbsStrList.Add(dataString[6]);
        }

        //Function to remove orbs
        private void removeOrbs(string message)
        {
            try
            {
                PictureBox orbToRemove;
                string[] dataString = message.Split(',');
                orbToRemove = new PictureBox();
                int index = orbsStrList.FindIndex(a => a.Contains(dataString[7]));
                orbToRemove = balls_orbs[index];
                orbToRemove.BackColor = Color.FromArgb(192, 255, 255);
                //balls_orbs.Remove(orbToRemove);
                //orbsStrList.Remove(dataString[7]);
                //this.Controls.Remove(orbToRemove);
                Console.WriteLine("Orbs should be removed:" + dataString[7]);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //Function to send player's data to the server
        private void sendPlayerData(int top, int left)
        {
            string[] dataArray = clientPlayerInfo.Split(',');
            Console.WriteLine("clientInfo: "+clientPlayerInfo);
            dataArray[1] = top.ToString();
            dataArray[2] = left.ToString();
            string dataToSend = "";
            foreach(string str in dataArray)
            {
                dataToSend += str +",";
            }
            dataToSend.Remove(dataToSend.Length - 1,1);
            byte[] data1 = new byte[1024];
            data1 = Encoding.ASCII.GetBytes(dataToSend);
            Console.WriteLine("printing send:"+ dataToSend);
            server.Send(data1,0,data1.Length,0);
        }

        //Function to receive data from the server
        private string messageReceived()
        {   
            Console.WriteLine("function called for messageReceived");
            byte[] data2 = new byte[1024];
            string stringData;

            try
            {
                server.ReceiveTimeout = 150;
                Console.WriteLine("function called for messageReceived try block 1");
                int recv = server.Receive(data2,0,data2.Length,0);

                if (recv == 0)
                    return "";

                if (recv > 60)
                {
                    stringData = Encoding.ASCII.GetString(data2, 0, 65);
                }
                else
                {
                    stringData = Encoding.ASCII.GetString(data2, 0, recv);
                }
                Console.WriteLine(stringData);
                string dataFromServer = stringData;
                //dataFromServer = dataFromServer.Remove(recv); 
                Console.WriteLine("function called for messageReceived try block 2");

                return dataFromServer;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("function called for messageReceived catch block");
                return "";
            }catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }
            
        }
    }
}
