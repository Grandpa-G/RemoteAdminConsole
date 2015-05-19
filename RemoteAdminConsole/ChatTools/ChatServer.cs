using System.IO;
using System.Net;
using System;
using System.Threading;
using Chat = System.Net;
using System.Collections;
//*****************************************************************************************
//                           LICENSE INFORMATION
//*****************************************************************************************
//   PC_Chat 1.0.0.0
//   Creates a basic basic server/client chat application in C#
//
//   Copyright (C) 2007  
//   Richard L. McCutchen 
//   Email: richard@psychocoder.net
//   Created: 16SEP07
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
//*****************************************************************************************
namespace RemoteAdminConsole
{
    class ChatServer
    {

        System.Net.Sockets.TcpListener chatServer;
        public static Hashtable nickName;
        public static Hashtable nickNameByConnect;

        public ChatServer()
        {
            //create our nickname and nickname by connection variables
            nickName = new Hashtable(100);
            nickNameByConnect = new Hashtable(100);
            //create our TCPListener object
            chatServer = new System.Net.Sockets.TcpListener(new IPEndPoint(IPAddress.Any, 23));
            //check to see if the server is running
            //while (true) do the commands
            while (true) 
            {
                //start the chat server
                chatServer.Start();
                //check if there are any pending connection requests
                if (chatServer.Pending())
                {
                    //if there are pending requests create a new connection
                    Chat.Sockets.TcpClient chatConnection = chatServer.AcceptTcpClient();
                    //display a message letting the user know they're connected
                    Console.WriteLine("You are now connected");
                    //create a new DoCommunicate Object
                    DoCommunicate comm = new DoCommunicate(chatConnection);
                }
            }
        }

        public static void SendMsgToAll(string nick, string msg)
        {
            //create a StreamWriter Object
            StreamWriter writer;
            ArrayList ToRemove = new ArrayList(0);
            //create a new TCPClient Array
            Chat.Sockets.TcpClient[] tcpClient = new Chat.Sockets.TcpClient[ChatServer.nickName.Count];
            //copy the users nickname to the CHatServer values
            ChatServer.nickName.Values.CopyTo(tcpClient, 0);
            //loop through and write any messages to the window
            for (int cnt = 0; cnt < tcpClient.Length; cnt++)
            {
                try
                {
                    //check if the message is empty, of the particular
                    //index of out array is null, if it is then continue
                    if (msg.Trim() == "" || tcpClient[cnt] == null)
                        continue;
                    //Use the GetStream method to get the current memory
                    //stream for this index of our TCPClient array
                    writer = new StreamWriter(tcpClient[cnt].GetStream());
                    //white our message to the window
                    writer.WriteLine(nick + ": " + msg);
                    //make sure all bytes are written
                    writer.Flush();
                    //dispose of the writer object until needed again
                    writer = null;
                }
                    //here we catch an exception that happens
                    //when the user leaves the chatroow
                catch (Exception e44)
                {
                    e44 = e44;
                    string str = (string)ChatServer.nickNameByConnect[tcpClient[cnt]];
                    //send the message that the user has left
                    ChatServer.SendSystemMessage("** " + str + " ** Has Left The Room.");
                    //remove the nickname from the list
                    ChatServer.nickName.Remove(str);
                    //remove that index of the array, thus freeing it up
                    //for another user
                    ChatServer.nickNameByConnect.Remove(tcpClient[cnt]);
                }
            }
        }

        public static void SendSystemMessage(string msg)
        {
            //create our StreamWriter object
            StreamWriter writer;
            ArrayList ToRemove = new ArrayList(0);
            //create our TcpClient array
            Chat.Sockets.TcpClient[] tcpClient = new Chat.Sockets.TcpClient[ChatServer.nickName.Count];
            //copy the nickname value to the chat servers list
            ChatServer.nickName.Values.CopyTo(tcpClient, 0);
            //loop through and write any messages to the window
            for (int i = 0; i < tcpClient.Length; i++)
            {
                try
                {
                    //check if the message is empty, of the particular
                    //index of out array is null, if it is then continue
                    if (msg.Trim() == "" || tcpClient[i] == null)
                        continue;
                    //Use the GetStream method to get the current memory
                    //stream for this index of our TCPClient array
                    writer = new StreamWriter(tcpClient[i].GetStream());
                    //send our message
                    writer.WriteLine(msg);
                    //make sure the buffer is empty
                    writer.Flush();
                    //dispose of our writer
                    writer = null;
                }
                catch (Exception e44)
                {
                    e44 = e44;
                    ChatServer.nickName.Remove(ChatServer.nickNameByConnect[tcpClient[i]]);
                    ChatServer.nickNameByConnect.Remove(tcpClient[i]);
                }
            }
        }
    }//end of class ChatServer
}
