DROP PROCEDURE IF EXISTS CreateQuestLog;
DELIMITER //
CREATE PROCEDURE CreateQuestLog(IN QuestID int, IN CharID int, IN iProgress int, IN Status int)
BEGIN
	IF ( SELECT EXISTS ( SELECT * FROM tbl_Quest_Log WHERE Quest_ID = QuestID AND Character_ID = CharID ) ) THEN
		SELECT Log_ID FROM tbl_Quest_Log WHERE Quest_ID = QuestID AND Character_ID = CharID;
    ELSE
		INSERT INTO tbl_Quest_Log ( Character_ID, Quest_ID, Quest_Status, Progress )
        SELECT CharID, QuestID, Status, iProgress;
        SELECT Log_ID FROM tbl_Quest_Log WHERE Quest_ID = QuestID AND Character_ID = CharID;
    END IF;
END //
DELIMITER ;

CALL CreateQuestLog(2,1,0,4);