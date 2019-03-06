DROP PROCEDURE IF EXISTS LogAccountActivity;
DELIMITER //
CREATE PROCEDURE LogAccountActivity(IN username varchar(255), IN activitytype tinyint unsigned, IN sessionid varchar(255), IN ReturnResults bit)
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
			(SELECT @accountid:=Account_ID FROM tbl_Accounts WHERE tbl_Accounts.Username = username),
            activitytype,
            @currenttime,
            sessionid
	;
    IF ReturnResults = 0 THEN
		SELECT * FROM tbl_Activity WHERE Account_ID = @accountid AND DTStamp = @currenttime;
	END IF;
END //
DELIMITER ;

CALL LogAccountActivity("Pavelow", 1, "Test", 1);