DROP PROCEDURE IF EXISTS CreateCharacter;
DELIMITER //
CREATE PROCEDURE CreateCharacter(IN AccountID int, IN iName varchar(255), IN iGender int)
BEGIN
	IF EXISTS ( SELECT * FROM tbl_Characters WHERE Character_Name = iName )  THEN
		SELECT -1;
	ELSE
		INSERT INTO tbl_Characters
			( Account_ID, Character_Name, Character_Level, Pos_X, Pos_Y, Pos_Z, Rotation_Y, Camera_Pos_X, Camera_Pos_Y, Camera_Pos_Z, Camera_Rotation_Y, Gender, Health, Strength, Agility )
		SELECT 
			AccountID, iName, 1, 0, 0, 0, 0, 0, 0, 0, 0, iGender, 100, 10, 10;
		SELECT Character_ID FROM tbl_Characters WHERE Character_Name = iName;
    END IF;
END //
DELIMITER ;

CALL CreateCharacter(1, 'Soulhammer', 0);