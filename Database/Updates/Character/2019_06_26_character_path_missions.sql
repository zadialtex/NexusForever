-- Dumping structure for table nexus_forever_character.character_path_episode
CREATE TABLE IF NOT EXISTS `character_path_episode` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `episodeId` int(10) unsigned NOT NULL DEFAULT '0',
  `rewardReceived` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`,`episodeId`),
  CONSTRAINT `FK_character_episode_id__character_id` FOREIGN KEY (`id`) REFERENCES `character` (`id`) ON DELETE CASCADE
);

-- Dumping structure for table nexus_forever_character.character_path_mission
CREATE TABLE IF NOT EXISTS `character_path_mission` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `episodeId` int(10) unsigned NOT NULL DEFAULT '0',
  `missionId` int(10) unsigned NOT NULL DEFAULT '0',
  `progress` int(10) unsigned NOT NULL DEFAULT '0',
  `state` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`,`missionId`,`episodeId`),
  KEY `FK_character_mission_id__character_id` (`id`,`episodeId`),
  CONSTRAINT `FK_character_mission_id__character_id` FOREIGN KEY (`id`, `episodeId`) REFERENCES `character_path_episode` (`id`, `episodeid`) ON DELETE CASCADE
);
