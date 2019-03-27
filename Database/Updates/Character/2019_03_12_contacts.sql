-- Dumping structure for table nexus_forever_character.contacts
CREATE TABLE IF NOT EXISTS `contacts` (
  `id` bigint(20) unsigned NOT NULL DEFAULT '0',
  `ownerId` bigint(20) unsigned NOT NULL DEFAULT '0',
  `contactId` bigint(20) unsigned NOT NULL DEFAULT '0',
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `inviteMessage` varchar(100) DEFAULT '',
  `privateNote` varchar(100) DEFAULT '',
  `accepted` tinyint(1) unsigned NOT NULL DEFAULT '0',
  `requestTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`,`ownerId`,`contactId`),
  KEY `FK__contacts_ownerId__character_id` (`ownerId`),
  KEY `FK__contacts_contactId__character_id` (`contactId`),
  CONSTRAINT `FK__contacts_contactId__character_id` FOREIGN KEY (`contactId`) REFERENCES `character` (`id`) ON DELETE CASCADE,
  CONSTRAINT `FK__contacts_ownerId__character_id` FOREIGN KEY (`ownerId`) REFERENCES `character` (`id`) ON DELETE CASCADE
)