ALTER TABLE `character`
	ADD COLUMN `totalXp` INT(10) UNSIGNED NOT NULL DEFAULT '0' AFTER `activeCostumeIndex`;