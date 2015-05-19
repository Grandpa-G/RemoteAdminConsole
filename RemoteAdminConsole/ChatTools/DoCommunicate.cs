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
    class DoCommunicate
    {
        Chat.Sockets.TcpClient client;
        System.IO.StreamReader reader;
        System.IO.StreamWriter writer;
        string nickName;

        public DoCommunicate(System.Net.Sockets.TcpClient tcpClient)
        {
            //create our TcpClient
            client = tcpClient;
            //create a new thread
            Thread chatThread = new Thread(new ThreadStart(startChat));
            //start the new thread
            chatThread.Start();
        }

        private string GetNick()
        {
            //ask the user what nickname they want to use
            writer.WriteLine("What is your nickname? ");
            //ensure the buffer is empty
            writer.Flush();
            //return the value the user provided
            return reader.ReadLine();
        }

        private void runChat()
            //use a try...catch to catch any exceptions
        {
            try
            {
                //set out line variable to an empty string
                string line = "";
                while (true)
                {
                    //read the curent line
                    line = reader.ReadLine();
                    //send our message
                    ChatServer.SendMsgToAll(nickName, line);
                }
            }
            catch (Exception e44) 
            { 
                Console.WriteLine(e44); 
            }
        }

        private void startChat()
        {
            //create our StreamReader object to read the current stream
            reader = new System.IO.StreamReader(client.GetStream());
            //create our StreamWriter objec to write to the current stream
            writer = new System.IO.StreamWriter(client.GetStream());
            writer.WriteLine("Welcome to PCChat!");
            //retrieve the users nickname they provided
            nickName = GetNick();
            //check is the nickname is already in session
            //prompt the user until they provide a nickname not in use
            while (ChatServer.nickName.Contains(nickName))
            {
                //since the nickname is in use we display that message,
                //then prompt them again for a nickname
                writer.WriteLine("ERROR - Nickname already exists! Please try a new one");
                nickName = GetNick();
            }
            //add their nickname to the chat server
            ChatServer.nickName.Add(nickName, client);
            ChatServer.nickNameByConnect.Add(client, nickName);
            //send a system message letting the other user
            //know that a new user has joined the chat
            ChatServer.SendSystemMessage("** " + nickName + " ** Has joined the room");
            writer.WriteLine("Now Talking.....\r\n-------------------------------");
            //ensure the buffer is empty
            writer.Flush();
            //create a new thread for this user
            Thread chatThread = new Thread(new ThreadStart(runChat));
            //start the thread
            chatThread.Start();
        }
    }
}

