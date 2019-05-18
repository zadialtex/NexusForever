-- Dumping structure for table nexus_forever_character.guild
CREATE TABLE IF NOT EXISTS `guild` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `name` varchar(50) NOT NULL,
  `leaderId` bigint(20) unsigned NOT NULL DEFAULT '0',
  `createTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `FK__guild_leaderId__character_id` (`leaderId`),
  CONSTRAINT `FK__guild_leaderId__character_id` FOREIGN KEY (`leaderId`) REFERENCES `character` (`id`) ON DELETE CASCADE
);

-- Dumping structure for table nexus_forever_character.guild_guild_data
CREATE TABLE IF NOT EXISTS `guild_guild_data` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `taxes` int(10) unsigned NOT NULL DEFAULT '0',
  `motd` varchar(200) NOT NULL DEFAULT '',
  `additionalInfo` varchar(200) NOT NULL DEFAULT '',
  `backgroundIconPartId` smallint(5) unsigned NOT NULL DEFAULT '0',
  `foregroundIconPartId` smallint(5) unsigned NOT NULL DEFAULT '0',
  `scanLinesPartId` smallint(5) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  CONSTRAINT `FK__guild_guild_data_id__guild_id` FOREIGN KEY (`id`) REFERENCES `guild` (`id`) ON DELETE CASCADE
);

-- Dumping structure for table nexus_forever_character.guild_member
CREATE TABLE IF NOT EXISTS `guild_member` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `characterId` bigint(20) unsigned NOT NULL DEFAULT '0',
  `rank` tinyint(3) unsigned NOT NULL,
  `note` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`id`,`characterId`),
  KEY `FK__guild_member_characterId__character_id` (`characterId`),
  KEY `FK__guild_member_id-rank__guild_rank_id-index` (`id`,`rank`),
  CONSTRAINT `FK__guild_member_characterId__character_id` FOREIGN KEY (`characterId`) REFERENCES `character` (`id`) ON DELETE CASCADE,
  CONSTRAINT `FK__guild_member_id__guild_id` FOREIGN KEY (`id`) REFERENCES `guild` (`id`) ON DELETE CASCADE
);

-- Dumping structure for table nexus_forever_character.guild_rank
CREATE TABLE IF NOT EXISTS `guild_rank` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `index` tinyint(3) unsigned NOT NULL,
  `name` varchar(50) NOT NULL,
  `permission` int(11) NOT NULL,
  `bankWithdrawPermission` bigint(20) unsigned NOT NULL DEFAULT '0',
  `moneyWithdrawalLimit` bigint(20) unsigned NOT NULL DEFAULT '0',
  `repairLimit` bigint(20) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`,`index`),
  CONSTRAINT `FK__guild_rank_id__guild_id` FOREIGN KEY (`id`) REFERENCES `guild` (`id`) ON DELETE CASCADE
);


-- Used for setting guild title
ALTER TABLE `character` 
    ADD COLUMN `guildAffiliation` bigint(20) unsigned NOT NULL DEFAULT '0' AFTER `timePlayedLevel`,
    ADD COLUMN `guildHolomarkMask` tinyint(3) unsigned NOT NULL DEFAULT '0' AFTER `guildAffiliation`;