DROP PROCEDURE IF EXISTS GetCharacterList;
DELIMITER //
CREATE PROCEDURE GetCharacterList(IN username varchar(255))
BEGIN
	SELECT tbl_Characters.* FROM tbl_Characters LEFT JOIN tbl_Accounts ON tbl_Accounts.Account_ID = tbl_Characters.Account_ID WHERE tbl_Accounts.Username = username;
END //
DELIMITER ;

CALL GetCharacterList("Pavelow");