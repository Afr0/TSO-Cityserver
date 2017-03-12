<?php 

/*This AvatarDataServlet is for TSO New and Improved (NAI)*/
/*echo '<?xml version="1.0" encoding="UTF-8" ?>
<The-Sims-Online>
	<Avatar-Data>
		<AvatarID>1337</AvatarID>
		<Name>JollySim</Name>
		<Shard-Name>Blazing Falls</Shard-Name>
	</Avatar-Data>
</The-Sims-Online>';*/

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

$AvatarID1 = 'null';
$AvatarID2 = 'null';
$AvatarID3 = 'null';

while($row = $STH->fetch()) 
{
	if($row['AuthTicket'] == $_COOKIE['TSOSession'])
	{
		$Authorized = true;
		echo('<?xml version="1.0" encoding="UTF-8" ?>');
		echo("<The-Sims-Online>\n");

 		$AvatarID1 = $row['AvatarID1'];
		$AvatarID2 = $row['AvatarID2'];
		$AvatarID3 = $row['AvatarID3'];
	}
}

$STH = $DBHandle->query('SELECT * FROM Avatars');

while($row = $STH->fetch()) 
{
	if($row['AvatarID'] == $AvatarID1 || $row['AvatarID'] == $AvatarID2 || $row['AvatarID'] == $AvatarID3)
	{
		echo("<Avatar-Data>\n");
		echo("<AvatarID>" . $row['AvatarID'] . "</AvatarID>\n");
    		echo("<Name>" . $row['Name'] . "</Name>\n");
    		echo("<Shard-Name>" . $row['ShardName'] . "</Shard-Name>\n");
		echo("</Avatar-Data>\n");
        }
}

echo("</The-Sims-Online>");

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