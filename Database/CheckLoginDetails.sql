DROP PROCEDURE IF EXISTS CheckLoginDetails;
DELIMITER //
CREATE PROCEDURE CheckLoginDetails(IN username varchar(255), IN password varchar(255))
BEGIN
	IF NOT username = "System" THEN
		SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Username = username AND tbl_Accounts.Password = password;
	END IF;
END //
DELIMITER ;

CALL CheckLoginDetails("Pavelow", "1234");