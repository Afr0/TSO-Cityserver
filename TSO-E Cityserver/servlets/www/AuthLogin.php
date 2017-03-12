<?php
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
 
# setting the fetch mode
$STH->setFetchMode(PDO::FETCH_ASSOC);

$Authorized = false;
 
while($row = $STH->fetch()) 
{
	#echo $row['name'] . "\n";
	if($row['Username'] == $_GET['username']) 
	{
		if($row['Password'] == $_GET['password']) 
		{
			$UUID = uniqid('', true);
			echo "Valid=TRUE \nTicket=" . $UUID;
			$Authorized = true;
		}
	}

}

$STH = $DBHandle->prepare('UPDATE Accounts SET AuthTicket= :AuthTicket WHERE Username= "' . $_GET['username'] . '"');
$STH->bindParam(':AuthTicket', $UUID, PDO::PARAM_STR);
$STH->execute();

# close the connection
$DBHandle = null;

#echo (strtolower($_GET['username']) === 'asdf' && $_GET['password'] === 'hjkl') ? 'Valid=TRUE
#Ticket=ghij' :

if($Authorized == false) 
{
	echo "Valid=FALSE\n
	Ticket=0\n
	reasoncode=INV-110\n
	reasontext=INV-110: The member name or password you have entered is incorrect. Please try again.\n
	reasonurl=";
}

?>