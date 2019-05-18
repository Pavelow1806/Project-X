DROP PROCEDURE IF EXISTS RegisterAccount;
DELIMITER //
CREATE PROCEDURE RegisterAccount(IN iUsername varchar(255), IN iPassword varchar(255), IN iEmail varchar(255))
BEGIN
	SET @currenttime = now();
	IF ( SELECT EXISTS ( SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Username = iUsername OR tbl_Accounts.Email = iEmail  ) ) THEN
		IF ( SELECT EXISTS ( SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Username = iUsername ) ) THEN
			SELECT CONCAT("The username '", iUsername, "' is already taken.") AS Result, -1 as Account_ID;
		ELSEIF ( SELECT EXISTS ( SELECT * FROM tbl_Accounts WHERE tbl_Accounts.Email = iEmail ) ) THEN
			SELECT CONCAT("The email '", iEmail, "' is already in use.") AS Result, -1 as Account_ID;
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
			iUsername,
            iEmail,
            iPassword,
            0;
		CALL LogAccountActivity(iUsername, 4, "System", 1);
        SELECT "The account was setup successfully." AS Result, Account_ID FROM tbl_Accounts WHERE Username = iUsername AND Password = iPassword AND Email = iEmail LIMIT 1;
	END IF;		
END //
DELIMITER ;

CALL RegisterAccount('someone1111', '123', '1231231111');