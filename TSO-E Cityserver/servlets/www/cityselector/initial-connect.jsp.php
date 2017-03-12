<?php

/*echo '<?xml version="1.0" encoding="UTF-8" ?>
<User-Authorized></User-Authorized>';*/

try
{
	$DBHandle = new PDO("sqlite:C:\Accounts.db");
	$DBHandle->setAttribute( PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION );
}
catch(PDOException $e) 
{
	echo $e->getMessage();
	file_put_contents('DBErrors.txt', $e->getMessage(), FILE_APPEND);
}

# using the shortcut ->query() method here since there are no variable
# values in the select statement.
$STH = $DBHandle->query('SELECT * FROM Accounts');
$AuthTicket = 'null';
$Authorized = false;

while($row = $STH->fetch()) 
{
	if($row['AuthTicket'] == $_GET['ticket']) 
	{
		$AuthTicket = $row['AuthTicket'];
		$Authorized = true;

		/* There is a cookie-parsing bug in versions of TSO prior to New & Improved,
		** in that the last cookie in the HTTP response is ignored.
		** Thus, create another cookie to ensure that TSOSession isn't the last
		** cookie. */
		setcookie('TSOSession', $AuthTicket, 0, '/');
		setcookie('TSOSession2', $AuthTicket, 0, '/');
		
		/* Normally, InitialConnectServlet responds with User-Authorized if the
		** "version" GET variable is up-to-date, and Patch-Result otherwise. In our
		** case, we want to respond with "up-to-date" most of the time.
		** However, when the updater wants to verify the game directory, it sends a
		** request with version=0.0.0.0 to ensure that the server responds with
		** Patch-Result so that it can get the URL of the TSOClient transmitter
		** (presumably Maxis-Patch-Production2). If User-Authorized is returned in this
		** case, the game verification will fail.
		** Thus, return Patch-Result if address=0.0.0.0, User-Authorized otherwise. */
		if ($_GET['version'] === '0.0.0.0') 
		{
			echo ('<?xml version="1.0" encoding="UTF-8" ?>
			<Patch-Result>
  			<Authorization-Ticket>' . $AuthTicket . '</Authorization-Ticket>
  			<Patch-Address>http://localhost/games/PSW/TSO/Maxis-Patch-Production2</Patch-Address>
			</Patch-Result>');
		} 
		else 
		{
			echo '<?xml version="1.0" encoding="UTF-8" ?>
			<User-Authorized></User-Authorized>';
		}
	}

}


# close the connection
$DBHandle = null;

if($Authorized == false)
{
	echo ('<?xml version="1.0" encoding="UTF-8" ?>
	<Error-Message>
  	<Error-Number>1337</Error-Number>
  	<Error>AuthTicket was invalid!</Error>
	</Error-Message>');
}
?>