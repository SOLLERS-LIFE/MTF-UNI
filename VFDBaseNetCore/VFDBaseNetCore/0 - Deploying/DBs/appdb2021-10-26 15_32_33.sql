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

-- Dumping structure for view appdb.bs_invite_traps_v
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `bs_invite_traps_v` (
	`Id` INT(11) NOT NULL,
	`id_bst` INT(11) NOT NULL,
	`user_email` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
	`_state` TINYINT(4) NOT NULL,
	`_stateVal` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
	`change_id_user` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
	`change_logged` DATETIME(6) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view appdb.bs_marks_v
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `bs_marks_v` (
	`Id` INT(11) NOT NULL,
	`mark_name` VARCHAR(127) NOT NULL COLLATE 'utf8mb4_general_ci',
	`id_team` INT(11) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view appdb.bs_teams_v
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `bs_teams_v` (
	`Id` INT(11) NOT NULL,
	`name` VARCHAR(127) NOT NULL COLLATE 'utf8mb4_general_ci'
) ENGINE=MyISAM;

-- Dumping structure for view appdb.bs_team_users_v_
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `bs_team_users_v_` (
	`id_bst` INT(11) NOT NULL,
	`id_user` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
	`_role` TINYINT(4) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for view appdb.TGO_trivial_table_v
-- Creating temporary table to overcome VIEW dependency errors
CREATE TABLE `TGO_trivial_table_v` (
	`Id` INT(11) NOT NULL,
	`ActionDate` DATETIME(6) NOT NULL,
	`ExactDate` DATETIME(6) NOT NULL,
	`Selected` TINYINT(1) NOT NULL
) ENGINE=MyISAM;

-- Dumping structure for procedure appdb.bm_marks_user_marks
DELIMITER //
CREATE PROCEDURE `bm_marks_user_marks`(
	IN `_id_user` VARCHAR(80)
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SELECT DISTINCT bms.Id AS Id, bms.mark_name AS mark_name,
	                CAST(0 AS INT) AS numRecs, CAST('' AS VARCHAR(50)) AS accessedBy, CAST('' AS VARCHAR(50)) AS team_name
	FROM (appdb.bs_marks_v AS bms
	JOIN appdb.bs_team_marks AS mtt ON mtt.id_mark = bms.Id)
	WHERE
	EXISTS(
	SELECT 1
	FROM appdb.bs_team_users AS tu
	WHERE tu.id_bst = mtt.id_bst AND tu.id_user = _id_user
	LIMIT 1) AND 
	NOT EXISTS(
	SELECT 1
	FROM appdb.bs_marks_to_users AS mtu
	WHERE mtu.id_mark = bms.Id AND mtu.id_user = _id_user
	LIMIT 1)
	ORDER BY bms.mark_name;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_marks_v_get
DELIMITER //
CREATE PROCEDURE `bm_marks_v_get`(
	IN `_id_team` INT,
	IN `_searchString` VARCHAR(80)
)
    READS SQL DATA
whole_proc:
BEGIN
   
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%', _searchString, '%');
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT bm.Id, bm.mark_name, bm.id_team as id_team, t.name as team_name '
		        'FROM bs_marks_v as bm '
		        'INNER JOIN appdb.bs_teams_v as t on t.Id = bm.id_team '
		        'WHERE bm.mark_name LIKE ? AND bm.id_team = ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING ss, _id_team;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST(? AS VARCHAR(1024)) AS accessedBy
	from tmp
	order by tmp.mark_name asc;';
	EXECUTE stm USING total_recs, '';

	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_marks_v_get_all
DELIMITER //
CREATE PROCEDURE `bm_marks_v_get_all`(
	IN `_searchString` VARCHAR(80)
)
    READS SQL DATA
whole_proc:
BEGIN
   
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%', _searchString, '%');
	
	CREATE TEMPORARY TABLE tmp ENGINE=Innodb
	SELECT bm.Id, bm.mark_name, bm.id_team as id_team, t.name as team_name
	FROM appdb.bs_marks_v as bm
	INNER JOIN appdb.bs_teams_v as t on t.Id = bm.id_team
	WHERE bm.mark_name LIKE ss
	;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	select tmp.*, CAST(total_recs AS UNSIGNED) AS numRecs, CAST('' AS VARCHAR(1024)) AS accessedBy
	from tmp
	order by tmp.mark_name asc
	;
		
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_mark_add
DELIMITER //
CREATE PROCEDURE `bm_mark_add`(
	IN `_name` VARCHAR(64),
	IN `_id_team` INT
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE rc int DEFAULT 0;
   DECLARE msg varchar(255);
   DECLARE _id_mark INT DEFAULT 0;

   DECLARE EXIT HANDLER -- vfd pattern for stored procedures
   FOR SQLEXCEPTION
   BEGIN
     ROLLBACK; -- vfd pattern for stored procedures
      -- GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
      -- SELECT
        -- CAST(rc AS UNSIGNED) AS RetValueInt,
        -- CAST(msg AS char(64)) AS RetValueString;
     RESIGNAL; -- vfd pattern for stored procedures
   END;
    
   SET _name = LTRIM(RTRIM(_name));
    
   START TRANSACTION; -- vfd pattern for stored procedures 
		
		SELECT id
		FROM appdb.bs_marks AS m
		WHERE m.name = _name
		INTO _id_mark;
		   
		IF _id_mark = 0 THEN
	      INSERT INTO appdb.bs_marks (name)
	      VALUES (LTRIM(RTRIM(_name)));
	      SET _id_mark = LAST_INSERT_ID();
		END IF;
	       
	   INSERT INTO appdb.bs_team_marks (id_bst, id_mark)
	   VALUES (_id_team, _id_mark);

   COMMIT; -- vfd pattern for stored procedures

   SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(RTRIM(LTRIM(_name)) AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_mark_delete
DELIMITER //
CREATE PROCEDURE `bm_mark_delete`(
	IN `_id` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    DELETE FROM bs_marks
    WHERE Id=_id;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_id AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_mark_exclude
DELIMITER //
CREATE PROCEDURE `bm_mark_exclude`(
	IN `_idMark` INT,
	IN `_idTeam` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    DELETE FROM appdb.bs_team_marks
    WHERE id_mark = _idMark AND
	       id_bst = _idTeam;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('OK' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_mark_rename
DELIMITER //
CREATE PROCEDURE `bm_mark_rename`(
	IN `_id` INT,
	IN `_newName` VARCHAR(64)
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    UPDATE bs_marks
    SET NAME = RTRIM(LTRIM(_newName))
    WHERE Id=_id;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_id AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_mark_user_allow
DELIMITER //
CREATE PROCEDURE `bm_mark_user_allow`(
	IN `_email` VARCHAR(127),
	IN `_id_user` VARCHAR(64),
	IN `_id_team` INT,
	IN `_id_mark` INT
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
      RESIGNAL;
    END;
    
    START TRANSACTION;

    DELETE FROM appdb.bs_marks_to_users
    WHERE id_user=_id_user
         AND id_team=_id_team
         AND id_mark=_id_mark
    ;

    COMMIT;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bm_mark_user_disallow
DELIMITER //
CREATE PROCEDURE `bm_mark_user_disallow`(
	IN `_email` VARCHAR(127),
	IN `_id_user` VARCHAR(64),
	IN `_id_team` INT,
	IN `_id_mark` INT
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
      RESIGNAL;
    END;
    
    START TRANSACTION;

    INSERT INTO appdb.bs_marks_to_users (id_user, id_team, id_mark)
    VALUES (_id_user, _id_team, _id_mark)
    ;

    COMMIT;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_teams_invitations_number
DELIMITER //
CREATE PROCEDURE `bs_teams_invitations_number`()
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    SET rc = (SELECT COUNT(*) FROM appdb.bs_invite_traps_v tv WHERE tv._state=0);

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('OK' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_teams_members_number
DELIMITER //
CREATE PROCEDURE `bs_teams_members_number`()
    READS SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    SET rc = (SELECT COUNT(*) FROM appdb.bs_team_users_v_);

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('OK' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_teams_number
DELIMITER //
CREATE PROCEDURE `bs_teams_number`()
    READS SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    SET rc = (SELECT COUNT(*) FROM appdb.bs_teams_v);

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('OK' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_add
DELIMITER //
CREATE PROCEDURE `bs_team_add`(
	IN `_name` VARCHAR(64)
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
      -- GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
      -- SELECT
        -- CAST(rc AS UNSIGNED) AS RetValueInt,
        -- CAST(msg AS char(64)) AS RetValueString;
      RESIGNAL; -- vfd pattern for stored procedures
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    INSERT INTO bs_teams (name)
    VALUES (LTRIM(RTRIM(_name)));

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_name AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_configs_list
DELIMITER //
CREATE PROCEDURE `bs_team_configs_list`(
	IN `_id_team` INT
)
whole_proc:
BEGIN
 DECLARE rc int DEFAULT -1;
 DECLARE msg varchar(255);
 DECLARE val VARCHAR(127) DEFAULT "";

 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   RESIGNAL;
 END;
 
 START TRANSACTION;

	SELECT tc.id_team, tc.id_cnf, tca.cnfName, ts.name AS teamName, LTRIM(RTRIM(tc.cnfValue)) AS cnfValue
	FROM appdb.bs_team_configs AS tc
	     INNER JOIN appdb.bs_teams_v AS ts ON ts.Id = tc.id_team
	     INNER JOIN appdb.bs_team_configs_avl AS tca ON tca.Id = tc.id_cnf
	WHERE tc.id_team = _id_team
	ORDER BY tca.cnfName
	;
 
 COMMIT;
 
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_config_currentuser_get
DELIMITER //
CREATE PROCEDURE `bs_team_config_currentuser_get`(
	IN `_cnfName` VARCHAR(15)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);
    DECLARE val INT DEFAULT 0;
    DECLARE _id_team INT DEFAULT 0;

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION;

		SELECT MIN(CAST(tc.cnfValue AS INT))
		FROM appdb.bs_team_configs AS tc
		     INNER JOIN appdb.bs_teams_v AS ts ON ts.Id = tc.id_team
		     INNER JOIN appdb.bs_team_configs_avl AS tca ON tca.Id = tc.id_cnf
		WHERE tca.cnfName = LTRIM(RTRIM(_cnfName))
		INTO val
		;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(val AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_config_get
DELIMITER //
CREATE PROCEDURE `bs_team_config_get`(
	IN `_id_team` INT,
	IN `_id_cnf` INT
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);
    DECLARE val VARCHAR(127) DEFAULT "";

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION;

		SELECT LTRIM(RTRIM(uc.cnfValue))
		FROM appdb.bs_team_configs AS tc
		WHERE tc.id_team = _id_team
		      AND tc.id_cnf = _id_cnf
		INTO val
		;
		
		IF (FOUND_ROWS()=0)
		THEN
		  SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Team Configuration parameter doesn\'t exists.';
		END IF;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(val AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_config_set
DELIMITER //
CREATE PROCEDURE `bs_team_config_set`(
	IN `_id_team` INT,
	IN `_id_cnf` INT,
	IN `_val` VARCHAR(127)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    SET _val = LTRIM(RTRIM(_val));
    
    START TRANSACTION;

		UPDATE appdb.bs_team_configs
		SET cnfValue = _val
		WHERE id_team = _id_team
		      AND id_cnf = _id_cnf
		;
		
		IF ROW_COUNT()=0 THEN
		  SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Team Configuration parameter doesn\'t exists.';
		END IF;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_val AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_delete
DELIMITER //
CREATE PROCEDURE `bs_team_delete`(
	IN `_id` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    DELETE FROM bs_teams
    WHERE Id=_id;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_id AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_invitation_add
DELIMITER //
CREATE PROCEDURE `bs_team_invitation_add`(
	IN `_email` VARCHAR(64),
	IN `_id_bst` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL; -- vfd pattern for stored procedures
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    INSERT INTO appdb.bs_invite_traps (id_bst, user_email)
    VALUES (_id_bst, RTRIM(LTRIM(_email)));
    
    IF EXISTS(SELECT *
	 			  FROM appdb.bs_invite_traps AS it
				  WHERE it.id_bst=_id_bst AND
				        it.user_email=RTRIM(LTRIM(_email)) AND
						  it._state=1)
    THEN
       	SET rc := 1;
    END IF;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(RTRIM(LTRIM(_email)) AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_invitation_cancel
DELIMITER //
CREATE PROCEDURE `bs_team_invitation_cancel`(
	IN `_idInv` INT,
	IN `_idTeam` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    UPDATE appdb.bs_invite_traps AS it
    SET it._state = 2
    WHERE it.Id=_idInv AND
	       it.id_bst=_idTeam;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST('OK' AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_member_change_role
DELIMITER //
CREATE PROCEDURE `bs_team_member_change_role`(
	IN `_idUser` VARCHAR(64),
	IN `_newRole` TINYINT,
	IN `_idTeam` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    UPDATE appdb.bs_team_users AS tu
    SET tu._role = _newRole
    WHERE tu.id_bst=_idTeam AND
	       tu.id_user=_idUser;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_idUser AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_member_exclude
DELIMITER //
CREATE PROCEDURE `bs_team_member_exclude`(
	IN `_idUser` VARCHAR(64),
	IN `_idTeam` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    DELETE FROM appdb.bs_team_users
    WHERE id_bst=_idTeam AND
	       id_user=_idUser;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_idUser AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_team_rename
DELIMITER //
CREATE PROCEDURE `bs_team_rename`(
	IN `_id` INT,
	IN `_newName` VARCHAR(64)
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; 
      RESIGNAL;
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    UPDATE bs_teams
    SET NAME = RTRIM(LTRIM(_newName))
    WHERE Id=_id;

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_id AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.bs_verify_authority_on
DELIMITER //
CREATE PROCEDURE `bs_verify_authority_on`(
	IN `_idAuth` VARCHAR(50),
	IN `_id` VARCHAR(50)
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
	DECLARE msg varchar(255);
	
	DECLARE EXIT HANDLER
	FOR SQLEXCEPTION
	BEGIN
		ROLLBACK; 
		RESIGNAL;
	END;
	
	START TRANSACTION;
		IF EXISTS(SELECT *
		 			 FROM appdb.bs_team_users AS btu1
					      INNER JOIN appdb.bs_team_users AS btu2 ON btu2.id_bst=btu1.id_bst AND 
																					 btu2.id_user=_idAuth AND
																					 btu2._role=1 -- manager
					WHERE btu1.id_user=_id AND
					      btu1._role<>1) AND
			NOT IsSUbyId(_id)
		THEN
			SET rc:=-1;
		END IF;
	COMMIT;
	
	SELECT
	CAST(rc AS INT) AS RetValueInt,
	CAST(_id AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_invitations_check
DELIMITER //
CREATE PROCEDURE `bt_invitations_check`(
	IN `_email` VARCHAR(64)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
	DECLARE msg varchar(255);
	
	DECLARE done INT DEFAULT FALSE;
	DECLARE _id, _id_bst INT DEFAULT 0;
	
	DECLARE _invits CURSOR
	FOR
	SELECT it.Id, it.id_bst
	FROM appdb.bs_invite_traps AS it
	WHERE it.user_email=LTRIM(RTRIM(_email)) AND
			it._state=0;
	
	DECLARE CONTINUE HANDLER
	FOR NOT FOUND 
	SET done = TRUE;
	
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
	ROLLBACK; 
	RESIGNAL;
	END;
	
	START TRANSACTION; -- vfd pattern for stored procedures 
		OPEN _invits;
		read_loop: LOOP
			FETCH _invits INTO _id, _id_bst;
			IF done THEN
      		LEAVE read_loop;
    		END IF;
    		
    	   INSERT INTO appdb.bs_security_disable (id_user, _state)
			VALUES (WhoIsThis(),1);
			 
			INSERT INTO appdb.bs_team_users (id_bst, id_user, _role)
		   SELECT _id_bst, u.Id, 0
		   FROM cid.AspNetUsers AS u
			WHERE u.NormalizedEmail=LTRIM(RTRIM(_email));
		    
		   CASE ROW_COUNT()
		   WHEN 0 THEN BEGIN END;
		   WHEN 1 THEN
		     	UPDATE appdb.bs_invite_traps AS it
		     	SET it._state=1
		     	WHERE it.Id=_id;
		   END CASE;
		 
			DELETE FROM appdb.bs_security_disable
		   WHERE id_user=WhoIsThis() AND
		         _state=1;
    		
    		
			SET rc := rc + 1;
		END LOOP;
		CLOSE _invits;
	COMMIT; -- vfd pattern for stored procedures
	
	SELECT
	CAST(rc AS INT) AS RetValueInt,
	CAST(_email AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_marks_v_get_byid
DELIMITER //
CREATE PROCEDURE `bt_marks_v_get_byid`(
	IN `_id` INT
)
    READS SQL DATA
whole_proc:
BEGIN
   
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(5000) DEFAULT '';
   DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SELECT bm.Id, bm.mark_name, bm.id_team as id_team, t.name as team_name,
	CAST(0 AS UNSIGNED) AS numRecs, CAST('' AS VARCHAR(1024)) AS accessedBy
	FROM bs_marks_v as bm
	INNER JOIN appdb.bs_teams_v as t on t.Id = bm.id_team
	WHERE bm.Id = _id
	;
	

END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_teams_v_get
DELIMITER //
CREATE PROCEDURE `bt_teams_v_get`(
	IN `_searchString` VARCHAR(80),
	IN `_smth` VARCHAR(80)
)
    READS SQL DATA
whole_proc:
BEGIN
   
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%'); -- , _searchString, '%');
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT bt.Id,bt.name '
		        'FROM bs_teams_v as bt '
		        'WHERE bt.name LIKE ?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING ss;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.name asc;';
	EXECUTE stm USING total_recs;

	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_teams_v_get_byid
DELIMITER //
CREATE PROCEDURE `bt_teams_v_get_byid`(
	IN `_id` INT
)
    READS SQL DATA
whole_proc:
BEGIN
   
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(5000) DEFAULT '';
   DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%'); -- , _searchString, '%');
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT bt.Id,bt.name '
		        'FROM bs_teams_v as bt '
		        'WHERE bt.name LIKE ? '
						  'AND bt.Id=?;';
	PREPARE stm FROM stmt;
	EXECUTE stm USING ss,_id;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.name asc;';
	EXECUTE stm USING total_recs;

	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_teamUsers_get
DELIMITER //
CREATE PROCEDURE `bt_teamUsers_get`(
	IN `_id_team` INT,
	IN `_ss` VARCHAR(64)
)
    READS SQL DATA
whole_proc:
BEGIN
   
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET ss = CONCAT('%',_ss,'%'); -- , _searchString, '%');
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT bt.Id,bt.name '
		        'FROM appdb.bs_team_users as btu '
		        'WHERE (btu.) AND (IsSU() OR '
		              'EXISTS(SELECT * FROM bs_team_users AS btu WHERE btu.id_bst=bt.id and btu.id_user=WhoIsThis()) '
				        ') '
				        'AND (bt.name LIKE ?);';
	PREPARE stm FROM stmt;
	EXECUTE stm USING ss;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs
	from tmp
	order by tmp.name asc;';
	EXECUTE stm USING total_recs;

	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_team_history_get
DELIMITER //
CREATE PROCEDURE `bt_team_history_get`(
	IN `_idTeam` INT,
	IN `_ss` VARCHAR(64)
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET _ss := CONCAT('%',_ss,'%'); -- , _searchString, '%');
	
	CREATE TEMPORARY TABLE tmp ENGINE=Innodb
	SELECT btuv.Id, btuv.id_bst, btuv.user_email,
			 btuv._state, btuv._stateVal,
			 btuv.change_id_user, btuv.change_logged
	FROM appdb.bs_invite_traps_v as btuv
	WHERE btuv.id_bst=_idTeam AND
			btuv._state<>0 AND
	      btuv.user_email LIKE _ss;
		
	-- SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SELECT tmp.* -- , CAST(total_recs AS UNSIGNED) AS numRecs
	FROM tmp
	order by tmp.change_logged DESC, tmp.user_email ASC;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_team_invitations_get
DELIMITER //
CREATE PROCEDURE `bt_team_invitations_get`(
	IN `_idTeam` INT,
	IN `_ss` VARCHAR(64)
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET _ss := CONCAT('%',_ss,'%'); -- , _searchString, '%');
	
	CREATE TEMPORARY TABLE tmp ENGINE=Innodb
	SELECT btuv.Id, btuv.id_bst, btuv.user_email,
			 btuv._state, btuv._stateVal,
			 btuv.change_id_user, btuv.change_logged
	FROM appdb.bs_invite_traps_v as btuv
	WHERE btuv.id_bst=_idTeam AND
			btuv._state=0 AND
	      btuv.user_email LIKE _ss;
		
	-- SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SELECT tmp.* -- , CAST(total_recs AS UNSIGNED) AS numRecs
	FROM tmp
	order by tmp.user_email ASC;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_team_users_get_byid
DELIMITER //
CREATE PROCEDURE `bt_team_users_get_byid`(
	IN `_id_team` INT,
	IN `_ss` VARCHAR(64)
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET _ss := CONCAT('%',_ss,'%'); -- , _searchString, '%');
	
	CREATE TEMPORARY TABLE tmp ENGINE=Innodb
	SELECT btuv.id_bst, btuv.id_user, btuv._role,
			 u.FullName AS NAME, u.Email AS email, u.PhoneNumber AS phone
	FROM appdb.bs_team_users_v_ as btuv
	     INNER JOIN cid.AspNetUsers AS u ON u.Id=btuv.id_user
	WHERE btuv.id_bst=_id_team AND
	      u.FullName LIKE _ss;
	
		
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SELECT tmp.*, CAST(total_recs AS UNSIGNED) AS numRecs
	FROM tmp
	order by tmp.name ASC;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_team_users_marker_allowed
DELIMITER //
CREATE PROCEDURE `bt_team_users_marker_allowed`(
	IN `_team_name` VARCHAR(64),
	IN `_id_mark` INT
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	CREATE TEMPORARY TABLE tmp ENGINE=Innodb
	SELECT btuv.id_bst, btuv.id_user, btuv._role,
			 u.FullName AS NAME, u.Email AS email, u.PhoneNumber AS phone,
			 CAST(0 AS UNSIGNED) AS numRecs
	FROM appdb.bs_team_users_v_ as btuv
		  INNER JOIN appdb.bs_teams t ON t.Id=btuv.id_bst
	     INNER JOIN cid.AspNetUsers AS u ON u.Id=btuv.id_user
	     INNER JOIN appdb.bs_team_marks AS tm ON tm.id_bst=btuv.id_bst
	WHERE t.name=_team_name AND
	      tm.id_mark=_id_mark AND
	      NOT EXISTS(SELECT *
	                 FROM appdb.bs_marks_to_users AS mtu
	                 WHERE mtu.id_mark=_id_mark AND
	                       mtu.id_team=btuv.id_bst AND
	                       mtu.id_user=btuv.id_user)
	;
	
	SELECT * FROM tmp;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
	
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_team_users_marker_disallowed
DELIMITER //
CREATE PROCEDURE `bt_team_users_marker_disallowed`(
	IN `_team_name` VARCHAR(64),
	IN `_id_mark` INT
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE total_recs INT DEFAULT 0;
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	CREATE TEMPORARY TABLE tmp ENGINE=Innodb
	SELECT btuv.id_bst, btuv.id_user, btuv._role,
			 u.FullName AS NAME, u.Email AS email, u.PhoneNumber AS phone,
			 CAST(0 AS UNSIGNED) AS numRecs
	FROM appdb.bs_team_users_v_ as btuv
		  INNER JOIN appdb.bs_teams t ON t.Id=btuv.id_bst
	     INNER JOIN cid.AspNetUsers AS u ON u.Id=btuv.id_user
	     INNER JOIN appdb.bs_team_marks AS tm ON tm.id_bst=btuv.id_bst
	WHERE t.name=_team_name AND
	      tm.id_mark=_id_mark AND
	      EXISTS(SELECT *
	             FROM appdb.bs_marks_to_users AS mtu
	             WHERE mtu.id_mark=_id_mark AND
	                   mtu.id_user=btuv.id_user AND
	                   mtu.id_team=btuv.id_bst)
	;
	
	SELECT * FROM tmp;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.bt_team_user_teams
DELIMITER //
CREATE PROCEDURE `bt_team_user_teams`(
	IN `_id_user` VARCHAR(80)
)
    READS SQL DATA
whole_proc:
BEGIN
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SELECT tms.*, CAST(0 AS INT) AS numRecs
	FROM appdb.bs_team_users AS tu 
	     INNER JOIN appdb.bs_teams_v AS tms ON tms.Id = tu.id_bst
	WHERE tu.id_user = _id_user
	ORDER BY tms.name;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.cr_certIUop_add
DELIMITER //
CREATE PROCEDURE `cr_certIUop_add`(
	IN `_CertNo` VARCHAR(127),
	IN `_Company` VARCHAR(127),
	IN `_CustCode` VARCHAR(127),
	IN `_CustRef` VARCHAR(127),
	IN `_SerialNo` VARCHAR(127),
	IN `_PlantNo` VARCHAR(127),
	IN `_Descr` VARCHAR(127),
	IN `_Range` VARCHAR(127),
	IN `_CLoc` VARCHAR(127),
	IN `_CertDate` DATETIME,
	IN `_CalDate` DATETIME,
	IN `_RecalDate` DATETIME,
	IN `_Preadj` VARCHAR(15),
	IN `_author` VARCHAR(127),
	IN `_WEBLoc` VARCHAR(127)
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION;

    INSERT INTO cr_certsIUops (CertNo,Company,CustCode,CustRef,SerialNo,PlantNo,Descr,Range_,CLoc,
	                             CertDate,CalDate,RecalDate,
										  Preadj,
										  author,
										  WEBLoc)
    VALUES (LTRIM(RTRIM(_CertNo)),LTRIM(RTRIM(_Company)),LTRIM(RTRIM(_CustCode)),LTRIM(RTRIM(_CustRef)),LTRIM(RTRIM(_SerialNo)),LTRIM(RTRIM(_PlantNo)),
	 		   LTRIM(RTRIM(_Descr)),LTRIM(RTRIM(_Range)),LTRIM(RTRIM(_CLoc)),
	         _CertDate,_CalDate,_RecalDate,
				LTRIM(RTRIM(_Preadj)),
				LTRIM(RTRIM(_author)),
				LTRIM(RTRIM(_WEBLoc))
			  );

    COMMIT;
    
    SET rc = LAST_INSERT_ID();

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_CertNo AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.cr_certIUop_file_proceeded
DELIMITER //
CREATE PROCEDURE `cr_certIUop_file_proceeded`(
	IN `_name` VARCHAR(256)
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
      RESIGNAL;
    END;
    
    START TRANSACTION;

    INSERT INTO appdb.cr_certsIUops_files (DateOfUploading, filePath)
    VALUES (NOW(), LTRIM(RTRIM(_name)));

    COMMIT;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_name AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.cr_certIUop_proceed
DELIMITER //
CREATE PROCEDURE `cr_certIUop_proceed`(
	IN `_idOp` INT
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);
    DECLARE opRes INT DEFAULT -1;
    
    DECLARE _CertNo VARCHAR(127);
	 DECLARE _Company VARCHAR(127);
    DECLARE _CustCode VARCHAR(127);
	 DECLARE _CustRef VARCHAR(127);
	 DECLARE _SerialNo VARCHAR(127);
	 DECLARE _PlantNo VARCHAR(127);
	 DECLARE _Descr VARCHAR(127);
	 DECLARE _Range VARCHAR(127);
	 DECLARE _CLoc VARCHAR(127);
	 DECLARE _CertDate DATETIME;
	 DECLARE _CalDate DATETIME;
	 DECLARE _RecalDate DATETIME;
	 DECLARE _Preadj VARCHAR(15);
	 DECLARE _WEBLoc VARCHAR(127);
	 
	 DECLARE _id_team INT DEFAULT -1;
	 DECLARE _id_mark INT DEFAULT -1;
	 DECLARE _id_cert INT DEFAULT -1;

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    START TRANSACTION;
	    IF (NOT EXISTS (SELECT * FROM appdb.cr_certsIUops_results as res WHERE res.id_certsIUop=_idOp )) THEN
	    	 SELECT ops.CertNo, ops.Company, ops.CustCode, ops.CustRef,
	    	        ops.SerialNo, ops.PlantNo, ops.Descr, ops.Range_,
	    	        ops.CLoc,
	    	        ops.CertDate, ops.CalDate, ops.RecalDate,
	    	        ops.Preadj,
	    	        WEBLoc
	    	 FROM appdb.cr_certsIUops AS ops
	    	 WHERE ops.id=_idOp
	    	 INTO _CertNo, _Company, _CustCode, _CustRef,
	    	      _SerialNo, _PlantNo, _Descr, _Range,
	    	      _CLoc, 
	    	      _CertDate, _CalDate, _RecalDate,
	    	      _Preadj,
					_WEBLoc;
	    	 
			 
			 SELECT t.Id FROM appdb.bs_teams AS t WHERE t.name=_CustCode INTO _id_team;     
	    	 IF (_id_team = -1) THEN
	    	   INSERT INTO appdb.bs_teams (name) VALUES (_CustCode);
	    	   SET _id_team = LAST_INSERT_ID(); 
			 END IF; 
			 
			 IF (LTRIM(RTRIM(_WEBLoc)) = '' OR _WEBLoc IS NULL) THEN
	    	     SET _WEBLoc = _CustCode;
			 END IF;
			 SELECT m.Id 
			 FROM appdb.bs_marks AS m 
			 WHERE m.name=_WEBLoc
			 INTO _id_mark; 
			     
	    	 IF (_id_mark = -1) THEN
	    	   INSERT INTO appdb.bs_marks (name) VALUES (_WEBLoc);
	    	   SET _id_mark = LAST_INSERT_ID(); 
	    	 END IF;
			 
			 IF NOT EXISTS(SELECT * FROM appdb.bs_team_marks AS tm WHERE tm.id_bst=_id_team AND tm.id_mark=_id_mark) THEN
			   INSERT INTO appdb.bs_team_marks (id_bst, id_mark)
			   VALUES (_id_team, _id_mark);
			 END IF;
			 
			 SELECT c.id
			 FROM appdb.sco_certs AS c
			 WHERE c.CertNo=_CertNO
			 INTO _id_cert;
			 
			 IF _id_cert = -1 THEN
			   INSERT INTO appdb.sco_certs (CertNo,
				 									  SerialNo, PlantNo,
													  Descr, Range_,
													  CertDate, CalDate, RecalDate,
													  Preadj,
													  id_company, id_location,
													  CLoc)
			   VALUES (_CertNo, 
	    	           _SerialNo, _PlantNo,
							_Descr, _Range, 
	    	            _CertDate, _CalDate, _RecalDate,
	    	            _Preadj,
							_id_team, _id_mark,
							_CLoc);
				SET _id_cert = LAST_INSERT_ID();
				SET opRes = 1;
			 ELSE
			   UPDATE appdb.sco_certs
			   SET SerialNo = _SerialNo, PlantNo = _PlantNo,
					 Descr = _Descr, Range_ = _Range,
					 CertDate = _CertDate, CalDate = _CalDate, RecalDate = _RecalDate,
					 Preadj = Preadj,
					 id_company = _id_team, id_location = _id_mark,
					 CLoc = _CLoc
				WHERE id=_id_cert;
				SET opRes = 2;
			 END IF;
			 
			 INSERT INTO appdb.cr_certsIUops_results (id_certsIUop,
 				                                       state)
 			 VALUES (_idOp,
				 		opRes);
			 
		 END IF;
    COMMIT;
    
    SELECT
      CAST(opRes AS INT) AS RetValueInt,
      CAST(_CertNo AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.cr_TransformInconsistence_add
DELIMITER //
CREATE PROCEDURE `cr_TransformInconsistence_add`(
	IN `_author` VARCHAR(127),
	IN `_errno` INT,
	IN `_descr` VARCHAR(1024),
	IN `_rawRec` TEXT
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; -- vfd pattern for stored procedures
      -- GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
      -- SELECT
        -- CAST(rc AS UNSIGNED) AS RetValueInt,
        -- CAST(msg AS char(64)) AS RetValueString;
      RESIGNAL; -- vfd pattern for stored procedures
    END;
    
    START TRANSACTION; -- vfd pattern for stored procedures 

    INSERT INTO cr_transform_inconsistences (author,errno,descr,rawRec)
    VALUES (LTRIM(RTRIM(author)),_errno,LTRIM(RTRIM(_descr)),_rawRec);

    COMMIT; -- vfd pattern for stored procedures

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_errno AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_certsDatesToFBB
DELIMITER //
CREATE PROCEDURE `sa_certsDatesToFBB`()
    MODIFIES SQL DATA
BEGIN

DECLARE fbb DATE DEFAULT("1991-06-13");

UPDATE appdb.sco_certs
SET certDateFBB = DATEDIFF(CertDate,fbb),
    recalDateFBB = DATEDIFF(RecalDate,fbb)
;

END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_create_appuser
DELIMITER //
CREATE PROCEDURE `sa_create_appuser`(
	IN `nm` varchar(255),
	IN `old_psw` varchar(255),
	IN `default_role_name` varchar(255)
)
    MODIFIES SQL DATA
whole_proc:
  BEGIN
    DECLARE rc,
            num_rec int DEFAULT 0;
    DECLARE msg,
            pwd,
            pwd_hash,
            pwd_hash_ed25519 varchar(255);

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

    START TRANSACTION; -- vfd pattern for stored procedures 
      -- check if user exists and password is correct
      SET pwd_hash = PASSWORD(old_psw);
      SET pwd_hash_ed25519 = ed25519_password(old_psw);
      SELECT
        COUNT(*) INTO num_rec
      FROM mysql.user u
      WHERE u.User = nm
      AND (u.Password = pwd_hash
      OR (u.authentication_string = pwd_hash_ed25519
      AND u.plugin = "ed25519"));
      IF num_rec = 1 THEN
        SET rc = 1;
        SELECT
          CAST(rc AS UNSIGNED) AS RetValueInt,
          CAST(old_psw AS char(127)) AS RetValueString;
      COMMIT; -- vfd pattern for stored procedures
      LEAVE whole_proc;
    END IF;

    -- create new password and add or replace user
    SET pwd = CONCAT(UUID(), "!@_£$&~¬`", CONV(FLOOR(RAND() * 99999999999999), 10, 36));

    SET @sql = CONCAT("CREATE OR REPLACE USER '", `nm`, "'@'%' IDENTIFIED VIA ed25519 USING PASSWORD('", `pwd`, "');");
    PREPARE stmt FROM @sql;
    EXECUTE stmt;

    SET @sql = CONCAT("GRANT ", `default_role_name`, " TO '", `nm`, "'@'%';");
    PREPARE stmt FROM @sql;
    EXECUTE stmt;

    SET @sql = CONCAT("SET DEFAULT ROLE ", `default_role_name`, " FOR '", `nm`, "'@'%';");
    PREPARE stmt FROM @sql;
    EXECUTE stmt;

    DEALLOCATE PREPARE stmt;

    COMMIT; -- vfd pattern for stored procedures
    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(pwd AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_global_config_get
DELIMITER //
CREATE PROCEDURE `sa_global_config_get`(
	IN `_cnfName` VARCHAR(15),
	IN `_cnfDefault` VARCHAR(127)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);
    DECLARE val VARCHAR(127) DEFAULT "";

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    SET _cnfName = LTRIM(RTRIM(_cnfName));
    SET _cnfDefault = LTRIM(RTRIM(_cnfDefault));
    
    START TRANSACTION;

		SELECT LTRIM(RTRIM(uc.cnfValue))
		FROM appdb.sa_global_configs AS uc
		WHERE uc.cnfName = _cnfName
		INTO val
		;
		
		IF (FOUND_ROWS()=0)
		THEN
		  INSERT INTO appdb.sa_global_configs (cnfName, cnfValue)
		  VALUES (_cnfName, _cnfDefault)
		  ;
		  
		  SET val = _cnfDefault;
		END IF;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(val AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_global_config_get_sys
DELIMITER //
CREATE PROCEDURE `sa_global_config_get_sys`(
	IN `_cnfName` VARCHAR(15),
	IN `_cnfDefault` VARCHAR(127),
	OUT _cnfVal VARCHAR(127)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    SET _cnfName = LTRIM(RTRIM(_cnfName));
    SET _cnfDefault = LTRIM(RTRIM(_cnfDefault));
    
    START TRANSACTION;

		SELECT LTRIM(RTRIM(uc.cnfValue))
		FROM appdb.sa_global_configs AS uc
		WHERE uc.cnfName = _cnfName
		INTO _cnfVal
		;
		
		IF (FOUND_ROWS()=0)
		THEN
		  INSERT INTO appdb.sa_global_configs (cnfName, cnfValue)
		  VALUES (_cnfName, _cnfDefault)
		  ;
		  
		  SET _cnfVal = _cnfDefault;
		END IF;
    
    COMMIT;
    
    
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_global_config_set
DELIMITER //
CREATE PROCEDURE `sa_global_config_set`(
	IN `_cnfName` VARCHAR(15),
	IN `_cnfValue` VARCHAR(127)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    SET _cnfName = LTRIM(RTRIM(_cnfName));
    SET _cnfValue = LTRIM(RTRIM(_cnfValue));
    
    START TRANSACTION;

		UPDATE appdb.sa_global_configs
		SET cnfValue = _cnfValue
		WHERE cnfName = _cnfName
		;
		
		IF ROW_COUNT()=0 THEN
		  INSERT INTO appdb.sa_global_configs (cnfName, cnfValue)
		  VALUES (_cnfName, _cnfValue)
		  ;
		END IF;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_cnfValue AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_tech_day_start
DELIMITER //
CREATE PROCEDURE `sa_tech_day_start`(
	IN `_cdt` DATETIME
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
   CALL WTFlogs.NLog_AddEntry_Perm('SELF-SERVICE-APPDB', NOW(), 'Error', CONCAT(CAST(rc AS VARCHAR(15)), " - ", msg));
 END;
 
 START TRANSACTION;
  
 OPTIMIZE TABLE appdb.sa_uidTranslator;
 ALTER TABLE appdb.sa_uidTranslator ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 OPTIMIZE TABLE appdb.cr_certsIUops;
 ALTER TABLE appdb.cr_certsIUops ENGINE=InnoDB, ALGORITHM=INPLACE;
 OPTIMIZE TABLE appdb.cr_certsIUops_results;
 ALTER TABLE appdb.cr_certsIUops_results ENGINE=InnoDB, ALGORITHM=INPLACE;
 OPTIMIZE TABLE appdb.cr_transform_inconsistences;
 ALTER TABLE appdb.cr_transform_inconsistences ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 OPTIMIZE TABLE appdb.sa_tech_ticks_protocol;
 ALTER TABLE appdb.sa_tech_ticks_protocol ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 OPTIMIZE TABLE appdb.sco_certs;
 ALTER TABLE appdb.sco_certs ENGINE=InnoDB, ALGORITHM=INPLACE;
 OPTIMIZE TABLE appdb.sco_certs_catched;
 ALTER TABLE appdb.sco_certs_catched ENGINE=InnoDB, ALGORITHM=INPLACE;
 
 FLUSH TABLES;
 
 CALL WTFlogs.NLog_AddEntry_Perm('SELF-SERVICE-APPDB', NOW(), 'Info', 'Day Start complited');
 
 COMMIT;
 
END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_tech_tick
DELIMITER //
CREATE PROCEDURE `sa_tech_tick`()
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);
 DECLARE _cdt, _ldt DATETIME;

 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   INSERT INTO appdb.sa_tech_ticks_protocol (logged, _type, dscr)
   VALUES (_cdt, 2, CONCAT('Error: ', CAST(rc AS VARCHAR(15)), " - ", msg));
 END;
 
 SET _cdt = NOW();
 
 START TRANSACTION;
   SELECT MAX(pr.logged)
   FROM appdb.sa_tech_ticks_protocol AS pr
   INTO _ldt;
   IF _ldt IS NULL THEN
     SET _ldt = '0001/01/01';
   END IF;
   
   INSERT INTO appdb.sa_tech_ticks_protocol (logged, _type, dscr)
   VALUES (_cdt, 0, 'sa_tech_tick completed');
 COMMIT;
 
END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_uidTranslate
DELIMITER //
CREATE PROCEDURE `sa_uidTranslate`(
	IN `_id_user` VARCHAR(64),
	OUT `_uid` INT
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
 
 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   RESIGNAL;
 END;
 
 START TRANSACTION;
 
 	SELECT uid
 	FROM appdb.sa_uidTranslator AS tr
 	WHERE tr.id_user = _id_user
 	INTO _uid
 	;
 	
 	IF _uid IS NULL THEN
 	
 		SELECT MAX(uid)
 		FROM appdb.sa_uidTranslator
 		INTO _uid
 		;
 		IF _uid IS NULL THEN
 			SET _uid = 0;
 		END IF;
 		SET _uid = _uid+1;
 		
 		INSERT INTO appdb.sa_uidTranslator (id_user, uid)
 		VALUES (_id_user, _uid)
 		;
 		
 	END IF;
 
 COMMIT;
 
END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_user_config_get
DELIMITER //
CREATE PROCEDURE `sa_user_config_get`(
	IN `_cnfName` VARCHAR(15),
	IN `_cnfDefault` VARCHAR(127)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);
    DECLARE val VARCHAR(127) DEFAULT "";

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    SET _cnfName = LTRIM(RTRIM(_cnfName));
    SET _cnfDefault = LTRIM(RTRIM(_cnfDefault));
    
    START TRANSACTION;

		SELECT LTRIM(RTRIM(uc.cnfValue))
		FROM appdb.sa_user_configs AS uc
		WHERE uc.uid = WhoIsThis()
		      AND uc.cnfName = _cnfName
		INTO val
		;
		
		IF (FOUND_ROWS()=0)
		THEN
		  INSERT INTO appdb.sa_user_configs (uid, cnfName, cnfValue)
		  VALUES (WhoIsThis(), _cnfName, _cnfDefault)
		  ;
		  
		  SET val = _cnfDefault;
		END IF;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(val AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.sa_user_config_set
DELIMITER //
CREATE PROCEDURE `sa_user_config_set`(
	IN `_cnfName` VARCHAR(15),
	IN `_cnfValue` VARCHAR(127)
)
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT -1;
    DECLARE msg varchar(255);
    DECLARE val VARCHAR(127) DEFAULT "";

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      RESIGNAL;
    END;
    
    SET _cnfName = LTRIM(RTRIM(_cnfName));
    SET _cnfValue = LTRIM(RTRIM(_cnfValue));
    
    START TRANSACTION;

		UPDATE appdb.sa_user_configs
		SET cnfValue = _cnfValue
		WHERE uid = WhoIsThis()
		      AND cnfName = _cnfName
		;
		
		IF ROW_COUNT()=0 THEN
		  INSERT INTO appdb.sa_user_configs (uid, cnfName, cnfValue)
		  VALUES (WhoIsThis(), _cnfName, _cnfValue)
		  ;
		END IF;
    
    COMMIT;
    
    SET rc := 0;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(_cnfValue AS CHAR(127)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_Build_new_wmd_v1
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_Build_new_wmd_v1`(
	IN `_uid` VARCHAR(50),
	IN `_wfid` VARCHAR(50),
	IN `_numDays` INT,
	IN `_overdueDaysMax` INT
)
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);
 DECLARE _now DATE DEFAULT(CURDATE());
 
 DECLARE fbb DATE DEFAULT("1991-06-13");
 DECLARE _nowFBB INT DEFAULT(0);
 DECLARE _ubFBB, _lbFBB INT DEFAULT(0);
 
 DECLARE _numDaysLow INT DEFAULT 0;
 DECLARE _uidTranslated INT DEFAULT(-1);

 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     CAST(msg AS char(64)) AS RetValueString;
   RESIGNAL;
 END;
 
 SET _uid = WhoIsThis();
 SET _wfid = IFNULL(_wfid, '');
 
 CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
 
 SET _nowFBB = DATEDIFF(CURDATE(),fbb);
 
 CASE _numDays
 WHEN 0 THEN SET _numDaysLow = 0; -- All being recalibrated
 WHEN -1 THEN SET _numDaysLow = 0; -- All Valid
 WHEN 30 THEN SET _numDaysLow = 0; -- Due in 1 month
 WHEN 60 THEN SET _numDaysLow = 30; -- Due in 1 month
 WHEN 90 THEN SET _numDaysLow = 60; -- Due in 2-3 months
 WHEN 180 THEN SET _numDaysLow = 90; -- Due in 4-6 months
 WHEN 365 THEN SET _numDaysLow = 180; -- Due in 7-12 months
 WHEN -3652 THEN SET _numDaysLow = 0; -- Due in 13+ months
 WHEN -2 THEN SET _numDaysLow = 0; -- Overdue (less _overdueDaysMax days)
 WHEN -3 THEN SET _numDaysLow = 0; -- Overdue (less 1 year)
 WHEN -3650 THEN SET _numDaysLow = 0; -- Overdue (all)
 WHEN -3649 THEN SET _numDaysLow = 0; -- Not being recalibrated
 WHEN -3651 THEN SET _numDaysLow = 0; -- All
 WHEN -4 THEN SET _numDaysLow = 0; -- All Recent
 WHEN -5 THEN SET _numDaysLow = 0; -- Due Today
 ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown intertval for due in.';
 END CASE;
 
 SET _lbFBB = _nowFBB + _numDaysLow;
 SET _ubFBB = _nowFBB + _numDays;
 
 DROP TEMPORARY TABLE IF EXISTS tma;
 CREATE TEMPORARY TABLE tma ENGINE=Innodb
 SELECT bm.Id AS id_mark, bm.mark_name, bm.id_team as id_team, t.name AS team_name
 FROM appdb.bs_marks_v as bm
      INNER JOIN appdb.bs_teams AS t ON t.Id = bm.id_team
 ;

 START TRANSACTION;
 
	 IF _numDays>0 THEN
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE crts.recalDateFBB > _lbFBB AND
		      -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND
		      crts.recalDateFBB <= _ubFBB
				-- datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0
				-- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
				AND crts.recalDateFBB > 0
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=0 THEN -- All being recalibrated
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE crts.recalDateFBB > _ubFBB 
		      -- (datediff(crts.RecalDate,_now)>=0)
				-- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
				AND crts.recalDateFBB > 0
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-5 THEN -- Due Today
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE crts.recalDateFBB = _nowFBB 
		      -- (datediff(crts.RecalDate,_now)>=0)
				-- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
				AND crts.recalDateFBB > 0
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-1 THEN -- All Valid
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE (
		       -- datediff(crts.RecalDate,_now)>=0
		       crts.recalDateFBB > _nowFBB
				 -- OR DATEDIFF(crts.RecalDate,'1991/01/01')=0
				 -- OR datediff(crts.RecalDate,'0001/01/01')=0
				 OR crts.recalDateFBB < 0
				)
				-- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-3651 THEN -- All
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark;
	 END IF;
	 IF _numDays=-4 THEN -- All Recent
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-2 THEN -- Overdue not more then _overdueDaysMax
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE (
		       -- datediff(_now,crts.RecalDate)>0
		       crts.recalDateFBB <= _nowFBB
	   		 -- AND (datediff(_now,crts.RecalDate)<=_overdueDaysMax)
	   		 AND crts.recalDateFBB >= _nowFBB - _overdueDaysMax
             -- AND (NOT (DATEDIFF(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0)
             AND crts.recalDateFBB > 0
				 )
				 -- AND (crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo))
			 AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-3 THEN -- Overdue not more than 365 days
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE 
		      -- datediff(_now,crts.RecalDate)>0
		      crts.recalDateFBB <= _nowFBB
	   		-- AND (datediff(_now,crts.RecalDate)<=365)
	   		AND crts.recalDateFBB > _nowFBB - 365
            -- AND (NOT (DATEDIFF(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0))
            AND crts.recalDateFBB > 0
				-- AND (crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo))
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-3650 THEN -- Overdue any
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE -- datediff(_now,crts.RecalDate)>0
		      crts.recalDateFBB <= _nowFBB
		      -- AND (NOT (DATEDIFF(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0))
		      AND crts.recalDateFBB > 0
				-- AND (crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo))
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 IF _numDays=-3652 THEN -- Due in 13+ months
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL 360 DAY))>=0
	         crts.recalDateFBB > _nowFBB + 365
	         AND crts.recalDateFBB > 0
	   		-- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
	   		AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
	   ;
	 END IF;
	 IF _numDays=-3649 THEN -- Not being recalibrated
	   INSERT INTO appdb.sco_certs_catched (prevID,
		                                     CertNo, id_company, SerialNo, PlantNo,
		                                     Descr, Range_, 
		                                     id_location,
		                                     CertDate, CalDate, RecalDate, Preadj,
												       marked,
														 company_name, location_name,
														 CLoc, recalDateFBB, uidTranslated)
	   SELECT DISTINCT crts.id,
	   		 crts.CertNo, crts.id_company, crts.SerialNo, crts.PlantNo,
	   		 crts.Descr, crts.Range_,
	   		 crts.id_location,
	   		 crts.CertDate, crts.CalDate, crts.RecalDate, crts.Preadj,
	   		 FALSE,
	   		 tma.team_name, tma.mark_name,
	   		 -- tm.name, m.mark_name,
	   		 crts.CLoc, crts.recalDateFBB, _uidTranslated
	   FROM tma
	        INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                              crts.id_location = tma.id_mark
	        -- appdb.sco_certs AS crts
	        -- INNER JOIN appdb.bs_marks_v  AS m ON m.Id = crts.id_location
	        -- INNER JOIN appdb.bs_teams_v AS tm ON tm.Id = crts.id_company
	   WHERE -- (datediff(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0)
	         (crts.recalDateFBB < 0) -- OR crts.recalDateFBB = _nowFBB)
				-- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
				AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
		;
	 END IF;
	 
 COMMIT;
 
 DROP TEMPORARY TABLE IF EXISTS tma;

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_deSelectAllVisible_wdates_wcf
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_deSelectAllVisible_wdates_wcf`(
	IN `_uid` VARCHAR(64),
	IN `_searchString` VARCHAR(64),
	IN `_searchField` VARCHAR(64),
	IN `_selectedOnly` VARCHAR(15),
	IN `_ids` DATETIME,
	IN `_ide` DATETIME,
	IN `_rds` DATETIME,
	IN `_rde` DATETIME,
	IN `_cf` VARCHAR(255)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
 	DECLARE msg varchar(255);
 	
   DECLARE _CertNo, _SerialNo, _PlantNo, _Descr, _CLoc VARCHAR(127) DEFAULT '%';
   DECLARE _location_name VARCHAR(127) DEFAULT '%'; 
	DECLARE _selonly INT DEFAULT 0;
   DECLARE ss VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   DECLARE _uidTranslated INT DEFAULT(-1);
 	
 	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
   	ROLLBACK; -- vfd pattern for stored procedures
   	RESIGNAL; -- vfd pattern for stored procedures
 	END;
 	
 	SET _uid = WhoIsThis();
	CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
	
	CALL WTFlogs.eventsLogCache_parseCmbFilter_new(_cf); -- result in TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50))
	SELECT _filter INTO _CertNo FROM WTFlogs.temp_parse WHERE _field='CertNumber';
   SELECT _filter INTO _SerialNo FROM WTFlogs.temp_parse WHERE _field='SerialNumber';
   SELECT _filter INTO _PlantNo FROM WTFlogs.temp_parse WHERE _field='PlantNumber';
   SELECT _filter INTO _Descr FROM WTFlogs.temp_parse WHERE _field='Description';
   SELECT _filter INTO _location_name FROM WTFlogs.temp_parse WHERE _field='Location';
   SELECT _filter INTO _CLoc FROM WTFlogs.temp_parse WHERE _field='CLoc';
 	
 	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'CertNo' THEN SET _CertNo = ss;
   WHEN 'SerialNo' THEN SET _SerialNo = ss;
   WHEN 'PlantNo' THEN SET _PlantNo = ss;
   WHEN 'Descr' THEN SET _Descr = ss;
   WHEN 'location_name' THEN SET _location_name = ss;
   WHEN 'CLoc' THEN SET _CLoc = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown search field.';
   END CASE;
   IF _selectedOnly = 'true' THEN
   	SET _selonly = 1;
   END IF;
   
   SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT id '
		        'FROM appdb.sco_certs_catched '
		        'WHERE uidTranslated = ? '
		        'AND CertNo LIKE ? '
		        'AND SerialNo LIKE ? '
		        'AND PlantNo LIKE ? '
			     'AND Descr LIKE ? '
			     'AND location_name LIKE ? '
			     'AND CLoc LIKE ? '
			     'AND marked = CASE WHEN ? = 1 THEN 1 ELSE marked END '
			     'AND datediff(CertDate,?)>=0 AND datediff(CertDate,?)<=0 '
			     'AND datediff(RecalDate,?)>=0 AND datediff(RecalDate,?)<=0 '
				  ';';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uidTranslated, _CertNo, _SerialNo, _PlantNo, _Descr, _location_name, _CLoc, _selonly,
	                  _ids, _ide, _rds, _rde;
	DEALLOCATE PREPARE stm;
 	
 	START TRANSACTION;
 		UPDATE appdb.sco_certs_catched AS clc
 				 INNER JOIN tmp ON tmp.id=clc.id
 		SET clc.marked = 0;
 		SET rc = ROW_COUNT();
 	COMMIT;
 	
 	DROP TEMPORARY TABLE IF EXISTS tmp;
 	SELECT
	   CAST(rc AS INT) AS RetValueInt,
	   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_Drop
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_Drop`(
	IN `_uid` varchar(50),
	IN `_wfid` varchar(50)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);
 DECLARE _uidTranslated INT DEFAULT(-1);

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
 
 SET _uid = WhoIsThis();
 CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
	
 SET _wfid = IFNULL(_wfid, '');

 START TRANSACTION; -- vfd pattern for stored procedures 
   DELETE appdb.sco_certs_cacheControl
   FROM appdb.sco_certs_cacheControl
   WHERE uid = _uid;
   INSERT INTO appdb.sco_certs_cacheControl (uid, wfid)
   VALUES (_uid, _wfid);

   DELETE appdb.sco_certs_catched
   FROM appdb.sco_certs_catched
   WHERE uidTranslated = _uidTranslated;
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_GetEntry
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_GetEntry`(
	IN `_recId` INT,
	IN `_uId` VARCHAR(50)
)
    READS SQL DATA
BEGIN
	DECLARE _uidTranslated INT DEFAULT(-1);

	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;

	SET _uid = WhoIsThis();
	CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
	
   SET _recId = IFNULL(_recId, 0);

	SELECT srt.id, srt.CertNo, srt.SerialNo, srt.PlantNo, srt.Descr, srt.Range_,
			 srt.CertDate, srt.CalDate, srt.RecalDate,
			 srt.Preadj,
			 srt.prevID, srt.marked, CAST(0 AS UNSIGNED) AS numRecs,
			 srt.location_name, srt.company_name,
			 srt.CLoc, CAST("" AS VARCHAR(32)) AS bckg_color,
			 CAST(IF(srt.recalDateFBB>=0,DATE_FORMAT(srt.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	FROM appdb.sco_certs_catched AS srt
	WHERE srt.id = _recId and
	      srt.uidTranslated = _uidTranslated;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_GetPage_wcmpflt_wso_new_wtms_wcf
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_GetPage_wcmpflt_wso_new_wtms_wcf`(
	IN `_uid` varchar(50),
	IN `_searchString` varchar(50),
	IN `_CurrentSort` varchar(50),
	IN `_SortDirection` varchar(50),
	IN `_pageIndex` int,
	IN `_pageSize` int,
	IN `_searchField` VARCHAR(50),
	IN `_selectedOnly` VARCHAR(15),
	IN `_ids` DATETIME,
	IN `_ide` DATETIME,
	IN `_rds` DATETIME,
	IN `_rde` DATETIME,
	IN `_cf` VARCHAR(255)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
   DECLARE flt VARCHAR(500) DEFAULT 'not defined';
   DECLARE _CertNo, _SerialNo, _PlantNo, _Descr, _location_name, _CLoc VARCHAR(127) DEFAULT '%';
   DECLARE ss, srt VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(8000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   DECLARE _selonly INT DEFAULT 0;
   DECLARE _uidTranslated INT DEFAULT(-1);
   
	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
		RESIGNAL; -- vfd pattern for stored procedures
	END;
	
	SET _uid = WhoIsThis();
	CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
	
	CALL WTFlogs.eventsLogCache_parseCmbFilter_new(_cf); -- result in TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50))
	SELECT _filter INTO _CertNo FROM WTFlogs.temp_parse WHERE _field='CertNumber';
   SELECT _filter INTO _SerialNo FROM WTFlogs.temp_parse WHERE _field='SerialNumber';
   SELECT _filter INTO _PlantNo FROM WTFlogs.temp_parse WHERE _field='PlantNumber';
   SELECT _filter INTO _Descr FROM WTFlogs.temp_parse WHERE _field='Description';
   SELECT _filter INTO _location_name FROM WTFlogs.temp_parse WHERE _field='Location';
   SELECT _filter INTO _CLoc FROM WTFlogs.temp_parse WHERE _field='CLoc';
	
	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'CertNo' THEN SET _CertNo = ss;
   WHEN 'SerialNo' THEN SET _SerialNo = ss;
   WHEN 'PlantNo' THEN SET _PlantNo = ss;
   WHEN 'Descr' THEN SET _Descr = ss;
   WHEN 'location_name' THEN SET _location_name = ss;
   WHEN 'CLoc' THEN SET _CLoc = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown search field.';
   END CASE;
   IF _selectedOnly = 'true' THEN
   	SET _selonly = 1;
   END IF;

	SET srt = CONCAT(_CurrentSort, _SortDirection);
	
	SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT id, prevID, '
		        'CertNo, SerialNo, PlantNo, '
		        'Descr, Range_, '
		        'CertDate, CalDate, RecalDate, Preadj, '
		        'marked, '
		        'company_name, location_name, '
		        'CLoc, '
		        'recalDateFBB '
		        'FROM appdb.sco_certs_catched '
		        'WHERE uidTranslated = ? '
		        'AND CertNo LIKE ? '
		        'AND SerialNo LIKE ? '
		        'AND PlantNo LIKE ? '
			     'AND Descr LIKE ? '
			     'AND location_name LIKE ? '
			     'AND CLoc LIKE ? '
			     'AND marked = CASE WHEN ? = 1 THEN 1 ELSE marked END '
			     'AND datediff(CertDate,?)>=0 AND datediff(CertDate,?)<=0 '
			     'AND datediff(RecalDate,?)>=0 AND datediff(RecalDate,?)<=0 '
				  ';';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uidTranslated, _CertNo, _SerialNo, _PlantNo, _Descr, _location_name, _CLoc, _selonly,
	                  _ids, _ide, _rds, _rde;
	DEALLOCATE PREPARE stm;
	
	SELECT COUNT(*) INTO total_recs FROM tmp;
	
	SET _pageIndex = IFNULL(_pageIndex, 0);
	SET _pageSize = IFNULL(_pageSize, 2147483647);
	
	SET off = (_pageIndex - 1) * _pageSize,
	ps = _pageSize;
	
	CASE srt
	WHEN 'sortA_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.CertNo asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortA_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.CertNo desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.SerialNo asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortB_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.SerialNo desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.PlantNo asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortC_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.PlantNo desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortD_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.RecalDate asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortD_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.RecalDate desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortE_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recall") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.location_name asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortE_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recall") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.location_name desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortF_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.CertDate asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortF_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.CertDate desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortG_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.Descr asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortG_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.Descr desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortH_' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.CLoc asc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	WHEN 'sortH_desc' THEN PREPARE stm FROM 'select tmp.*, CAST(? AS UNSIGNED) AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
	, CAST(IF(tmp.recalDateFBB>=0,DATE_FORMAT(tmp.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
	from tmp
	order by tmp.Cloc desc
	limit ? offset ?;';
	  EXECUTE stm USING total_recs, ps, off;
	ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown sort order.';
	END CASE;
	
	DROP TEMPORARY TABLE IF EXISTS tmp;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_GetSelected
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_GetSelected`()
    MODIFIES SQL DATA
BEGIN
 DECLARE rc, numRecs int DEFAULT 0;
 DECLARE _msg VARCHAR(1024);
 DECLARE _uid VARCHAR(50);
 DECLARE _uidTranslated INT DEFAULT(-1);
 
 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, _msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     _msg AS RetValueString;
   RESIGNAL;
 END;
 
 SET _uid = WhoIsThis();
 CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
 
 DROP TEMPORARY TABLE IF EXISTS yopanat;
 CREATE TEMPORARY TABLE yopanat
 SELECT sc.id, sc.prevID,
	     sc.CertNo, sc.SerialNo, sc.PlantNo,
		  sc.Descr, sc.Range_,
		  sc.CertDate, sc.CalDate, sc.RecalDate, sc.Preadj,
		  sc.marked,
		  sc.company_name, sc.location_name,
		  sc.CLoc,
		  sc.recalDateFBB
 FROM appdb.sco_certs_catched AS sc
 WHERE sc.uidTranslated = _uidTranslated AND
       sc.marked <> 0;
       
 SELECT COUNT(*) INTO numRecs FROM yopanat;
  
 SELECT y.*, numRecs AS numRecs, CAST("" AS VARCHAR(32)) AS bckg_color
 , CAST(IF(y.recalDateFBB>=0,DATE_FORMAT(y.RecalDate,"%d/%m/%y"),"No Recal") AS VARCHAR(15)) AS recalDatePresentation
 FROM yopanat AS y;
 DROP TEMPORARY TABLE IF EXISTS yopanat;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_invertSelectionAllVisible_wdates_wcf
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_invertSelectionAllVisible_wdates_wcf`(
	IN `_uid` VARCHAR(64),
	IN `_searchString` VARCHAR(64),
	IN `_searchField` VARCHAR(64),
	IN `_selectedOnly` VARCHAR(15),
	IN `_ids` DATETIME,
	IN `_ide` DATETIME,
	IN `_rds` DATETIME,
	IN `_rde` DATETIME,
	IN `_cf` VARCHAR(255)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
 	DECLARE msg varchar(255);
 	
   DECLARE _CertNo, _SerialNo, _PlantNo, _Descr, _CLoc VARCHAR(127) DEFAULT '%';
   DECLARE _location_name VARCHAR(127) DEFAULT '%'; 
	DECLARE _selonly INT DEFAULT 0;
   DECLARE ss VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   DECLARE _uidTranslated INT DEFAULT(-1);
 	
 	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
   	ROLLBACK; -- vfd pattern for stored procedures
   	RESIGNAL; -- vfd pattern for stored procedures
 	END;
 	
 	SET _uid = WhoIsThis();
	CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
	
	CALL WTFlogs.eventsLogCache_parseCmbFilter_new(_cf); -- result in TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50))
	SELECT _filter INTO _CertNo FROM WTFlogs.temp_parse WHERE _field='CertNumber';
   SELECT _filter INTO _SerialNo FROM WTFlogs.temp_parse WHERE _field='SerialNumber';
   SELECT _filter INTO _PlantNo FROM WTFlogs.temp_parse WHERE _field='PlantNumber';
   SELECT _filter INTO _Descr FROM WTFlogs.temp_parse WHERE _field='Description';
   SELECT _filter INTO _location_name FROM WTFlogs.temp_parse WHERE _field='Location';
   SELECT _filter INTO _CLoc FROM WTFlogs.temp_parse WHERE _field='CLoc';
 	
 	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'CertNo' THEN SET _CertNo = ss;
   WHEN 'SerialNo' THEN SET _SerialNo = ss;
   WHEN 'PlantNo' THEN SET _PlantNo = ss;
   WHEN 'Descr' THEN SET _Descr = ss;
   WHEN 'location_name' THEN SET _location_name = ss;
   WHEN 'CLoc' THEN SET _CLoc = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown search field.';
   END CASE;
   IF _selectedOnly = 'true' THEN
   	SET _selonly = 1;
   END IF;
   
   SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT id '
		        'FROM appdb.sco_certs_catched '
		        'WHERE uidTranslated = ? '
		        'AND CertNo LIKE ? '
		        'AND SerialNo LIKE ? '
		        'AND PlantNo LIKE ? '
			     'AND Descr LIKE ? '
			     'AND location_name LIKE ? '
			     'AND CLoc LIKE ? '
			     'AND marked = CASE WHEN ? = 1 THEN 1 ELSE marked END '
			     'AND datediff(CertDate,?)>=0 AND datediff(CertDate,?)<=0 '
			     'AND datediff(RecalDate,?)>=0 AND datediff(RecalDate,?)<=0 '
				  ';';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uidTranslated, _CertNo, _SerialNo, _PlantNo, _Descr, _location_name, _CLoc, _selonly,
	                  _ids, _ide, _rds, _rde;
	DEALLOCATE PREPARE stm;
 	
 	START TRANSACTION;
 		UPDATE appdb.sco_certs_catched AS clc
 				 INNER JOIN tmp ON tmp.id=clc.id
 		SET clc.marked = NOT clc.marked;
 		SET rc = ROW_COUNT();
 	COMMIT;
 	
 	DROP TEMPORARY TABLE IF EXISTS tmp;
 	SELECT
	   CAST(rc AS INT) AS RetValueInt,
	   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_marked_to_csv_new_01
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_marked_to_csv_new_01`()
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE _msg VARCHAR(1024);
 DECLARE _uid VARCHAR(50);
 DECLARE _delim CHAR DEFAULT ',';
 DECLARE _uidTranslated INT DEFAULT(-1);
 
 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, _msg = MESSAGE_TEXT;
   SELECT
     CAST(rc AS UNSIGNED) AS RetValueInt,
     _msg AS RetValueString;
   RESIGNAL;
 END;
 
 DROP TEMPORARY TABLE IF EXISTS yopanat;
 CREATE TEMPORARY TABLE yopanat (id INT, str VARCHAR(2048));
 
 SET _uid = WhoIsThis();
 CALL appdb.sa_uidTranslate(_uid, _uidTranslated);

 INSERT INTO yopanat (id, str)
 VALUES (0, CONCAT('CertNo',_delim,
                   -- 'id_company',_delim,
                   'SerialNo',_delim,
                   'PlantNo',_delim,
                   'Descr',_delim,
                   'Range_',_delim,
                   -- 'id_location',_delim,
                   'CertDate',_delim,
                   'CalDate',_delim,
                   'RecalDate',_delim,
                   'Preadj',_delim,
                   'company_name',_delim,
                   'location_name',_delim,
                   'CLoc',_delim
                  )
        );
 INSERT INTO yopanat (id, str)
 SELECT sc.id, 
        CONCAT(
 		  REPLACE(CAST(sc.CertNo AS VARCHAR(50)), _delim, '|'), -- _delim,REPLACE(CAST(sc.id_company AS VARCHAR(64)), _delim, '|'),
		  _delim,REPLACE(CAST(sc.SerialNo AS VARCHAR(50)), _delim, '|'),_delim,REPLACE(CAST(sc.PlantNo AS VARCHAR(50)), _delim, '|'),
        _delim,REPLACE(CAST(sc.Descr AS VARCHAR(50)), _delim, '|'),_delim,REPLACE(CAST(sc.Range_ AS VARCHAR(50)), _delim, '|'),
        -- _delim,REPLACE(CAST(sc.id_location AS VARCHAR(50)), _delim, '|'),
        _delim,REPLACE(CAST(DATE_FORMAT(sc.CertDate, '%d-%m-%Y') AS VARCHAR(50)), _delim, '|'),_delim,REPLACE(CAST(DATE_FORMAT(sc.CalDate, '%d-%m-%Y') AS VARCHAR(50)), _delim, '|'),
		  _delim,REPLACE(CAST(DATE_FORMAT(sc.RecalDate, '%d-%m-%Y') AS VARCHAR(50)), _delim, '|'),_delim,REPLACE(CAST(sc.Preadj AS VARCHAR(50)), _delim, '|'),
        _delim,REPLACE(CAST(sc.company_name AS VARCHAR(50)), _delim, '|'),_delim,REPLACE(CAST(sc.location_name AS VARCHAR(50)), _delim, '|'),
		  _delim,REPLACE(CAST(sc.company_name AS VARCHAR(50)), _delim, '|'),
		  _delim
		  ) AS str
 FROM appdb.sco_certs_catched AS sc
 WHERE sc.uidTranslated = _uidTranslated AND
       sc.marked <> 0 AND
       1 = 1;
  
 SELECT
   y.id AS RetValueInt,
   y.str AS RetValueString
 FROM yopanat AS y;
 DROP TEMPORARY TABLE IF EXISTS yopanat;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_selectAllVisible_wdates_wcf
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_selectAllVisible_wdates_wcf`(
	IN `_uid` VARCHAR(64),
	IN `_searchString` VARCHAR(64),
	IN `_searchField` VARCHAR(64),
	IN `_selectedOnly` VARCHAR(15),
	IN `_ids` DATETIME,
	IN `_ide` DATETIME,
	IN `_rds` DATETIME,
	IN `_rde` DATETIME,
	IN `_cf` VARCHAR(255)
)
    MODIFIES SQL DATA
whole_proc:
BEGIN
	DECLARE rc int DEFAULT 0;
 	DECLARE msg varchar(255);
 	
   DECLARE _CertNo, _SerialNo, _PlantNo, _Descr, _CLoc VARCHAR(127) DEFAULT '%';
	DECLARE _location_name VARCHAR(127) DEFAULT '%'; 
	DECLARE _selonly INT DEFAULT 0;
   DECLARE ss VARCHAR(50) DEFAULT '';
   DECLARE stmt VARCHAR(1000) DEFAULT '';
   DECLARE off, ps, total_recs INT DEFAULT 0;
   DECLARE _uidTranslated INT DEFAULT(-1);
 	
 	DECLARE EXIT HANDLER -- vfd pattern for stored procedures
	FOR SQLEXCEPTION
	BEGIN
   	ROLLBACK; -- vfd pattern for stored procedures
   	RESIGNAL; -- vfd pattern for stored procedures
 	END;
 	
 	SET _uid = WhoIsThis();
	CALL appdb.sa_uidTranslate(_uid, _uidTranslated);
	
	CALL WTFlogs.eventsLogCache_parseCmbFilter_new(_cf); -- result in TABLE temp_parse (_field VARCHAR(50), _filter VARCHAR(50))
	SELECT _filter INTO _CertNo FROM WTFlogs.temp_parse WHERE _field='CertNumber';
   SELECT _filter INTO _SerialNo FROM WTFlogs.temp_parse WHERE _field='SerialNumber';
   SELECT _filter INTO _PlantNo FROM WTFlogs.temp_parse WHERE _field='PlantNumber';
   SELECT _filter INTO _Descr FROM WTFlogs.temp_parse WHERE _field='Description';
   SELECT _filter INTO _location_name FROM WTFlogs.temp_parse WHERE _field='Location';
   SELECT _filter INTO _CLoc FROM WTFlogs.temp_parse WHERE _field='CLoc';
 	
 	SET ss = CONCAT('%', LTRIM(RTRIM(_searchString)), '%');
	CASE _searchField
   WHEN 'CertNo' THEN SET _CertNo = ss;
   WHEN 'SerialNo' THEN SET _SerialNo = ss;
   WHEN 'PlantNo' THEN SET _PlantNo = ss;
   WHEN 'Descr' THEN SET _Descr = ss;
   WHEN 'location_name' THEN SET _location_name = ss;
   WHEN 'CLoc' THEN SET _CLoc = ss;
   ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Unknown search field.';
   END CASE;
   IF _selectedOnly = 'true' THEN
   	SET _selonly = 1;
   END IF;
   
   SET stmt = 'CREATE TEMPORARY TABLE tmp ENGINE=Innodb '
	           'SELECT id '
		        'FROM appdb.sco_certs_catched '
		        'WHERE uidTranslated = ? '
		        'AND CertNo LIKE ? '
		        'AND SerialNo LIKE ? '
		        'AND PlantNo LIKE ? '
			     'AND Descr LIKE ? '
			     'AND location_name LIKE ? '
			     'AND CLoc LIKE ? '
			     'AND marked = CASE WHEN ? = 1 THEN 1 ELSE marked END '
			     'AND datediff(CertDate,?)>=0 AND datediff(CertDate,?)<=0 '
			     'AND datediff(RecalDate,?)>=0 AND datediff(RecalDate,?)<=0 '
				  ';';
	PREPARE stm FROM stmt;
	EXECUTE stm USING _uidTranslated, _CertNo, _SerialNo, _PlantNo, _Descr, _location_name, _CLoc, _selonly,
	                  _ids, _ide, _rds, _rde;
	DEALLOCATE PREPARE stm;
 	
 	START TRANSACTION;
 		UPDATE appdb.sco_certs_catched AS clc
 				 INNER JOIN tmp ON tmp.id=clc.id
 		SET clc.marked = 1;
 		SET rc = ROW_COUNT();
 	COMMIT;
 	
 	DROP TEMPORARY TABLE IF EXISTS tmp;
 	SELECT
	   CAST(rc AS INT) AS RetValueInt,
	   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCertsCache_SelectionToggle
DELIMITER //
CREATE PROCEDURE `SCOCertsCache_SelectionToggle`(
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
   UPDATE appdb.sco_certs_catched
   SET marked = IF(marked=0,1,0)
   WHERE ID=_id_cache_rec;
 COMMIT; -- vfd pattern for stored procedures

 SELECT
   CAST(rc AS INT) AS RetValueInt,
   CAST('OK' AS char(64)) AS RetValueString;
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCerts_contentPurge
DELIMITER //
CREATE PROCEDURE `SCOCerts_contentPurge`()
    MODIFIES SQL DATA
whole_proc:
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);

 DECLARE EXIT HANDLER -- vfd pattern for stored procedures
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK; -- vfd pattern for stored procedures
   RESIGNAL; -- vfd pattern for stored procedures
 END;
 
 TRUNCATE TABLE appdb.sco_certs_catched;
 TRUNCATE TABLE appdb.sco_certs_cacheControl;
 TRUNCATE TABLE appdb.sco_certs;
 
 TRUNCATE TABLE appdb.cr_certsIUops_results;
 TRUNCATE TABLE appdb.cr_certsIUops_results;
 TRUNCATE TABLE appdb.cr_transform_inconsistences;
 
 TRUNCATE TABLE appdb.bs_marks_to_users;
 TRUNCATE TABLE appdb.bs_team_marks;
 TRUNCATE TABLE appdb.bs_marks;
  
 START TRANSACTION;
 
   INSERT INTO appdb.bs_marks (name)
   SELECT tms.name
   FROM appdb.bs_teams AS tms
   ;
   
   INSERT INTO appdb.bs_team_marks (id_bst, id_mark)
   SELECT tms.Id, mrks.Id
   FROM appdb.bs_teams AS tms
        INNER JOIN appdb.bs_marks AS mrks ON mrks.name=tms.name
   ;
        
 COMMIT;
 
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCerts_get_num_new_actual_v3
DELIMITER //
CREATE PROCEDURE `SCOCerts_get_num_new_actual_v3`(
	IN `_overdueDays` INT
)
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);
 
 DECLARE _numDays INT DEFAULT 0;
 DECLARE fbb DATE DEFAULT("1991-06-13");
 DECLARE _nowFBB INT DEFAULT(0);
 DECLARE _ubFBB, _lbFBB INT DEFAULT(0);
 DECLARE _numDaysLow INT DEFAULT 0;
 DECLARE _now DATE DEFAULT(CURDATE());
 
 DECLARE certs_total_num INT DEFAULT 0;
 DECLARE certs_grand_total_num INT DEFAULT 0;
 DECLARE certs_overdue_num INT DEFAULT 0;
 DECLARE certs_overdue_num_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_today INT DEFAULT 0;
 DECLARE certs_duein_today_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_1m INT DEFAULT 0;
 DECLARE certs_duein_1m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_2m INT DEFAULT 0;
 DECLARE certs_duein_2m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_3m INT DEFAULT 0;
 DECLARE certs_duein_3m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_46m INT DEFAULT 0;
 DECLARE certs_duein_46m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_712m INT DEFAULT 0;
 DECLARE certs_duein_712m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_13plusm INT DEFAULT 0;
 DECLARE certs_duein_13plusm_prcs DOUBLE DEFAULT 0;
 DECLARE certs_no_recalibrate INT DEFAULT 0;
 DECLARE certs_no_recalibrate_prcs DOUBLE DEFAULT 0;

 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   RESIGNAL;
 END;
 
 SET _nowFBB = DATEDIFF(CURDATE(),fbb);
 
 DROP TEMPORARY TABLE IF EXISTS tma;
 CREATE TEMPORARY TABLE tma ENGINE=Innodb
 SELECT bm.Id AS id_mark, bm.mark_name, bm.id_team as id_team, t.name AS team_name
 FROM appdb.bs_marks_v as bm
      INNER JOIN appdb.bs_teams AS t ON t.Id = bm.id_team
 ;

 START TRANSACTION;
 
    CREATE TEMPORARY TABLE scocrtstmp ENGINE=INNODB
    SELECT DISTINCT crts.*
    FROM -- appdb.sco_certs AS crts
         tma
	      INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                            crts.id_location = tma.id_mark
    WHERE ( -- true
	        crts.recalDateFBB >= _nowFBB - _overdueDays -- DATEDIFF(crts.RecalDate,_now)>=(-_overdueDays)
			  -- OR crts.recalDateFBB < 0 -- DATEDIFF(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0
			 )
			 AND crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
	       -- AND EXISTS(SELECT * FROM appdb.bs_marks_v  AS m WHERE m.Id = crts.id_location)
	       -- AND EXISTS(SELECT * FROM appdb.bs_teams_v AS tm WHERE tm.Id = crts.id_company)
	       -- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.SerialNo=crts.SerialNo)
	 ;
 
    SELECT DISTINCT COUNT(*) INTO certs_total_num
	 FROM scocrtstmp;
	       
	 SELECT DISTINCT COUNT(*) INTO certs_grand_total_num
	 FROM -- appdb.sco_certs AS crts
	      tma
	      INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                            crts.id_location = tma.id_mark
	 -- WHERE EXISTS(SELECT * FROM appdb.bs_marks_v  AS m WHERE m.Id = crts.id_location) AND
	 --       EXISTS(SELECT * FROM appdb.bs_teams_v AS tm WHERE tm.Id = crts.id_company)
	 ;
		     
	 -- Due in today 
	 SET _numDays = -1;
	 SET _numDaysLow = 0;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_today
	   FROM scocrtstmp AS crts
	   WHERE crts.recalDateFBB = _nowFBB
	   ;		
	   IF certs_total_num <> 0 THEN
			SET certs_duein_today_prcs = ROUND((certs_duein_today * 100.00)/certs_total_num,1);
		END IF;
		
	 -- Due in 1 month 
	 SET _numDays = 30;
	 SET _numDaysLow = 0;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_1m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;		
	   IF certs_total_num <> 0 THEN
			SET certs_duein_1m_prcs = ROUND((certs_duein_1m * 100.00)/certs_total_num,1);
		END IF;
			
	 -- Due in 2 months	
	 SET _numDays = 60;
	 SET _numDaysLow = 30;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_2m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_2m_prcs = ROUND((certs_duein_2m * 100.00)/certs_total_num,1);
		END IF;
		
	 -- Due in 3 months	
	 SET _numDays = 90;
	 SET _numDaysLow = 60;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_3m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_3m_prcs = ROUND((certs_duein_3m * 100.00)/certs_total_num,1);
		END IF;
	
	 -- Due in 4-6 months			
	 SET _numDays = 180;
	 SET _numDaysLow = 90;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_46m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_46m_prcs = ROUND((certs_duein_46m * 100.00)/certs_total_num,1);
		END IF;

    -- Due in 7-12 months				
	 SET _numDays = 365;
	 SET _numDaysLow = 180;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_712m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_712m_prcs = ROUND((certs_duein_712m * 100.00)/certs_total_num,1);
		END IF;
	 
	 -- Due in 13+ months			
	 SELECT DISTINCT COUNT(*) INTO certs_duein_13plusm
	 FROM scocrtstmp AS crts
	 WHERE crts.recalDateFBB > _nowFBB + 365 -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL 360 DAY))>=0
	 ;
	 IF certs_total_num <> 0 THEN
	 	SET certs_duein_13plusm_prcs = ROUND((certs_duein_13plusm * 100.00)/certs_total_num,1);
	 END IF;
	 
	 -- Overdue
	 SELECT DISTINCT COUNT(*) INTO certs_overdue_num
	 FROM scocrtstmp AS crts
	 WHERE -- (datediff(_now,crts.RecalDate)>0) AND
			 -- (NOT (DATEDIFF(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0))
			 crts.recalDateFBB <= _nowFBB
			 AND crts.recalDateFBB > 0
	 ;
	 IF certs_total_num <> 0 THEN
	 	SET certs_overdue_num_prcs = ROUND((certs_overdue_num * 100.00)/certs_total_num,1);
	 END IF; 
	 
	 -- Not being recalibrated        
	 SELECT DISTINCT COUNT(*) INTO certs_no_recalibrate
	 FROM scocrtstmp AS crts
    WHERE -- (datediff(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0)
          crts.recalDateFBB < 0
          -- OR crts.recalDateFBB = _nowFBB
	 ;
    IF certs_total_num <> 0 THEN
	 	SET certs_no_recalibrate_prcs = ROUND((certs_no_recalibrate * 100.00)/certs_total_num,1);
	 END IF;
	 
	 DROP TEMPORARY TABLE IF EXISTS scocrtstmp;
	 
 COMMIT;

 SELECT
   CAST(certs_total_num AS INT) AS certs_total_num,
   CAST(certs_grand_total_num AS INT) AS certs_grand_total_num,
   CAST(certs_overdue_num AS INT) AS certs_overdue_num,
   CAST(certs_duein_today AS INT) AS certs_duein_today,
   CAST(certs_duein_1m AS INT) AS certs_duein_1m,
   CAST(certs_duein_2m AS INT) AS certs_duein_2m,
   CAST(certs_duein_3m AS INT) AS certs_duein_3m,
   CAST(certs_duein_46m AS INT) AS certs_duein_46m,
   CAST(certs_duein_712m AS INT) AS certs_duein_712m,
   CAST(certs_duein_13plusm AS INT) AS certs_duein_13plusm,
   CAST(certs_no_recalibrate AS INT) AS certs_no_recalibrate,
   CAST(certs_overdue_num_prcs AS DOUBLE) AS certs_overdue_num_prcs,
   CAST(certs_duein_today_prcs AS DOUBLE) AS certs_duein_today_prcs,
   CAST(certs_duein_1m_prcs AS DOUBLE) AS certs_duein_1m_prcs,
   CAST(certs_duein_2m_prcs AS DOUBLE) AS certs_duein_2m_prcs,
   CAST(certs_duein_3m_prcs AS DOUBLE) AS certs_duein_3m_prcs,
   CAST(certs_duein_46m_prcs AS DOUBLE) AS certs_duein_46m_prcs,
   CAST(certs_duein_712m_prcs AS DOUBLE) AS certs_duein_712m_prcs,
   CAST(certs_duein_13plusm_prcs AS DOUBLE) AS certs_duein_13plusm_prcs,
   CAST(certs_no_recalibrate_prcs AS DOUBLE) AS certs_no_recalibrate_prcs;
   
END//
DELIMITER ;

-- Dumping structure for procedure appdb.SCOCerts_get_num_new_v3
DELIMITER //
CREATE PROCEDURE `SCOCerts_get_num_new_v3`()
    MODIFIES SQL DATA
BEGIN
 DECLARE rc int DEFAULT 0;
 DECLARE msg varchar(255);
 
 DECLARE _numDays INT DEFAULT 0;
 DECLARE fbb DATE DEFAULT("1991-06-13");
 DECLARE _nowFBB INT DEFAULT(0);
 DECLARE _ubFBB, _lbFBB INT DEFAULT(0);
 DECLARE _numDaysLow INT DEFAULT 0;
 DECLARE _now DATE DEFAULT(CURDATE());
 
 DECLARE certs_total_num INT DEFAULT 0;
 DECLARE certs_grand_total_num INT DEFAULT 0;
 DECLARE certs_overdue_num INT DEFAULT 0;
 DECLARE certs_overdue_num_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_today INT DEFAULT 0;
 DECLARE certs_duein_today_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_1m INT DEFAULT 0;
 DECLARE certs_duein_1m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_2m INT DEFAULT 0;
 DECLARE certs_duein_2m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_3m INT DEFAULT 0;
 DECLARE certs_duein_3m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_46m INT DEFAULT 0;
 DECLARE certs_duein_46m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_712m INT DEFAULT 0;
 DECLARE certs_duein_712m_prcs DOUBLE DEFAULT 0;
 DECLARE certs_duein_13plusm INT DEFAULT 0;
 DECLARE certs_duein_13plusm_prcs DOUBLE DEFAULT 0;
 DECLARE certs_no_recalibrate INT DEFAULT 0;
 DECLARE certs_no_recalibrate_prcs DOUBLE DEFAULT 0;

 DECLARE EXIT HANDLER
 FOR SQLEXCEPTION
 BEGIN
   ROLLBACK;
   RESIGNAL;
 END;
 
 SET _nowFBB = DATEDIFF(CURDATE(),fbb);
 
 DROP TEMPORARY TABLE IF EXISTS tma;
 CREATE TEMPORARY TABLE tma ENGINE=Innodb
 SELECT bm.Id AS id_mark, bm.mark_name, bm.id_team as id_team, t.name AS team_name
 FROM appdb.bs_marks_v as bm
      INNER JOIN appdb.bs_teams AS t ON t.Id = bm.id_team
 ;

 START TRANSACTION;
 
    CREATE TEMPORARY TABLE scocrtstmp ENGINE=INNODB
    SELECT DISTINCT crts.*
    FROM -- appdb.sco_certs AS crts
         tma
	      INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                            crts.id_location = tma.id_mark
    WHERE crts.certDateFBB = (SELECT MAX(crts1.certDateFBB) FROM appdb.sco_certs crts1 WHERE crts1.id_company=tma.id_team AND crts1.SerialNo=crts.SerialNo)
	       -- AND EXISTS(SELECT * FROM appdb.bs_marks_v  AS m WHERE m.Id = crts.id_location)
	       -- AND EXISTS(SELECT * FROM appdb.bs_teams_v AS tm WHERE tm.Id = crts.id_company)
	       -- AND crts.CertDate = (SELECT MAX(crts1.CertDate) FROM appdb.sco_certs crts1 WHERE crts1.SerialNo=crts.SerialNo)
	 ;
 
    SELECT DISTINCT COUNT(*) INTO certs_total_num
	 FROM scocrtstmp;
	       
	 SELECT DISTINCT COUNT(*) INTO certs_grand_total_num
	 FROM -- appdb.sco_certs AS crts
	      tma
	      INNER JOIN appdb.sco_certs AS crts ON crts.id_company = tma.id_team AND
	                                            crts.id_location = tma.id_mark
	 -- WHERE EXISTS(SELECT * FROM appdb.bs_marks_v  AS m WHERE m.Id = crts.id_location) AND
	 --       EXISTS(SELECT * FROM appdb.bs_teams_v AS tm WHERE tm.Id = crts.id_company)
	 ;
		     
	 -- Due in today 
	 SET _numDays = -1;
	 SET _numDaysLow = 0;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_today
	   FROM scocrtstmp AS crts
	   WHERE crts.recalDateFBB = _nowFBB
	   ;		
	   IF certs_total_num <> 0 THEN
			SET certs_duein_today_prcs = ROUND((certs_duein_today * 100.00)/certs_total_num,1);
		END IF;
		
	 -- Due in 1 month 
	 SET _numDays = 30;
	 SET _numDaysLow = 0;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_1m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;		
	   IF certs_total_num <> 0 THEN
			SET certs_duein_1m_prcs = ROUND((certs_duein_1m * 100.00)/certs_total_num,1);
		END IF;
			
	 -- Due in 2 months	
	 SET _numDays = 60;
	 SET _numDaysLow = 30;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_2m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_2m_prcs = ROUND((certs_duein_2m * 100.00)/certs_total_num,1);
		END IF;
		
		-- Due in 3 months	
	 SET _numDays = 90;
	 SET _numDaysLow = 60;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_3m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_3m_prcs = ROUND((certs_duein_3m * 100.00)/certs_total_num,1);
		END IF;
	
	 -- Due in 4-6 months			
	 SET _numDays = 180;
	 SET _numDaysLow = 90;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_46m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_46m_prcs = ROUND((certs_duein_46m * 100.00)/certs_total_num,1);
		END IF;

    -- Due in 7-12 months				
	 SET _numDays = 365;
	 SET _numDaysLow = 180;
	 SET _lbFBB = _nowFBB + _numDaysLow;
    SET _ubFBB = _nowFBB + _numDays;
	   
	   SELECT DISTINCT COUNT(*) INTO certs_duein_712m
	   FROM scocrtstmp AS crts
	   WHERE -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL _numDaysLow DAY))>=0 AND datediff(DATE_ADD(_now,INTERVAL _numDays DAY),crts.RecalDate)>0;
	         crts.recalDateFBB > _lbFBB AND
	   		crts.recalDateFBB <= _ubFBB
	   ;	
	   IF certs_total_num <> 0 THEN
			SET certs_duein_712m_prcs = ROUND((certs_duein_712m * 100.00)/certs_total_num,1);
		END IF;
	 
	 -- Due in 13+ months			
	 SELECT DISTINCT COUNT(*) INTO certs_duein_13plusm
	 FROM scocrtstmp AS crts
	 WHERE crts.recalDateFBB > _nowFBB + 365 -- datediff(crts.RecalDate, DATE_ADD(_now,INTERVAL 360 DAY))>=0
	 ;
	 IF certs_total_num <> 0 THEN
	 	SET certs_duein_13plusm_prcs = ROUND((certs_duein_13plusm * 100.00)/certs_total_num,1);
	 END IF;
	 
	 -- Overdue
	 SELECT DISTINCT COUNT(*) INTO certs_overdue_num
	 FROM scocrtstmp AS crts
	 WHERE -- (datediff(_now,crts.RecalDate)>0) AND
			 -- (NOT (DATEDIFF(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0))
			 crts.recalDateFBB <= _nowFBB
			 AND crts.recalDateFBB >= 0
	 ;
	 IF certs_total_num <> 0 THEN
	 	SET certs_overdue_num_prcs = ROUND((certs_overdue_num * 100.00)/certs_total_num,1);
	 END IF; 
	 
	 -- Not being recalibrated        
	 SELECT DISTINCT COUNT(*) INTO certs_no_recalibrate
	 FROM scocrtstmp AS crts
    WHERE -- (datediff(crts.RecalDate,'1991/01/01')=0 OR datediff(crts.RecalDate,'0001/01/01')=0)
          crts.recalDateFBB < 0
          -- OR crts.recalDateFBB = _nowFBB
	 ;
    IF certs_total_num <> 0 THEN
	 	SET certs_no_recalibrate_prcs = ROUND((certs_no_recalibrate * 100.00)/certs_total_num,1);
	 END IF;
	 
	 DROP TEMPORARY TABLE IF EXISTS scocrtstmp;
	 
 COMMIT;

 SELECT
   CAST(certs_total_num AS INT) AS certs_total_num,
   CAST(certs_grand_total_num AS INT) AS certs_grand_total_num,
   CAST(certs_overdue_num AS INT) AS certs_overdue_num,
   CAST(certs_duein_today AS INT) AS certs_duein_today,
   CAST(certs_duein_1m AS INT) AS certs_duein_1m,
   CAST(certs_duein_2m AS INT) AS certs_duein_2m,
   CAST(certs_duein_3m AS INT) AS certs_duein_3m,
   CAST(certs_duein_46m AS INT) AS certs_duein_46m,
   CAST(certs_duein_712m AS INT) AS certs_duein_712m,
   CAST(certs_duein_13plusm AS INT) AS certs_duein_13plusm,
   CAST(certs_no_recalibrate AS INT) AS certs_no_recalibrate,
   CAST(certs_overdue_num_prcs AS DOUBLE) AS certs_overdue_num_prcs,
   CAST(certs_duein_today_prcs AS DOUBLE) AS certs_duein_today_prcs,
   CAST(certs_duein_1m_prcs AS DOUBLE) AS certs_duein_1m_prcs,
   CAST(certs_duein_2m_prcs AS DOUBLE) AS certs_duein_2m_prcs,
   CAST(certs_duein_3m_prcs AS DOUBLE) AS certs_duein_3m_prcs,
   CAST(certs_duein_46m_prcs AS DOUBLE) AS certs_duein_46m_prcs,
   CAST(certs_duein_712m_prcs AS DOUBLE) AS certs_duein_712m_prcs,
   CAST(certs_duein_13plusm_prcs AS DOUBLE) AS certs_duein_13plusm_prcs,
   CAST(certs_no_recalibrate_prcs AS DOUBLE) AS certs_no_recalibrate_prcs;
   
END//
DELIMITER ;

-- Dumping structure for procedure appdb.TGO_trivial_table_fe
DELIMITER //
CREATE PROCEDURE `TGO_trivial_table_fe`(IN uId varchar(255), IN prm01 varchar(255))
    READS SQL DATA
whole_proc:
  BEGIN
    DECLARE EXIT HANDLER -- vfd pattern for stored procedures
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK; -- vfd pattern for stored procedures
      RESIGNAL; -- vfd pattern for stored procedures
    END;

    START TRANSACTION; -- vfd pattern for stored procedures 
      SELECT
        ttt.*
      FROM TGO_trivial_table ttt
      WHERE ttt.ActorId = uId;
    COMMIT; -- vfd pattern for stored procedures
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.vfd_template
DELIMITER //
CREATE PROCEDURE `vfd_template`(
	IN `prm01` varchar(255)
)
    SQL SECURITY INVOKER
whole_proc:
  BEGIN
    DECLARE rc int DEFAULT 0;
    DECLARE msg varchar(255);

    DECLARE EXIT HANDLER
    FOR SQLEXCEPTION
    BEGIN
      ROLLBACK;
      -- GET DIAGNOSTICS CONDITION 1 rc = MYSQL_ERRNO, msg = MESSAGE_TEXT;
      -- SELECT
        -- CAST(rc AS UNSIGNED) AS RetValueInt,
        -- CAST(msg AS char(64)) AS RetValueString;
      RESIGNAL;
    END;

    START TRANSACTION;

    IF 1 = 1 THEN
      COMMIT;

      SET rc = 1;
      SELECT
        CAST(rc AS UNSIGNED) AS RetValueInt,
        CAST(prm01 AS char(127)) AS RetValueString;

      LEAVE whole_proc;
    END IF;

    COMMIT;

    SELECT
      CAST(rc AS INT) AS RetValueInt,
      CAST(prm01 AS char(64)) AS RetValueString;
  END//
DELIMITER ;

-- Dumping structure for procedure appdb.Z_TST
DELIMITER //
CREATE PROCEDURE `Z_TST`(
	IN `_ds` DATE,
	IN `_de` DATE
)
BEGIN
	SELECT _de - _ds;
END//
DELIMITER ;

-- Dumping structure for function appdb.IsSU
DELIMITER //
CREATE FUNCTION `IsSU`() RETURNS tinyint(1)
    READS SQL DATA
BEGIN
  DECLARE res BOOLEAN DEFAULT(FALSE);
  DECLARE wh VARCHAR(80) DEFAULT '';
  
  SET wh = SUBSTRING_INDEX (SESSION_USER(), '@', 1);
  
  IF EXISTS (SELECT *
             FROM cid.AspNetUserRoles AS ur
                  INNER JOIN cid.CommonConfig cc ON cc.SUSRID=ur.RoleId
             WHERE ur.UserId=wh) OR
     wh='root' OR
     wh='appsa'
  THEN
    SET res := TRUE;
  END IF;
 
  RETURN res;
END//
DELIMITER ;

-- Dumping structure for function appdb.IsSUbyId
DELIMITER //
CREATE FUNCTION `IsSUbyId`(`_id` VARCHAR(64)
) RETURNS tinyint(1)
    READS SQL DATA
BEGIN
  DECLARE res BOOLEAN DEFAULT(FALSE);
  
  IF EXISTS (SELECT *
             FROM cid.AspNetUserRoles AS ur
                  INNER JOIN cid.CommonConfig cc ON cc.SUSRID=ur.RoleId
             WHERE ur.UserId=_id)
  THEN
    SET res := TRUE;
  END IF;
 
  RETURN res;
END//
DELIMITER ;

-- Dumping structure for function appdb.WhoIsThis
DELIMITER //
CREATE FUNCTION `WhoIsThis`() RETURNS varchar(255) CHARSET utf8mb4
    READS SQL DATA
    SQL SECURITY INVOKER
BEGIN
  RETURN SUBSTRING_INDEX (SESSION_USER(), '@', 1);
END//
DELIMITER ;

-- Dumping structure for event appdb.sa_tech_day_start
DELIMITER //
CREATE EVENT `sa_tech_day_start` ON SCHEDULE EVERY 1 DAY STARTS '2021-10-22 00:35:05' ON COMPLETION NOT PRESERVE ENABLE DO BEGIN
  CALL appdb.sa_tech_day_start (NOW());
END//
DELIMITER ;

-- Dumping structure for event appdb.sa_tech_tick_call
DELIMITER //
CREATE EVENT `sa_tech_tick_call` ON SCHEDULE EVERY 5 MINUTE STARTS '2021-09-16 10:28:19' ON COMPLETION PRESERVE ENABLE DO CALL appdb.sa_tech_tick//
DELIMITER ;

-- Dumping structure for trigger appdb.bs_invite_traps_after_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_invite_traps_after_delete` AFTER DELETE ON `bs_invite_traps` FOR EACH ROW BEGIN
   DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
   
	INSERT INTO appdb.bs_objects_protocol (_type, id_object, id_user, logged, action_dscr)
	VALUES (1, OLD.Id, _who, NOW(), 'deleted');
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_invite_traps_after_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_invite_traps_after_insert` AFTER INSERT ON `bs_invite_traps` FOR EACH ROW BEGIN
	INSERT INTO appdb.bs_objects_protocol (_type, id_object, id_user, logged, action_dscr)
	VALUES (1, NEW.Id, NEW.change_id_user, NEW.change_logged, 'created');
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_invite_traps_after_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_invite_traps_after_update` AFTER UPDATE ON `bs_invite_traps` FOR EACH ROW BEGIN
	INSERT INTO appdb.bs_objects_protocol (_type, id_object, id_user, logged, action_dscr)
	VALUES (1, NEW.Id, NEW.change_id_user, NEW.change_logged, 'state updated');
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_invite_traps_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_invite_traps_before_delete` BEFORE DELETE ON `bs_invite_traps` FOR EACH ROW BEGIN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Invitation cannot be deleted.';
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_invite_traps_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_invite_traps_before_insert` BEFORE INSERT ON `bs_invite_traps` FOR EACH ROW BEGIN
	 DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	 
    IF NOT (IsSU() OR
	         EXISTS(SELECT *
				       FROM appdb.bs_team_users AS tu
						 WHERE tu.id_bst=NEW.id_bst AND
						       tu.id_user=_who AND
								 tu._role=1)
			  )
	 THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
    
    IF EXISTS(SELECT *
	           FROM appdb.bs_invite_traps it
				  WHERE it.id_bst=NEW.id_bst AND
				        it.user_email=NEW.user_email AND
						  it._state IN (0,1))
    THEN
    		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'There are another active or completed invitations for this e-mail.';
    END IF;
    
    INSERT INTO appdb.bs_security_disable (id_user, _state)
	 VALUES (_who,1);
	 
	 INSERT INTO appdb.bs_team_users (id_bst, id_user, _role)
    SELECT NEW.id_bst, u.Id, 0
    FROM cid.AspNetUsers AS u
	 WHERE u.NormalizedEmail=NEW.user_email;
    
    CASE ROW_COUNT()
    	WHEN 0 THEN BEGIN END;
    	WHEN 1 THEN SET NEW._state:=1;
    	ELSE SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'More then one invite for email address.';
    END CASE;
 
	 DELETE FROM appdb.bs_security_disable
    WHERE id_user=_who AND
   		 _state=1;
   		 
    SET NEW.change_id_user := _who;
	 SET NEW.change_logged := NOW();
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_invite_traps_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_invite_traps_before_update` BEFORE UPDATE ON `bs_invite_traps` FOR EACH ROW `whole_proc`:
BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
   IF OLD.user_email<>NEW.user_email THEN
	  	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'email cannot be updated.';
   END IF;
   
   SET NEW.change_id_user := _who;
	SET NEW.change_logged := NOW();
	 
	IF OLD._state<>NEW._state THEN
 	  	IF OLD._state=0 AND NEW._state=2 THEN
         IF (IsSU() OR 
			   EXISTS(SELECT *
				       FROM appdb.bs_team_users tu
						 WHERE tu.id_bst=NEW.id_bst AND
						       tu.id_user=_who AND
								 tu._role=1)
		   )THEN
				  LEAVE `whole_proc`;
		   END IF;
		   SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
		 END IF;
		 IF OLD._state=0 AND NEW._state=1 THEN
		    IF (EXISTS(SELECT *
			            FROM appdb.bs_security_disable AS sd
				         WHERE sd.id_user=_who AND
						         sd._state=1))
		  	 THEN
		  	    LEAVE `whole_proc`;
		  	 END IF;
		 END IF;
		 IF OLD._state=1 AND NEW._state=3 THEN
		    IF (EXISTS(SELECT *
			            FROM appdb.bs_security_disable AS sd
				         WHERE sd.id_user=_who AND
						         sd._state=1))
		  	 THEN
		  	    LEAVE `whole_proc`;
		  	 END IF;
		 END IF;
		  	
		 SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Such a combination of old and new states is not allowed.';  
  	END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_marks_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_marks_before_delete` BEFORE DELETE ON `bs_marks` FOR EACH ROW BEGIN
   DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT IsSU()
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
   IF EXISTS (SELECT *
			     FROM appdb.bs_team_marks tm
				  WHERE tm.id_mark=OLD.id)
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Correspondent entities exists - deletion impossible (A1).';
   END IF;
   IF EXISTS (SELECT *
			     FROM appdb.sco_certs AS crt
				  WHERE crt.id_location=OLD.id)
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Correspondent entities exists - deletion impossible (A2).';
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_marks_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_marks_before_insert` BEFORE INSERT ON `bs_marks` FOR EACH ROW BEGIN
	IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_marks_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_marks_before_update` BEFORE UPDATE ON `bs_marks` FOR EACH ROW BEGIN
	IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_marks_to_users_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_marks_to_users_before_delete` BEFORE DELETE ON `bs_marks_to_users` FOR EACH ROW BEGIN
   DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
   IF NOT (EXISTS (SELECT *
			     FROM appdb.bs_team_users tu
				  WHERE tu.id_bst = OLD.id_team and
				        tu._role = 1)
		     OR IsSU()
			 ) 
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You have no priviledges to execute this operation.';
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_marks_to_users_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_marks_to_users_before_insert` BEFORE INSERT ON `bs_marks_to_users` FOR EACH ROW BEGIN
   DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
   IF NOT (EXISTS (SELECT *
			     FROM appdb.bs_team_users tu
				  WHERE tu.id_bst = NEW.id_team and
				        tu._role = 1)
		     OR IsSU()
			 ) 
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You have no priviledges to execute this operation.';
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_teams_after_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_teams_after_insert` AFTER INSERT ON `bs_teams` FOR EACH ROW BEGIN
  DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
  
  INSERT INTO appdb.bs_security_disable (id_user, _state)
  VALUES (_who,5);
	
    INSERT INTO appdb.bs_team_configs (id_team, id_cnf, cnfValue)
    SELECT NEW.Id, tc.Id, tc.cnfDefault
    FROM appdb.bs_team_configs_avl AS tc
    ;
    
  DELETE FROM appdb.bs_security_disable
  WHERE id_user=_who AND
   	 _state=5; 
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_teams_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_teams_before_delete` BEFORE DELETE ON `bs_teams` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
    IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
    
    IF EXISTS(SELECT *
              FROM bs_team_users btu
              WHERE btu.id_bst = OLD.Id) OR
       EXISTS(SELECT *
              FROM bs_invite_traps as btr
              WHERE btr.id_bst = OLD.Id) OR
       EXISTS(SELECT *
              FROM appdb.bs_team_marks AS btm
              WHERE btm.id_bst = OLD.Id)
    THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'A team with members, invites or marks cannot be deleted.';
    END IF;
    
    IF EXISTS(SELECT *
              FROM appdb.bs_team_configs AS tc
              WHERE tc.id_team = OLD.Id)
    THEN
     INSERT INTO appdb.bs_security_disable (id_user, _state)
	  VALUES (_who,5);
	  
      DELETE FROM appdb.bs_team_configs
      WHERE id_team = OLD.Id;
      
     DELETE FROM appdb.bs_security_disable
     WHERE id_user=_who AND
   	     _state=5;
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_teams_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_teams_before_insert` BEFORE INSERT ON `bs_teams` FOR EACH ROW BEGIN
    IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_teams_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_teams_before_update` BEFORE UPDATE ON `bs_teams` FOR EACH ROW BEGIN
    IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_avl_after_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_avl_after_insert` AFTER INSERT ON `bs_team_configs_avl` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	INSERT INTO appdb.bs_security_disable (id_user, _state)
	VALUES (_who,5);
	
    INSERT INTO appdb.bs_team_configs (id_team, id_cnf, cnfValue)
    SELECT ts.Id, NEW.Id, NEW.cnfDefault
    FROM appdb.bs_teams AS ts
    ;
    
   DELETE FROM appdb.bs_security_disable
   WHERE id_user=_who AND
   	   _state=5;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_avl_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_avl_before_delete` BEFORE DELETE ON `bs_team_configs_avl` FOR EACH ROW BEGIN 
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
    IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
    
    IF EXISTS(SELECT *
              FROM appdb.bs_team_configs AS tc
              WHERE tc.id_cnf = OLD.Id)
    THEN
     INSERT INTO appdb.bs_security_disable (id_user, _state)
	  VALUES (_who,5);
	  
      DELETE FROM appdb.bs_team_configs
      WHERE id_cnf = OLD.Id;
      
     DELETE FROM appdb.bs_security_disable
     WHERE id_user=_who AND
   	 _state=5; 
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_avl_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_avl_before_insert` BEFORE INSERT ON `bs_team_configs_avl` FOR EACH ROW BEGIN
    IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_avl_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_avl_before_update` BEFORE UPDATE ON `bs_team_configs_avl` FOR EACH ROW BEGIN
    IF NOT IsSU() THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_before_delete` BEFORE DELETE ON `bs_team_configs` FOR EACH ROW BEGIN 
   DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (EXISTS(SELECT *
				      FROM appdb.bs_security_disable AS sd
						WHERE sd.id_user=_who AND
						      sd._state=5)
			  )
	 THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_before_insert` BEFORE INSERT ON `bs_team_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (EXISTS(SELECT *
				      FROM appdb.bs_security_disable AS sd
						WHERE sd.id_user=_who AND
						      sd._state=5)
			  )
	 THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_configs_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_configs_before_update` BEFORE UPDATE ON `bs_team_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF (NEW.id_team <> OLD.id_team OR NEW.id_cnf <> OLD.id_cnf) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'These fields cannot be updated.';
	END IF;
	
	IF NOT (IsSU() OR 
           EXISTS (SELECT *
			          FROM appdb.bs_team_users tu
						 WHERE tu.id_bst = OLD.id_team AND
						       tu.id_user = _who AND
								 tu._role=1)
		    ) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
  
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_marks_after_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_marks_after_delete` AFTER DELETE ON `bs_team_marks` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT IsSU()
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
   
   IF NOT EXISTS(SELECT * FROM appdb.bs_team_marks WHERE id_mark = OLD.id_mark) THEN 
	   DELETE FROM appdb.bs_marks
	   WHERE Id = OLD.id_mark;
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_marks_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_marks_before_delete` BEFORE DELETE ON `bs_team_marks` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT IsSU()
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
   
   IF EXISTS (SELECT *
			     FROM appdb.sco_certs AS crt
				  WHERE crt.id_location=OLD.id_mark AND crt.id_company=OLD.id_bst)
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Correspondent entities exists - deletion impossible. (B1)';
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_marks_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_marks_before_insert` BEFORE INSERT ON `bs_team_marks` FOR EACH ROW BEGIN
   DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT IsSU()
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
   IF EXISTS (SELECT *
			     FROM appdb.bs_team_marks tm
				  WHERE tm.id_mark = NEW.id_mark and
				        tm.id_bst = NEW.id_bst)
	THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'The entity already belongs to this set - no need to add.';
   END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_marks_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_marks_before_update` BEFORE UPDATE ON `bs_team_marks` FOR EACH ROW BEGIN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Mission impossible ;-)';
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_users_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_users_before_delete` BEFORE DELETE ON `bs_team_users` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU() OR 
           (EXISTS (SELECT *
			           FROM appdb.bs_team_users tu
						  WHERE tu.id_bst=OLD.id_bst AND
						        tu.id_user=_who AND
								  tu._role=1)
				 AND OLD._role<>1)
		    ) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
   
   INSERT INTO appdb.bs_security_disable (id_user, _state)
	VALUES (_who,1);
	
	UPDATE appdb.bs_invite_traps it
	INNER JOIN cid.AspNetUsers AS u ON u.NormalizedEmail=it.user_email
	SET _state=3
	WHERE it.id_bst=OLD.id_bst AND
	      u.Id=OLD.id_user;
      
	DELETE FROM appdb.bs_security_disable
   WHERE id_user=_who AND
         _state=1;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_users_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_users_before_insert` BEFORE INSERT ON `bs_team_users` FOR EACH ROW BEGIN
	 DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	 
    IF NOT (IsSU() OR
	         EXISTS(SELECT *
				       FROM appdb.bs_security_disable AS sd
						 WHERE sd.id_user=_who AND
						       sd._state=1)
			  )
	 THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.bs_team_users_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `bs_team_users_before_update` BEFORE UPDATE ON `bs_team_users` FOR EACH ROW BEGIN
DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();

IF NEW.id_bst<>OLD.id_bst OR NEW.id_user<>OLD.id_user THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'These fields cannot be updated.';
END IF;

IF NEW._role<>OLD._role THEN
	IF NOT (IsSU() OR 
           (EXISTS (SELECT *
			           FROM appdb.bs_team_users tu
						  WHERE tu.id_bst=NEW.id_bst AND
						        tu.id_user=_who AND
								  tu._role=1)
				 AND OLD._role<>1)
		    ) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
   END IF;
END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.cr_certsIUops_after_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `cr_certsIUops_after_insert` AFTER INSERT ON `cr_certsIUops` FOR EACH ROW `whole_proc`:
BEGIN

LEAVE `whole_proc`;

	IF EXISTS(SELECT *
				 FROM appdb.cr_certsIUops AS ops
				 WHERE ops.CertNo=NEW.CertNo AND
				       ops.Company=NEW.Company AND
						 ops.CustCode=NEW.CustCode AND
						 ops.CustRef=NEW.CustRef AND
						 ops.SerialNo=NEW.SerialNo AND
						 ops.PlantNo=NEW.PlantNo AND
						 ops.Descr=NEW.Descr AND
						 ops.Range_=NEW.Range_ AND
						 ops.CLoc=NEW.CLoc AND
						 ops.CertDate=NEW.CertDate AND
						 ops.CalDate=NEW.CalDate AND
						 ops.RecalDate=NEW.RecalDate AND
						 ops.Preadj=NEW.Preadj
				 HAVING COUNT(*)>1)
	THEN
	   INSERT INTO appdb.cr_certsIUops_results (id_certsIUop, state, DateIn, id_cert, description)
	   VALUES (NEW.id, 3, NOW(), 
		        (SELECT id FROM appdb.sco_certs WHERE CertNo=NEW.CertNo),
				  'was imported before, no changes found'
				 );
	END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sa_global_configs_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sa_global_configs_before_delete` BEFORE DELETE ON `sa_global_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU()) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sa_global_configs_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sa_global_configs_before_insert` BEFORE INSERT ON `sa_global_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU()) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sa_global_configs_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sa_global_configs_before_update` BEFORE UPDATE ON `sa_global_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU()) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sa_user_configs_before_delete
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sa_user_configs_before_delete` BEFORE DELETE ON `sa_user_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU() OR OLD.uid  = _who) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sa_user_configs_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sa_user_configs_before_insert` BEFORE INSERT ON `sa_user_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU() OR NEW.uid = _who) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sa_user_configs_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sa_user_configs_before_update` BEFORE UPDATE ON `sa_user_configs` FOR EACH ROW BEGIN
	DECLARE _who VARCHAR(64) DEFAULT WhoIsThis();
	
	IF NOT (IsSU() OR NEW.uid = _who) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'You don\'t have enought priveledges to execute this operation.';
    END IF;
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sco_certs_before_insert
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sco_certs_before_insert` BEFORE INSERT ON `sco_certs` FOR EACH ROW BEGIN

DECLARE fbb DATE DEFAULT("1991-06-13");

SET NEW.certDateFBB = DATEDIFF(NEW.CertDate,fbb);
SET NEW.recalDateFBB = DATEDIFF(NEW.RecalDate,fbb);

END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.sco_certs_before_update
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER `sco_certs_before_update` BEFORE UPDATE ON `sco_certs` FOR EACH ROW BEGIN

DECLARE fbb DATE DEFAULT("1991-06-13");

SET NEW.certDateFBB = DATEDIFF(NEW.CertDate,fbb);
SET NEW.recalDateFBB = DATEDIFF(NEW.RecalDate,fbb);

END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for trigger appdb.tgo_tt_bi
SET @OLDTMP_SQL_MODE=@@SQL_MODE, SQL_MODE='STRICT_TRANS_TABLES,ERROR_FOR_DIVISION_BY_ZERO,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION';
DELIMITER //
CREATE TRIGGER tgo_tt_bi
BEFORE INSERT
ON TGO_trivial_table
FOR EACH ROW
BEGIN
  SET NEW.ActorId = WhoIsThis();
END//
DELIMITER ;
SET SQL_MODE=@OLDTMP_SQL_MODE;

-- Dumping structure for view appdb.bs_invite_traps_v
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `bs_invite_traps_v`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `bs_invite_traps_v` AS select distinct `it`.`Id` AS `Id`,`it`.`id_bst` AS `id_bst`,`it`.`user_email` AS `user_email`,`it`.`_state` AS `_state`,cast(case `it`.`_state` when 0 then 'active' when 1 then 'completed' when 2 then 'cancelled' when 3 then 'excluded' else '?unknown?' end as char(64) charset utf8mb4) AS `_stateVal`,`it`.`change_id_user` AS `change_id_user`,`it`.`change_logged` AS `change_logged` from `appdb`.`bs_invite_traps` `it` where exists(select 1 from `appdb`.`bs_team_users` `bstu` where `bstu`.`id_bst` = `it`.`id_bst` and `bstu`.`_role` = 1 and `bstu`.`id_user` = `WhoIsThis`() limit 1) or `IsSU`();

-- Dumping structure for view appdb.bs_marks_v
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `bs_marks_v`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `bs_marks_v` AS select distinct `bms`.`Id` AS `Id`,`bms`.`name` AS `mark_name`,`mtt`.`id_bst` AS `id_team` from (`appdb`.`bs_marks` `bms` join `appdb`.`bs_team_marks` `mtt` on(`mtt`.`id_mark` = `bms`.`Id`)) where `IsSU`() or !`IsSU`() and exists(select 1 from `appdb`.`bs_team_users` `tu` where `tu`.`id_bst` = `mtt`.`id_bst` and `tu`.`id_user` = `WhoIsThis`() limit 1) and !exists(select 1 from `appdb`.`bs_marks_to_users` `mtu` where `mtu`.`id_mark` = `bms`.`Id` and `mtu`.`id_user` = `WhoIsThis`() limit 1);

-- Dumping structure for view appdb.bs_teams_v
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `bs_teams_v`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `bs_teams_v` AS select distinct `bts`.`Id` AS `Id`,`bts`.`name` AS `name` from `appdb`.`bs_teams` `bts` where `IsSU`() or exists(select 1 from `appdb`.`bs_team_users` `btu` where `btu`.`id_bst` = `bts`.`Id` and `btu`.`id_user` = `WhoIsThis`() limit 1);

-- Dumping structure for view appdb.bs_team_users_v_
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `bs_team_users_v_`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `bs_team_users_v_` AS select distinct `bstu`.`id_bst` AS `id_bst`,`bstu`.`id_user` AS `id_user`,`bstu`.`_role` AS `_role` from `appdb`.`bs_team_users` `bstu` where exists(select 1 from `appdb`.`bs_teams_v` `bst` where `bst`.`Id` = `bstu`.`id_bst` limit 1);

-- Dumping structure for view appdb.TGO_trivial_table_v
-- Removing temporary table and create final VIEW structure
DROP TABLE IF EXISTS `TGO_trivial_table_v`;
CREATE ALGORITHM=UNDEFINED SQL SECURITY DEFINER VIEW `TGO_trivial_table_v` AS select distinct `ttt`.`Id` AS `Id`,`ttt`.`ActionDate` AS `ActionDate`,`ttt`.`ExactDate` AS `ExactDate`,`ttt`.`Selected` AS `Selected` from `TGO_trivial_table` `ttt` where `ttt`.`ActorId` = substring_index(user(),'@',1);

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
