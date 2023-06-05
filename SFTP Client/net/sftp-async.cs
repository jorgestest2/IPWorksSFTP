/*
 * IPWorks SFTP 2022 .NET Edition - Sample Project
 *
 * This sample project demonstrates the usage of IPWorks SFTP in a 
 * simple, straightforward way. This is not intended to be a complete 
 * application. Error handling and other checks are simplified for clarity.
 *
 * Copyright (c) 2023 /n software inc. www.nsoftware.com
 */

using System.Collections.Generic;
﻿using System;
using System.Threading.Tasks;
using nsoftware.async.IPWorksSFTP;

class sftpDemo
{
  private static Sftp sftp = new nsoftware.async.IPWorksSFTP.Sftp();

  static async Task Main(string[] args)
  {
    if (args.Length < 6)
    {
      Console.WriteLine("usage: sftp /s hostserver /u user /p password\n");
      Console.WriteLine("  hostserver   the host sftp server to connect to");
      Console.WriteLine("  user         the username to use for authentication");
      Console.WriteLine("  password     the password to use for authentication");
      Console.WriteLine("\nExample: sftp /s 192.168.1.2 /u myusername /p mypassword\n");
    }
    else
    {
      sftp.OnTransfer += sftp_OnTransfer;
      sftp.OnSSHServerAuthentication += sftp_OnSSHServerAuthentication;
      sftp.OnDirList += sftp_OnDirList;
      sftp.OnDisconnected += sftp_OnDisconnected;

      try
      {
        Dictionary<string, string> myArgs = ConsoleDemo.ParseArgs(args);

        sftp.SSHAuthMode = SftpSSHAuthModes.amPassword;
        sftp.SSHHost = myArgs["s"];
        sftp.SSHUser = myArgs["u"];
        sftp.SSHPassword = myArgs["p"];

        // Default port for SFTP is 22.
        await sftp.SSHLogon(sftp.SSHHost, 22);

        Console.WriteLine("Type \"?\" for a list of commands.");
        Console.Write("sftp> ");
        string command;
        string[] arguments;
        while (true)
        {
          command = Console.ReadLine();
          arguments = command.Split();

          if (arguments[0].Equals("?") || arguments[0].Equals("help"))
          {
            Console.WriteLine("?       cd      ls      pwd");
            Console.WriteLine("get     put     rm      mkdir");
            Console.WriteLine("rmdir   mv      exit");
          }
          else if (arguments[0].Equals("quit") || arguments[0].Equals("exit"))
          {
            await sftp.SSHLogoff();
            break;
          }
          else if (arguments[0].Equals("cd"))
          {
            if (arguments.Length > 1) await sftp.ChangeRemotePath(arguments[1]);
          }
          else if (arguments[0].Equals("get"))
          {
            if (arguments.Length > 1)
            {
              sftp.RemoteFile = arguments[1];
              sftp.LocalFile = arguments[1];
              await sftp.Download();
              Console.WriteLine("File downloaded");
            }
          }
          else if (arguments[0].Equals("ls"))
          {
            if (arguments.Length > 1)
            {
              string pathname = sftp.RemotePath;
              await sftp.ChangeRemotePath(arguments[1]);
              await sftp.ListDirectory();
              await sftp.ChangeRemotePath(pathname);
            }
            else
            {
              await sftp.ListDirectory();
            }
          }
          else if (arguments[0].Equals("mkdir"))
          {
            if (arguments.Length > 1) await sftp.MakeDirectory(arguments[1]);
          }
          else if (arguments[0].Equals("mv"))
          {
            if (arguments.Length > 2)
            {
              sftp.RemoteFile = arguments[1];
              await sftp.RenameFile(arguments[2]);
            }
          }
          else if (arguments[0].Equals("put"))
          {
            if (arguments.Length > 2)
            {
              sftp.LocalFile = arguments[1];
              sftp.RemoteFile = arguments[2];
              await sftp.Upload();
              Console.WriteLine("File uploaded");
            }
          }
          else if (arguments[0].Equals("pwd"))
          {
            Console.WriteLine(sftp.RemotePath);
          }
          else if (arguments[0].Equals("rm"))
          {
            if (arguments.Length > 1) await sftp.DeleteFile(arguments[1]);
          }
          else if (arguments[0].Equals("rmdir"))
          {
            if (arguments.Length > 1) await sftp.RemoveDirectory(arguments[1]);
          }
          else if (arguments[0].Equals(""))
          {
            // Do nothing.
          }
          else
          {
            Console.WriteLine("Invalid command.");
          } // End of command checking.

          sftp.RemoteFile = "";
          Console.Write("sftp> ");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: " + ex.Message);
      }
    }
  }

  #region "Events"

  private static void sftp_OnSSHServerAuthentication(object sender, SftpSSHServerAuthenticationEventArgs e)
  {
    if (e.Accept) return;
    Console.Write("Server provided the following certificate:\nIssuer: " + e.CertIssuer + "\nSubject: " + e.CertSubject + "\n");
    Console.Write("The following problems have been determined for this certificate: " + e.Status + "\n");
    Console.Write("Would you like to continue anyways? [y/n] ");
    if (Console.Read() == 'y') e.Accept = true;
  }

  private static void sftp_OnTransfer(object sender, SftpTransferEventArgs e)
  {
    Console.WriteLine(e.Text);
  }

  private static void sftp_OnDirList(object sender, SftpDirListEventArgs e)
  {
    Console.WriteLine(e.DirEntry);
  }

  private static void sftp_OnDisconnected(object sender, SftpDisconnectedEventArgs e)
  {
    Console.WriteLine("Disconnected with status code " + e.StatusCode + " and description " + e.Description);
    Environment.Exit(0);
  }

  #endregion
}


class ConsoleDemo
{
  public static Dictionary<string, string> ParseArgs(string[] args)
  {
    Dictionary<string, string> dict = new Dictionary<string, string>();

    for (int i = 0; i < args.Length; i++)
    {
      // If it starts with a "/" check the next argument.
      // If the next argument does NOT start with a "/" then this is paired, and the next argument is the value.
      // Otherwise, the next argument starts with a "/" and the current argument is a switch.

      // If it doesn't start with a "/" then it's not paired and we assume it's a standalone argument.

      if (args[i].StartsWith("/"))
      {
        // Either a paired argument or a switch.
        if (i + 1 < args.Length && !args[i + 1].StartsWith("/"))
        {
          // Paired argument.
          dict.Add(args[i].TrimStart('/'), args[i + 1]);
          // Skip the value in the next iteration.
          i++;
        }
        else
        {
          // Switch, no value.
          dict.Add(args[i].TrimStart('/'), "");
        }
      }
      else
      {
        // Standalone argument. The argument is the value, use the index as a key.
        dict.Add(i.ToString(), args[i]);
      }
    }
    return dict;
  }
}