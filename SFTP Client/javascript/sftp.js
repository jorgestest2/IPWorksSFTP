/*
 * IPWorks SFTP 2022 JavaScript Edition - Demo Application
 *
 * Copyright (c) 2023 /n software inc. - All rights reserved. - www.nsoftware.com
 *
 */
 
const readline = require("readline");
const ipworkssftp = require("@nsoftware/ipworkssftp");

if(!ipworkssftp) {
  console.error("Cannot find ipworkssftp.");
  process.exit(1);
}
let rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

main();

async function main() {
  const argv = process.argv;
  if(argv.length !== 5) {
    console.log("Usage: node sftp.js  host  user  password");
    process.exit();
  }

  const sftp1 = new ipworkssftp.sftp();

  sftp1.setSSHUser(argv[3]);
  sftp1.setSSHPassword(argv[4]);
  sftp1.setSSHAuthMode(2);

  sftp1.on("DirList", (e) =>{
    if(e.isDir) {
      console.log(`<dir> ${e.fileName}`);
    }
    else {
      console.log(e.fileName);
    }
  })
  .on('SSHStatus', (e) => {
    console.log(`SSH Status: ${e.message}`);
  })
  .on('SSHServerAuthentication',  (e) => (e.accept = true));
 

  //Logon using the hostname from the command line params and port 22
  await sftp1.SSHLogon(argv[2],22).catch((err) => {
    console.log(`Error: ${err.message}`);
    process.exit();
  });
  console.log("Succesfully logged on.");
  printMenu();
  sftpprompt();

  rl.on('line', async function(line) {

    const args = line.split(/\s+/);
    const cmd = args[0];

    switch (cmd) {
      case "?":
      case "help":
      {
        printMenu();
        sftpprompt();
        break;
      }
      case "close":
      case "quit":
      case "bye":
      {
        await sftp1.SSHLogoff();
        process.exit(0);
      }
      case "put":{
        if(args.length<3) {
          console.log('Usage: put localfile remotefile');
          sftpprompt();
          break;
        }
        sftp1.setLocalFile(args[1]);
        sftp1.setRemoteFile(args[2]);

        try {
          await sftp1.upload();
          console.log("Upload finished");
        } catch (ex) {
          console.log(`Error Put: ${ex}`)
        }
        sftp1.setLocalFile('');
        sftp1.setRemoteFile('');
        sftpprompt();
        break;
      }
      case "ls" : {
        try {
          if (args.length <=1) {
            await sftp1.listDirectory();
          } else {
            const curPath = sftp1.getRemotePath();
            sftp1.setRemotePath(args[1]);
            try {
              await sftp1.listDirectory();
            } catch (ex) {
              console.log(`Error: ${ex}`);
              sftp1.setRemotePath(curPath);
            }
          }
        } catch (ex) {
          console.log(`Error: ${ex}`)
        }
        sftpprompt();
        break;  
      }
      case "pwd" : {
        console.log(await sftp1.getRemotePath());
        sftpprompt();
        break;
                
      }
      case "cd" : {
        if(args.length <=1) {
          console.log("Usage: cd remote path");
          sftpprompt();
        } else {
          await sftp1.setRemotePath(args[1]).catch((err) => { console.log(`Error: ${err.message}`); });
          sftpprompt();
        }
        break;
                
      }
      case "rmdir" : {
        if(args.length <=1) {
          console.log("Usags: rmdir remote folder");
          sftpprompt();
        } else {
          await sftp1.removeDirectory(args[1]).catch((err) => { console.log(`Error: ${err.message}`); });
          sftpprompt();
        }
        break;
      }
      case "append" : {
        if(args.length<3) {
          console.log('Usage: append localfile remotefile');
          sftpprompt();
          break;
        }

        sftp1.setLocalFile(args[1]);        
        sftp1.setRemoteFile(args[2]);  
        try {
          await sftp1.append();
          console.log("Append finished");
        } catch (ex) {
          console.log(`Error: ${ex}`);
        }
        sftp1.setLocalFile('');
        sftp1.setRemoteFile('');       
        sftpprompt();       
        break;
      }
      case "mkdir" : {
        if(args.length <=1) {
          console.log("Usage: mkdir folder");
        } else {
          try {
            await sftp1.makeDirectory(args[1]);
          } catch (ex) {
            console.log(`Error: ${ex}`);
          }
        }
        sftpprompt();
        break;
      }
      case "get" : {
        if(args.length<3) {
          console.log('Usage: get remotefile localfile');
          sftpprompt();
          break;
        } else {
          sftp1.setRemoteFile(args[1]);
          sftp1.setLocalFile(args[2]);
          try {
            sftp1.download();
            console.log("Download finished");
          } catch (ex) {
            console.log(`Error: ${ex}`);
          }
        }
        sftpprompt();
        break;
      }
      case "rm" : {
        if(args.length <=1) {
          console.log("Usage: rm file");
        } else {
          try {
            await sftp1.deleteFile(args[1]);
            console.log("Deleted file");
          } catch (ex) {
            console.log(`Error: ${ex}`);
          }
      }
        sftpprompt();
        break;
      }
    }
  });
}

function sftpprompt(){
  process.stdout.write('sftp> ');  
}

function printMenu(){
  console.log("Available Commands:\n?        bye     help     put     rmdir");
  console.log("append   cd      ls       pwd     mkdir");
  console.log("quit     get     rm");
}



function prompt(promptName, label, punctuation, defaultVal)
{
  lastPrompt = promptName;
  lastDefault = defaultVal;
  process.stdout.write(`${label} [${defaultVal}] ${punctuation} `);
}
