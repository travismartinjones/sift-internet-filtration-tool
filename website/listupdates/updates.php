<?

$user="s207454rw";
$password="TqwnQ4yA";
$database="s207454_sift";
$hostname="mysql4-s";

mysql_connect($hostname,$user,$password) or die("Unable to open database");
mysql_select_db($database);

switch($_GET['action'])
{
	case "categories":
		getCategories();
		break;
	case "listUpdates":
		printListUpdates();
		break;
}


mysql_close();

function getCategories()
{
	print '<?xml version="1.0" encoding="utf-8" ?>';
	print '<categories>';

	$rootCategories = getCategoriesByParentID(NULL);
	
	while($row = mysql_fetch_array($rootCategories, MYSQL_ASSOC))	
		printCategory($row);	

	print '</categories>';
}

function getCategoriesByParentID($parentID)
{
	if($parentID == NULL)
		$query = "SELECT * FROM Category WHERE ParentID IS NULL AND IsEnabled = 1 ORDER BY Name";
	else
		$query = "SELECT * FROM Category WHERE ParentID = '".$parentID."' AND IsEnabled = 1 ORDER BY Name";

	return mysql_query($query);
}

function getTrueFalse($value)
{
	if($value)
		return "true";
	else
		return "false";		
}

function printCategory($categoryRow)
{
	
	print '<category id="'.$categoryRow["CategoryID"].'" Name="'.$categoryRow["Name"].'" Description="'.$categoryRow["Description"].'" CreateDate="'.$categoryRow["CreateDate"].'" IsRecommended="'.getTrueFalse($categoryRow["IsRecommended"]).'">';
	
	$childCategories = getCategoriesByParentID($categoryRow["CategoryID"]);
	
	if(mysql_num_rows($childCategories) != 0)
	{
		print '<categories>';
		
		while($row = mysql_fetch_array($childCategories, MYSQL_ASSOC))		
			printCategory($row);		
		
		print '</categories>';
	}
	
	print '</category>';
}

function getGetListsToUpdate()
{
	//  only pull lists that have at least one list entry update to send to the client
	// $query = "SELECT DISTINCT(ListID) FROM List";	
	
	// $updatedListIDs = mysql_query($query);
	
	// while($row = mysql_fetch_array($updatedListIDs, MYSQL_ASSOC))
	// {
		// $listIDs[] = $row["ListID"];
	// }
	
	$i = 1;
	
	while($idShort = $_GET['id'.$i])
	{		
		$lastUpdatedShort = $_GET['dt'.$i];
		
		if($lastUpdatedShort <> NULL)
		{
			$id = substr($idShort,0,8)."-".substr($idShort,8,4)."-".substr($idShort,12,4)."-".substr($idShort,16,4)."-".substr($idShort,20,12);
			$lastUpdated = date("Y/m/d H:i:s",$lastUpdatedShort);
			
			//print '<id>'.$id.'</id>';
			//print '<lastUpdated>'.$lastUpdated.'</lastUpdated>';
			
			$query = "SELECT COUNT(*) As UpdateCount FROM ListEntry WHERE ListID = '".$id."' AND CreateDate > '".$lastUpdated."'";
		
			$result = mysql_query($query);
			
			$row = mysql_fetch_array($result, MYSQL_ASSOC);
			
			if($row["UpdateCount"] > 0) 
			{			
				$listsToUpdate[$i-1][0] = $id;
				$listsToUpdate[$i-1][1] = $lastUpdated;
			}
		}
		$i=$i+1;
	}
	
	return $listsToUpdate;
}

function getListEntriesToUpdateByListID($listID, $lastUpdated)
{
	$query = 
"SELECT le.ListEntryID, le.ListID, le.Value, le.Action, date_format(le.CreateDate,'%m/%d/%Y %H:%i:%s') AS CreateDate FROM ListEntry le 
INNER JOIN
(
	SELECT ListID, Value, MAX(CreateDate) AS CreateDate FROM ListEntry
	WHERE CreateDate > '".$lastUpdated."'
	GROUP BY ListID, Value

) AS ListEntryTop
ON ListEntryTop.CreateDate = le.CreateDate AND ListEntryTop.Value = le.Value AND ListEntryTop.ListID = le.ListID
WHERE le.ListID='".$listID."' AND le.CreateDate > '".date("Y/m/d H:i:s",$lastUpdated)."';";		

	//print '<sql>'.$query.'</sql>';

	mysql_query("SET OPTION SQL_BIG_SELECTS=1");
	$result = mysql_query($query);
	mysql_query("SET OPTION SQL_BIG_SELECTS=0");
	
	return $result;
}

function getListActionText($action)
{
	$listUpdateTypeAdd = 1;
	$listUpdateTypeRemove = 2;	
	
	switch($action)
	{
		case $listUpdateTypeAdd:
			return "Add";
			break;
		case $listUpdateTypeRemove:
			return "Remove";
			break;
	}
}

function printListEntryUpdates($listID, $lastUpdated)
{	
	$xmlTranslation = array(
		"&" => "&amp;",
		"<" => "&lt;",
		">" => "&gt;",
		"'" => "&apos;",
		"\"" => "&quot;"
	);
	
	$listEntriesToUpdate = getListEntriesToUpdateByListID($listID,$lastUpdated);
	
	if(mysql_num_rows($listEntriesToUpdate) != 0)	
	{
		print '<listUpdate ListId="'.$listID.'">';
		while($row = mysql_fetch_array($listEntriesToUpdate, MYSQL_ASSOC))
		{				
			$escapedValue = utf8_encode(strtr($row["Value"],$xmlTranslation));						
			print '<listEntryUpdate Value="'.$escapedValue.'" Action="'.getListActionText($row["Action"]).'" DateCreated="'.$row["CreateDate"].'" />';
		}
		print '</listUpdate>';
	}
}

function printListUpdates()
{
	print '<?xml version="1.0" encoding="utf-8" ?>';
	print '<configuration>';
	print '<configSections>';
    print '<section name="ListUpdateSettings" type="Sift.Resources.Settings.ListUpdateSettings, Sift.Resources" />';
	print '</configSections>';
	print '<ListUpdateSettings>';
    print '<listUpdates>';
	
	$listsToUpdate = getGetListsToUpdate();
	
	for($i=0; $i < count($listsToUpdate); $i=$i+1)
	{		
		print '<listEntryUpdates>';
		
		printListEntryUpdates($listsToUpdate[$i][0],$listsToUpdate[$i][1]);
		
		print '</listEntryUpdates>';
	}
	
    print '</listUpdates>';
	print '</ListUpdateSettings>';
	print '</configuration>';
}

?>