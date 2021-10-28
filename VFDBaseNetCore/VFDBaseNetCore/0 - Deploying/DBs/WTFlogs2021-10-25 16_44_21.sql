-- --------------------------------------------------------
-- Host:                         k8s-carrier.sollers-sftdev.lab
-- Server version:               10.5.10-MariaDB-log - Source distribution
-- Server OS:                    Linux
-- HeidiSQL Version:             11.3.0.6337
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

-- Dumping structure for procedure WTFlogs.claimsLogCache_Build
DELIMITER //
CREATE PROCEDURE `claimsLogCache_Build`(
	IN `_uid` VARCHAR(50),
	IN `_wfid` VARCHAR(50),
	IN `_numDays` INT
)
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _wfid = IFNULL(_wfid, '');

 START TRANSACTION; -- vfd pattern for stored procedures 
   INSERT INTO ClaimsLogCatched (actorId,logged,author,
	                              claimText,viewed,viewedby,
											prevID)
   SELECT _uid,gl.logged,gl.author,
			 gl.claimText,gl.viewed,viewedby,
			 gl.ID
   FROM ClaimsLog gl
   WHERE gl.logged>DATE_SUB(NOW(),INTERVAL _numDays HOUR);
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_deSelectAllVisible
DELIMITER //
CREATE PROCEDURE `claimsLogCache_deSelectAllVisible`(
	IN `_uid` VARCHAR(64),
	IN `_searchString` VARCHAR(64),
	IN `_searchField` VARCHAR(64)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
 	DECLARE msg varchar(255);
 	
   DECLARE author, claimText VARCHAR(50) DEFAULT '%';
   DECLARE ss VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
 	
 	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
   	ROLLBACK; -- vfd pattern for stored procedures
   	RESIGNAL; -- vfd pattern for stored procedures
 	END;
 	
 	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'author' THEN SET author = ss;
   WHEN 'text' THEN SET claimText = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort field.';
   END CASE;
   
   SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT Id,ActorId,author,logged,viewed,'
		        'viewedby, CAST(claimText AS VARCHAR(164)) AS claimText, prevID, marked '
		        'FROM ClaimsLogCatched '
		        'WHERE ActorId = ? '
		        'AND author LIKE ? '
			     'AND claimText LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uid, author, claimText;
	DEALLOCATE PREPARE stm;
 	
 	START TRANSACTION;
 		UPDATE WTFlogs.ClaimsLogCatched AS clc
 				 INNER JOIN tmp ON tmp.Id=clc.ID
 		SET clc.marked = 0;
 		SET rc = ROW_COUNT();
 	COMMIT;
 	
 	DROP TEMPORARY TABLE IF EXISTS tmp;
 	SELECT
	   CAST(rc AS INT) AS RetValueInt,
	   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_Drop
DELIMITER //
CREATE PROCEDURE `claimsLogCache_Drop`(
	IN `_uid` varchar(50),
	IN `_wfid` varchar(50)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _wfid = IFNULL(_wfid, '');

 START TRANSACTION; -- vfd pattern for stored procedures 
   DELETE claimsLogcacheControl
   FROM claimsLogcacheControl
   WHERE uid = _uid;
   INSERT INTO claimsLogcacheControl (uid, wfid)
   VALUES (_uid, _wfid);

   DELETE ClaimsLogCatched
   FROM ClaimsLogCatched
   WHERE actorId = _uid;
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_GetEntry
DELIMITER //
CREATE PROCEDURE `claimsLogCache_GetEntry`(
	IN `_recId` INT,
	IN `_uId` VARCHAR(50)
)
    READS SQL DATA
BEGIN
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;

	SET _uid = IFNULL(_uid, '');
   SET _recId = IFNULL(_recId, 0);

	SELECT glc.ID, glc.author, glc.logged, glc.viewed, glc.viewedby, glc.claimText, glc.prevID,
	       glc.marked, CAST(0 AS UNSIGNED) AS numRecs
	FROM ClaimsLogCatched glc
	WHERE glc.ID = _recId and
	      glc.actorID = _uId;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_GetPage_wcmpflt
DELIMITER //
CREATE PROCEDURE `claimsLogCache_GetPage_wcmpflt`(
	IN `_uid` varchar(50),
	IN `_searchString` varchar(50),
	IN `_CurrentSort` varchar(50),
	IN `_SortDirection` varchar(50),
	IN `_pageIndex` int,
	IN `_pageSize` int,
	IN `_searchField` VARCHAR(50)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE flt VARCHAR(500) DEFAULT 'not defined';
   DECLARE author, claimText VARCHAR(50) DEFAULT '%';
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'author' THEN SET author = ss;
   WHEN 'text' THEN SET claimText = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort field.';
   END CASE;

	SET srt = CONCAT(_CurrentSort, _SortDirection);
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT Id,ActorId,author,logged,viewed,'
		        'viewedby, CAST(claimText AS VARCHAR(164)) AS claimText, prevID, marked '
		        'FROM ClaimsLogCatched '
		        'WHERE ActorId = ? '
		        'AND author LIKE ? '
			     'AND claimText LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uid, author, claimText;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SET _pageIndex = IFNULL(_pageIndex, 0);
	SET _pageSize = IFNULL(_pageSize, 2147483647);
	
	SET off = (_pageIndex - 1) * _pageSize,
	ps = _pageSize;
	
	CASE srt
	WHEN 'sortA_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortA_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.author asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.author desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.viewed asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.viewed desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort order.';
	END CASE;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_invertSelectionAllVisible
DELIMITER //
CREATE PROCEDURE `claimsLogCache_invertSelectionAllVisible`(
	IN `_uid` VARCHAR(64),
	IN `_searchString` VARCHAR(64),
	IN `_searchField` VARCHAR(64)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
 	DECLARE msg varchar(255);
 	
   DECLARE author, claimText VARCHAR(50) DEFAULT '%';
   DECLARE ss VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
 	
 	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
   	ROLLBACK; -- vfd pattern for stored procedures
   	RESIGNAL; -- vfd pattern for stored procedures
 	END;
 	
 	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'author' THEN SET author = ss;
   WHEN 'text' THEN SET claimText = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort field.';
   END CASE;
   
   SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT Id,ActorId,author,logged,viewed,'
		        'viewedby, CAST(claimText AS VARCHAR(164)) AS claimText, prevID, marked '
		        'FROM ClaimsLogCatched '
		        'WHERE ActorId = ? '
		        'AND author LIKE ? '
			     'AND claimText LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uid, author, claimText;
	DEALLOCATE PREPARE stm;
 	
 	START TRANSACTION;
 		UPDATE WTFlogs.ClaimsLogCatched AS clc
 				 INNER JOIN tmp ON tmp.Id=clc.ID
 		SET clc.marked = NOT clc.marked;
 		SET rc = ROW_COUNT();
 	COMMIT;
 	
 	DROP TEMPORARY TABLE IF EXISTS tmp;
 	SELECT
	   CAST(rc AS INT) AS RetValueInt,
	   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_selectAllVisible
DELIMITER //
CREATE PROCEDURE `claimsLogCache_selectAllVisible`(
	IN `_uid` VARCHAR(64),
	IN `_searchString` VARCHAR(64),
	IN `_searchField` VARCHAR(64)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
 	DECLARE msg varchar(255);
 	
   DECLARE author, claimText VARCHAR(50) DEFAULT '%';
   DECLARE ss VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
 	
 	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
   	ROLLBACK; -- vfd pattern for stored procedures
   	RESIGNAL; -- vfd pattern for stored procedures
 	END;
 	
 	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'author' THEN SET author = ss;
   WHEN 'text' THEN SET claimText = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort field.';
   END CASE;
   
   SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT Id,ActorId,author,logged,viewed,'
		        'viewedby, CAST(claimText AS VARCHAR(164)) AS claimText, prevID, marked '
		        'FROM ClaimsLogCatched '
		        'WHERE ActorId = ? '
		        'AND author LIKE ? '
			     'AND claimText LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uid, author, claimText;
	DEALLOCATE PREPARE stm;
 	
 	START TRANSACTION;
 		UPDATE WTFlogs.ClaimsLogCatched AS clc
 				 INNER JOIN tmp ON tmp.Id=clc.ID
 		SET clc.marked = 1;
 		SET rc = ROW_COUNT();
 	COMMIT;
 	
 	DROP TEMPORARY TABLE IF EXISTS tmp;
 	SELECT
	   CAST(rc AS INT) AS RetValueInt,
	   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogCache_SelectionToggle
DELIMITER //
CREATE PROCEDURE `claimsLogCache_SelectionToggle`(
	IN `_id_cache_rec` INT
)
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _id_cache_rec = IFNULL(_id_cache_rec, 0);

 START TRANSACTION; -- vfd pattern for stored procedures 
   UPDATE WTFlogs.ClaimsLogCatched
   SET marked = IF(marked=0,1,0)
   WHERE ID=_id_cache_rec;
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLogEntryReviewedToggle
DELIMITER //
CREATE PROCEDURE `claimsLogEntryReviewedToggle`(
	IN `_uId` VARCHAR(50),
	IN `_recId` INT
)
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _recId = IFNULL(_recId, 0);

 START TRANSACTION; -- vfd pattern for stored procedures 
   UPDATE ClaimsLog
   SET viewed = IF(viewed IS NULL,NOW(),NULL),
   	 viewedby = _uid
   WHERE ID=_recID;
   
	   
   UPDATE ClaimsLogCatched
   SET viewed = IF(viewed IS NULL,NOW(),NULL),
   	 viewedby = _uid
   WHERE prevID=_recID;
   
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.claimsLog_addEntry
DELIMITER //
CREATE PROCEDURE `claimsLog_addEntry`(
	IN `_author` VARCHAR(50),
	IN `_claimText` VARCHAR(1024)
)
    MODIFIES SQL DATA
BEGIN
	DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     RESIGNAL; -- vfd pattern for stored procedures
   END;

   START TRANSACTION;
  		INSERT INTO WTFlogs.ClaimsLog (author, claimText)
   	VALUES (_author, _claimText);
   COMMIT;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_Build
DELIMITER //
CREATE PROCEDURE `eventsLogCache_Build`(
	IN `_uid` VARCHAR(50),
	IN `_wfid` VARCHAR(50),
	IN `_numDays` INT
)
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _wfid = IFNULL(_wfid, '');

 START TRANSACTION; -- vfd pattern for stored procedures 
   INSERT INTO GeneralLogCatched (actorId,machineName,appIdent,logged,_level,
	                               message,logger,properties,callsite,_exception,
											 url,reqhost,uId,prevID)
   SELECT _uid,gl.machineName,gl.appIdent,gl.logged,gl._level,
          gl.message,gl.logger,gl.properties,gl.callsite,gl._exception,
          gl.url,gl.reqhost,gl.uId,gl.ID
   FROM GeneralLog gl
   WHERE gl.logged>DATE_SUB(NOW(),INTERVAL _numDays HOUR);
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_Drop
DELIMITER //
CREATE PROCEDURE `eventsLogCache_Drop`(
	IN `_uid` varchar(50),
	IN `_wfid` varchar(50)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _wfid = IFNULL(_wfid, '');

 START TRANSACTION; -- vfd pattern for stored procedures 
   DELETE cacheControls
   FROM cacheControls
   WHERE uid = _uid;
   INSERT INTO cacheControls (uid, wfid)
   VALUES (_uid, _wfid);

   DELETE GeneralLogCatched
   FROM GeneralLogCatched
   WHERE actorId = _uid;
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_GetCmbFilter
DELIMITER //
CREATE PROCEDURE `eventsLogCache_GetCmbFilter`(
	IN `_uid` VARCHAR(50),
	IN `_template` VARCHAR(500)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc INT DEFAULT 0;
 DECLARE flt VARCHAR(500) DEFAULT NULL;
 
 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _template = IFNULL(_template, 'not defined');

 START TRANSACTION; -- vfd pattern for stored procedures 
   IF NOT EXISTS (SELECT * FROM combinedFilters WHERE uid=_uid AND subsys='eventsLog') THEN
     INSERT INTO combinedFilters (uid, filter, subsys)
     VALUES (_uid,_template,'eventsLog');
   END IF;
 COMMIT; -- vfd pattern for stored procedures

 SELECT  CAST(rc AS INT) AS RetValueInt, CAST(filter AS VARCHAR(500)) AS RetValueString
 FROM combinedFilters
 WHERE uid=_uid AND subsys='eventsLog';
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_GetCount
DELIMITER //
CREATE PROCEDURE `eventsLogCache_GetCount`(
	IN `_uid` varchar(50),
	IN `_searchString` varchar(50),
	IN `_wfid` varchar(50)
)
    READS SQL DATA
BEGIN
	DECLARE _cnt int UNSIGNED DEFAULT 0;
	DECLARE _ss char(50);
	
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	-- SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = '=== TST POINT 01 ===';
	
	SET _ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	
	SELECT
	 COUNT(*) INTO _cnt
	FROM cacheControls
	WHERE uid = _uid
	AND wfid = _wfid;
	IF _cnt <> 1 THEN
	 SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Another instance of this page is active under your login.';
	END IF;

	SELECT COUNT(*) INTO _cnt
   FROM GeneralLogCatched glc
   WHERE glc.actorId = _uid
         AND glc.appIdent LIKE _ss;
   
  SELECT
    CAST(_cnt AS INT) AS RetValueInt,
    CAST(_uid AS char(50)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_GetEntry
DELIMITER //
CREATE PROCEDURE `eventsLogCache_GetEntry`(
	IN `_recId` INT,
	IN `_uId` VARCHAR(50)
)
    READS SQL DATA
BEGIN
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;

	SET _uid = IFNULL(_uid, '');
   SET _recId = IFNULL(_recId, 0);

	SELECT glc.ID, glc.machineName, glc.appIdent, glc.logged, glc._level, glc.message,
	       glc.logger, glc.properties, glc.callsite, glc._exception, glc.url,
	       glc.reqhost, glc.uId, glc.prevID, CAST(0 AS UNSIGNED) AS numRecs
	FROM GeneralLogCatched glc
	WHERE glc.ID = _recId and
	      glc.actorID = _uId;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_GetPage
DELIMITER //
CREATE PROCEDURE `eventsLogCache_GetPage`(
	IN `_uid` varchar(50),
	IN `_searchString` varchar(50),
	IN `_CurrentSort` varchar(50),
	IN `_SortDirection` varchar(50),
	IN `_pageIndex` int,
	IN `_pageSize` int
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
    
	SET _pageIndex = IFNULL(_pageIndex, 0);
	SET _pageSize = IFNULL(_pageSize, 2147483647);
	
	SET @ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%'),
	@srt = CONCAT(_CurrentSort, _SortDirection);

	SET @stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	            'SELECT Id,ActorId,machineName,appIdent,logged,'
			      '_level,message,logger,properties,callsite,'
				   '_exception,url,reqhost,uId,prevID '
				   'FROM GeneralLogCatched '
				   'WHERE ActorId = ? '
				   'AND appIdent LIKE ?;';
	PREPARE stm FROM @stmt;
	EXECUTE stm USING _uid, @ss;

	SET @off = (_pageIndex - 1) * _pageSize,
	@ps = _pageSize;
	
	CASE @srt
	WHEN 'sortA_' THEN PREPARE stm FROM 'select tmp.*, CAST(0 AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged asc
	limit ? offset ?;';
	  EXECUTE stm USING @ps, @off;
	WHEN 'sortA_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(0 AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged desc
	limit ? offset ?;';
	  EXECUTE stm USING @ps, @off;
	WHEN 'sortB_' THEN PREPARE stm FROM 'select tmp.*, CAST(0 AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.appIdent asc
	limit ? offset ?;';
	  EXECUTE stm USING @ps, @off;
	WHEN 'sortB_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(0 AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.appIdent desc
	limit ? offset ?;';
	  EXECUTE stm USING @ps, @off;
	WHEN 'sortC_' THEN PREPARE stm FROM 'select tmp.*, CAST(0 AS UNSIGNED) AS numRecs
	from tmp
	order by tmp._level asc
	limit ? offset ?;';
	  EXECUTE stm USING @ps, @off;
	WHEN 'sortC_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(0 AS UNSIGNED) AS numRecs
	from tmp
	order by tmp._level desc
	limit ? offset ?;';
	  EXECUTE stm USING @ps, @off;
	ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort order.';
	END CASE;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_GetPage_wcmpflt
DELIMITER //
CREATE PROCEDURE `eventsLogCache_GetPage_wcmpflt`(
	IN `_uid` varchar(50),
	IN `_searchString` varchar(50),
	IN `_CurrentSort` varchar(50),
	IN `_SortDirection` varchar(50),
	IN `_pageIndex` int,
	IN `_pageSize` int
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE flt VARCHAR(500) DEFAULT 'not defined';
   DECLARE machineName, _level, message, logger, _exception VARCHAR(50) DEFAULT '%';
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%'),
	    srt = CONCAT(_CurrentSort, _SortDirection);
	
	SET flt := eventsLogCache_GetCmbFilter_base(_uid);
   CALL eventsLogCache_parseCmbFilter_new(flt); -- result in TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50))
   SELECT _filter INTO machineName FROM temp_parse WHERE _field='machineName';
   SELECT _filter INTO _level FROM temp_parse WHERE _field='level';
   SELECT _filter INTO message FROM temp_parse WHERE _field='message';
   SELECT _filter INTO logger FROM temp_parse WHERE _field='logger';
   SELECT _filter INTO _exception FROM temp_parse WHERE _field='exception';
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT Id,ActorId,machineName,appIdent,logged,'
		        '_level,message,logger,properties,callsite,'
			     '_exception,url,reqhost,uId,prevID '
		        'FROM GeneralLogCatched '
		        'WHERE ActorId = ? '
		        'AND appIdent LIKE ? '
			     'AND machineName LIKE ? '
				  'AND _level LIKE ? '
				  'AND message LIKE ? '
				  'AND logger LIKE ? '
				  'AND _exception LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uid, ss, machineName, _level, message, logger, _exception;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SET _pageIndex = IFNULL(_pageIndex, 0);
	SET _pageSize = IFNULL(_pageSize, 2147483647);
	
	SET off = (_pageIndex - 1) * _pageSize,
	ps = _pageSize;
	
	CASE srt
	WHEN 'sortA_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortA_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.appIdent asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.appIdent desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp._level asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp._level desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort order.';
	END CASE;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_GetPage_wcmpflt01
DELIMITER //
CREATE PROCEDURE `eventsLogCache_GetPage_wcmpflt01`(
	IN `_uid` varchar(50),
	IN `_searchString` varchar(50),
	IN `_CurrentSort` varchar(50),
	IN `_SortDirection` varchar(50),
	IN `_pageIndex` int,
	IN `_pageSize` int,
	IN `_searchField` VARCHAR(50)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE flt VARCHAR(500) DEFAULT 'not defined';
   DECLARE appIdent, machineName, _level, message, logger, _exception, callsite VARCHAR(50) DEFAULT '%';
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET flt := eventsLogCache_GetCmbFilter_base(_uid);
   CALL eventsLogCache_parseCmbFilter_new(flt); -- result in TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50))
   SELECT _filter INTO appIdent FROM temp_parse WHERE _field='appIdent';
   SELECT _filter INTO machineName FROM temp_parse WHERE _field='machineName';
   SELECT _filter INTO _level FROM temp_parse WHERE _field='level';
   SELECT _filter INTO message FROM temp_parse WHERE _field='message';
   SELECT _filter INTO logger FROM temp_parse WHERE _field='logger';
   SELECT _filter INTO _exception FROM temp_parse WHERE _field='exception';
   SELECT _filter INTO callsite FROM temp_parse WHERE _field='callsite';
   
   SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'appIdent' THEN SET appIdent := ss;
   WHEN 'machineName' THEN SET machineName := ss;
   WHEN 'level' THEN SET _level := ss;
   WHEN 'message' THEN SET message := ss;
   WHEN 'logger' THEN SET logger := ss;
   WHEN 'exception' THEN SET _exception := ss;
   WHEN 'callsite' THEN SET callsite := ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort field.';
   END CASE;

	SET srt = CONCAT(_CurrentSort, _SortDirection);
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT Id,ActorId,machineName,appIdent,logged,'
		        '_level,message,logger,properties,callsite,'
			     '_exception,url,reqhost,uId,prevID '
		        'FROM GeneralLogCatched '
		        'WHERE ActorId = ? '
		        'AND appIdent LIKE ? '
			     'AND machineName LIKE ? '
				  'AND _level LIKE ? '
				  'AND message LIKE ? '
				  'AND logger LIKE ? '
				  'AND _exception LIKE ? '
				  'AND callsite LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uid, appIdent, machineName, _level, message, logger, _exception, callsite;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SET _pageIndex = IFNULL(_pageIndex, 0);
	SET _pageSize = IFNULL(_pageSize, 2147483647);
	
	SET off = (_pageIndex - 1) * _pageSize,
	ps = _pageSize;
	
	CASE srt
	WHEN 'sortA_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortA_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.logged desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.appIdent asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.appIdent desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp._level asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp._level desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort order.';
	END CASE;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_parseCmbFilter
DELIMITER //
CREATE PROCEDURE `eventsLogCache_parseCmbFilter`(
	IN `_flt` VARCHAR(500)
)
    MODIFIES SQL DATA
BEGIN                                
  DECLARE _wrd, _fict, _val, _ln VARCHAR(50) DEFAULT '';
  DECLARE _i INT DEFAULT 1;
  
  DECLARE EXIT HANDLER -- vfd pattern for stored procedures
  FOR SQLEXCEPTION
  BEGIN
    RESIGNAL; -- vfd pattern for stored procedures
  END;
  
  DROP TEMPORARY TABLE IF EXISTS temp_parse;                                
  CREATE TEMPORARY TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50));   
  
  SIGNAL SQLSTATE '45000' SET
      MYSQL_ERRNO = 31001,
      MESSAGE_TEXT = 'Procedure eventsLogCache_parseCmbFilter is outdated, use eventsLogCache_parseCmbFilter_new.';
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_parseCmbFilter_new
DELIMITER //
CREATE PROCEDURE `eventsLogCache_parseCmbFilter_new`(
	IN `_flt` VARCHAR(500)
)
    MODIFIES SQL DATA
BEGIN                                
  DECLARE _wrd, _fict, _val, _ln VARCHAR(50) DEFAULT '';
  DECLARE _i INT DEFAULT 1;
  DECLARE _pos INT DEFAULT 0;
  
  DECLARE EXIT HANDLER -- vfd pattern for stored procedures
  FOR SQLEXCEPTION
  BEGIN
    RESIGNAL; -- vfd pattern for stored procedures
  END;
  
  DROP TEMPORARY TABLE IF EXISTS temp_parse;                                
  CREATE TEMPORARY TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50));   
  
  SET _flt := explodePrepare(_flt,' ');
  
  SET _ln := explodeOccurence(_flt,'\n',_i);
  SET _ln := REGEXP_REPLACE(_ln, '\n', '');
  SET _i := 1;
  WHILE _ln <> '' AND _i <= 100 DO
    SET _ln = RTRIM(LTRIM(_ln));
    SET _wrd := RTRIM(LTRIM(SUBSTRING(_ln FROM 1 FOR LOCATE(' ',_ln))));
    SET _pos := LOCATE('like',_ln);
    IF _pos <> 0 THEN
	   SET _val := RTRIM(LTRIM(SUBSTRING(_ln,_pos+CHAR_LENGTH('like')+1)));
      IF _val IS NULL THEN
        SET _val := '';
      END IF;
    ELSE
      SET _wrd := '';
    END IF;
    
    IF _wrd <> '' THEN
      INSERT INTO temp_parse (_field,_filter)
      VALUES (_wrd,_val);
    END IF;
    
    SET _i := _i+1;
    SET _ln := explodeOccurence(_flt,'\n',_i);
    SET _ln := LTRIM(RTRIM(REGEXP_REPLACE(_ln, '\n', '')));
  END WHILE;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.eventsLogCache_UpdateCmbFilter
DELIMITER //
CREATE PROCEDURE `eventsLogCache_UpdateCmbFilter`(
	IN `_uid` VARCHAR(50),
	IN `_filter` VARCHAR(500)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc INT DEFAULT 0;
  
 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');
 SET _filter = IFNULL(_filter, 'not defined');

 START TRANSACTION; -- vfd pattern for stored procedures 
   UPDATE combinedFilters
     SET filter=_filter
     WHERE uid=_uid AND subsys='eventsLog';
   IF ROW_COUNT()=0 THEN
     INSERT INTO combinedFilters (uid, filter, subsys)
     VALUES (_uid,_filter,'eventsLog');
   END IF;
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST(_filter AS VARCHAR(500)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.NLog_AddEntry_p
DELIMITER //
CREATE PROCEDURE `NLog_AddEntry_p`(
	IN `machineName` varchar(255),
	IN `appIdent` varchar(300),
	IN `logged` datetime,
	IN `lvl` varchar(15),
	IN `message` varchar(1024),
	IN `logger` varchar(300),
	IN `properties` varchar(1024),
	IN `callsite` varchar(300),
	IN `expn` varchar(4096),
	IN `url` varchar(1024),
	IN `reqhost` varchar(1024),
	IN `uId` varchar(300)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255);

   DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
     SELECT
       CAST(rc AS UNSIGNED) AS RetValueInt,
       CAST(msg AS char(64)) AS RetValueString;
     RESIGNAL; -- vfd pattern for stored procedures
   END;

   START TRANSACTION;
  		INSERT INTO WTFlogs.GeneralLog (machineName, logged, _level, message, logger,
      								        properties, callsite, _exception, url,
              								  reqhost, uId, appIdent)
   	VALUES (machineName, logged, lvl, message, logger, properties, callsite, expn, url, reqhost, uId, appIdent);
   COMMIT;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.NLog_AddEntry_Perm
DELIMITER //
CREATE PROCEDURE `NLog_AddEntry_Perm`(
	IN `appIdent` varchar(300),
	IN `logged` datetime,
	IN `lvl` varchar(15),
	IN `message` varchar(1024)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255);

   DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
     SELECT
       CAST(rc AS UNSIGNED) AS RetValueInt,
       CAST(msg AS char(64)) AS RetValueString;
     RESIGNAL; -- vfd pattern for stored procedures
   END;

   START TRANSACTION;
  		INSERT INTO WTFlogs.GeneralLogPermanent (appIdent, logged, _level, message)
   	VALUES (appIdent, logged, lvl, message);
   COMMIT;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.NLog_AddEntry_p_simple
DELIMITER //
CREATE PROCEDURE `NLog_AddEntry_p_simple`(
	IN `appIdent` varchar(300),
	IN `lvl` varchar(15),
	IN `message` varchar(1024),
	IN `logger` varchar(300)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255);
   
   DECLARE `machineName` varchar(255) DEFAULT('');
   DECLARE `properties` varchar(1024) DEFAULT('');
	DECLARE `callsite` varchar(300) DEFAULT('');
	DECLARE `expn` varchar(4096) DEFAULT('');
	DECLARE `url` varchar(1024) DEFAULT('');
	DECLARE `reqhost` varchar(1024) DEFAULT('');
	DECLARE `uId` varchar(300) DEFAULT ('');

   DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
     SELECT
       CAST(rc AS UNSIGNED) AS RetValueInt,
       CAST(msg AS char(64)) AS RetValueString;
     RESIGNAL; -- vfd pattern for stored procedures
   END;

   START TRANSACTION;
  		INSERT INTO WTFlogs.GeneralLog (machineName, logged, _level, message, logger,
      								        properties, callsite, _exception, url,
              								  reqhost, uId, appIdent)
   	VALUES (machineName, logged, lvl, message, logger, properties, callsite, expn, url, reqhost, uId, appIdent);
   COMMIT;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.NLog_AddRecord
DELIMITER //
CREATE PROCEDURE `NLog_AddRecord`(
	IN `machineName` varchar(255),
	IN `appIdent` varchar(300),
	IN `logged` datetime,
	IN `lvl` varchar(15),
	IN `message` varchar(1024),
	IN `logger` varchar(300),
	IN `properties` varchar(1024),
	IN `callsite` varchar(300),
	IN `expn` varchar(4096),
	IN `url` varchar(1024),
	IN `reqhost` varchar(1024),
	IN `uId` varchar(300)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255) DEFAULT ('');

   DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
     SELECT
       CAST(rc AS UNSIGNED) AS RetValueInt,
       CAST(msg AS char(64)) AS RetValueString;
     RESIGNAL; -- vfd pattern for stored procedures
   END;

   START TRANSACTION;
  		INSERT INTO WTFlogs.GeneralLog (machineName, logged, _level, message, logger,
      								        properties, callsite, _exception, url,
              								  reqhost, uId, appIdent)
   	VALUES (machineName, logged, lvl, message, logger, properties, callsite, expn, url, reqhost, uId, appIdent);
   COMMIT;
   
   SELECT
      CAST(rc AS UNSIGNED) AS RetValueInt,
      CAST(msg AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure WTFlogs.sa_self_service
DELIMITER //
CREATE PROCEDURE `sa_self_service`()
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   CALL WTFlogs.NLog_AddEntry_Perm('SELF-SERVICE-WTFlogs', NOW(), 'Error', CONCAT(CAST(rc AS VARCHAR(15)), " - ", msg));
 END;
 
 START TRANSACTION;
  
 OPTIMIZE TABLE WTFlogs.ChatHistory;
 ALTER TABLE WTFlogs.ChatHistory ENGINE=InnoDB, ALGORITHM=INPLACE;
 OPTIMIZE TABLE WTFlogs.ClaimsLog;
 ALTER TABLE WTFlogs.ClaimsLog ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 OPTIMIZE TABLE WTFlogs.ClaimsLogCatched;
 ALTER TABLE WTFlogs.ClaimsLogCatched ENGINE=InnoDB, ALGORITHM=INPLACE;
 OPTIMIZE TABLE WTFlogs.GeneralLogCatched;
 ALTER TABLE WTFlogs.GeneralLogCatched ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 OPTIMIZE TABLE WTFlogs.GeneralLogPermanent;
 ALTER TABLE WTFlogs.GeneralLogPermanent ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 DELETE FROM WTFlogs.GeneralLog
 WHERE DATEDIFF(NOW(),logged)>30*3
 ;
 OPTIMIZE TABLE WTFlogs.GeneralLog;
 ALTER TABLE WTFlogs.GeneralLog ENGINE=InnoDB, ALGORITHM=INPLACE;
  
 FLUSH TABLES;
 
 CALL WTFlogs.NLog_AddEntry_Perm('SELF-SERVICE-WTFlogs', NOW(), 'Info', 'self-service complited'); 
 
 COMMIT;
 
END//
DELIMITER ;

-- Dumping structure for function WTFlogs.eventsLogCache_GetCmbFilter_base
DELIMITER //
CREATE FUNCTION `eventsLogCache_GetCmbFilter_base`(`_uid` VARCHAR(50)
) RETURNS varchar(500) CHARSET utf8mb4
    READS SQL DATA
whole_proc:
BEGIN
 DECLARE flt VARCHAR(500) DEFAULT NULL;
 
 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 SET _uid = IFNULL(_uid, '');

 SELECT filter
 INTO flt
 FROM combinedFilters
 WHERE uid=_uid AND subsys='eventsLog';
 
 RETURN flt;
END//
DELIMITER ;

-- Dumping structure for function WTFlogs.explodeOccurence
DELIMITER //
CREATE FUNCTION `explodeOccurence`(`_str` VARCHAR(500),
	`_delim` VARCHAR(12),
	`_pos` INT
) RETURNS varchar(255) CHARSET utf8mb4
    READS SQL DATA
BEGIN
	RETURN REPLACE(SUBSTRING(SUBSTRING_INDEX(_str, _delim, _pos),
	       LENGTH(SUBSTRING_INDEX(_str, _delim, _pos-1)) + 1),
	       _delim, '');
END//
DELIMITER ;

-- Dumping structure for function WTFlogs.explodePrepare
DELIMITER //
CREATE FUNCTION `explodePrepare`(`_str` VARCHAR(500),
	`_delim` VARCHAR(12)
) RETURNS varchar(255) CHARSET utf8mb4
    READS SQL DATA
BEGIN
	SET _str := REGEXP_REPLACE(_str, '\n\r|\r\n|\n|\r', '\n');
	SET _str := REGEXP_REPLACE(_str, '(\n){2,}', '\n');
	-- SET _str := REGEXP_REPLACE(_str, '( ){2,}', ' ');
	SET _str := REGEXP_REPLACE(_str, '(\\*){1,}', '%');
	RETURN _str;
END//
DELIMITER ;

-- Dumping structure for event WTFlogs.sa_self_service_call
DELIMITER //
CREATE EVENT `sa_self_service_call` ON SCHEDULE EVERY 1 DAY STARTS '2021-10-20 03:15:00' ON COMPLETION NOT PRESERVE ENABLE DO CALL WTFlogs.sa_self_service//
DELIMITER ;

-- Dumping structure for trigger WTFlogs.ClaimsLog_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `ClaimsLog_before_delete` BEFORE DELETE ON `ClaimsLog` FOR EACH ROW BEGIN
	SIGNAL SQLSTATE '45000' SET MYSQL_ERRNO=00001, MESSAGE_TEXT='Claims Log Records cannot be deleted.';
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger WTFlogs.ClaimsLog_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `ClaimsLog_before_insert` BEFORE INSERT ON `ClaimsLog` FOR EACH ROW BEGIN
	  SET NEW.logged = NOW();
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger WTFlogs.ClaimsLog_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `ClaimsLog_before_update` BEFORE UPDATE ON `ClaimsLog` FOR EACH ROW BEGIN
	IF (NEW.author <> OLD.author)
		|| (NEW.logged <> OLD.logged)
		|| (NEW.claimText <> OLD.claimText)
	THEN
		SIGNAL SQLSTATE '45000' SET MYSQL_ERRNO=00001, MESSAGE_TEXT='Claims Log Records cannot be updated.';
	END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger WTFlogs.gl_bu_tr
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER gl_bu_tr
	BEFORE UPDATE
	ON GeneralLog
	FOR EACH ROW
BEGIN
  SIGNAL SQLSTATE '45000' SET MYSQL_ERRNO=00001, MESSAGE_TEXT='General Log Records cannot be updated.';
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger WTFlogs.gl_i_tr
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER gl_i_tr
	BEFORE INSERT
	ON GeneralLog
	FOR EACH ROW
BEGIN
  SET NEW.properties = CONCAT_WS(' ',NEW.properties,DATE_FORMAT(NEW.logged,'%Y-%m-%d %T.%f'));
  SET NEW.logged = NOW();
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
