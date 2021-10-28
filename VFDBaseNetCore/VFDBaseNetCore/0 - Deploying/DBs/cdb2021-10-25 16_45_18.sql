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

-- Dumping structure for procedure cdb.commonUsersCache_Drop
DELIMITER //
CREATE PROCEDURE `commonUsersCache_Drop`(_uid varchar(50),
_wfid varchar(50))
    MODIFIES SQL DATA
BEGIN
  SET _uid = IFNULL(_uid, '');
  SET _wfid = IFNULL(_wfid, '');

  START TRANSACTION;
    DELETE cacheControls
      FROM cacheControls
    WHERE uid = _uid;
    INSERT INTO cacheControls (uid, wfid)
      VALUES (_uid, _wfid);

    DELETE CachedCommonUsers
      FROM CachedCommonUsers
    WHERE ActorId = _uid;
  COMMIT;

  SELECT
    CAST(5 AS UNSIGNED) AS RetValueInt,
    CAST(_uid AS char(50)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure cdb.commonUsersCache_wSrtAndFlt_Count
DELIMITER //
CREATE PROCEDURE `commonUsersCache_wSrtAndFlt_Count`(
	IN `_uid` varchar(50),
	IN `_currentSearchField` varchar(50),
	IN `_searchString` varchar(50),
	IN `_wfid` varchar(50)
)
    READS SQL DATA
BEGIN
  DECLARE _cnt int UNSIGNED DEFAULT 0;
  DECLARE _ss char(50);
  SET _ss = CONCAT('%', _searchString, '%');
  
  -- SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = '=== TST POINT 02 ===';

  SELECT
    COUNT(*) INTO _cnt
  FROM cacheControls
  WHERE uid = _uid
  AND wfid = _wfid;
  IF _cnt <> 1 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Another instance of this page is active under your login.';
  END IF;

  CASE _currentSearchField
    WHEN 'by EMail' THEN SELECT
          COUNT(*) INTO _cnt
        FROM CachedCommonUsers ccu
        WHERE ccu.ActorId = _uid
        AND ccu.Email LIKE _ss;
    WHEN 'by Name' THEN SELECT
          COUNT(*) INTO _cnt
        FROM CachedCommonUsers ccu
        WHERE ccu.ActorId = _uid
        AND ccu.FullName LIKE _ss;
    ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown search field.';
  END CASE;

  SELECT
    CAST(_cnt AS UNSIGNED) AS RetValueInt,
    CAST(_uid AS char(50)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure cdb.commonUsersCache_wSrtAndFlt_Data
DELIMITER //
CREATE PROCEDURE `commonUsersCache_wSrtAndFlt_Data`(
	IN `_uid` varchar(50),
	IN `_currentSearchField` varchar(50),
	IN `_searchString` varchar(50),
	IN `_CurrentSort` varchar(50),
	IN `_SortDirection` varchar(50),
	IN `_pageIndex` int,
	IN `_pageSize` int
)
    MODIFIES SQL DATA
BEGIN
  SET _pageIndex = IFNULL(_pageIndex, 0);
  SET _pageSize = IFNULL(_pageSize, 2147483647);

  SET @ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%'),
  @srt = CONCAT(_CurrentSort, _SortDirection);

  CASE _currentSearchField
    WHEN 'by EMail' THEN CREATE TEMPORARY TABLE tmp ENGINE = MEMORY
        SELECT
          Id,
          ActorId,
          UserId,
          UserName,
          FullName,
          Email,
          EmailConfirmed,
          PhoneNumber,
          PhoneNumberConfirmed,
          TwoFactorEnabled,
          LockoutEnd,
          LockoutEnabled,
          AccessFailedCount,
          toBeAddressed
        FROM CachedCommonUsers
        WHERE ActorId = _uid
        AND Email LIKE @ss;
    WHEN 'by Name' THEN CREATE TEMPORARY TABLE tmp ENGINE = MEMORY
        SELECT
          Id,
          ActorId,
          UserId,
          UserName,
          FullName,
          Email,
          EmailConfirmed,
          PhoneNumber,
          PhoneNumberConfirmed,
          TwoFactorEnabled,
          LockoutEnd,
          LockoutEnabled,
          AccessFailedCount,
          toBeAddressed
        FROM CachedCommonUsers
        WHERE ActorId = _uid
        AND FullName LIKE @ss;
    ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown search field.';
  END CASE;

  SET @off = (_pageIndex - 1) * _pageSize,
  @ps = _pageSize;

  CASE @srt
    WHEN 'NoNo' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.Email asc
		;';
        EXECUTE stm;
    WHEN 'sortA_' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.Email asc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortA_desc' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.Email desc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortB_' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.FullName asc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortB_desc' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.FullName desc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortC_' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.PhoneNumber asc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortC_desc' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.PhoneNumber desc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortD_' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.LockoutEnd asc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    WHEN 'sortD_desc' THEN PREPARE stm FROM 'select tmp.*, 0 as Selected
		from tmp
		order by tmp.LockoutEnd desc
		limit ? offset ?;';
        EXECUTE stm USING @ps, @off;
    ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort order.';
  END CASE;

  DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure cdb.getTail
DELIMITER //
CREATE PROCEDURE `getTail`(p1 varchar(250))
    MODIFIES SQL DATA
BEGIN
  SET @id = 0;

  START TRANSACTION;
    SELECT
      MIN(id) INTO @id
    FROM ExchangeStore
    WHERE UserUID = p1;

    SELECT
      *
    FROM ExchangeStore
    WHERE ID = @id;

    DELETE ExchangeStore
      FROM ExchangeStore
    WHERE ID = @id;
  COMMIT;
END//
DELIMITER ;

-- Dumping structure for procedure cdb.sa_add_announcement
DELIMITER //
CREATE PROCEDURE `sa_add_announcement`(
	IN `_cnt` TEXT
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255) DEFAULT '';

   DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     RESIGNAL;
   END;
    
   SET _cnt = LTRIM(RTRIM(_cnt));
    
   START TRANSACTION;
		
		INSERT INTO cdb._announcements (content, DateIn)
	   VALUES (_cnt, NOW());

   COMMIT;

   SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(RTRIM(LTRIM(msg)) AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure cdb.sa_add_loginsdisabler
DELIMITER //
CREATE PROCEDURE `sa_add_loginsdisabler`(
	IN `_cnt` TEXT
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255) DEFAULT '';

   DECLARE EXIT HANDLER
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK;
     RESIGNAL;
   END;
    
   SET _cnt = LTRIM(RTRIM(_cnt));
    
   START TRANSACTION;
		
		INSERT INTO cdb._loginsdisablers (content, DateIn)
	   VALUES (_cnt, NOW());

   COMMIT;

   SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(RTRIM(LTRIM(msg)) AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure cdb.sa_get_latest_announcement
DELIMITER //
CREATE PROCEDURE `sa_get_latest_announcement`()
    READS SQL DATA
whole_proc:
BEGIN
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SELECT *
	FROM cdb._announcements AS an
	WHERE an.Id = (SELECT MAX(an1.Id) FROM cdb._announcements AS an1)
	;

END//
DELIMITER ;

-- Dumping structure for procedure cdb.sa_get_latest_loginsdisabler
DELIMITER //
CREATE PROCEDURE `sa_get_latest_loginsdisabler`()
    READS SQL DATA
whole_proc:
BEGIN
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SELECT *
	FROM cdb._loginsdisablers AS an
	WHERE an.Id = (SELECT MAX(an1.Id) FROM cdb._loginsdisablers AS an1)
	;

END//
DELIMITER ;

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
