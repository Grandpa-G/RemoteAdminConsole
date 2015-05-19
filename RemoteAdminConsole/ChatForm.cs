using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Reflection;

namespace RemoteAdminConsole
{
    public partial class ChatForm : Form
    {
        private static string chatWith = "";
        private static string chatWithIndex = "";

        byte[] m_dataBuffer = new byte[10];
        IAsyncResult m_result;
        public AsyncCallback m_pfnCallBack;
        public Socket m_clientSocket;
        private event AddMessage m_AddMessage;
        public ChatForm(string with, string index)
        {
            // Add Message Event handler for Form decoupling from input thread
            m_AddMessage = new AddMessage(OnAddMessage);

            InitializeComponent();
            chatWith = with;
            chatWithIndex = index;
            chatformChatWith.Text = with;
        }
        public void OnAddMessage(string sMessage)
        {
            // Thread safe operation here
            DateTime now = DateTime.Now;
            chatformList.Items.Add(now.ToString("t") + " " + sMessage);
            chatformList.SelectedIndex = chatformList.Items.Count - 1;
            chatformList.ClearSelected();
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            Connect();
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string chat = string.Format("`9:`{0}", chatWithIndex);
            if (SendMessage(chat))
            {
                chatformText.Text = "";
                UpdateControls(false);
            }
         }
 
       private void chatformText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                chatformSend.PerformClick();
            }
        }
        private void chatformClear_Click(object sender, EventArgs e)
        {
            chatformText.Text = "";
            chatformList.Items.Clear();
        }
        private void chatformSubmit_Click(object sender, EventArgs e)
        {
            string chat = string.Format("`0:{0}:{1}", chatWithIndex.PadLeft(4, '0'), chatformText.Text);
             string message = "`8:``" + chatformText.Text;
            if (SendMessage(chat))
            {
                //            chatList.Items.Add("To->All: " + chatText.Text);
                chatformText.Text = "";
            }
            SendMessage(message);
        }
        private void Connect()
        {
            try
            {
                UpdateControls(false);
                // Create the socket instance
                m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(GUIMain.ipServer, GUIMain.chatPort);
                // Connect to the remote host
                m_clientSocket.Connect(ipEnd);
                if (m_clientSocket.Connected)
                {
                    UpdateControls(true);
                    //Wait for data asynchronously 
                    WaitForData();
                    string chat = string.Format("`0:{0}:{1}", chatWithIndex.PadLeft(4, '0'), "Requesting private chat: use /rc <reply> to respond.");
                    SendMessage(chat);
                }
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                Console.WriteLine(str);
                UpdateControls(false);
            }
        }
        bool SendMessage(string msg)
        {
            if (m_clientSocket == null || !m_clientSocket.Connected)
                return false;

            try
            {
                // New code to send strings
                NetworkStream networkStream = new NetworkStream(m_clientSocket);
                System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(networkStream);
                streamWriter.Write(msg);
                streamWriter.Flush();
 //               Console.WriteLine("Sent>" + msg);

                /* Use the following code to send bytes
                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString ());
                if(m_clientSocket != null){
                    m_clientSocket.Send (byData);
                }
                */
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                return false;
            }
            return true;
        }
        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = m_clientSocket;
                // Start listening to the data asynchronously
                m_result = m_clientSocket.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, m_pfnCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }

        }
        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[1024];
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String sRecieved = new System.String(chars);

                string action = sRecieved.Substring(0, 3);
                string message = "";
                if (sRecieved.Length > 8)
                    message = sRecieved.Substring(8);
                 string[] s = sRecieved.Split('`');
                switch (action)
                {
                    case "`0:":
                        break;
                    case "`1:":
                        Invoke(m_AddMessage, new string[] { "Left" });
                        this.Invoke((MethodInvoker)delegate
                        {
                            // close the form on the forms thread
                            this.Close();
                        });
                        break;
                    case "`3:":
                        string[] channelEndPoint = s[3].Split('|');
                        Invoke(m_AddMessage, new string[] { s[3] });
//                        string header = "`4:`" + s[2] + "`" + channelEndPoint[1];
                        string header = "`4:`" + chatWithIndex + "`" + channelEndPoint[1];
                        SendMessage(header);

                        break;

                    default:
                        Invoke(m_AddMessage, new string[] { sRecieved });
                        break;
                }
                Console.WriteLine(sRecieved);
                WaitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
//                chatformStatus.Text = se.Message + " Server Connect failed!";
            }
        }
        private void UpdateControls(bool connected)
        {

            string connectStatus = connected ? "Connected" : "Not Connected";
            Console.WriteLine(connectStatus);
            chatformStatus.Text = connectStatus;
        }
        void CloseConnection()
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
            }
        }

        void DisconnectConnection()
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
                UpdateControls(false);
            }
        }


 
        /*
        private event AddMessage m_AddMessage;
        public ChatForm(string with, string index)
        {
            // Add Message Event handler for Form decoupling from input thread
            m_AddMessage = new AddMessage(OnAddMessage);

            InitializeComponent();
            chatWith = with;
            chatWithIndex = index;
            chatformChatWith.Text = with;
        }
        public void OnAddMessage(string sMessage)
        {
            // Thread safe operation here
            chatformList.Items.Add(sMessage);
        }

        private void ChatForm_Load(object sender, EventArgs e)
        {
            Cursor cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                // Close the socket if it is still open
                if (m_sock != null && m_sock.Connected)
                {
                    m_sock.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(10);
                    m_sock.Close();
                }

                // Create the socket object
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint epServer = new IPEndPoint(GUIMain.ipServer, GUIMain.chatPort);

                // Connect to server non-Blocking method
                m_sock.Blocking = false;
                AsyncCallback onconnect = new AsyncCallback(OnConnect);
                m_sock.BeginConnect(epServer, onconnect, m_sock);
                chatformStatus.Text = "Chat Connected";
                chatformSend.Enabled = true;
            }
            catch (Exception ex)
            {
                chatformStatus.Text = ex.Message + " Server Connect failed!";
                chatformSend.Enabled = false;
            }
            Cursor.Current = cursor;

        }
        private void chatformText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                chatformSend.PerformClick();
            }
        }
        private void chatformClear_Click(object sender, EventArgs e)
        {
            chatformText.Text = "";
            chatformList.Items.Clear();
        }

        private Socket m_sock = null;
        private void chatformSubmit_Click(object sender, EventArgs e)
        {
            string me = "Me-> ";
            chatformList.Items.Add(me + chatformText.Text);
            string chat = string.Format("`0:{0}:{1}", chatWithIndex.PadLeft(4, '0'), chatformText.Text);
            Send(chat);
            chatformText.Text = "";
        }
        static Socket sock;

        public void OnConnect(IAsyncResult ar)
        {
            // Socket was the passed in object
            sock = (Socket)ar.AsyncState;

            // Check if we were sucessfull
            try
            {
                //    sock.EndConnect( ar );
                if (sock.Connected)
                    SetupRecieveCallback(sock);
                else
                    Console.WriteLine("Unable to connect to remote machine", "Connect Failed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Unusual error during Connect!");
            }
        }
        private byte[] m_byBuff = new byte[256];    // Recieved data buffer
        public void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Setup Recieve Callback failed!");
            }
        }
        public void OnRecievedData(IAsyncResult ar)
        {
            if (ar == null)
                Console.WriteLine("ar null");
            // Socket was the passed in object
            if ((Socket)ar.AsyncState == null)
                Console.WriteLine("sock null");
            Socket sock = (Socket)ar.AsyncState;


            // Check if we got any data
            try
            {
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    // Wrote the data to the List
                    string sRecieved = Encoding.ASCII.GetString(m_byBuff, 0, nBytesRec);
                    // Get the constructor and create an instance of MagicClass
                    if (sRecieved.StartsWith("`1:")) { }
                    //                       Invoke(m_RefreshChatPlayers, new string[] { sRecieved });
                    else
                        Invoke(m_AddMessage, new string[] { sRecieved });

                    // If the connection is still usable restablish the callback
                    SetupRecieveCallback(sock);
                }
                else
                {
                    if (sock == null)
                        Console.WriteLine("shut 1");
                    // If no data was recieved then the connection is probably dead
                    Console.WriteLine("Client {0}, disconnected", sock.RemoteEndPoint);
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Unusual error during Recieve!");
            }
        }

        public void Send(String data)
        {
            if (sock == null)
                Console.WriteLine("send 1");
            // Convert to byte array and send.
            Byte[] byteDateLine = System.Text.Encoding.ASCII.GetBytes(data.ToCharArray());
            sock.Send(byteDateLine, byteDateLine.Length, 0);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            if (ar == null)
                Console.WriteLine("xar null");
            // Socket was the passed in object
            if ((Socket)ar.AsyncState == null)
                Console.WriteLine("xsock null");
            try
            {
                // Retrieve the socket from the state object.
                Socket sock = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = sock.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

*/
    }
}
