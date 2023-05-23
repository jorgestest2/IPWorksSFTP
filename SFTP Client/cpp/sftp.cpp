/*
 * IPWorks SFTP 2022 C++ Edition - Demo Application
 *
 * Copyright (c) 2023 /n software inc. - All rights reserved. - www.nsoftware.com
 *
 */

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include "../../include/ipworkssftp.h"

#define LINE_LEN 80

class MySFTP : public SFTP
{
public:

	MySFTP()
	{
	}

	virtual int FireSSHServerAuthentication(SFTPSSHServerAuthenticationEventParams *e)
	{
		if (e->Accept) return 0;
		printf("\nServer provided the following fingerprint:\n %s\n",
		       e->Fingerprint);
		printf("Would you like to continue? [y/n] ");
		char command[LINE_LEN];
		fgets(command,LINE_LEN,stdin);
		command[strlen(command)-1] = '\0';
		if (!strcmp(command, "y")) e->Accept = true;
		else exit(0);
		return 0;
	}

	virtual int FireDirList(SFTPDirListEventParams *e)
	{
		printf( "%s\n", e->DirEntry );
		return 0;
	}

	virtual int FireSSHStatus(SFTPSSHStatusEventParams *e)
	{
		printf( "%s\n", e->Message );
		return 0;
	}

	virtual int FireError( SFTPErrorEventParams *e )
	{
		printf("Error %i: %s", e->ErrorCode, e->Description);
		return 0;
	}


};

int main(int argc, char **argv)
{

	MySFTP sftp;                  // SFTP object
	char command[LINE_LEN];     // user's command
	char *argument;             // arguments to the user's command
	char pathname[LINE_LEN];    // for use with the ls command
	int ret_code=0;

	//  If at least three arguments follow "sftp" at the command line,
	//   read them and log the user into a server.
	if (argc >= 4)
	{
		sftp.SetSSHUser(argv[2]);
		sftp.SetSSHPassword(argv[3]);
		ret_code = sftp.SSHLogon(argv[1], 22);
	}
	else
	{
		printf ("SSH Server: " );
		fgets(command,LINE_LEN,stdin);
		command[strlen(command)-1] = '\0';
		sftp.SetSSHHost( command );
		printf ("User: " );
		fgets(command,LINE_LEN,stdin);
		command[strlen(command)-1] = '\0';
		sftp.SetSSHUser( command );
		printf("Password: ");
		fgets(command,LINE_LEN,stdin);
		command[strlen(command)-1] = '\0';
		sftp.SetSSHPassword( command );
		ret_code = sftp.SSHLogon(sftp.GetSSHHost(), 22);
	}
	if (ret_code) goto done;

	while (1)
	{

		sftp.SetRemoteFile("");
		printf( "\nsftp> " );
		fgets(command,LINE_LEN,stdin);
		command[strlen(command)-1] = '\0';
		argument = strtok( command, " \t\n" );

		if ( ! strcmp(command, "?") )
		{
			printf( "?          exit      help     put\n"
			        "append     cd        ls       pwd\n"
			        "mkdir      rmdir     rm       get      mv\n");
		}

		else if ( ! strcmp(command, "append") )
		{
			argument = strtok( NULL, " \t\n" );
			sftp.SetLocalFile(argument);
			argument = strtok( NULL, " \t\n" );
			sftp.SetRemoteFile(argument);
			ret_code = sftp.Append();
		}

		else if ( ! strcmp(command, "exit") )
		{
			ret_code = sftp.SSHLogoff();
			exit(0);
		}

		else if ( ! strcmp(command, "cd") )
		{
			if ( argument = strtok( NULL, " \t\n" ) )
			{
				sftp.SetRemotePath(argument);
			}
		}

		else if ( ! strcmp(command, "get") )
		{
			argument = strtok( NULL, " \t\n" );
			sftp.SetRemoteFile(argument);
			sftp.SetLocalFile(argument);
			ret_code = sftp.Download();
			printf(  "Download complete.\n");
		}

		else if ( ! strcmp(command, "help") )
		{
			printf( "?          exit      help     put\n"
			        "append     cd        ls       pwd\n"
			        "mkdir      rmdir     rm       get      mv\n");
		}

		else if ( ! strcmp(command, "ls") )
		{
			if ( argument = strtok( NULL, " \t\n" ) )
			{
				strcpy(pathname, sftp.GetRemotePath());
				int ret_code = sftp.SetRemotePath(argument);
				if (!ret_code)
				{
					ret_code = sftp.ListDirectory();
				}
				if (!ret_code)
				{
					ret_code = sftp.SetRemotePath(pathname);
				}
			}
			else
			{
				ret_code = sftp.ListDirectory();
			}
		}

		else if ( ! strcmp(command, "mkdir") )
		{
			if ( argument = strtok( NULL, " \t\n" ) )
			{
				ret_code = sftp.MakeDirectory(argument);
			}
		}

		else if ( ! strcmp(command, "mv") )
		{
			argument = strtok( NULL, " \t\n" );
			sftp.SetRemoteFile(argument);
			argument = strtok( NULL, " \t\n" );
			ret_code = sftp.RenameFile(argument);
		}
		else if ( ! strcmp(command, "put") )
		{
			argument = strtok( NULL, " \t\n" );
			sftp.SetRemoteFile(argument);
			sftp.SetLocalFile(argument);
			ret_code = sftp.Upload();
			printf(  "Upload complete.\n");
		}
		else if ( ! strcmp(command, "pwd") )
		{
			printf( "%s\n", sftp.GetRemotePath() );
		}
		else if ( ! strcmp(command, "rm") )
		{
			if ( argument = strtok( NULL, " \t\n" ) )
			{
				ret_code = sftp.DeleteFile(argument);
			}
		}
		else if ( ! strcmp(command, "rmdir") )
		{
			if ( argument = strtok( NULL, " \t\n" ) )
			{
				ret_code = sftp.RemoveDirectory(argument);
			}
		}
		else if ( ! strcmp(command, "") )
		{
			// Do nothing
		}
		else
		{
			printf( "Bad command / Not implemented in demo.\n" );
		} // end of command checking
		if (ret_code)     // Got an error.  The user is done.
		{
			printf("\nError: %d", ret_code);
			if (sftp.GetLastError())
			{
				printf( " \"%s\"\n", sftp.GetLastError() );
			}
		}
		ret_code = 0;   // flush out error
	}  // end of main while loop

done:
	if (ret_code)     // Got an error.  The user is done.
	{
		printf( "\nError: %d", ret_code );
		if (sftp.GetLastError())
		{
			printf( " \"%s\"\n", sftp.GetLastError() );
		}
	}
	exit(ret_code);
	return 0;
}


