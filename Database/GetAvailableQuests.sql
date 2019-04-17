DROP PROCEDURE IF EXISTS GetAvailableQuests;
DELIMITER //
CREATE PROCEDURE GetAvailableQuests(IN CharID int)
BEGIN
	SELECT A.* 
    FROM tbl_Quests AS A
    LEFT JOIN tbl_Quest_Log
    ON A.Quest_ID = tbl_Quest_Log.Quest_ID
    WHERE 
		Quest_Status is null
	AND
		(SELECT Quest_Status FROM tbl_Quest_Log WHERE Quest_ID = A.Start_Requirement_Quest_ID) = 2;
END //
DELIMITER ;

CALL GetAvailableQuests(1);