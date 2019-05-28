ALTER TABLE `character` 
    ADD COLUMN `bindPoint` smallint(5) unsigned NOT NULL DEFAULT '0' AFTER `worldZoneId`;