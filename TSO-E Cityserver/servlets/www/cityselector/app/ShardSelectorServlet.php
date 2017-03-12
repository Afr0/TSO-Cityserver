<?php
/*This ShardSelectorServlet is for TSO New and Improved (NAI)*/ 
/*echo '<?xml version="1.0" encoding="UTF-8" ?>
<Shard-Selection>
  <Connection-Address>localhost:49</Connection-Address>
  <Authorization-Ticket>2002</Authorization-Ticket>
  <PlayerID>42</PlayerID>
  <ConnectionID>20301</ConnectionID>
  <EntitlementLevel>3</EntitlementLevel>
  <AvatarID>1337</AvatarID>
</Shard-Selection>';*/

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

if(empty($_GET['avatarId']) || !isset($_GET['avatarId'])) #No AvatarID means player wanted to create a new avatar.
{
	$UUID = mt_rand(0, 0x7fffffff); #Generate an AvatarID
	$AvatarID1 = 0;
	$AvatarID2 = 0;
	$AvatarID3 = 0;

	$STH = $DBHandle->query('SELECT AvatarID FROM Avatars where AvatarID =' . $UUID);

	while($row = $STH->fetch()) #Make sure that the ID doesn't already exist.
	{
		$UUID = genRandomNumber(15, false); #Generate an AvatarID
		$STH = $DBHandle->query('SELECT ID FROM Avatars where ID =' . $UUID);
	}

	$STH = $DBHandle->query('SELECT * FROM Accounts');
	$PlayerID = 0;

	while($row = $STH->fetch()) 
	{
		if($row['AuthTicket'] == $_COOKIE['TSOSession'])
		{
			$PlayerID = $row['ID'];
			$AvatarID1 = $row['AvatarID1'];
			$AvatarID2 = $row['AvatarID2'];
			$AvatarID3 = $row['AvatarID3'];
		}
	}

	$Authorized = false;

	if($AvatarID1 == 0 || $AvatarID2 == 0 || $AvatarID3 == 0)
        {
		$Authorized = true;
        }
	
	if($Authorized == true)
	{
            $STH = $DBHandle->query('SELECT * FROM CityServers');

            while($row = $STH->fetch()) 
            {
                if(strtoupper($row['Name']) == strtoupper($_GET['shardName']))
                {
                	echo '<?xml version="1.0" encoding="UTF-8" ?>';
                    	echo("\n");
                   	echo("<Shard-Selection>\n");
                    	echo("  <Connection-Address>localhost:" . $row['Port'] . "</Connection-Address>\n");
                    	echo("  <Authorization-Ticket>" . $_COOKIE['TSOSession'] . "</Authorization-Ticket>\n");
                    	echo("  <PlayerID>" . $PlayerID . "</PlayerID>\n");
                    	#Not sure what this means...
                    	echo("  <ConnectionID>" . 20301 . "</ConnectionID>\n");
                    	#TODO: Output 1 for CSR
                    	echo("  <EntitlementLevel>" . 3 . "</EntitlementLevel>\n");
                    	echo("  <AvatarID>" . $UUID . "</AvatarID>\n");
                	echo("</Shard-Selection>");		
                }
            }
	}
	else
	{
		file_put_contents('Error.txt', 'No available AvatarIDs!');
		echo '<?xml version="1.0" encoding="UTF-8" ?>';
		echo("\n");
		echo("<Error-Message>\n");
  		echo("<Error-Number>1337</Error-Number>\n");
  		echo("<Error>You need to delete an avatar before creating more!</Error>\n");
		echo("</Error-Message>");
	}
        
        $Authorized = false;

	#TODO: Figure out if it's AvatarID1, 2 or 3
	if($AvatarID1 == 0)
	{
            $STH = $DBHandle->prepare('UPDATE Accounts SET AvatarID1= :AvatarID WHERE AuthTicket= "' . $_COOKIE['TSOSession'] . '"');
            $Authorized = true;
	}
	else if($AvatarID2 == 0)
	{
            if($Authorized == false)
            {
		$STH = $DBHandle->prepare('UPDATE Accounts SET AvatarID2= :AvatarID WHERE AuthTicket= "' . $_COOKIE['TSOSession'] . '"');
		$Authorized = true;
            }
	}
	else if($AvatarID3 == 0)
	{
            if($Authorized == false)
            {
		$STH = $DBHandle->prepare('UPDATE Accounts SET AvatarID3= :AvatarID WHERE AuthTicket= "' . $_COOKIE['TSOSession'] . '"');
		$Authorized = true;
            }
	}
	
	If($Authorized == true)
	{
		$STH->bindParam(':AvatarID', $UUID, PDO::PARAM_STR);
		$STH->execute();
	}
        
        # close the connection
        $DBHandle = null;

} 
else	#Client provided an AvatarID, so no new character is going to be created.
{
	$STH = $DBHandle->query('SELECT * FROM Accounts');
	$PlayerID = 0;

	while($row = $STH->fetch()) 
	{
		if($row['AuthTicket'] == $_COOKIE['TSOSession'])
		{
			$PlayerID = $row['ID'];
		}
	}

	$STH = $DBHandle->query('SELECT * FROM CityServers');

        while($row = $STH->fetch()) 
        {
        	if($row['Name'] == $_GET['shardName'])
                {
                	echo '<?xml version="1.0" encoding="UTF-8" ?>';
                    	echo("\n");
                    	echo("<Shard-Selection>\n");
                    	echo("<Connection-Address>localhost:" . $row['Port'] . "</Connection-Address>\n");
                    	echo("<Authorization-Ticket>" . $_COOKIE['TSOSession'] . "</Authorization-Ticket>\n");
                    	echo("<PlayerID>" . $PlayerID . "</PlayerID>\n");
                    	#Not sure what this means...
                    	echo("<ConnectionID>" . 1 . "</ConnectionID>\n");
                    	#TODO: Output 1 for CSR
                    	echo("<EntitlementLevel>" . 0 . "</EntitlementLevel>\n");
                    	echo("<AvatarID>" . $_GET['avatarId'] . "</AvatarID>\n");
                	echo("</Shard-Selection>");
        	}
	}
}?>