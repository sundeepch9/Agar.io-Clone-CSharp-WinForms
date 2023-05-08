using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class ThreadedTcpSrvr
{
    private TcpListener client;

    public ThreadedTcpSrvr()
    {
        client = new TcpListener(9050);
        client.Start();

        Console.WriteLine("Waiting for clients...");
        while (true)
        {
            while (!client.Pending())
            {
                Thread.Sleep(1000);
            }

            ConnectionThread newconnection = new ConnectionThread();
            newconnection.threadListener = this.client;
            Thread newthread = new Thread(new ThreadStart(newconnection.HandleConnection));
            newthread.Start();
        }
    }

    public static void Main()
    {
        ThreadedTcpSrvr server = new ThreadedTcpSrvr();
    }
}

class ConnectionThread
{
    public TcpListener threadListener;
    private static int connections = 0;
    private static List<string> players = new List<string>();
    private static int playerNo = 0;
    private static int playerCheckerNo = 0;
    private static List<string> orbsList = new List<string>();
    private static bool orbsFunc = true;
    private static List<string> orbsToRemove = new List<string>();
    private static bool respawn = true;

    public void HandleConnection()
    {
        //int recv;
        byte[] data = new byte[1024];

        TcpClient client = threadListener.AcceptTcpClient();
        NetworkStream ns = client.GetStream();
        connections++;
        int currentPlayerNo = playerNo;
        playerNo++;
        Console.WriteLine("[SERVER] New client accepted: {0} active connections", connections);
        //Send player data
        Random rand = new Random();

        //creating and sending cell/orb location and data
        string stringData = "";
         if (orbsFunc)
        {
            createOrbs();
            orbsFunc = false;
        }
        
        for(int j=0; j< orbsList.Count; j++)
        {
            Thread.Sleep(50);
            data = new byte[1024];
            string ballDataStr = orbsList[j];
            data = Encoding.ASCII.GetBytes(ballDataStr);
            ns.Write(data, 0, data.Length);
            Console.WriteLine(ballDataStr);
        }

        //Sending initial player data to spawn on client
        string sendData = "[SERVER sendData]," + rand.Next(0, 600) + "," + rand.Next(0, 800) + ",30,30," + rand.Next(0, 255) + ";" + rand.Next(0, 255) + ";" + rand.Next(0, 255) + ",30,player" + currentPlayerNo+",[End]";
        players.Add(sendData);
        playerCheckerNo = currentPlayerNo;
        data = Encoding.ASCII.GetBytes(sendData);
        Console.WriteLine(Encoding.ASCII.GetString(data, 0, data.Length));
        ns.Write(data, 0, data.Length);
        int n = 0;

        for(int k = 0; k < 5; k++)
        {
            byte[] dataResend = new byte[1024];
            string playersClient = players[currentPlayerNo];
            dataResend = Encoding.ASCII.GetBytes(playersClient);
            ns.Write(dataResend, 0, dataResend.Length);
            Console.WriteLine(Encoding.ASCII.GetString(dataResend, 0, dataResend.Length));
            Console.WriteLine(players.Count);
        }

        try
        {
            while (true)
            {
                ballCollision(currentPlayerNo); //Checking for cell collisions
                
                bool collided = playerCollision(currentPlayerNo); //checking for player collisions
                
                if ((n < orbsToRemove.Count) || (collided)) //If the player collided with cells or player, then send the updated player data
                {
                    byte[] data1 = new byte[1024];
                    string playersClientStr = players[currentPlayerNo];
                    data1 = Encoding.ASCII.GetBytes(playersClientStr);
                    ns.Write(data1, 0, data1.Length);
                    Console.WriteLine(Encoding.ASCII.GetString(data1, 0, data1.Length));
                    Console.WriteLine(players.Count);
                }
                
                //Sending data to remove cells that were absorbed by the player
                if (orbsToRemove.Count > 0)
                {
                    while(n < orbsToRemove.Count)
                    {
                        Thread.Sleep(150);
                        byte[] data0 = new byte[1024];
                        string orbsDataStr = "[OrbRemove]," + orbsToRemove[n];
                        data0 = Encoding.ASCII.GetBytes(orbsDataStr);
                        ns.Write(data0, 0, data0.Length);
                        Console.WriteLine("sending to remove orbs:" + orbsDataStr);
                        //orbsToRemove.Remove(orb);// error here
                        n++;
                    }
                }

                //Sending other players data to spawn and move them on client
                if (players.Count >= 1)
                {
                    for (int i = 0; i < players.Count; i++) //To create picture boxes of players
                    {
                        if (i != currentPlayerNo)
                        {
                            Thread.Sleep(150);
                            byte[] data2 = new byte[1024];
                            string playersDataStr = players[i];
                            data2 = Encoding.ASCII.GetBytes(playersDataStr);
                            ns.Write(data2, 0, data2.Length);
                            Console.WriteLine("From other players loop:"+playersDataStr);
                        }
                        else
                        {
                            Console.WriteLine("Printed other players");
                        }
                    }
                }
                
                //To receive movements of player and save in players list

                byte[] data3 = new byte[1024];
               
                Console.WriteLine("Receiving data");
                int recv = ns.Read(data3, 0, data3.Length);

                if (recv == 0)
                    break;

                stringData = Encoding.ASCII.GetString(data3, 0, recv);
                    //Thread.Sleep(50);
                    
                if (recv > 70)
                {
                    stringData = stringData.Substring(0, 65);
                    string[] dataArray1 = stringData.Split(',');

                    string playerStr = players[currentPlayerNo];
                    string[] dataArray2 = playerStr.Split(',');
                    dataArray2[1] = dataArray1[1];
                    dataArray2[2] = dataArray1[2];
                    string dataToUpdate = "";
                    foreach (string str in dataArray2)
                    {
                        dataToUpdate += str + ",";
                    }
                    dataToUpdate = dataToUpdate + "eeeeeeeeeee";
                    dataToUpdate = dataToUpdate.Substring(0, 60);
                    players[currentPlayerNo] = dataToUpdate;
                }
                else if(recv < 70)
                {
                    //players[currentPlayerNo] = stringData;
                    string[] dataArray1 = stringData.Split(',');

                    string playerStr = players[currentPlayerNo];
                    string[] dataArray2 = playerStr.Split(',');
                    dataArray2[1] = dataArray1[1];
                    dataArray2[2] = dataArray1[2];
                    string dataToUpdate = "";
                    foreach (string str in dataArray2)
                    {
                        dataToUpdate += str + ",";
                    }
                    dataToUpdate.Remove(dataToUpdate.Length - 1, 1);
                    players[currentPlayerNo] = dataToUpdate;

                }
                Console.WriteLine(players[currentPlayerNo]);
                Console.WriteLine("Received data");
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.ToString());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        
    ns.Close();
    client.Close();
    connections--;
    players.Remove(stringData);
    playerNo--;
    Console.WriteLine("Client disconnected: {0} active connections", connections);
    }

    //Checking player collisions
    private bool playerCollision(int playerNum)
    {
        bool collided = false;
        if (players.Count > 1)
        {
            int playerX = getX(players[playerNum]);
            int playerY = getY(players[playerNum]);
            int playerScore = getScore(players[playerNum]);
            for (int i = 0; i < players.Count; i++)
            {
                if(i != playerNo)
                {
                    int player2X = getX(players[i]);
                    int player2Y = getY(players[i]);
                    int player2Score = getScore(players[i]);
                    double dis = Math.Sqrt(Math.Pow((playerX - player2X), 2) + Math.Pow((playerY - player2Y), 2));
                    if ((dis <= playerScore - player2Score)&&(playerScore > player2Score))
                    {
                        Console.WriteLine("Player collision against other players > ");
                        string playerStr = players[playerNum];
                        string[] dataArray = playerStr.Split(',');
                        int playerH = int.Parse(dataArray[3]);
                        int playerW = int.Parse(dataArray[4]);
                        if (playerScore < 100)
                        {
                            playerH = playerH + 20;
                            playerW = playerW + 20;
                            playerScore = playerScore + 20;
                        }
                        dataArray[3] = playerH.ToString();
                        dataArray[4] = playerW.ToString();
                        dataArray[6] = playerScore.ToString();
                        string dataToUpdate = "";
                        foreach (string str in dataArray)
                        {
                            dataToUpdate += str + ",";
                        }
                        dataToUpdate.Remove(dataToUpdate.Length - 1, 1);
                        Console.WriteLine("> player collision: "+dataToUpdate);
                        players[playerNum] = dataToUpdate;

                        Console.WriteLine("Player collision against other players < ");
                        Random rand = new Random();
                        string otherPlayerStr = players[i];
                        string[] dataArrayOther = otherPlayerStr.Split(',');
                        dataArrayOther[0] = "[SERVERsendDataR]";
                        dataArrayOther[1] = rand.Next(0, 700).ToString();
                        dataArrayOther[2] = rand.Next(0, 700).ToString();
                        dataArrayOther[3] = "30";
                        dataArrayOther[4] = "30";
                        dataArrayOther[6] = "30";
                        string dataToUpdateOther = "";
                        foreach (string str in dataArrayOther)
                        {
                            dataToUpdateOther += str + ",";
                        }
                        dataToUpdateOther.Remove(dataToUpdateOther.Length - 1, 1);
                        Console.WriteLine("< player collision: " + dataToUpdateOther);
                        players[i] = dataToUpdateOther;
                        //Thread.Sleep(1000);
                        collided = true;
                    }
                }
            }
        }
        return collided;
    }

    //Checking cell/orb collisions
    private void ballCollision(int playerNum)
    {
        try
        {
            string orbToBeRemoved = "";
            foreach (string orb in orbsList)
            {
                int ballX = getX(orb);
                int ballY = getY(orb);

                int playerX = getX(players[playerNum]);
                int playerY = getY(players[playerNum]);
                int playerScore = getScore(players[playerNum]);

                double dis = Math.Sqrt(Math.Pow((playerX - ballX), 2) + Math.Pow((playerY - ballY), 2));
                if (dis <= playerScore)
                {
                    string playerStr = players[playerNum];
                    string[] dataArray = playerStr.Split(',');
                    int playerH = int.Parse(dataArray[3]);
                    int playerW = int.Parse(dataArray[4]);
                    playerH = playerH + 2;
                    playerW = playerW + 2;
                    playerScore = playerScore + 2;
                    dataArray[3] = playerH.ToString();
                    dataArray[4] = playerW.ToString();
                    dataArray[6] = playerScore.ToString();
                    string dataToUpdate = "";
                    foreach (string str in dataArray)
                    {
                        dataToUpdate += str + ",";
                    }
                    dataToUpdate.Remove(dataToUpdate.Length - 1, 1);
                    players[playerNum] = dataToUpdate;
                    orbToBeRemoved = orb;
                    orbsToRemove.Add(orbToBeRemoved);
                }
            }
            if (orbToBeRemoved != "")
            {
                orbsList.Remove(orbToBeRemoved);
            }
            Console.WriteLine("looped");
        }
        catch(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
            
    }

    //creating cells/orbs
    private void createOrbs()
    {
        Random rand = new Random();
        string orbs = "";
        for(int i = 0;i < 50;i++)
        {
            orbs = "[SERVER balls]," +rand.Next(0, 600) +","+rand.Next(0, 800) +",7,7," + rand.Next(0,255) +";" + rand.Next(0,255) +";" + rand.Next(0,255) +","+i+",[End]"; //[],top,left,height,width,color,no
            orbsList.Add(orbs);
        }

    }

    private int getX(string message)
    {
        string[] dataStr = message.Split(',');
        int top = int.Parse(dataStr[1]);
        int height = int.Parse(dataStr[3]);
        int playerX = (top + (top + height)) / 2;
        return playerX;
    }
    private int getY(string message)
    {
        string[] dataStr = message.Split(',');
        int left = int.Parse(dataStr[2]);
        int width = int.Parse(dataStr[4]);
        int playerY = (left + (left + width)) / 2;
        return playerY;
    }

    private int getScore(string message)
    {
        string[] dataStr = message.Split(',');
        int playerScore = int.Parse(dataStr[6]);
        return playerScore;
    }
}