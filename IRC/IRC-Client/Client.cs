﻿using IRC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using static IRC_Client.Intermediate;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;

namespace IRC_Client
{
    public class Client
    {
        int svPort;
        public IServer svProxy;
        public User myUser;
        public ArrayList usersList;
        bool loggedIn = false;
        Form1 chat;

        public Dictionary<string, IClient> connected = new Dictionary<string, IClient>();

        public event MessageReceived testEvent;

        public int TEST = -1;

        public Client(int svPort, int cliPort)
        {
            this.svPort = svPort;
            this.cliPort = cliPort;
            
            setupConfig();
            instance = this;
            
        }

        public void setForm(Form1 form) {
            chat = form;
        }

        public void ReceiveMessage(Intermediate.Message msg)
        {
            MessageReceived listener = null;
            Delegate[] dels = testEvent.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (MessageReceived)del;
                    listener.Invoke(msg);
                }
                catch (Exception ex)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    testEvent -= listener;
                }
            }
        }

        public void setupConfig()
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 0;

            // Create the channel.
            TcpChannel clientChannel = new TcpChannel(props, null, provider);

            // Register the channel.
            ChannelServices.RegisterChannel(clientChannel, false);

            // Create an instance of the remote object.
            svProxy = (IServer)Activator.GetObject(typeof(IServer),
                "tcp://localhost:" + svPort + "/Server");         
        }

        public bool logIn(string nickname, string password)
        {            
            Console.WriteLine("<LOG IN> The client is invoking the remote object.");

            //TODO access to this client peercomunication service address / port
            string address = "TODO:HARDCODED";
            int port = -1;
            bool loggedIn;

            loggedIn = svProxy.logIn(nickname, password, address, port);

            //TODO - HANDLE FORM ON LOGGIN ERROR
            if (!loggedIn)
                return false;

            usersList = svProxy.getUsersList();

            foreach (User us in usersList)
            {
                if (us.nickname.Equals(nickname))
                {
                    myUser = us;
                    break;
                }
            }

            //Console.WriteLine("Log in result: " + users.ToString());
            return true;
        }
		
//TODO merge login methods
		/*
		public void logIn(string nickname, string password)
        {  // Invoke a method on the remote object.
            Console.WriteLine("<LOG IN> The client is invoking the remote object.");
            Console.WriteLine("Log in result: " +  svProxy.logIn(nickname, password));

            //initialize remote client object

            /*// Create the channel.
            TcpChannel channel = new TcpChannel();

            // Register the channel.
            ChannelServices.RegisterChannel(channel, false);*/

            // Register as client for remote object.
            WellKnownClientTypeEntry remoteType = new WellKnownClientTypeEntry(
                typeof(IClient), "tcp://localhost:" + cliPort + "/ClientRemote");
            RemotingConfiguration.RegisterWellKnownClientType(remoteType);
            Console.WriteLine("Port " + cliPort);
            /*
            // Create a message sink.
            string objectUri;
            System.Runtime.Remoting.Messaging.IMessageSink messageSink =
                channel.CreateMessageSink(
                    "tcp://localhost:" + cliPort + "/ClientRemote", null,
                    out objectUri);
            Console.WriteLine("The URI of the message sink is {0}.",
                objectUri);
            if (messageSink != null)
            {
                Console.WriteLine("The type of the message sink is {0}.",
                    messageSink.GetType().ToString());
            }

            // Create an instance of the remote object.
            cliProxy = (IClient)Activator.GetObject(typeof(IClient),
                "tcp://localhost:" + cliPort + "/ClientRemote");
             */

            //cliProxy.ReceiveMessage("pasteis", "cenas", DateTime.Now);
           
            
            
            //falta a verificacao
            loggedIn = true;


            //chat.changeForm();
            /*
            if (TEST != -1)
            {
                connectChat(TEST);

                //cliProxy.ReceiveMessage("pasteis", "cenas", DateTime.Now);
            }
            */
            //chat.Show();
            

        } */



        internal void removeUserFromList(User us)
        {
            usersList.Remove(us);
        }

        internal void addUserToList(User us)
        {
            usersList.Add(us);
        }

        public bool signUp(string username, string nickname, string password)
        {  // Invoke a method on the remote object.
            Console.WriteLine("<CLI - SIGN UP> The client is invoking the remote object.");

            return svProxy.signUp(username, nickname, password);
        }
        
        public void logOut()
        {
            svProxy.logOut(myUser);
            usersList = null;
        }

        public void connectChat(int port) {
            Console.WriteLine("CONECTING");
            IClient cliProxy = (IClient)Activator.GetObject(typeof(IClient),
                    "tcp://localhost:" + port + "/ClientRemote");
            connected.Add(port.ToString(), cliProxy);
            Console.WriteLine("CONECTED");
            chat.AddTab(port.ToString());
            //Console.WriteLine("DEVIA CRIAR UMA TAB");
        }

        public void connectChat(int port, Intermediate.Message msg) {
            IClient cliProxy = (IClient)Activator.GetObject(typeof(IClient),
                    "tcp://localhost:" + port + "/ClientRemote");
            connected.Add(port.ToString(), cliProxy);
            chat.AddTab(port.ToString(), msg);
        }

        public void sendMessage(string sender, string msg, string receiver)
        {
            Console.WriteLine("PASTEISSSSSSSSSS");
            IClient proxy = connected[receiver];
            proxy.ReceiveMessage(cliPort.ToString(), msg, DateTime.Now);
        }

        

    }
}
