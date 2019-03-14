DROP PROCEDURE IF EXISTS RegisterAccount;
DELIMITER //
CREATE PROCEDURE RegisterAccount(IN username varchar(255), IN password varchar(255), IN email varchar(255))
BEGIN
	SET @currenttime = now();
	IF ( SELECT EXISTS ( SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Username = username OR tbl_Accounts.Email = email  ) ) THEN
		IF ( SELECT EXISTS ( SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Username = username ) ) THEN
			SELECT CONCAT("The username '", username, "' is already taken.") AS Result;
		ELSEIF ( SELECT EXISTS ( SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Email = email ) ) THEN
			SELECT CONCAT("The email '", email, "' is already in use.") AS Result;
		END IF;
	ELSE
		INSERT INTO tbl_Accounts
			(
				Username,
                Email,
                Password,
                Logged_In
			)
		SELECT
			username,
            email,
            password,
            0;
		CALL LogAccountActivity(username, 4, "System", 1);
        SELECT "The account was setup successfully." AS Result, (SELECT Account_ID FROM tbl_Accounts WHERE Username = username AND Email = email) AS Account_ID;
	END IF;		
END //
DELIMITER ;

CALL RegisterAccount("Fiori", "123", "fiori94@gmail.com");