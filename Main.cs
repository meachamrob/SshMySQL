//author: brian liceaga
//Connects via SSH to a remote server & MySQL to run queries

using System;
using System.Net;
using System.Data;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Configuration;
using System.IO;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Channels;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Renci.SshNet
{
  class MainClass
	{
		public static void ShowLocalIP (string[] args)
		{
			string name = (args.Length < 1) ? Dns.GetHostName() : args[0];
			try{
				IPAddress[] localIP = Dns.GetHostEntry(name).AddressList;
				foreach(IPAddress address in localIP) 
					Console.WriteLine("Localhost:  {0}/{1}",name,address);
			} catch(Exception e){
				Console.WriteLine(e.Message);
			}
		}
		
		public static string GetMACAddress()
		{
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
			String MacAddress = string.Empty;
			foreach (NetworkInterface adapter in nics)
			{
				if (MacAddress == String.Empty)// only return MAC Address from first card  
				{
					MacAddress = adapter.GetPhysicalAddress().ToString();
				}
			} 
			return MacAddress;
		}
		  
		public static void mysqlConnection ()
		{

			string connStr = "server=localhost;database=database1;uid=root;pwd=password;"; // MYSQL INFORMATION
			MySqlConnection mysqlConn = new MySqlConnection(connStr); //fill in db info

			try {

				Console.WriteLine("Connecting to MySQL...");
				mysqlConn.Open();
				Console.WriteLine ("MySql database connection activated");
				string query = "INSERT INTO database1(dataSequence) VALUES('AGTTTTTTC')"; //Create desired query 
				MySqlCommand cmd = new MySqlCommand(query, mysqlConn);
				cmd.ExecuteNonQuery();
				Console.WriteLine("Query Successful");
				 
			} catch (SocketException ex) {
				Console.WriteLine(ex.ToString());
			} finally {
				mysqlConn.Close();
			}
		}

		public static void Main (string[] args)
		{
			ShowLocalIP(args);
			string LocalMacAddress = GetMACAddress();
			Console.WriteLine("Local MAC Address: {0}", LocalMacAddress);
	
			string host = "50.52.219.135";      //ENTER SERVER HOST
			string user = "root";             //ENTER USER
			Console.Write("Password: "); 	  //ENTER SERVER PASSWORD
			string passwd = string.Empty;
		
			ConsoleKeyInfo keyInfo = Console.ReadKey(true); 
			while (keyInfo.Key != ConsoleKey.Enter) { 
				Console.Write(""); 
				passwd += keyInfo.KeyChar; 
				keyInfo = Console.ReadKey(true); 
			} 
			Console.WriteLine();

			PasswordConnectionInfo connectionInfo = new PasswordConnectionInfo(host,user, passwd);

			using (var sshClient = new SshClient(connectionInfo))
			{
				try 
				{
					sshClient.Connect();
					if (sshClient.IsConnected){
						Console.WriteLine("Ssh connection sucessful");
						uint sqlPort = 3306;
						var portFwd = new ForwardedPortLocal("127.0.0.1",sqlPort, "localhost", sqlPort);
						sshClient.AddForwardedPort(portFwd);
						portFwd.Start();
						if(portFwd.IsStarted) {
							Console.WriteLine("Port Forwarding has started.");
						}
						else {
							Console.WriteLine("Port Forwarding has failed.");
						}
						mysqlConnection(); //Begin mysql connection 

					} 
					else {
						Console.WriteLine("Ssh connection failed");
					}
				
				
					sshClient.Disconnect(); 
					Console.WriteLine("Ssh connection closed.");
				
				}
				catch (SocketException e){
					Console.WriteLine(e.Message);
	
				}
			}
		}
	}
}
