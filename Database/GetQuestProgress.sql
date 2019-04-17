DROP PROCEDURE IF EXISTS GetQuestProgress;
DELIMITER //
CREATE PROCEDURE GetQuestProgress()
BEGIN
	SELECT * 
    FROM tbl_Quest_Log
    LEFT JOIN tbl_Characters
    ON tbl_Quest_Log.Character_ID = tbl_Characters.Character_ID
    LEFT JOIN tbl_Quests
    ON tbl_Quests.Quest_ID = tbl_Quest_Log.Quest_ID
    LEFT JOIN tbl_Quest_Status
    ON tbl_Quest_Status.Status_ID = tbl_Quest_Log.Quest_Status;
END //
DELIMITER ;

CALL GetQuestProgress();