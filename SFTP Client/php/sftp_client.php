<?php $sendBuffer = TRUE; ob_start(); ?>
<html>
<head>
<title>IPWorks SFTP 2022 Demos - SFTP Client</title>
<link rel="stylesheet" type="text/css" href="stylesheet.css">
<meta name="description" content="IPWorks SFTP 2022 Demos - SFTP Client">
</head>

<body>

<div id="content">
<h1>IPWorks SFTP - Demo Pages</h1>
<h2>SFTP Client</h2>
<p>A full featured SFTP client built using the SFTP component.  It allows browsing of directories, uploads and downloads of files, and more.</p>
<a href="default.php">[Other Demos]</a>
<hr/>

<?php
require_once('../include/ipworkssftp_sftp.php');
require_once('../include/ipworkssftp_const.php');

?>

<?php
  class MySFTP extends IPWorksSFTP_SFTP{
    function FireSSHServerAuthentication($param) {
      $param['accept'] = true;
      /*
        This demo uses the above setting to unconditionally accept the server's host key.
        In production you can set the SSHAcceptServerHostKeyFingerPrint configuration setting to the 16-byte MD5 fingerprint
        to explictly accept only one host key value (see Configuration section in the help file for more detail).
        You may also set the SSHAcceptServerHostKeyEncoded property to the public key file in PEM format
      */
      return $param;
		}
  }

  $sftp = new MySFTP();
?>

<form method=post>
<center>
<table width="90%">

 <tr><td>Server:      <td><input type=text name=server value="<?php echo isset($_POST["server"])?$_POST["server"]:""; ?>" size=40>
 <tr><td>User:        <td><input type=text name=user value="<?php echo isset($_POST["user"])?$_POST["user"]:""; ?>" size=20>
 <tr><td>Password:    <td><input type=password name=password value="<?php echo isset($_POST["password"])?$_POST["password"]:""; ?>" size=20>

 <tr><td><td><input type=submit value="  Go!  ">

</table>
</center>
</form>

<?php
if ($_SERVER['REQUEST_METHOD'] == "POST") {

  $sftp->setSSHHost($_POST["server"]);
  $sftp->setSSHUser($_POST["user"]);
  $sftp->setSSHPassword($_POST["password"]);

  try{
    $sftp->doListDirectory();
  } catch (Exception $e) {
    echo 'Error: ',  $e->getMessage(), "\n";
  }
?>

<p><i>Retrieving file list from SFTP server...</i><p>

<center>
<table width="90%">
  <tr>
    <th>File Name</th>
    <th>File Size</th>
    <th>File Time</th>
  </tr>

<?php
  for($i=0; $i < $sftp->getDirListCount(); $i++) {
?>
  <tr>
    <td nowrap><?php echo htmlspecialchars($sftp->getDirListFileName($i)); ?></td>
		<?php
      if($sftp->getDirListIsDir($i)){
       echo "<td nowrap>&lt;dir&gt;</td>";
      }
      else {
       echo "<td nowrap>" . htmlspecialchars($sftp->getDirListFileSize($i)) . "</td>";
      }
      echo "<td nowrap>" . htmlspecialchars($sftp->getDirListFileTime($i)) . "</td>";
    ?>
  </tr>

<?php
  }
  $sftp->doSSHLogoff();
?>

</table>
</center>

<p><i>Disconnected from SFTP server.</i>

<?php
}
?>


<br/>
<br/>
<br/>
<hr/>
NOTE: These pages are simple demos, and by no means complete applications.  They
are intended to illustrate the usage of the IPWorks SFTP objects in a simple,
straightforward way.  What we are hoping to demonstrate is how simple it is to
program with our components.  If you want to know more about them, or if you have
questions, please visit <a href="http://www.nsoftware.com/?demopg-IFPHA" target="_blank">www.nsoftware.com</a> or
contact our technical <a href="http://www.nsoftware.com/support/">support</a>.
<br/>
<br/>
Copyright (c) 2023 /n software inc.
<br/>
<br/>
</div>

<div id="footer">
<center>
IPWorks SFTP 2022 - Copyright (c) 2023 /n software inc. - For more information, please visit our website at <a href="http://www.nsoftware.com/?demopg-IFPHA" target="_blank">www.nsoftware.com</a>.
</center>
</div>

</body>
</html>

<?php if ($sendBuffer) ob_end_flush(); else ob_end_clean(); ?>
