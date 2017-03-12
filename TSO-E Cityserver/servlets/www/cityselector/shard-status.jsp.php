<?php /*echo '<?xml version="1.0" encoding="UTF-8" ?>
<Shard-Status-List>
	<Shard-Status>
		<Location>public</Location>
		<Name>Alphaville</Name>
		<Rank>0</Rank>
		<GMT-Range>-6</GMT-Range>
		<Status>Up</Status>
		<Map>2</Map>
		<Online-Avatars>220</Online-Avatars>
		<MotD-List>
			<MotD>
				<MotD-From>Fatbag</MotD-From>
				<MotD-Subject>Happy Hacking!</MotD-Subject>
				<MotD-Message>This server is hosted by Donald Trump has Tiny Hands v. 1.0. Spread the word!</MotD-Message>
			</MotD>
		</MotD-List>
	</Shard-Status>
	<Shard-Status>
		<Location>public</Location>
		<Name>Blazing Falls</Name>
		<Rank>1</Rank>
		<GMT-Range>-6</GMT-Range>
		<Status>Up</Status>
		<Map>1</Map>
		<Online-Avatars>220</Online-Avatars>
		<MotD-List>
			<MotD>
				<MotD-From>Fatbag</MotD-From>
				<MotD-Subject>Happy Hacking!</MotD-Subject>
				<MotD-Message>This server is hosted by Donald Trump has Tiny Hands v. 1.0. Spread the word!</MotD-Message>
			</MotD>
		</MotD-List>
	</Shard-Status>
</Shard-Status-List>';*/

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
$Authorized = false;

while($row = $STH->fetch()) 
{
	if($row['AuthTicket'] == $_COOKIE['TSOSession'])
	{
		$Authorized = true;
		echo('<?xml version="1.0" encoding="UTF-8" ?>');
		echo("\n"); 
		echo("<Shard-Status-List>\n");
	}
}

$STH = $DBHandle->query('SELECT * FROM CityServers');

while($row = $STH->fetch()) 
{
	if($Authorized == true)
	{
		if($row['Status'] == "Up" || $row['Status'] == "Frontier")
		{
			echo("<Shard-Status>");
			echo("<Name>" . $row['Name'] . "</Name>\n");
			echo("<Rank>" . $row['Rank'] . "</Rank>\n");
			echo("<Status>" . $row['Status'] . "</Status>\n");
			echo("<Map>" . $row['Map'] . "</Map>\n");
			echo("<Online-Avatars>" . $row['OnlineAvatars'] . "</Online-Avatars>\n");
			echo("<MotD-List>\n");
			echo("<MotD-From>" . $row['MOTDFrom'] . "</MotD-From>\n");
			echo("<MotD-Subject>" . $row['MOTDSubject'] . "</MotD-Subject>\n");
			echo("<MotD-Message>" . $row['MOTDMessage'] . "</MotD-Message>\n");
			echo("</MotD-List>\n");
			echo("</Shard-Status>");
		}
	}
}

echo("</Shard-Status-List>");

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

