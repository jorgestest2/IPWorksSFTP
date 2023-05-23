import SwiftUI
import IPWorksSFTP

struct ContentView: View, SFTPDelegate {
  func onConnected(statusCode: Int32, description: String) {}
  func onConnectionStatus(connectionEvent: String, statusCode: Int32, description: String) {}
  func onDirList(dirEntry: String, fileName: String, isDir: Bool, fileSize: Int64, fileTime: String, isSymlink: Bool) {
    outputRes += "\(fileName) \n"
  }
  func onDisconnected(statusCode: Int32, description: String) {}
  func onEndTransfer(direction: Int32, localFile: String, remoteFile: String) {}
  func onError(errorCode: Int32, description: String, localFile: String, remoteFile: String) {
    outputRes += "Error \(errorCode): \(description)"
  }
  func onLog(logLevel: Int32, message: String, logType: String) {}
  func onSSHCustomAuth(packet: inout String) {}
  func onSSHKeyboardInteractive(name: String, instructions: String, prompt: String, response: inout String, echoResponse: Bool) {}
  func onSSHServerAuthentication(hostKey: Data, fingerprint: String, keyAlgorithm: String, certSubject: String, certIssuer: String, status: String, accept: inout Bool) {
    accept = true
  }
  func onSSHStatus(message: String) {}
  func onStartTransfer(direction: Int32, localFile: String, remoteFile: String) {}
  func onTransfer(direction: Int32, localFile: String, remoteFile: String, bytesTransferred: Int64, percentDone: Int32, text: Data, cancel: inout Bool) {}
  
  var client = SFTP()
  var documentsPath = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true)[0] + "/"
  @State private var server: String = ""
  @State private var username: String = ""
  @State private var password: String = ""
  @State private var filename: String = ""
  @State private var outputRes: String = ""
  @State private var connected = false
  
  func connectedChange() -> String
  {
    if (connected)
    {
      return "Disconnect"
    }
    else
    {
      return "Connect"
    }
  }
  
  var body: some View {
    VStack(alignment: .center)
    {
      Text("This demo uses the SFTP module to connect to a remote SFTP server and list files. You may also enter a local or remote file, then select Get to download or Put to upload.")
        .foregroundColor(Color.blue)
      HStack{
        Text("Server:")
        TextField("remote host", text: $server)
          .autocapitalization(/*@START_MENU_TOKEN@*/.none/*@END_MENU_TOKEN@*/)
      }
      HStack{
        
        Text("Username:")
        
        TextField("username", text: $username)
          .autocapitalization(/*@START_MENU_TOKEN@*/.none/*@END_MENU_TOKEN@*/)
      }
      HStack{
        Text("Password:")
        
        SecureField("password", text: $password)
          .autocapitalization(/*@START_MENU_TOKEN@*/.none/*@END_MENU_TOKEN@*/)
      }
      connectButton()
      
      Group
      {
        HStack{
          Text("Filename:")
          TextField("enter remote/local filename", text: $filename)
            .autocapitalization(/*@START_MENU_TOKEN@*/.none/*@END_MENU_TOKEN@*/)
        }
        HStack()
        {
          downloadButton()
          uploadButton()
        }
        
        Text("Output:")
        TextEditor(text: $outputRes)
          .border(Color.black, width: 1)
      }
    }
    .padding(.all, 8.0)
  }
  
  @ViewBuilder
  private func connectButton() -> some View {
    Button(action:
            {
      //client.runtimeLicense = ""
      client.delegate = self
      outputRes = ""
      do
      {
        if (client.connected == true)
        {
          try client.SSHLogoff()
        }
        else
        {
          client.sshUser = username
          client.sshPassword = password
          try client.SSHLogon(sshHost: server, sshPort: 22)
          outputRes += "Logged on. Listing directory...\n"
          print(client.remotePath)
          try client.listDirectory()
        }
        connected = client.connected
      }
      catch
      {
        do
        {
          try client.SSHLogoff()
        }
        catch {}
        outputRes += "Error: \(error)"
        return
      }
    }, label:
            {
      Text("\(connectedChange())")
        .font(.system(size: 20))
        .frame(minWidth: 150, minHeight: 40)
        .background(RoundedRectangle(cornerRadius: 8)
          .fill(Color.gray))
    })
    .buttonStyle(PlainButtonStyle())
    .padding(.bottom, 20)
  }
  
  @ViewBuilder
  private func downloadButton() -> some View {
    Button(action:
            {
      do
      {
        client.remoteFile = filename
        client.localFile = documentsPath + filename
        try client.download()
        
        print("File successfully downloaded")
      }
      catch
      {
        print(error)
        return
      }
    }, label: {
      Text("Get").font(.system(size: 20))
        .frame(minWidth: 150, minHeight: 40)
        .background(RoundedRectangle(cornerRadius: 8)
          .fill(Color.gray))
    })
    .buttonStyle(PlainButtonStyle())
    .disabled(connected == false)
  }
  
  @ViewBuilder
  private func uploadButton() -> some View {
    Button(action:
            {
      outputRes = ""
      do
      {
        client.localFile = documentsPath + filename
        client.remoteFile = filename
        try client.upload()
        try client.listDirectory()
        print("File successfully uploaded")
      }
      catch
      {
        print(error)
        return
      }
    }, label: {
      Text("Put")
        .font(.system(size: 20))
        .frame(minWidth: 150, minHeight: 40)
        .background(RoundedRectangle(cornerRadius: 8)
          .fill(Color.gray))
    })
    .buttonStyle(PlainButtonStyle())
    .disabled(connected == false)
  }
  
}

struct ContentView_Previews: PreviewProvider {
  static var previews: some View {
    ContentView()
  }
}
