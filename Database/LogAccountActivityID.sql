DROP PROCEDURE IF EXISTS LogAccountActivityID;
DELIMITER //
CREATE PROCEDURE LogAccountActivityID(IN CharacterID int, IN activitytype tinyint unsigned, IN sessionid varchar(255), IN ReturnResults bit)
BEGIN
	SET @currenttime = now();
    SET @accountid = 0;
	INSERT INTO tbl_Activity
		(
			Account_ID,
            Activity_Type,
            DTStamp,
            Session_ID
		)
	SELECT
			(SELECT Account_ID FROM tbl_Characters WHERE Character_ID = CharacterID),
            activitytype,
            @currenttime,
            sessionid
	;
    IF ReturnResults = 0 THEN
		SELECT * FROM tbl_Activity WHERE Account_ID = @accountid AND DTStamp = @currenttime;
	END IF;
END //
DELIMITER ;

CALL LogAccountActivityID(1, 1, "Test", 0);