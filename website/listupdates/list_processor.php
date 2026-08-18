<?

$user="s207454rw";
$password="TqwnQ4yA";
$database="s207454_sift";
$hostname="mysql4-s";
$listspath="/home/groups/s/si/sift/htdocs/listupdates/BL";

echo "Opening connection\n";
mysql_connect($hostname,$user,$password) or die("Unable to open database");
mysql_select_db($database);

$listCategories = getDirectories($listspath);

foreach($listCategories as $category)
	processCategory($listspath,$category, NULL);

echo "Closing connection\n";
mysql_close();

function NewGuid()
{
	return mysql_result(mysql_query('SELECT UUID()'),0);
}

function getCategoryByCategoryID($categoryID)
{
	// see if the database has this category, if not, add it
        $query = "SELECT * FROM Category WHERE CategoryID = '".$categoryID."'";

        $result = mysql_query($query);

	if(mysql_num_rows($result) != 0)
		return mysql_fetch_array($result, MYSQL_ASSOC);
	
	return NULL;	
}

function getListEntryIDByListIDValueAction($listID, $value, $action)
{
	// get the list unique identifier
	$query = "SELECT * FROM ListEntry WHERE ListID = '".$listID."' AND Value = '".$value."' AND Action = ".$action." ORDER BY CreateDate DESC";

	$result = mysql_query($query);

	if(mysql_num_rows($result) != 0)
	{
		$row = mysql_fetch_array($result, MYSQL_ASSOC);
		return $row["ListID"];
	}
	
	return NULL;
}

function getListIDByCategoryIDListType($categoryID, $listType)
{
	// get the list unique identifier
	$query = "SELECT * FROM List WHERE CategoryID = '".$categoryID."' AND ListType = ".$listType;

	$result = mysql_query($query) or die(mysql_error());

	if(mysql_num_rows($result) != 0)
	{
		$row = mysql_fetch_array($result, MYSQL_ASSOC);
		return $row["ListID"];
	}
	
	return NULL;
}

function getCategoryIDByShallaDirectoryName($shallaName)
{
	if($shallaName != NULL)
	{
		// get the directory unique identifier
		$query = "SELECT * FROM Category WHERE ShallaDirectoryName = '".$shallaName."'";

		$result = mysql_query($query);

		if(mysql_num_rows($result) != 0)
		{
        	        $row = mysql_fetch_array($result, MYSQL_ASSOC);
			return $row["CategoryID"];
		}
	}

	return NULL;
}

function insertListEntry($listID, $value, $action)
{
	$query = "INSERT INTO ListEntry VALUES ('".NewGuid()."','".$listID."','".$value."',".$action.",CURRENT_TIMESTAMP());";
		
	//echo $query."\n";
	mysql_query($query);
}

function insertList($categoryID, $listType)
{
	$listTypeIP = 1;
	$listTypeURL = 2;
	$listTypeDomain = 3;

	$query = "INSERT INTO List VALUES ('".NewGuid()."','".$categoryID."',";
	
	switch($listType)
	{
		case $listTypeIP:
			$query = $query."'IP List'";
			break;
		case $listTypeURL:
			$query = $query."'URL List'";
			break;
		case $listTypeDomain:
			$query = $query."'Domain List'";
			break;
	}
	
	$query = $query.",".$listType.",CURRENT_TIMESTAMP());";
	
	echo $query."\n";
	mysql_query($query);
	
	// return the id of the newly inserted row
	return getListIDByCategoryIDListType($categoryID, $listType);
}

function insertCategory($category, $shallaName, $parentCategoryID)
{
	$query = "INSERT INTO Category VALUES ('".NewGuid()."',";
	
	if($parentCategoryID == NULL)
		$query = $query."NULL";
	else
		$query = $query."'".$parentCategoryID."'";

	$query = $query.",'".$category."','','".$shallaName."',CURRENT_TIMESTAMP());";

	//echo $query."\n";
	mysql_query($query);
}

function processCategory($parentDirectory,$category, $parentCategory)
{
	if($parentCategory == NULL)
		echo "Processing $category\n";
	else
		echo "Processing $parentCategory/$category\n";
	
	$shallaDirectoryName = "/".$category;

	if($parentCategory != NULL)
		$shallaDirectoryName = $parentCategory.$shallaDirectoryName;

	$categoryID = getCategoryIDByShallaDirectoryName($shallaDirectoryName);
	$parentCategoryID = getCategoryIDByShallaDirectoryName($parentCategory);

	if($parentCategory != NULL && $parentCategoryID == NULL)
	{
		// the parent has not been made yet, we cannot continue
		echo "Parent Category ".$parentCategory." does not exist in the database yet.\n";
		return;
	}

	if(getCategoryByCategoryID($categoryID) == NULL)
	{
		// the row doesn't exist, add it
		insertCategory($category, $parentCategory."/".$category, $parentCategoryID);
		// get the newly created category id
		$categoryID = getCategoryIDByShallaDirectoryName($shallaDirectoryName);
	}	

	// process the subdirectories first
	$subdirectories = getDirectories($parentDirectory."/".$category);
	foreach($subdirectories as $subdirectory)
	{
		processCategory($parentDirectory."/".$category,$subdirectory,$parentCategory."/".$category);
	}

	// process the list files in this directory
	$files = getFiles($parentDirectory."/".$category);

	foreach($files as $file)
	{
		processListFile($parentDirectory."/".$category,$file,$categoryID);
	}
}

function processListFile($directory,$filename,$categoryID)
{
	// constants
	$listTypeIP = 1;
	$listTypeURL = 2;
	$listTypeDomain = 3;

	$listUpdateTypeAdd = 1;
	$listUpdateTypeRemove = 2;	
	
	echo "Processing list $filename\n";

	$file = fopen($directory."/".$filename,"r") or die("Error opening ".$directory."/".$filename);

	$listIPID = getListIDByCategoryIDListType($categoryID,$listTypeIP);
	$listDomainID = getListIDByCategoryIDListType($categoryID,$listTypeDomain);
	$listURLID = getListIDByCategoryIDListType($categoryID,$listTypeDomain);

	while(!feof($file))
	{
		$line = fgets($file);
		$line = trim($line); // remove whitespace and newlines
	
		if($filename == "domains")
			$listEntryType = $listTypeDomain; // domain;
		else if($filename == "urls")
			$listEntryType = $listTypeURL; // url
			
		// sometimes IPs are mixed into the lists
		if(preg_match(
			'/^(?:25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)(?:[.](?:25[0-5]|2[0-4]\d|1\d\d|[1-9]\d|\d)){3}$/',
			$line))
			$listEntryType = $listTypeIP; // ip		
			
		switch($listEntryType)
		{
			case $listTypeIP:
				// the list has not been created yet for this category and list type, create it
				if($listIPID == NULL) 
					$listIPID = insertList($categoryID, $listTypeIP);				
				
				// if the list entry does not exist, or it exists, but the most recent value is a removal, add the entry back in
				if(getListEntryIDByListIDValueAction($listIPID, $line, $listUpdateTypeAdd) == NULL)
					insertListEntry($listIPID, $line, $listUpdateTypeAdd);
				break;
			case $listTypeDomain:
				if($listDomainID == NULL) // the list has not been created yet for this category and list type, create it
					$listDomainID = insertList($categoryID, $listTypeDomain);				
				
				// if the list entry does not exist, or it exists, but the most recent value is a removal, add the entry back in
				if(getListEntryIDByListIDValueAction($listDomainID, $line, $listUpdateTypeAdd) == NULL)				
					insertListEntry($listDomainID, $line, $listUpdateTypeAdd);
				break;
			case $listTypeURL:
				if($listURLID == NULL) // the list has not been created yet for this category and list type, create it					
					$listURLID = insertList($categoryID, $listTypeURL);				
								
				// if the list entry does not exist, or it exists, but the most recent value is a removal, add the entry back in
				if(getListEntryIDByListIDValueAction($listURLID, $line, $listUpdateTypeAdd) == NULL)
					insertListEntry($listURLID, $line, $listUpdateTypeAdd);
				break;
		}
	}

	fclose($file);
}

function getFiles($parentDirectory)
{
	$handle = opendir($parentDirectory);

	$fileList = Array();
	
	while($file = readdir($handle))
	{
		if(!(is_dir($parentDirectory."/".$file)))
		{
			$fileList[] = $file;
		}
	}

	closedir($handle);

	return $fileList;
}

function getDirectories($parentDirectory)
{
	// directories to ignore
	$exclusions[] = ".";
	$exclusions[] = "..";

	$directoryList = Array();

	$handle = opendir($parentDirectory);

	while($directory = readdir($handle))
	{
		if((is_dir($parentDirectory."/".$directory)) && (!(in_array($directory, $exclusions))))
		{
			$directoryList[] = $directory;
		}
	}

	closedir($handle);

	return $directoryList;
}

?>
